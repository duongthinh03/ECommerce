using ECommerceApi.DTOs.Order;
using ECommerceApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceApi.Controllers
{
    [Authorize]
    [Route("api/orders")]
    public class OrdersController(IOrderService service) : ApiControllerBase
    {
        private int CurrentUserIdOrThrow => CurrentUserId ?? throw new UnauthorizedAccessException("Cần đăng nhập");

        // Đặt hàng (checkout từ giỏ)
        [HttpPost]
        public async Task<IActionResult> Checkout([FromBody] CheckoutRequest request)
        {
            var response = await service.CheckoutAsync(CurrentUserIdOrThrow, request);
            return CreatedResponse(response, "Đặt hàng thành công");
        }

        // Danh sách đơn của tôi
        [HttpGet]
        public async Task<IActionResult> GetMyOrders()
        {
            var response = await service.GetMyOrdersAsync(CurrentUserIdOrThrow);
            return OkResponse(response);
        }

        // Chi tiết 1 đơn
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var response = await service.GetByIdAsync(CurrentUserIdOrThrow, id);
            return OkResponse(response);
        }
    }
}
