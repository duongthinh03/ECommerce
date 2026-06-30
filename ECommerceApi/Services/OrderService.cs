using ECommerceApi.DTOs.Order;
using ECommerceApi.Models;
using ECommerceApi.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using KeyNotFoundException = System.Collections.Generic.KeyNotFoundException;

namespace ECommerceApi.Services
{
    public class OrderService(IUnitOfWork uow, ICouponService couponService) : IOrderService
    {
        private const decimal FlatShippingFee = 30000m;   // phí ship phẳng (MVP)

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
                    Status = OrderStatus.Confirmed,        // COD: xác nhận luôn
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
                    Status = OrderStatus.Confirmed,
                    Note = "Đơn được tạo",
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
            return ToDto(order);
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
