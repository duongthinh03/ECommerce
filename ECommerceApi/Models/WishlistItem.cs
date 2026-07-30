namespace ECommerceApi.Models;

// Sản phẩm yêu thích. KHÔNG kế thừa BaseEntity → xóa cứng khi bỏ thích
// (tránh vướng unique (UserId, ProductId) khi thích lại).
public class WishlistItem
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
}
