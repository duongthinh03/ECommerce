namespace ECommerceApi.Common;

/// <summary>
/// Cấu hình SePay (VietQR + webhook). Account/BankCode để appsettings được;
/// ApiKey (xác thực webhook) nên để user-secrets.
/// </summary>
public class SePaySettings
{
    public const string SectionName = "SePay";

    public string Account { get; set; } = "";     // số tài khoản ngân hàng nhận tiền
    public string BankCode { get; set; } = "";     // mã/tên ngắn ngân hàng (vd: Vietcombank, MB, ACB)
    public string ApiKey { get; set; } = "";       // key xác thực webhook (header: Authorization: Apikey <key>)
    public string QrTemplate { get; set; } = "compact";   // empty | compact | qronly
    public int ExpiryMinutes { get; set; } = 15;   // đơn chuyển khoản chưa trả quá số phút này → tự hủy + hoàn kho
}
