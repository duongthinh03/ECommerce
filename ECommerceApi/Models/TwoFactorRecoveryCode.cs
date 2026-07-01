namespace ECommerceApi.Models;

/// <summary>
/// Mã khôi phục 2FA — dùng 1 lần khi mất app Authenticator. Lưu hash (BCrypt), không lưu plaintext.
/// </summary>
public class TwoFactorRecoveryCode : BaseEntity
{
    public int UserId { get; set; }
    public string CodeHash { get; set; } = null!;
    public DateTime? UsedAt { get; set; }        // null = chưa dùng
}
