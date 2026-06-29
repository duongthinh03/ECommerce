namespace ECommerceApi.Models;

/// <summary>
/// Base cho các entity cần soft-delete + audit thời gian.
/// Bảng nối thuần (CategoryAttribute, VariantAttributeValue) KHÔNG kế thừa.
/// </summary>
public abstract class BaseEntity
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }   // soft-delete: null = còn sống
}
