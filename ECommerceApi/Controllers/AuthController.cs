using System.Security.Claims;
using ECommerceApi.Common;
using ECommerceApi.DTOs.Auth;
using ECommerceApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace ECommerceApi.Controllers
{
    [Route("api/auth")]
    public class AuthController(IAuthService authService, IOptions<JwtSettings> jwtOptions, IHostEnvironment env) : ApiControllerBase
    {
        private readonly JwtSettings _jwt = jwtOptions.Value;
        // dev chạy http → cookie Secure=false mới gửi được; production bật Secure
        private bool CookieSecure => !env.IsDevelopment();

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            await authService.RegisterAsync(request);
            return OkResponse<object?>(null, "Đăng ký thành công. Vui lòng nhập mã OTP gửi tới email để kích hoạt tài khoản.");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var response = await authService.LoginAsync(request);
            if (response.Requires2FA)   // cần mã 2FA → chưa cấp cookie, báo FE nhập mã
                return OkResponse(response, "Cần mã xác thực 2 lớp");
            AuthCookies.SetAuth(Response, response, _jwt.RefreshTokenDays, CookieSecure);
            return OkResponse(response, "Đăng nhập thành công");
        }

        [HttpPost("google")]
        public async Task<IActionResult> Google([FromBody] GoogleLoginRequest req)
        {
            var response = await authService.GoogleLoginAsync(req.IdToken);
            AuthCookies.SetAuth(Response, response, _jwt.RefreshTokenDays, CookieSecure);
            return OkResponse(response, "Đăng nhập Google thành công");
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshRequest? request)
        {
            // ưu tiên refresh token từ cookie httpOnly; fallback body (tương thích client cũ)
            var token = Request.Cookies[AuthCookies.Refresh] ?? request?.RefreshToken;
            if (string.IsNullOrEmpty(token))
                throw new UnauthorizedAccessException("Thiếu refresh token");

            var response = await authService.RefreshAsync(token);
            AuthCookies.SetAuth(Response, response, _jwt.RefreshTokenDays, CookieSecure);
            return OkResponse(response);
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            AuthCookies.Clear(Response);
            return OkResponse<object?>(null, "Đã đăng xuất");
        }

        [Authorize]
        [HttpGet("me")]
        public IActionResult Me()
        {
            // lấy thông tin user từ claims trong JWT (đã đọc từ cookie)
            var user = new UserInfo
            {
                Id = CurrentUserId ?? 0,
                Email = User.FindFirstValue(ClaimTypes.Email) ?? "",
                FullName = User.FindFirstValue("name") ?? "",
                Role = User.FindFirstValue(ClaimTypes.Role) ?? "Customer"
            };
            return OkResponse(user);
        }

        // ===== 2FA (TOTP) =====
        [Authorize]
        [HttpPost("2fa/setup")]
        public async Task<IActionResult> Setup2FA()
        {
            var r = await authService.SetupTwoFactorAsync(CurrentUserId ?? 0);
            return OkResponse(r);
        }

        [Authorize]
        [HttpPost("2fa/enable")]
        public async Task<IActionResult> Enable2FA([FromBody] TwoFactorCodeRequest req)
        {
            var recoveryCodes = await authService.EnableTwoFactorAsync(CurrentUserId ?? 0, req.Code);
            return OkResponse(new { recoveryCodes }, "Đã bật xác thực 2 lớp");
        }

        [Authorize]
        [HttpPost("2fa/disable")]
        public async Task<IActionResult> Disable2FA([FromBody] TwoFactorCodeRequest req)
        {
            await authService.DisableTwoFactorAsync(CurrentUserId ?? 0, req.Code);
            return OkResponse<object?>(null, "Đã tắt xác thực 2 lớp");
        }

        [Authorize]
        [HttpGet("2fa/status")]
        public async Task<IActionResult> Status2FA()
        {
            var enabled = await authService.IsTwoFactorEnabledAsync(CurrentUserId ?? 0);
            return OkResponse(new TwoFactorStatusResponse { Enabled = enabled });
        }

        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest request)
        {
            await authService.VerifyOtpAsync(request.Email, request.Otp);
            return OkResponse<object?>(null, "Xác thực email thành công");
        }

        [HttpPost("resend-otp")]
        public async Task<IActionResult> ResendOtp([FromBody] ResendOtpRequest request)
        {
            await authService.ResendOtpAsync(request.Email);
            return OkResponse<object?>(null, "Đã gửi lại mã OTP");
        }

        // ===== Tài khoản =====
        [Authorize]
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile() =>
            OkResponse(await authService.GetProfileAsync(CurrentUserId ?? 0));

        [Authorize]
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request) =>
            OkResponse(await authService.UpdateProfileAsync(CurrentUserId ?? 0, request), "Đã cập nhật thông tin");

        [Authorize]
        [HttpPost("avatar")]
        public async Task<IActionResult> UploadAvatar(IFormFile file, [FromServices] IImageStorage storage)
        {
            if (file is null || file.Length == 0) return BadRequestResponse("Chưa chọn ảnh");
            if (file.Length > 5 * 1024 * 1024) return BadRequestResponse("Ảnh tối đa 5MB");
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" }.Contains(ext))
                return BadRequestResponse("Chỉ nhận ảnh jpg/png/webp/gif");

            await using var stream = file.OpenReadStream();
            var url = await storage.SaveAsync(stream, file.FileName, file.ContentType);
            await authService.UpdateAvatarAsync(CurrentUserId ?? 0, url);
            return OkResponse(new { url }, "Đã cập nhật ảnh đại diện");
        }

        [Authorize]
        [HttpDelete("avatar")]
        public async Task<IActionResult> DeleteAvatar()
        {
            await authService.UpdateAvatarAsync(CurrentUserId ?? 0, null);
            return OkResponse<object?>(null, "Đã xóa ảnh đại diện");
        }

        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            await authService.ChangePasswordAsync(CurrentUserId ?? 0, request);
            AuthCookies.Clear(Response);   // đổi mật khẩu → thu hồi token, buộc đăng nhập lại
            return OkResponse<object?>(null, "Đã đổi mật khẩu. Vui lòng đăng nhập lại.");
        }

        // ===== Quên mật khẩu (ẩn danh) =====
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            await authService.ForgotPasswordAsync(request.Email);
            // luôn trả OK để không lộ email nào tồn tại
            return OkResponse<object?>(null, "Nếu email tồn tại, mã đặt lại mật khẩu đã được gửi.");
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            await authService.ResetPasswordAsync(request);
            return OkResponse<object?>(null, "Đặt lại mật khẩu thành công. Vui lòng đăng nhập.");
        }
    }
}
