namespace ECommerceApi.DTOs.Auth;

public class LoginRequest
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string? TwoFactorCode { get; set; }   // mã 6 số từ app Authenticator (nếu bật 2FA)
}
