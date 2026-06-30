namespace ECommerceApi.DTOs.Cart
{
    public class CartDto
    {
        public int Id { get; set; }
        public List<CartItemDto> Items { get; set; } = [];
        public int TotalQuantity { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
