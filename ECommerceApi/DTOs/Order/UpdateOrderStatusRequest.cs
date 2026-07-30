using ECommerceApi.Models;

namespace ECommerceApi.DTOs.Order;

public class UpdateOrderStatusRequest
{
    public OrderStatus Status { get; set; }   // "Confirmed" / "Packing" / "Shipping" ...
    public string? Note { get; set; }
}
