namespace ECommerceApi.Models;

public class Address : BaseEntity
{
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public string FullName { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public string Province { get; set; } = null!;   // Tỉnh/Thành
    public string District { get; set; } = null!;    // Quận/Huyện
    public string Ward { get; set; } = null!;        // Phường/Xã
    public string AddressLine { get; set; } = null!; // Số nhà, đường

    public bool IsDefault { get; set; }              // địa chỉ mặc định của user
}
