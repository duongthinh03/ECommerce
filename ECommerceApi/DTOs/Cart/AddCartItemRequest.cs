namespace ECommerceApi.DTOs.Cart
{
    public class AddCartItemRequest
    {
        public int VariantId { get; set; }
        public int Quantity { get; set; } = 1;
    }
}
