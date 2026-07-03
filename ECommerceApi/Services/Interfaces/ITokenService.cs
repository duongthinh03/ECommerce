using ECommerceApi.Models;

namespace ECommerceApi.Services;

public interface ITokenService
{
    (string Token, DateTime ExpiresAt) GenerateAccessToken(User user, string roleName);
    string GenerateRefreshToken();
}
