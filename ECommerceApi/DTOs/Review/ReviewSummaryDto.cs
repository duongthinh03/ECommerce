namespace ECommerceApi.DTOs.Review;

// Trả cho GET /api/products/{id}/reviews: điểm TB + danh sách + trạng thái của user hiện tại
public class ReviewSummaryDto
{
    public double AverageRating { get; set; }
    public int Count { get; set; }
    public bool CanReview { get; set; }   // đã mua & chưa đánh giá → được viết
    public bool HasReviewed { get; set; } // đã đánh giá rồi
    public List<ReviewDto> Items { get; set; } = [];
}
