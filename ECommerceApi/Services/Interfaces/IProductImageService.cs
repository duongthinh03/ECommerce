using ECommerceApi.DTOs.Catalog;

namespace ECommerceApi.Services;

public interface IProductImageService
{
    Task<IEnumerable<ProductImageDto>> GetByProductAsync(int productId);
    Task<ProductImageDto> AddAsync(int productId, string imageUrl);
    Task DeleteAsync(int productId, int imageId);
}
