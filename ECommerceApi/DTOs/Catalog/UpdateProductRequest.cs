namespace ECommerceApi.DTOs.Catalog
{
    public class UpdateProductRequest
    {
        public string Name { get; set; } = null!;
        public string Slug { get; set; } = null!;
        public string? Description { get; set; }
        public int CategoryId { get; set; }
        public int? BrandId { get; set; }
        public decimal DisplayPrice { get; set; }
        public string? Thumbnail { get; set; }
        public bool IsActive { get; set; }
    }
}