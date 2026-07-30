namespace ECommerceApi.DTOs.Auth;

public class TwoFactorSetupResponse
{
    public string Secret { get; set; } = null!;       // base32 — để nhập tay vào app Authenticator
    public string OtpauthUri { get; set; } = null!;   // otpauth://... — để render QR
}

public class TwoFactorCodeRequest
{
    public string Code { get; set; } = null!;
}

public class TwoFactorStatusResponse
{
    public bool Enabled { get; set; }
}
