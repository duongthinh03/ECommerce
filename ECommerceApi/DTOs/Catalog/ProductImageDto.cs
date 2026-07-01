namespace ECommerceApi.DTOs.Catalog;

public class ProductImageDto
{
    public int Id { get; set; }
    public string ImageUrl { get; set; } = null!;
    public bool IsMain { get; set; }
    public int SortOrder { get; set; }
}

public class AddProductImageRequest
{
    public string ImageUrl { get; set; } = null!;
}
