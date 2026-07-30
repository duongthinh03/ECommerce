using ECommerceApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceApi.Controllers;

[Authorize(Roles = "Admin")]
[Route("api/upload")]
public class UploadsController(IImageStorage storage) : ApiControllerBase
{
    private static readonly string[] Allowed = [".jpg", ".jpeg", ".png", ".webp", ".gif"];
    private const long MaxBytes = 5 * 1024 * 1024;   // 5MB

    [HttpPost]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        if (file is null || file.Length == 0)
            return BadRequestResponse("Chưa chọn ảnh");
        if (file.Length > MaxBytes)
            return BadRequestResponse("Ảnh tối đa 5MB");

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!Allowed.Contains(ext))
            return BadRequestResponse("Chỉ nhận ảnh jpg/png/webp/gif");

        await using var stream = file.OpenReadStream();
        var url = await storage.SaveAsync(stream, file.FileName, file.ContentType);
        return OkResponse(new { url }, "Tải ảnh thành công");
    }
}
