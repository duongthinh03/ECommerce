using System.Text.RegularExpressions;
using ECommerceApi.Common;
using ECommerceApi.DTOs.Payment;
using ECommerceApi.Models;
using ECommerceApi.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ECommerceApi.Services;

public partial class PaymentService(
    IUnitOfWork uow, IOptions<SePaySettings> options, ILogger<PaymentService> logger, IOrderNotifier notifier) : IPaymentService
{
    private readonly SePaySettings _s = options.Value;

    public string BuildQrUrl(string orderCode, decimal amount)
    {
        var des = Uri.EscapeDataString(orderCode);
        return $"https://qr.sepay.vn/img?acc={_s.Account}&bank={_s.BankCode}" +
               $"&amount={(long)amount}&des={des}&template={_s.QrTemplate}";
    }

    public bool VerifyApiKey(string? authorizationHeader)
    {
        // Chưa cấu hình ApiKey (dev) → chấp nhận, nhưng cảnh báo. Production BẮT BUỘC set ApiKey.
        if (string.IsNullOrWhiteSpace(_s.ApiKey))
        {
            logger.LogWarning("SePay ApiKey chưa cấu hình — webhook đang chạy KHÔNG xác thực (chỉ nên ở dev).");
            return true;
        }

        if (string.IsNullOrWhiteSpace(authorizationHeader)) return false;
        // SePay gửi: "Authorization: Apikey <key>"
        var token = authorizationHeader.Replace("Apikey", "", StringComparison.OrdinalIgnoreCase).Trim();
        return token == _s.ApiKey;
    }

    public async Task<bool> HandleSepayWebhookAsync(SepayWebhookPayload payload)
    {
        // chỉ xử lý tiền VÀO
        if (!string.Equals(payload.TransferType, "in", StringComparison.OrdinalIgnoreCase))
            return false;

        var txRepo = uow.Repository<SepayTransaction>();

        // chống trùng theo id giao dịch SePay
        if (await txRepo.Query().AnyAsync(t => t.SepayId == payload.Id))
            return true;

        // khớp đơn: tìm OrderCode (ORD + số) trong nội dung chuyển khoản
        int? matchedOrderId = null;
        Order? paidOrder = null;
        var content = payload.Content ?? "";
        var m = OrderCodeRegex().Match(content);
        if (m.Success)
        {
            var orderRepo = uow.Repository<Order>();
            var order = await orderRepo.Query()
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.OrderCode == m.Value);
            if (order is not null && order.Status == OrderStatus.Cancelled)
            {
                // Đơn đã bị hủy (quá hạn) mà tiền vẫn về → kho đã hoàn, KHÔNG confirm. Cần hoàn tiền thủ công.
                logger.LogWarning("⚠️ Nhận tiền cho đơn ĐÃ HỦY {Code} ({Amount:N0}đ) — cần hoàn tiền thủ công",
                    order.OrderCode, payload.TransferAmount);
            }
            else if (order is not null
                && order.PaymentStatus != PaymentStatus.Paid
                && payload.TransferAmount >= order.FinalAmount)   // đủ tiền (cho phép trả dư)
            {
                order.PaymentStatus = PaymentStatus.Paid;
                order.PaidAt = DateTime.UtcNow;
                if (order.Status == OrderStatus.Pending)
                    order.Status = OrderStatus.Confirmed;
                orderRepo.Update(order);

                await uow.Repository<OrderStatusHistory>().AddAsync(new OrderStatusHistory
                {
                    OrderId = order.Id,
                    Status = order.Status,
                    Note = $"Đã nhận thanh toán {payload.TransferAmount:N0}đ qua SePay",
                    ChangedBy = "sepay"
                });
                matchedOrderId = order.Id;
                paidOrder = order;
                logger.LogInformation("💰 Đơn {Code} đã thanh toán qua SePay", order.OrderCode);
            }
        }

        // luôn lưu log giao dịch (dù khớp hay không) — audit + chống trùng
        DateTime.TryParse(payload.TransactionDate, out var txDate);
        await txRepo.AddAsync(new SepayTransaction
        {
            SepayId = payload.Id,
            Gateway = payload.Gateway ?? "",
            Content = content.Length > 500 ? content[..500] : content,
            TransferType = payload.TransferType ?? "",
            TransferAmount = payload.TransferAmount,
            ReferenceCode = payload.ReferenceCode,
            TransactionDate = txDate == default ? DateTime.UtcNow : txDate,
            OrderId = matchedOrderId
        });
        await uow.CommitAsync();

        // email báo đã nhận thanh toán (sau commit; notifier tự nuốt lỗi)
        if (paidOrder?.User is not null)
            await notifier.PaymentReceivedAsync(paidOrder, paidOrder.User.Email, paidOrder.User.FullName);

        return matchedOrderId is not null;
    }

    [GeneratedRegex(@"ORD\d+")]
    private static partial Regex OrderCodeRegex();
}
