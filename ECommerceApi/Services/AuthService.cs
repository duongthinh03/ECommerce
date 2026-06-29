using ECommerceApi.Common;
using ECommerceApi.DTOs.Auth;
using ECommerceApi.Models;
using ECommerceApi.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using BC = BCrypt.Net.BCrypt;

namespace ECommerceApi.Services;

public class AuthService(IUnitOfWork uow, ITokenService tokenService, IOptions<JwtSettings> jwtOptions) : IAuthService
{
    private const int CustomerRoleId = 4;   // seed: 1 Admin, 2 Manager, 3 Staff, 4 Customer
    private readonly JwtSettings _jwt = jwtOptions.Value;

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
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

        return await IssueTokensAsync(user, roleName: "Customer");
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
}
