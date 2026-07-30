using ECommerceApi.DTOs.Payment;

namespace ECommerceApi.Services;

public interface IPaymentService
{
    /// <summary>Sinh URL ảnh VietQR để khách quét chuyển khoản (nội dung = orderCode).</summary>
    string BuildQrUrl(string orderCode, decimal amount);

    /// <summary>Xác thực header Authorization của webhook SePay (Apikey).</summary>
    bool VerifyApiKey(string? authorizationHeader);

    /// <summary>Xử lý 1 giao dịch webhook: chống trùng, khớp đơn, đánh dấu đã thanh toán. Trả true nếu khớp + cập nhật đơn.</summary>
    Task<bool> HandleSepayWebhookAsync(SepayWebhookPayload payload);
}
