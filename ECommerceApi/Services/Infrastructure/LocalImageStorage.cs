namespace ECommerceApi.Services;

// Fallback dev: lưu vào wwwroot/uploads, trả URL tuyệt đối (theo host request). MẤT khi redeploy.
public class LocalImageStorage(IWebHostEnvironment env, IHttpContextAccessor http) : IImageStorage
{
    public async Task<string> SaveAsync(Stream content, string fileName, string contentType)
    {
        var webRoot = env.WebRootPath ?? Path.Combine(env.ContentRootPath, "wwwroot");
        var dir = Path.Combine(webRoot, "uploads");
        Directory.CreateDirectory(dir);

        var name = $"{Guid.NewGuid():N}{Path.GetExtension(fileName).ToLowerInvariant()}";
        var savePath = Path.Combine(dir, name);
        await using (var fs = File.Create(savePath))
            await content.CopyToAsync(fs);

        var req = http.HttpContext!.Request;
        return $"{req.Scheme}://{req.Host}/uploads/{name}";
    }
}
