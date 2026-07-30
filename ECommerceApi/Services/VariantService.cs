using ECommerceApi.DTOs.Catalog;
using ECommerceApi.Models;
using ECommerceApi.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using KeyNotFoundException = System.Collections.Generic.KeyNotFoundException;  // alias GreenDonut

namespace ECommerceApi.Services;

public class VariantService(IUnitOfWork uow) : IVariantService
{
    public async Task<IEnumerable<VariantDto>> GetByProductAsync(int productId)
    {
        await EnsureProductExists(productId);
        var variants = await uow.Repository<ProductVariant>().Query()
            .Where(v => v.ProductId == productId)        // chỉ lấy variant của product này
            .OrderBy(v => v.Id)
            .ToListAsync();
        return variants.Select(ToDto);
    }

    public async Task<VariantDto> GetByIdAsync(int productId, int id)
    {
        var variant = await uow.Repository<ProductVariant>().Query()
            .FirstOrDefaultAsync(v => v.Id == id && v.ProductId == productId)   // phải khớp cả product
            ?? throw new KeyNotFoundException($"Không tìm thấy variant id={id} của product {productId}");
        return ToDto(variant);
    }

    public async Task<VariantDto> CreateAsync(int productId, CreateVariantRequest request)
    {
        await EnsureProductExists(productId);     // product cha phải tồn tại
        var repo = uow.Repository<ProductVariant>();

        if (await repo.Query().AnyAsync(v => v.Sku == request.Sku))
            throw new InvalidOperationException($"Sku '{request.Sku}' đã tồn tại");

        var variant = new ProductVariant
        {
            ProductId = productId,                // gán từ route
            Sku = request.Sku,
            OptionName = request.OptionName,
            Price = request.Price,
            CompareAtPrice = request.CompareAtPrice,
            Stock = request.Stock,
            ImageUrl = request.ImageUrl,
            Weight = request.Weight,
            IsActive = request.IsActive
        };
        await repo.AddAsync(variant);
        await uow.CommitAsync();
        return ToDto(variant);
    }

    public async Task<VariantDto> UpdateAsync(int productId, int id, UpdateVariantRequest request)
    {
        var repo = uow.Repository<ProductVariant>();
        var variant = await repo.Query()
            .FirstOrDefaultAsync(v => v.Id == id && v.ProductId == productId)
            ?? throw new KeyNotFoundException($"Không tìm thấy variant id={id} của product {productId}");

        // đổi Sku thì check trùng (trừ chính nó)
        if (variant.Sku != request.Sku &&
            await repo.Query().AnyAsync(v => v.Sku == request.Sku))
            throw new InvalidOperationException($"Sku '{request.Sku}' đã tồn tại");

        variant.Sku = request.Sku;
        variant.OptionName = request.OptionName;
        variant.Price = request.Price;
        variant.CompareAtPrice = request.CompareAtPrice;
        variant.Stock = request.Stock;
        variant.ImageUrl = request.ImageUrl;
        variant.Weight = request.Weight;
        variant.IsActive = request.IsActive;

        repo.Update(variant);
        await uow.CommitAsync();
        return ToDto(variant);
    }

    public async Task DeleteAsync(int productId, int id)
    {
        var repo = uow.Repository<ProductVariant>();
        var variant = await repo.Query()
            .FirstOrDefaultAsync(v => v.Id == id && v.ProductId == productId)
            ?? throw new KeyNotFoundException($"Không tìm thấy variant id={id} của product {productId}");

        repo.Delete(variant);   // soft-delete
        await uow.CommitAsync();
    }

    // helper dùng chung: product cha có tồn tại không
    private async Task EnsureProductExists(int productId)
    {
        var exists = await uow.Repository<Product>().Query().AnyAsync(p => p.Id == productId);
        if (!exists)
            throw new KeyNotFoundException($"Không tìm thấy product id={productId}");
    }

    private static VariantDto ToDto(ProductVariant v) => new()
    {
        Id = v.Id,
        ProductId = v.ProductId,
        Sku = v.Sku,
        OptionName = v.OptionName,
        Price = v.Price,
        CompareAtPrice = v.CompareAtPrice,
        Stock = v.Stock,
        ImageUrl = v.ImageUrl,
        Weight = v.Weight,
        IsActive = v.IsActive
    };
}