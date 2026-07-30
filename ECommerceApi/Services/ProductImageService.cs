using ECommerceApi.DTOs.Catalog;
using ECommerceApi.Models;
using ECommerceApi.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace ECommerceApi.Services;

// Gallery ảnh phụ của product. Ảnh bìa (card) là Product.Thumbnail, quản lý riêng ở form.
public class ProductImageService(IUnitOfWork uow) : IProductImageService
{
    public async Task<IEnumerable<ProductImageDto>> GetByProductAsync(int productId)
    {
        var images = await uow.Repository<ProductImage>().Query()
            .Where(i => i.ProductId == productId)
            .OrderBy(i => i.SortOrder).ThenBy(i => i.Id)
            .ToListAsync();
        return images.Select(ToDto);
    }

    public async Task<ProductImageDto> AddAsync(int productId, string imageUrl)
    {
        var exists = await uow.Repository<Product>().Query().AnyAsync(p => p.Id == productId);
        if (!exists) throw new KeyNotFoundException($"Không tìm thấy product id={productId}");

        var img = new ProductImage
        {
            ProductId = productId,
            ImageUrl = imageUrl,
            IsMain = false,
            SortOrder = 0,
            CreatedAt = DateTime.UtcNow
        };
        await uow.Repository<ProductImage>().AddAsync(img);
        await uow.CommitAsync();
        return ToDto(img);
    }

    public async Task DeleteAsync(int productId, int imageId)
    {
        var repo = uow.Repository<ProductImage>();
        var img = await repo.Query().FirstOrDefaultAsync(i => i.Id == imageId && i.ProductId == productId)
            ?? throw new KeyNotFoundException($"Không tìm thấy ảnh id={imageId}");
        repo.Delete(img);   // ProductImage không soft-delete → xóa cứng
        await uow.CommitAsync();
    }

    private static ProductImageDto ToDto(ProductImage i) => new()
    {
        Id = i.Id,
        ImageUrl = i.ImageUrl,
        IsMain = i.IsMain,
        SortOrder = i.SortOrder
    };
}
