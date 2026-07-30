using System.ComponentModel.DataAnnotations;

namespace ECommerceApi.DTOs.Auth;

// Thông tin hồ sơ (GET /profile)
public class ProfileDto
{
    public int Id { get; set; }
    public string Email { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string? Phone { get; set; }
    public string? AvatarUrl { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? Provider { get; set; }       // "Google" nếu đăng nhập Google
    public bool EmailConfirmed { get; set; }
    public bool Is2FAEnabled { get; set; }
    public bool HasPassword { get; set; }
}

public class UpdateProfileRequest
{
    [Required(ErrorMessage = "Vui lòng nhập họ tên")]
    [MaxLength(100)]
    public string FullName { get; set; } = null!;

    [MaxLength(20)]
    public string? Phone { get; set; }

    public string? AvatarUrl { get; set; }
    public DateOnly? DateOfBirth { get; set; }

    [RegularExpression("Male|Female|Other", ErrorMessage = "Giới tính không hợp lệ")]
    public string? Gender { get; set; }
}

public class ChangePasswordRequest
{
    [Required]
    public string CurrentPassword { get; set; } = null!;

    [Required, MinLength(6, ErrorMessage = "Mật khẩu mới tối thiểu 6 ký tự")]
    public string NewPassword { get; set; } = null!;
}

public class ForgotPasswordRequest
{
    [Required, EmailAddress]
    public string Email { get; set; } = null!;
}

public class ResetPasswordRequest
{
    [Required, EmailAddress]
    public string Email { get; set; } = null!;

    [Required]
    public string Otp { get; set; } = null!;

    [Required, MinLength(6, ErrorMessage = "Mật khẩu mới tối thiểu 6 ký tự")]
    public string NewPassword { get; set; } = null!;
}
