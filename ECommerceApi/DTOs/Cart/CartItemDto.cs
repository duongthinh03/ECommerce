namespace ECommerceApi.DTOs.Cart
{
    public class CartItemDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public string? Thumbnail { get; set; }     // ảnh variant → fallback ảnh sản phẩm
        public int VariantId { get; set; }
        public string Sku { get; set; } = null!;
        public int Quantity { get; set; }
        public decimal Price { get; set; }        // giá snapshot lúc thêm
        public decimal LineTotal { get; set; }    // Price * Quantity
        public int Stock { get; set; }             // tồn kho hiện tại của variant (để cảnh báo hết/thiếu)
    }
}
