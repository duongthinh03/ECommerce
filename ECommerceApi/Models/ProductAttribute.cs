namespace ECommerceApi.Models;

// Tên class là ProductAttribute (KHÔNG phải "Attribute") để tránh đụng System.Attribute.
// Bảng DB vẫn là "Attributes" (map trong config) theo schema Mục 1.
public class ProductAttribute
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;          // "Size", "Màu", "Độ cứng (U)"
    public string Code { get; set; } = null!;          // "size", "color", "stiffness" (unique)
    public AttributeType Type { get; set; } = AttributeType.Select;
    public bool IsVariant { get; set; }                // true = sinh SKU (size/màu); false = spec thường

    public ICollection<AttributeValue> Values { get; set; } = [];
    public ICollection<CategoryAttribute> CategoryAttributes { get; set; } = [];
    public ICollection<VariantAttributeValue> VariantAttributeValues { get; set; } = [];
}
