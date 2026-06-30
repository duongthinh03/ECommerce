using ECommerceApi.DTOs.Auth;

namespace ECommerceApi.Services;

public interface IAuthService
{
    Task RegisterAsync(RegisterRequest request);   // KHÔNG cấp token — phải verify email mới login
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<AuthResponse> RefreshAsync(string refreshToken);
    Task VerifyOtpAsync(string email, string otp);
    Task ResendOtpAsync(string email);
}
