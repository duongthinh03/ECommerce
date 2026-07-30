using ECommerceApi.DTOs.Review;

namespace ECommerceApi.Services;

public interface IReviewService
{
    Task<ReviewSummaryDto> GetForProductAsync(int productId, int? currentUserId);
    Task<ReviewDto> CreateAsync(int userId, int productId, CreateReviewRequest request);
    Task DeleteAsync(int userId, int reviewId, bool isStaff);
}
