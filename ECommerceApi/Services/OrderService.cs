using ECommerceApi.Common;
using ECommerceApi.DTOs.Order;
using ECommerceApi.Models;
using ECommerceApi.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using KeyNotFoundException = System.Collections.Generic.KeyNotFoundException;

namespace ECommerceApi.Services
{
    public class OrderService(
        IUnitOfWork uow, ICouponService couponService, IPaymentService paymentService,
        IOrderNotifier notifier, IOptions<SePaySettings> sepayOptions, ILogger<OrderService> logger) : IOrderService
    {
        private const decimal FlatShippingFee = 30000m;   // phí ship phẳng (MVP)
        private readonly SePaySettings _sepay = sepayOptions.Value;

        // chuyển khoản qua SePay (VietQR) — phân biệt với COD
        private static bool IsBankTransfer(string method) =>
            method.Equals("SePay", StringComparison.OrdinalIgnoreCase)
            || method.Equals("BankTransfer", StringComparison.OrdinalIgnoreCase);

        public async Task<OrderDto> CheckoutAsync(int userId, CheckoutRequest request)
        {
            // 1) Lấy giỏ user (kèm variant + product)
            var cart = await uow.Repository<Cart>().Query()
                .Include(c => c.Items).ThenInclude(i => i.Variant)
                .Include(c => c.Items).ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart is null || cart.Items.Count == 0)
                throw new InvalidOperationException("Giỏ hàng trống");

            // 2) ══ BẮT ĐẦU TRANSACTION ══
            await uow.BeginAsync();
            try
            {
                var bankTransfer = IsBankTransfer(request.PaymentMethod);
                var order = new Order
                {
                    OrderCode = GenerateOrderCode(),
                    UserId = userId,
                    ShipRecipient = request.ShipRecipient,
                    ShipPhone = request.ShipPhone,
                    ShipProvince = request.ShipProvince,
                    ShipDistrict = request.ShipDistrict,
                    ShipWard = request.ShipWard,
                    ShipAddressLine = request.ShipAddressLine,
                    PaymentMethod = request.PaymentMethod,
                    Note = request.Note,
                    // SePay: chờ thanh toán rồi mới confirm; COD: xác nhận luôn
                    Status = bankTransfer ? OrderStatus.Pending : OrderStatus.Confirmed,
                    PaymentStatus = PaymentStatus.Unpaid,
                    ShippingFee = FlatShippingFee,
                    DiscountAmount = 0
                };

                decimal subtotal = 0;
                var variantRepo = uow.Repository<ProductVariant>();

                foreach (var item in cart.Items)
                {
                    var variant = item.Variant
                        ?? throw new InvalidOperationException("Variant không hợp lệ trong giỏ");

                    // kiểm tồn
                    if (variant.Stock < item.Quantity)
                        throw new InvalidOperationException(
                            $"'{item.Product?.Name}' không đủ hàng (còn {variant.Stock})");

                    // TRỪ KHO
                    variant.Stock -= item.Quantity;
                    variantRepo.Update(variant);

                    // SNAPSHOT giá/tên/sku lúc đặt
                    var lineTotal = variant.Price * item.Quantity;
                    subtotal += lineTotal;

                    order.Items.Add(new OrderItem
                    {
                        ProductId = item.ProductId,
                        VariantId = item.VariantId,
                        ProductName = item.Product?.Name ?? "",
                        Sku = variant.Sku,
                        ImageUrl = variant.ImageUrl,
                        Quantity = item.Quantity,
                        Price = variant.Price,
                        DiscountAmount = 0,
                        FinalPrice = lineTotal
                    });
                }

                order.TotalAmount = subtotal;

                // áp dụng coupon (nếu có)
                Coupon? coupon = null;
                decimal discount = 0;
                if (!string.IsNullOrWhiteSpace(request.CouponCode))
                    (coupon, discount) = await couponService.ValidateAsync(request.CouponCode, userId, subtotal);

                order.DiscountAmount = discount;
                order.FinalAmount = subtotal + order.ShippingFee - discount;

                order.StatusHistories.Add(new OrderStatusHistory
                {
                    Status = order.Status,
                    Note = bankTransfer ? "Đơn được tạo — chờ thanh toán chuyển khoản" : "Đơn được tạo",
                    ChangedBy = "system"
                });

                await uow.Repository<Order>().AddAsync(order);

                // xóa giỏ sau khi đặt
                var cartItemRepo = uow.Repository<CartItem>();
                foreach (var ci in cart.Items.ToList())
                    cartItemRepo.Delete(ci);

                // lưu Order trước để lấy Id (transaction vẫn mở)
                await uow.SaveChangesAsync();

                // ghi usage coupon + tăng UsedCount
                if (coupon is not null)
                {
                    await uow.Repository<CouponUsage>().AddAsync(new CouponUsage
                    {
                        CouponId = coupon.Id,
                        UserId = userId,
                        OrderId = order.Id,
                        DiscountAmount = discount
                    });
                    coupon.UsedCount++;
                    uow.Repository<Coupon>().Update(coupon);
                }

                // ══ COMMIT (lưu usage + commit transaction) ══
                await uow.CommitAsync();

                // email xác nhận đơn (lỗi gửi không chặn — notifier tự nuốt)
                var buyer = await uow.Repository<User>().Query().FirstOrDefaultAsync(u => u.Id == userId);
                if (buyer is not null)
                    await notifier.OrderPlacedAsync(order, buyer.Email, buyer.FullName);

                return await GetByIdAsync(userId, order.Id);
            }
            catch
            {
                await uow.RollbackAsync();   // lỗi giữa chừng → hoàn tác HẾT
                throw;
            }
        }

        public async Task<IEnumerable<OrderDto>> GetMyOrdersAsync(int userId)
        {
            var orders = await uow.Repository<Order>().Query()
                .Include(o => o.Items)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.Id)
                .ToListAsync();
            return orders.Select(ToDto);
        }

        public async Task<OrderDto> GetByIdAsync(int userId, int orderId)
        {
            var order = await uow.Repository<Order>().Query()
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId)
                ?? throw new KeyNotFoundException($"Không tìm thấy đơn id={orderId}");

            var dto = ToDto(order);
            // đơn SePay chưa thanh toán → kèm QR để FE hiển thị lại
            if (IsBankTransfer(order.PaymentMethod) && order.PaymentStatus != PaymentStatus.Paid)
                dto.PaymentQrUrl = paymentService.BuildQrUrl(order.OrderCode, order.FinalAmount);
            return dto;
        }

        // --- Admin ---
        public async Task<IEnumerable<OrderDto>> GetAllAsync()
        {
            var orders = await uow.Repository<Order>().Query()
                .Include(o => o.Items)
                .OrderByDescending(o => o.Id)
                .ToListAsync();
            return orders.Select(ToDto);
        }

        public async Task<OrderDto> GetAdminByIdAsync(int orderId)
        {
            var order = await uow.Repository<Order>().Query()
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == orderId)
                ?? throw new KeyNotFoundException($"Không tìm thấy đơn id={orderId}");
            return ToDto(order);
        }

        public async Task<OrderDto> UpdateStatusAsync(int orderId, OrderStatus status, string? note, string changedBy)
        {
            var repo = uow.Repository<Order>();
            var order = await repo.Query()
                .Include(o => o.Items)
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == orderId)
                ?? throw new KeyNotFoundException($"Không tìm thấy đơn id={orderId}");

            order.Status = status;
            repo.Update(order);

            await uow.Repository<OrderStatusHistory>().AddAsync(new OrderStatusHistory
            {
                OrderId = order.Id,
                Status = status,
                Note = note,
                ChangedBy = changedBy
            });
            await uow.CommitAsync();

            // email báo đổi trạng thái cho khách
            if (order.User is not null)
                await notifier.StatusChangedAsync(order, order.User.Email, order.User.FullName);

            return ToDto(order);
        }

        // Job nền: tự hủy đơn chuyển khoản chưa thanh toán quá hạn → hoàn kho + hoàn coupon.
        public async Task<int> CancelExpiredUnpaidOrdersAsync()
        {
            var cutoff = DateTime.UtcNow.AddMinutes(-_sepay.ExpiryMinutes);

            var orderRepo = uow.Repository<Order>();
            var expired = await orderRepo.Query()
                .Include(o => o.Items)
                .Where(o => o.PaymentStatus != PaymentStatus.Paid
                            && o.Status == OrderStatus.Pending
                            && (o.PaymentMethod == "SePay" || o.PaymentMethod == "BankTransfer")
                            && o.CreatedAt < cutoff)
                .ToListAsync();

            if (expired.Count == 0) return 0;

            var variantRepo = uow.Repository<ProductVariant>();
            var usageRepo = uow.Repository<CouponUsage>();
            var couponRepo = uow.Repository<Coupon>();

            foreach (var order in expired)
            {
                // 1) HOÀN KHO (đảo lại việc trừ kho lúc tạo đơn)
                foreach (var item in order.Items)
                {
                    var variant = await variantRepo.GetByIdAsync(item.VariantId);
                    if (variant is not null)
                    {
                        variant.Stock += item.Quantity;
                        variantRepo.Update(variant);
                    }
                }

                // 2) HOÀN COUPON (nếu đơn có dùng) — trả lượt + xóa usage
                var usage = await usageRepo.Query().FirstOrDefaultAsync(u => u.OrderId == order.Id);
                if (usage is not null)
                {
                    var coupon = await couponRepo.GetByIdAsync(usage.CouponId);
                    if (coupon is not null && coupon.UsedCount > 0)
                    {
                        coupon.UsedCount--;
                        couponRepo.Update(coupon);
                    }
                    usageRepo.HardDelete(usage);   // xóa cứng để không tính vào giới hạn/lượt
                }

                // 3) HỦY ĐƠN + ghi lịch sử
                order.Status = OrderStatus.Cancelled;
                orderRepo.Update(order);
                await uow.Repository<OrderStatusHistory>().AddAsync(new OrderStatusHistory
                {
                    OrderId = order.Id,
                    Status = OrderStatus.Cancelled,
                    Note = $"Tự hủy: quá hạn thanh toán {_sepay.ExpiryMinutes} phút",
                    ChangedBy = "system"
                });
            }

            await uow.SaveChangesAsync();   // 1 lần lưu atomic cho cả lô
            logger.LogInformation("Đã tự hủy {Count} đơn chuyển khoản quá hạn (hoàn kho + coupon).", expired.Count);
            return expired.Count;
        }

        private static string GenerateOrderCode() =>
            $"ORD{DateTime.UtcNow:yyyyMMddHHmmss}{Random.Shared.Next(100, 999)}";

        private static OrderDto ToDto(Order o) => new()
        {
            Id = o.Id,
            OrderCode = o.OrderCode,
            Status = o.Status.ToString(),
            PaymentStatus = o.PaymentStatus.ToString(),
            PaymentMethod = o.PaymentMethod,
            TotalAmount = o.TotalAmount,
            ShippingFee = o.ShippingFee,
            DiscountAmount = o.DiscountAmount,
            FinalAmount = o.FinalAmount,
            ShipRecipient = o.ShipRecipient,
            ShipPhone = o.ShipPhone,
            ShipAddress = $"{o.ShipAddressLine}, {o.ShipWard}, {o.ShipDistrict}, {o.ShipProvince}",
            Note = o.Note,
            CreatedAt = o.CreatedAt,
            Items = o.Items.Select(i => new OrderItemDto
            {
                ProductId = i.ProductId,
                VariantId = i.VariantId,
                ProductName = i.ProductName,
                Sku = i.Sku,
                Quantity = i.Quantity,
                Price = i.Price,
                FinalPrice = i.FinalPrice
            }).ToList()
        };
    }
}
