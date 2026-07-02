namespace ECommerceApi.Services;

// Lưu ảnh upload → trả URL. Có Cloudinary (prod) + Local wwwroot (dev fallback).
public interface IImageStorage
{
    Task<string> SaveAsync(Stream content, string fileName, string contentType);
}
