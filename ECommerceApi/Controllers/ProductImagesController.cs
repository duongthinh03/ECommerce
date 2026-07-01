using ECommerceApi.DTOs.Catalog;
using ECommerceApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceApi.Controllers;

[Route("api/products/{productId:int}/images")]
public class ProductImagesController(IProductImageService service) : ApiControllerBase
{
    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> List(int productId) =>
        OkResponse(await service.GetByProductAsync(productId));

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Add(int productId, [FromBody] AddProductImageRequest req) =>
        CreatedResponse(await service.AddAsync(productId, req.ImageUrl));

    [Authorize(Roles = "Admin")]
    [HttpDelete("{imageId:int}")]
    public async Task<IActionResult> Delete(int productId, int imageId)
    {
        await service.DeleteAsync(productId, imageId);
        return OkResponse<object?>(null, "Đã xóa ảnh");
    }
}
