namespace ECommerceApi.DTOs.Catalog
{
    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Slug { get; set; } = null!;
        public string? Description { get; set; }
        public int CategoryId { get; set; }
        public string? CategoryName { get; set; }   // lấy từ navigation Category.Name
        public int? BrandId { get; set; }
        public string? BrandName { get; set; }       // lấy từ navigation Brand.Name
        public decimal DisplayPrice { get; set; }
        public int ViewCount { get; set; }
        public int SoldCount { get; set; }
        public string? Thumbnail { get; set; }
        public bool IsActive { get; set; }
        public bool InStock { get; set; }   // còn hàng? = có ≥1 variant active và Stock > 0
    }
}