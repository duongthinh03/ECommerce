namespace ECommerceApi.Models;

public class Brand : BaseEntity
{
    public string Name { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public string? Description { get; set; }

    public ICollection<Product> Products { get; set; } = [];
}
