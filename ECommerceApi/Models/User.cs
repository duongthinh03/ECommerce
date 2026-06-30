namespace ECommerceApi.Models;

public class User : BaseEntity
{
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;   // BCrypt
    public string FullName { get; set; } = null!;
    public string? Phone { get; set; }
    public string? AvatarUrl { get; set; }

    public int RoleId { get; set; }
    public Role Role { get; set; } = null!;

    public bool EmailConfirmed { get; set; }
    public bool Is2FAEnabled { get; set; }
    public string? TwoFASecret { get; set; }            // mã hóa at-rest (Phase 2)

    // External login (Google...) — cột để sẵn, dùng ở Phase 2
    public string? Provider { get; set; }
    public string? ProviderId { get; set; }

    public int FailedLoginCount { get; set; }
    public DateTime? LockoutEnd { get; set; }
    public DateTime? LastLoginAt { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
    public ICollection<Address> Addresses { get; set; } = [];
}
