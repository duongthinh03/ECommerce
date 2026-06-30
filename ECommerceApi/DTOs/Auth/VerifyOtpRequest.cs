namespace ECommerceApi.DTOs.Auth;

public class VerifyOtpRequest
{
    public string Email { get; set; } = null!;
    public string Otp { get; set; } = null!;
}

public class ResendOtpRequest
{
    public string Email { get; set; } = null!;
}
