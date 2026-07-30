namespace ECommerceApi.Models
{
    public class OrderStatusHistory
    {
        public int Id { get; set; }

        public int OrderId { get; set; }
        public Order Order { get; set; } = null!;

        public OrderStatus Status { get; set; }
        public string? Note { get; set; }
        public string? ChangedBy { get; set; }    // ai đổi (NV/hệ thống)
        public DateTime CreatedAt { get; set; }
    }
}
