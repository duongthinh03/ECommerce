using ECommerceApi.DTOs.Address;

namespace ECommerceApi.Services;

public interface IAddressService
{
    Task<IEnumerable<AddressDto>> GetMyAddressesAsync(int userId);
    Task<AddressDto> GetByIdAsync(int userId, int id);
    Task<AddressDto> CreateAsync(int userId, CreateAddressRequest request);
    Task<AddressDto> UpdateAsync(int userId, int id, UpdateAddressRequest request);
    Task<AddressDto> SetDefaultAsync(int userId, int id);
    Task DeleteAsync(int userId, int id);
}
