namespace ECommerceApi.Models;

public class CartItem
{
    public int Id { get; set; }

    public int CartId { get; set; }
    public Cart Cart { get; set; } = null!;

    public int ProductId { get; set; }              // kèm cho tiện hiển thị
    public Product Product { get; set; } = null!;

    public int VariantId { get; set; }              // thứ THỰC SỰ mua (SKU)
    public ProductVariant Variant { get; set; } = null!;

    public int Quantity { get; set; }
    public decimal Price { get; set; }              // snapshot giá lúc thêm

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}