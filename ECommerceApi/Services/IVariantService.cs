using ECommerceApi.DTOs.Catalog;

namespace ECommerceApi.Services;

public interface IVariantService
{
    Task<IEnumerable<VariantDto>> GetByProductAsync(int productId);
    Task<VariantDto> GetByIdAsync(int productId, int id);
    Task<VariantDto> CreateAsync(int productId, CreateVariantRequest request);
    Task<VariantDto> UpdateAsync(int productId, int id, UpdateVariantRequest request);
    Task DeleteAsync(int productId, int id);
}