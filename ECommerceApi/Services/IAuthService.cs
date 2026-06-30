using ECommerceApi.DTOs.Auth;

namespace ECommerceApi.Services;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<AuthResponse> RefreshAsync(string refreshToken);
    Task VerifyOtpAsync(string email, string otp);
    Task ResendOtpAsync(string email);
}
