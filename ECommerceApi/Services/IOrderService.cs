using ECommerceApi.DTOs.Order;

namespace ECommerceApi.Services
{
    public interface IOrderService
    {
        Task<OrderDto> CheckoutAsync(int userId, CheckoutRequest request);
        Task<IEnumerable<OrderDto>> GetMyOrdersAsync(int userId);
        Task<OrderDto> GetByIdAsync(int userId, int orderId);
    }
}
