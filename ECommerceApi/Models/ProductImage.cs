namespace ECommerceApi.Models;

// Không kế thừa BaseEntity: schema chỉ có CreatedAt, không soft-delete.
public class ProductImage
{
    public int Id { get; set; }

    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public string ImageUrl { get; set; } = null!;
    public bool IsMain { get; set; }
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; }
}
