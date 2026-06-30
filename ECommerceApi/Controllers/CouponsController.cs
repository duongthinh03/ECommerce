using ECommerceApi.DTOs.Coupon;
using ECommerceApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceApi.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CouponsController(ICouponService service) : ApiControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var response = await service.GetAllAsync();
            return OkResponse(response);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCouponRequest request)
        {
            var response = await service.CreateAsync(request);
            return CreatedResponse(response, "Tạo coupon thành công");
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            await service.DeleteAsync(id);
            return OkResponse<object?>(null, "Đã xóa");
        }
    }
}
