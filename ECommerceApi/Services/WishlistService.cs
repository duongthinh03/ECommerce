using ECommerceApi.DTOs.Catalog;
using ECommerceApi.Models;
using ECommerceApi.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using KeyNotFoundException = System.Collections.Generic.KeyNotFoundException;

namespace ECommerceApi.Services;

public class WishlistService(IUnitOfWork uow, IProductService products) : IWishlistService
{
    public async Task<List<ProductDto>> GetMineAsync(int userId)
    {
        var ids = await uow.Repository<WishlistItem>().Query()
            .Where(w => w.UserId == userId)
            .OrderByDescending(w => w.Id)   // mới thích lên đầu
            .Select(w => w.ProductId)
            .ToListAsync();

        return await products.GetByIdsAsync(ids);
    }

    public async Task AddAsync(int userId, int productId)
    {
        var exists = await uow.Repository<Product>().Query().AnyAsync(p => p.Id == productId);
        if (!exists)
            throw new KeyNotFoundException($"Không tìm thấy sản phẩm id={productId}");

        var repo = uow.Repository<WishlistItem>();
        if (await repo.Query().AnyAsync(w => w.UserId == userId && w.ProductId == productId))
            return;   // đã có → idempotent, không lỗi

        await repo.AddAsync(new WishlistItem { UserId = userId, ProductId = productId });
        await uow.CommitAsync();
    }

    public async Task RemoveAsync(int userId, int productId)
    {
        var repo = uow.Repository<WishlistItem>();
        var item = await repo.Query().FirstOrDefaultAsync(w => w.UserId == userId && w.ProductId == productId);
        if (item is null) return;

        repo.Delete(item);   // xóa cứng (không kế thừa BaseEntity)
        await uow.CommitAsync();
    }
}
