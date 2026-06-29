namespace ECommerceApi.Models;

// Bảng nối: ngành (Category) nào dùng thuộc tính nào. Không kế thừa BaseEntity.
public class CategoryAttribute
{
    public int Id { get; set; }

    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    public int AttributeId { get; set; }
    public ProductAttribute Attribute { get; set; } = null!;

    public bool IsRequired { get; set; }
}
