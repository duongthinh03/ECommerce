using ECommerceApi.DTOs.Review;
using ECommerceApi.Models;
using ECommerceApi.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using KeyNotFoundException = System.Collections.Generic.KeyNotFoundException;

namespace ECommerceApi.Services;

public class ReviewService(IUnitOfWork uow) : IReviewService
{
    // "Đã mua" = đơn đã được xác nhận trở đi (COD: admin xác nhận; SePay: tự confirm khi Paid),
    // không tính đơn Pending / Cancelled / Refunded.
    private static readonly OrderStatus[] PurchasedStatuses =
        [OrderStatus.Confirmed, OrderStatus.Packing, OrderStatus.Shipping, OrderStatus.Delivered, OrderStatus.Completed];

    public async Task<ReviewSummaryDto> GetForProductAsync(int productId, int? currentUserId)
    {
        var reviews = await uow.Repository<ProductReview>().Query()
            .Where(r => r.ProductId == productId)
            .Include(r => r.User)
            .OrderByDescending(r => r.Id)
            .ToListAsync();

        var items = reviews.Select(r => new ReviewDto
        {
            Id = r.Id,
            UserId = r.UserId,
            UserName = r.User.FullName,
            UserAvatarUrl = r.User.AvatarUrl,
            Rating = r.Rating,
            Comment = r.Comment,
            CreatedAt = r.CreatedAt
        }).ToList();

        var summary = new ReviewSummaryDto
        {
            Count = items.Count,
            AverageRating = items.Count > 0 ? Math.Round(items.Average(i => i.Rating), 1) : 0,
            Items = items
        };

        if (currentUserId is int uid)
        {
            summary.HasReviewed = reviews.Any(r => r.UserId == uid);
            summary.CanReview = !summary.HasReviewed && await HasPurchasedAsync(uid, productId);
        }

        return summary;
    }

    public async Task<ReviewDto> CreateAsync(int userId, int productId, CreateReviewRequest request)
    {
        var productExists = await uow.Repository<Product>().Query().AnyAsync(p => p.Id == productId);
        if (!productExists)
            throw new KeyNotFoundException($"Không tìm thấy sản phẩm id={productId}");

        if (!await HasPurchasedAsync(userId, productId))
            throw new InvalidOperationException("Bạn chỉ có thể đánh giá sản phẩm đã mua.");

        var repo = uow.Repository<ProductReview>();
        if (await repo.Query().AnyAsync(r => r.ProductId == productId && r.UserId == userId))
            throw new InvalidOperationException("Bạn đã đánh giá sản phẩm này rồi.");

        var review = new ProductReview
        {
            ProductId = productId,
            UserId = userId,
            Rating = request.Rating,
            Comment = string.IsNullOrWhiteSpace(request.Comment) ? null : request.Comment.Trim()
        };
        await repo.AddAsync(review);
        await uow.CommitAsync();

        var user = await uow.Repository<User>().GetByIdAsync(userId);
        return new ReviewDto
        {
            Id = review.Id,
            UserId = userId,
            UserName = user?.FullName ?? "Người dùng",
            UserAvatarUrl = user?.AvatarUrl,
            Rating = review.Rating,
            Comment = review.Comment,
            CreatedAt = review.CreatedAt
        };
    }

    public async Task DeleteAsync(int userId, int reviewId, bool isStaff)
    {
        var repo = uow.Repository<ProductReview>();
        var review = await repo.GetByIdAsync(reviewId)
            ?? throw new KeyNotFoundException("Không tìm thấy đánh giá");

        if (review.UserId != userId && !isStaff)
            throw new UnauthorizedAccessException("Không thể xóa đánh giá của người khác");

        repo.Delete(review);   // soft-delete
        await uow.CommitAsync();
    }

    private async Task<bool> HasPurchasedAsync(int userId, int productId) =>
        await uow.Repository<OrderItem>().Query()
            .AnyAsync(oi => oi.ProductId == productId
                && oi.Order.UserId == userId
                && PurchasedStatuses.Contains(oi.Order.Status));
}
