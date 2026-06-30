namespace ECommerceApi.DTOs.Cart
{
    public class CartItemDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public int VariantId { get; set; }
        public string Sku { get; set; } = null!;
        public int Quantity { get; set; }
        public decimal Price { get; set; }        // giá snapshot lúc thêm
        public decimal LineTotal { get; set; }    // Price * Quantity
    }
}
