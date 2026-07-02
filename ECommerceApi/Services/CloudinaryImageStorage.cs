using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using ECommerceApi.Common;
using Microsoft.Extensions.Options;

namespace ECommerceApi.Services;

// Lưu ảnh lên Cloudinary → trả secure URL (CDN, không mất khi redeploy).
public class CloudinaryImageStorage : IImageStorage
{
    private readonly Cloudinary _cloudinary;

    public CloudinaryImageStorage(IOptions<CloudinarySettings> options)
    {
        var s = options.Value;
        _cloudinary = new Cloudinary(new Account(s.CloudName, s.ApiKey, s.ApiSecret));
        _cloudinary.Api.Secure = true;
    }

    public async Task<string> SaveAsync(Stream content, string fileName, string contentType)
    {
        var result = await _cloudinary.UploadAsync(new ImageUploadParams
        {
            File = new FileDescription(fileName, content),
            Folder = "shopviet",
            UniqueFilename = true
        });
        if (result.Error is not null)
            throw new InvalidOperationException(result.Error.Message);
        return result.SecureUrl.ToString();
    }
}
