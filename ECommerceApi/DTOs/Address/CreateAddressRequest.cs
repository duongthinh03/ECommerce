namespace ECommerceApi.DTOs.Address;

public class CreateAddressRequest
{
    public string FullName { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public string Province { get; set; } = null!;
    public string District { get; set; } = null!;
    public string Ward { get; set; } = null!;
    public string AddressLine { get; set; } = null!;
    public bool IsDefault { get; set; }
}
