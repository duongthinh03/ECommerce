using ECommerceApi.DTOs.Auth;
using ECommerceApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceApi.Controllers
{
    [Route("api/auth")]
    public class AuthController(IAuthService authService) : ApiControllerBase
    {
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var response = await authService.RegisterAsync(request);
            return OkResponse(response, "Đăng ký thành công");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var response = await authService.LoginAsync(request);
            return OkResponse(response, "Đăng nhập thành công");
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshRequest request)
        {
            var response = await authService.RefreshAsync(request.RefreshToken);
            return OkResponse(response);
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
