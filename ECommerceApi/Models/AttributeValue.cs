namespace ECommerceApi.Models;

public class AttributeValue
{
    public int Id { get; set; }

    public int AttributeId { get; set; }
    public ProductAttribute Attribute { get; set; } = null!;

    public string Value { get; set; } = null!;   // "42", "Đỏ", "3U", "Bìa cứng"

    public ICollection<VariantAttributeValue> VariantAttributeValues { get; set; } = [];
}
