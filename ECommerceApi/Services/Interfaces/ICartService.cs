using ECommerceApi.DTOs.Cart;

namespace ECommerceApi.Services
{
    public interface ICartService
    {
        Task<CartDto> GetCartAsync(int? userId, string? sessionId);
        Task<CartDto> AddItemAsync(int? userId, string? sessionId, AddCartItemRequest request);
        Task<CartDto> UpdateItemAsync(int? userId, string? sessionId, int variantId, int quantity);
        Task<CartDto> RemoveItemAsync(int? userId, string? sessionId, int variantId);
        Task ClearAsync(int? userId, string? sessionId);
        Task<CartDto> MergeAsync(int userId, string sessionId);
    }
}
