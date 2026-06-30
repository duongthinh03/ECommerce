using ECommerceApi.DTOs.Order;
using ECommerceApi.Models;

namespace ECommerceApi.Services
{
    public interface IOrderService
    {
        Task<OrderDto> CheckoutAsync(int userId, CheckoutRequest request);
        Task<IEnumerable<OrderDto>> GetMyOrdersAsync(int userId);
        Task<OrderDto> GetByIdAsync(int userId, int orderId);

        // --- Admin ---
        Task<IEnumerable<OrderDto>> GetAllAsync();
        Task<OrderDto> GetAdminByIdAsync(int orderId);
        Task<OrderDto> UpdateStatusAsync(int orderId, OrderStatus status, string? note, string changedBy);
    }
}
