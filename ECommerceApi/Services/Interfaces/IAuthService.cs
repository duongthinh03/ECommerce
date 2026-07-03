using ECommerceApi.DTOs.Auth;

namespace ECommerceApi.Services;

public interface IAuthService
{
    Task RegisterAsync(RegisterRequest request);   // KHÔNG cấp token — phải verify email mới login
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<AuthResponse> GoogleLoginAsync(string idToken);
    Task<AuthResponse> RefreshAsync(string refreshToken);
    Task VerifyOtpAsync(string email, string otp);
    Task ResendOtpAsync(string email);

    // Tài khoản (user đang đăng nhập)
    Task<ProfileDto> GetProfileAsync(int userId);
    Task<ProfileDto> UpdateProfileAsync(int userId, UpdateProfileRequest request);
    Task<string?> UpdateAvatarAsync(int userId, string? avatarUrl);   // null = xoá ảnh
    Task ChangePasswordAsync(int userId, ChangePasswordRequest request);

    // Quên mật khẩu (ẩn danh)
    Task ForgotPasswordAsync(string email);
    Task ResetPasswordAsync(ResetPasswordRequest request);

    // 2FA (TOTP)
    Task<TwoFactorSetupResponse> SetupTwoFactorAsync(int userId);
    Task<IReadOnlyList<string>> EnableTwoFactorAsync(int userId, string code);   // trả recovery codes
    Task DisableTwoFactorAsync(int userId, string code);
    Task<bool> IsTwoFactorEnabledAsync(int userId);
}
