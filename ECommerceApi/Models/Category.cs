namespace ECommerceApi.Models;

public class Category : BaseEntity
{
    public string Name { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public string? Description { get; set; }

    // Cây danh mục (self-reference)
    public int? ParentId { get; set; }
    public Category? Parent { get; set; }
    public ICollection<Category> Children { get; set; } = [];

    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<Product> Products { get; set; } = [];
}
