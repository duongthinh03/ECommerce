namespace ECommerceApi.DTOs.Auth;

public class AuthResponse
{
    public string AccessToken { get; set; } = null!;
    public string RefreshToken { get; set; } = null!;
    public DateTime AccessTokenExpiresAt { get; set; }
    public UserInfo User { get; set; } = null!;
    public bool Requires2FA { get; set; }   // true = cần nhập mã 2FA rồi login lại (chưa cấp token)
}

public class UserInfo
{
    public int Id { get; set; }
    public string Email { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string Role { get; set; } = null!;
}
