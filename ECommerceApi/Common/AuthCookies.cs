using ECommerceApi.DTOs.Auth;

namespace ECommerceApi.Common;

/// <summary>
/// Đặt/xóa access + refresh token dưới dạng cookie HttpOnly (JS không đọc được → chống XSS).
/// Access cookie sống dài như refresh, nhưng JWT bên trong hết hạn ngắn → tự đẩy về refresh.
/// </summary>
public static class AuthCookies
{
    public const string Access = "access_token";
    public const string Refresh = "refresh_token";
    private const string RefreshPath = "/api/auth";   // refresh cookie chỉ gửi tới /api/auth/*

    public static void SetAuth(HttpResponse res, AuthResponse auth, int refreshDays, bool secure)
    {
        var expires = DateTimeOffset.UtcNow.AddDays(refreshDays);

        res.Cookies.Append(Access, auth.AccessToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = secure,
            SameSite = SameSiteMode.Lax,
            IsEssential = true,
            Expires = expires
        });

        res.Cookies.Append(Refresh, auth.RefreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = secure,
            SameSite = SameSiteMode.Lax,
            IsEssential = true,
            Path = RefreshPath,
            Expires = expires
        });
    }

    public static void Clear(HttpResponse res)
    {
        res.Cookies.Delete(Access);
        res.Cookies.Delete(Refresh, new CookieOptions { Path = RefreshPath });
    }
}
