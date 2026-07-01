using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceApi.Controllers;

[Authorize(Roles = "Admin")]
[Route("api/upload")]
public class UploadsController(IWebHostEnvironment env) : ApiControllerBase
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

        var webRoot = env.WebRootPath ?? Path.Combine(env.ContentRootPath, "wwwroot");
        var dir = Path.Combine(webRoot, "uploads");
        Directory.CreateDirectory(dir);

        var name = $"{Guid.NewGuid():N}{ext}";
        var savePath = Path.Combine(dir, name);
        await using (var stream = System.IO.File.Create(savePath))
            await file.CopyToAsync(stream);

        // URL tuyệt đối để FE (origin khác) hiển thị <img src> trực tiếp
        var url = $"{Request.Scheme}://{Request.Host}/uploads/{name}";
        return OkResponse(new { url }, "Tải ảnh thành công");
    }
}
