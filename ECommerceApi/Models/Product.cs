namespace ECommerceApi.Models;

public class Product : BaseEntity
{
    public string Name { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public string? Description { get; set; }

    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    public int? BrandId { get; set; }          // nullable: SP có thể không có brand (vd sách)
    public Brand? Brand { get; set; }

    // Giá "từ..." hiển thị, denormalized — KHÔNG trừ kho. Nguồn sự thật ở ProductVariant.
    public decimal DisplayPrice { get; set; }

    public int ViewCount { get; set; }
    public int SoldCount { get; set; }
    public string? Thumbnail { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<ProductImage> Images { get; set; } = [];
    public ICollection<ProductVariant> Variants { get; set; } = [];
}
