namespace ECommerceApi.Models;

// Đánh giá sản phẩm — chỉ người đã mua (verified purchase) mới được tạo.
// 1 user / 1 product / 1 review đang hiệu lực (unique lọc theo DeletedAt IS NULL).
public class ProductReview : BaseEntity
{
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public int Rating { get; set; }        // 1..5
    public string? Comment { get; set; }
}
