namespace ECommerceApi.Models;

public class EmailOtp
{
    public int Id { get; set; }
    public string Email { get; set; } = null!;
    public string OtpCode { get; set; } = null!;
    public string Purpose { get; set; } = null!;   // "register" / "reset" ...
    public int AttemptCount { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool Used { get; set; }
    public DateTime CreatedAt { get; set; }
}
