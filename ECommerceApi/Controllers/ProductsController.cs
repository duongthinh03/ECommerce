using ECommerceApi.DTOs.Catalog;
using ECommerceApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceApi.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ProductsController(IProductService service) : ApiControllerBase
    {
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            var response = await service.GetAllAsync();
            return OkResponse(response);
        }

        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            var response = await service.GetByIdAsync(id);
            return OkResponse(response);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateProductRequest request)
        {
            var response = await service.CreateAsync(request);
            return CreatedResponse(response, "Tạo product thành công");
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateProductRequest request)
        {
            var response = await service.UpdateAsync(id, request);
            return OkResponse(response);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            await service.DeleteAsync(id);
            return OkResponse<object?>(null, "Đã xóa");
        }
    }
}