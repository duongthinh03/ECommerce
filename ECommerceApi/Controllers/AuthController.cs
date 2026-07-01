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
            await authService.EnableTwoFactorAsync(CurrentUserId ?? 0, req.Code);
            return OkResponse<object?>(null, "Đã bật xác thực 2 lớp");
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
    }
}
