using ECommerceApi.DTOs.Catalog;
using ECommerceApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceApi.Controllers
{
    [Authorize(Roles = "Admin")]            // mặc định cả controller cần Admin
    public class CategoriesController(ICategoryService service) : ApiControllerBase
    {
        [HttpGet]
        [AllowAnonymous]                    // đọc thì cho công khai (storefront)
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
        public async Task<IActionResult> Create([FromBody] CreateCategoryRequest request)
        {
            var response = await service.CreateAsync(request);
            return CreatedResponse(response, "Tạo category thành công");
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCategoryRequest request)
        {
            var response = await service.UpdateAsync(id, request);
            return OkResponse(response, "Cập nhật thành công");
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            await service.DeleteAsync(id);
            return OkResponse<object?>(null, "Đã xóa");
        }
    }
}
