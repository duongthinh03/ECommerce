using ECommerceApi.DTOs.Catalog;

namespace ECommerceApi.Services;

public interface IProductService
{
    Task<IEnumerable<ProductDto>> GetAllAsync();
    Task<ProductDto> GetByIdAsync(int id);
    Task<ProductDto> CreateAsync(CreateProductRequest request);
    Task<ProductDto> UpdateAsync(int id, UpdateProductRequest request);
    Task DeleteAsync(int id);
}