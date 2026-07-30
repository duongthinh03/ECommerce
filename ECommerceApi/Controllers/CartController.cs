using ECommerceApi.DTOs.Cart;
using ECommerceApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceApi.Controllers
{
    [Route("api/cart")]
    public class CartController(ICartService service) : ApiControllerBase
    {
         // Lấy danh tính giỏ: user từ JWT (nếu login) HOẶC sessionId từ header (guest)
        private (int? userId, string? sessionId) GetOwner()
        {
            var sessionId = Request.Headers["X-Session-Id"].FirstOrDefault();
            return (CurrentUserId, sessionId);
        }

        [HttpGet]
        public async Task<IActionResult> GetItem()
        {
            var (userId, sessionId) = GetOwner();
            return OkResponse(await service.GetCartAsync(userId, sessionId));
        }

        [HttpPost("items")]
        public async Task<IActionResult> AddItem([FromBody] AddCartItemRequest request)
        {
            var (userId, sessionId) = GetOwner();
            return OkResponse(await service.AddItemAsync(userId, sessionId, request), "Đã thêm vào giỏ");
        }

        [HttpPut("items/{variantId:int}")]
        public async Task<IActionResult> UpdateItem(int variantId, [FromBody] UpdateCartItemRequest request)
        {
            var (userId, sessionId) = GetOwner();
            return OkResponse(await service.UpdateItemAsync(userId, sessionId, variantId, request.Quantity), "Đã cập nhật");
        }

        [HttpDelete("items/{variantId:int}")]
        public async Task<IActionResult> RemoveItem(int variantId)
        {
            var (userId, sessionId) = GetOwner();
            return OkResponse(await service.RemoveItemAsync(userId, sessionId, variantId), "Đã xóa item");
        }

        [HttpDelete]
        public async Task<IActionResult> Clear()
        {
            var (userId, sessionId) = GetOwner();
            await service.ClearAsync(userId, sessionId);
            return OkResponse<object?>(null, "Đã xóa giỏ");
        }

        [Authorize]
        [HttpPost("merge")]
        public async Task<IActionResult> Merge([FromBody] MergeCartRequest request)
        {
            var userId = CurrentUserId
                ?? throw new UnauthorizedAccessException("Cần đăng nhập để merge giỏ");
            return OkResponse(await service.MergeAsync(userId, request.SessionId), "Đã gộp giỏ");
        }
    }
}