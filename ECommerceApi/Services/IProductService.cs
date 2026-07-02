using ECommerceApi.Common;
using ECommerceApi.DTOs.Catalog;

namespace ECommerceApi.Services;

public interface IProductService
{
    Task<PagedResult<ProductDto>> SearchAsync(ProductSearchQuery query);
    Task<ProductDto> GetByIdAsync(int id);
    Task<List<ProductDto>> GetByIdsAsync(IReadOnlyList<int> ids);   // giữ thứ tự ids
    Task<ProductDto> CreateAsync(CreateProductRequest request);
    Task<ProductDto> UpdateAsync(int id, UpdateProductRequest request);
    Task DeleteAsync(int id);
}