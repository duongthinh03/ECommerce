using ECommerceApi.DTOs.Catalog;
using ECommerceApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceApi.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("api/products/{productId:int}/variants")]
    public class VariantsController(IVariantService service) : ApiControllerBase
    {
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetByProduct(int productId)
        {
            var response = await service.GetByProductAsync(productId);
            return OkResponse(response);
        }

        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int productId, int id)
        {
            var response = await service.GetByIdAsync(productId, id);
            return OkResponse(response);
        }

        [HttpPost]
        public async Task<IActionResult> Create(int productId, [FromBody] CreateVariantRequest request)
        {
            var response = await service.CreateAsync(productId, request);
            return CreatedResponse(response, "Tạo variant thành công");
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int productId, int id, [FromBody] UpdateVariantRequest request)
        {
            var response = await service.UpdateAsync(productId, id, request);
            return OkResponse(response, "Cập nhật thành công");
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int productId, int id)
        {
            await service.DeleteAsync(productId, id);
            return OkResponse<object?>(null, "Đã xóa");
        }
    }
}