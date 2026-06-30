namespace ECommerceApi.Models
{
    public class OrderItem
    {
        public int Id { get; set; }

        public int OrderId { get; set; }
        public Order Order { get; set; } = null!;

        public int ProductId { get; set; }            // tham chiếu (không FK)
        public int VariantId { get; set; }
        public string ProductName { get; set; } = null!;   // copy cứng
        public string Sku { get; set; } = null!;
        public string? ImageUrl { get; set; }

        public int Quantity { get; set; }
        public decimal Price { get; set; }            // giá 1 đơn vị lúc đặt
        public decimal DiscountAmount { get; set; }
        public decimal FinalPrice { get; set; }       // (Price - Discount) * Quantity
    }
}
