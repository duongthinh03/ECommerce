using System.Security.Claims;
using ECommerceApi.DTOs.Order;
using ECommerceApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceApi.Controllers
{
    [Authorize(Roles = "Admin,Manager,Staff")]
    [Route("api/admin/orders")]
    public class AdminOrdersController(IOrderService service) : ApiControllerBase
    {
        // Tất cả đơn (admin)
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var response = await service.GetAllAsync();
            return OkResponse(response);
        }

        // Chi tiết 1 đơn bất kỳ
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var response = await service.GetAdminByIdAsync(id);
            return OkResponse(response);
        }

        // Đổi trạng thái + ghi history (ai đổi)
        [HttpPut("{id:int}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateOrderStatusRequest request)
        {
            var changedBy = User.FindFirstValue(ClaimTypes.Email) ?? $"user#{CurrentUserId}";
            var response = await service.UpdateStatusAsync(id, request.Status, request.Note, changedBy);
            return OkResponse(response, "Đã cập nhật trạng thái");
        }
    }
}
