using ECommerceApi.Common;
using ECommerceApi.DTOs.Auth;
using ECommerceApi.Models;
using ECommerceApi.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using BC = BCrypt.Net.BCrypt;

namespace ECommerceApi.Services;

public class AuthService(IUnitOfWork uow, ITokenService tokenService, IOptions<JwtSettings> jwtOptions, IEmailSender emailSender, ILogger<AuthService> logger) : IAuthService
{
    private const int CustomerRoleId = 4;   // seed: 1 Admin, 2 Manager, 3 Staff, 4 Customer
    private readonly JwtSettings _jwt = jwtOptions.Value;

    public async Task RegisterAsync(RegisterRequest request)
    {
        var users = uow.Repository<User>();

        var emailTaken = await users.Query().AnyAsync(u => u.Email == request.Email);
        if (emailTaken)
            throw new InvalidOperationException("Email đã được sử dụng");

        var user = new User
        {
            Email = request.Email,
            PasswordHash = BC.HashPassword(request.Password),
            FullName = request.FullName,
            Phone = request.Phone,
            RoleId = CustomerRoleId,
            IsActive = true
        };

        await users.AddAsync(user);
        await uow.CommitAsync();

        // Gửi OTP xác thực — KHÔNG cấp token (phải verify email mới đăng nhập được)
        try
        {
            await SendOtpAsync(user.Email, "register");
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Gửi OTP đăng ký thất bại cho {Email}", user.Email);
        }
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await uow.Repository<User>().Query()
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user is null || !BC.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Email hoặc mật khẩu không đúng");

        if (!user.IsActive)
            throw new UnauthorizedAccessException("Tài khoản đã bị khóa");

        if (!user.EmailConfirmed)
            throw new UnauthorizedAccessException("Email chưa xác thực. Vui lòng nhập mã OTP gửi tới email.");

        user.LastLoginAt = DateTime.UtcNow;
        uow.Repository<User>().Update(user);
        await uow.CommitAsync();

        return await IssueTokensAsync(user, user.Role?.Name);
    }

    public async Task<AuthResponse> RefreshAsync(string refreshToken)
    {
        var tokens = uow.Repository<RefreshToken>();

        var stored = await tokens.Query()
            .Include(t => t.User).ThenInclude(u => u.Role)
            .FirstOrDefaultAsync(t => t.Token == refreshToken);

        if (stored is null || stored.Revoked || stored.ExpiresAt <= DateTime.UtcNow)
            throw new UnauthorizedAccessException("Refresh token không hợp lệ hoặc đã hết hạn");

        // Rotate: thu hồi token cũ, cấp token mới
        stored.Revoked = true;
        tokens.Update(stored);

        return await IssueTokensAsync(stored.User, stored.User.Role?.Name);
    }

    private async Task<AuthResponse> IssueTokensAsync(User user, string? roleName)
    {
        roleName ??= "Customer";
        var (accessToken, expiresAt) = tokenService.GenerateAccessToken(user, roleName);
        var refreshToken = tokenService.GenerateRefreshToken();

        await uow.Repository<RefreshToken>().AddAsync(new RefreshToken
        {
            UserId = user.Id,
            Token = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwt.RefreshTokenDays),
            CreatedAt = DateTime.UtcNow,
            Revoked = false
        });
        await uow.CommitAsync();

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            AccessTokenExpiresAt = expiresAt,
            User = new UserInfo
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                Role = roleName ?? "Customer"
            }
        };
    }

    // sinh + lưu + "gửi" OTP (dev: log ra console)
    private async Task SendOtpAsync(string email, string purpose)
    {
        var code = Random.Shared.Next(100000, 999999).ToString();
        await uow.Repository<EmailOtp>().AddAsync(new EmailOtp
        {
            Email = email,
            OtpCode = code,
            Purpose = purpose,
            ExpiresAt = DateTime.UtcNow.AddMinutes(10),
            Used = false
        });
        await uow.CommitAsync();
        await emailSender.SendAsync(email, "Mã xác thực tài khoản ShopViet",
            $"Chào bạn,\n\n" +
            $"Mã OTP xác thực tài khoản ShopViet của bạn là: {code}\n" +
            $"Mã có hiệu lực trong 10 phút.\n\n" +
            $"Nếu bạn không yêu cầu mã này, vui lòng bỏ qua email.\n\n" +
            $"— ShopViet");
    }

    public async Task ResendOtpAsync(string email)
    {
        var exists = await uow.Repository<User>().Query().AnyAsync(u => u.Email == email);
        if (!exists) throw new InvalidOperationException("Email chưa đăng ký");
        await SendOtpAsync(email, "register");
    }

    public async Task VerifyOtpAsync(string email, string otp)
    {
        var otpRepo = uow.Repository<EmailOtp>();
        var record = await otpRepo.Query()
            .Where(o => o.Email == email && o.Purpose == "register" && !o.Used)
            .OrderByDescending(o => o.Id)
            .FirstOrDefaultAsync()
            ?? throw new InvalidOperationException("Không có mã OTP, hãy yêu cầu gửi lại");

        if (record.ExpiresAt < DateTime.UtcNow)
            throw new InvalidOperationException("Mã OTP đã hết hạn");
        if (record.AttemptCount >= 5)
            throw new InvalidOperationException("Nhập sai quá nhiều lần, hãy yêu cầu mã mới");

        if (record.OtpCode != otp)
        {
            record.AttemptCount++;
            otpRepo.Update(record);
            await uow.CommitAsync();
            throw new InvalidOperationException("Mã OTP không đúng");
        }

        record.Used = true;
        otpRepo.Update(record);

        var user = await uow.Repository<User>().Query().FirstOrDefaultAsync(u => u.Email == email);
        if (user is not null)
        {
            user.EmailConfirmed = true;
            uow.Repository<User>().Update(user);
        }
        await uow.CommitAsync();
    }
}
