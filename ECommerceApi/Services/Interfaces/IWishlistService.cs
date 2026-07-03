using ECommerceApi.DTOs.Catalog;

namespace ECommerceApi.Services;

public interface IWishlistService
{
    Task<List<ProductDto>> GetMineAsync(int userId);
    Task AddAsync(int userId, int productId);
    Task RemoveAsync(int userId, int productId);
}
