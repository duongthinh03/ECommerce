namespace ECommerceApi.DTOs.Catalog;

public class UpdateVariantRequest
{
    public string Sku { get; set; } = null!;
    public decimal Price { get; set; }
    public decimal? CompareAtPrice { get; set; }
    public int Stock { get; set; }
    public string? ImageUrl { get; set; }
    public decimal? Weight { get; set; }
    public bool IsActive { get; set; } = true;
}