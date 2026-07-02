using System.Security.Claims;
using ECommerceApi.Common;
using ECommerceApi.DTOs.Review;
using ECommerceApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceApi.Controllers;

[ApiController]
[Route("api/products/{productId:int}/reviews")]
public class ReviewsController(IReviewService service) : ControllerBase
{
    // userId nếu đã đăng nhập (GET cho phép ẩn danh → có thể null)
    private int? UserId
    {
        get
        {
            var id = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            return int.TryParse(id, out var uid) ? uid : null;
        }
    }

    private bool IsStaff =>
        User.IsInRole("Admin") || User.IsInRole("Manager") || User.IsInRole("Staff");

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Get(int productId) =>
        Ok(ApiResponse<ReviewSummaryDto>.Ok(await service.GetForProductAsync(productId, UserId)));

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create(int productId, [FromBody] CreateReviewRequest request)
    {
        var dto = await service.CreateAsync(UserId!.Value, productId, request);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<ReviewDto>.Ok(dto, "Đã gửi đánh giá"));
    }

    [HttpDelete("{reviewId:int}")]
    [Authorize]
    public async Task<IActionResult> Delete(int productId, int reviewId)
    {
        await service.DeleteAsync(UserId!.Value, reviewId, IsStaff);
        return Ok(ApiResponse<object?>.Ok(null, "Đã xóa đánh giá"));
    }
}
