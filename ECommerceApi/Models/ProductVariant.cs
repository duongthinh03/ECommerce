namespace ECommerceApi.Models;

// SKU thực sự bán & trừ kho. Nguồn sự thật giá + tồn.
public class ProductVariant : BaseEntity
{
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public string Sku { get; set; } = null!;
    public string? OptionName { get; set; }        // nhãn hiển thị của phiên bản (vd "3U", "Đỏ / Size 42") — nhãn tạm trước khi có Attribute
    public decimal Price { get; set; }
    public decimal? CompareAtPrice { get; set; }   // giá gạch ngang (khuyến mãi)
    public int Stock { get; set; }
    public string? ImageUrl { get; set; }
    public decimal? Weight { get; set; }           // kg, dùng tính phí ship sau
    public bool IsActive { get; set; } = true;
}
