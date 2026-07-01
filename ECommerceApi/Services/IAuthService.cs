using ECommerceApi.DTOs.Auth;

namespace ECommerceApi.Services;

public interface IAuthService
{
    Task RegisterAsync(RegisterRequest request);   // KHÔNG cấp token — phải verify email mới login
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<AuthResponse> RefreshAsync(string refreshToken);
    Task VerifyOtpAsync(string email, string otp);
    Task ResendOtpAsync(string email);

    // 2FA (TOTP)
    Task<TwoFactorSetupResponse> SetupTwoFactorAsync(int userId);
    Task<IReadOnlyList<string>> EnableTwoFactorAsync(int userId, string code);   // trả recovery codes
    Task DisableTwoFactorAsync(int userId, string code);
    Task<bool> IsTwoFactorEnabledAsync(int userId);
}
