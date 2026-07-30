using ECommerceApi.Common;

namespace ECommerceApi.DTOs.Catalog;

// Bind từ query string cho GET /api/products:
// ?q=vot&categoryId=3&minPrice=100000&maxPrice=500000&inStock=true&sort=price_asc&page=1&pageSize=20
public class ProductSearchQuery : PageRequest
{
    public string? Q { get; set; }               // tìm theo tên
    public int? CategoryId { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public bool? InStock { get; set; }           // true = chỉ còn hàng
    public string? Sort { get; set; }            // newest | price_asc | price_desc
}
