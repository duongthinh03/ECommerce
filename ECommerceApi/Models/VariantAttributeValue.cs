namespace ECommerceApi.Models;

// Bảng nối: tổ hợp option của 1 SKU. Vd variant#5 có (Size=42), (Màu=Đỏ).
public class VariantAttributeValue
{
    public int Id { get; set; }

    public int VariantId { get; set; }
    public ProductVariant Variant { get; set; } = null!;

    public int AttributeId { get; set; }
    public ProductAttribute Attribute { get; set; } = null!;

    public int AttributeValueId { get; set; }
    public AttributeValue AttributeValue { get; set; } = null!;
}
