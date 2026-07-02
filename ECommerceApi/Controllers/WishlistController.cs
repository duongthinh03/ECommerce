using ECommerceApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceApi.Controllers;

[Authorize]   // yêu thích luôn gắn với user đang đăng nhập
public class WishlistController(IWishlistService service) : ApiControllerBase
{
    private int UserId => CurrentUserId
        ?? throw new UnauthorizedAccessException("Cần đăng nhập");

    [HttpGet]
    public async Task<IActionResult> GetMine() =>
        OkResponse(await service.GetMineAsync(UserId));

    [HttpPost("{productId:int}")]
    public async Task<IActionResult> Add(int productId)
    {
        await service.AddAsync(UserId, productId);
        return OkResponse<object?>(null, "Đã thêm vào yêu thích");
    }

    [HttpDelete("{productId:int}")]
    public async Task<IActionResult> Remove(int productId)
    {
        await service.RemoveAsync(UserId, productId);
        return OkResponse<object?>(null, "Đã bỏ yêu thích");
    }
}
