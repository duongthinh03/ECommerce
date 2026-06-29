using ECommerceApi.DTOs.Catalog;
using ECommerceApi.Models;
using ECommerceApi.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using KeyNotFoundException = System.Collections.Generic.KeyNotFoundException;  // ⚠️ alias (bẫy GreenDonut)

namespace ECommerceApi.Services;

public class ProductService(IUnitOfWork uow) : IProductService
{
    public async Task<IEnumerable<ProductDto>> GetAllAsync()
    {
        var products = await uow.Repository<Product>().Query()
            .Include(p => p.Category)        // 🆕 nạp navigation để lấy CategoryName
            .Include(p => p.Brand)
            .OrderByDescending(p => p.Id)
            .ToListAsync();
        return products.Select(ToDto);
    }

    public async Task<ProductDto> GetByIdAsync(int id)
    {
        var product = await uow.Repository<Product>().Query()
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new KeyNotFoundException($"Không tìm thấy product id={id}");
        return ToDto(product);
    }

    public async Task<ProductDto> CreateAsync(CreateProductRequest request)
    {
        var repo = uow.Repository<Product>();

        if (await repo.Query().AnyAsync(p => p.Slug == request.Slug))
            throw new InvalidOperationException($"Slug '{request.Slug}' đã tồn tại");

        // validate FK: Category phải tồn tại
        var categoryExists = await uow.Repository<Category>().Query()
            .AnyAsync(c => c.Id == request.CategoryId);
        if (!categoryExists)
            throw new InvalidOperationException($"Category id={request.CategoryId} không tồn tại");

        // nếu có BrandId thì cũng phải tồn tại
        if (request.BrandId is not null)
        {
            var brandExists = await uow.Repository<Brand>().Query()
                .AnyAsync(b => b.Id == request.BrandId);
            if (!brandExists)
                throw new InvalidOperationException($"Brand id={request.BrandId} không tồn tại");
        }

        var product = new Product
        {
            Name = request.Name,
            Slug = request.Slug,
            Description = request.Description,
            CategoryId = request.CategoryId,
            BrandId = request.BrandId,
            DisplayPrice = request.DisplayPrice,
            Thumbnail = request.Thumbnail,
            IsActive = request.IsActive
        };

        await repo.AddAsync(product);
        await uow.CommitAsync();

        // lấy lại kèm tên Category/Brand để trả DTO đầy đủ
        return await GetByIdAsync(product.Id);
    }

    public async Task<ProductDto> UpdateAsync(int id, UpdateProductRequest request)
    {
        var repo = uow.Repository<Product>();
        var product = await repo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Không tìm thấy product id={id}");

        product.Name = request.Name;
        product.Slug = request.Slug;
        product.Description = request.Description;
        product.CategoryId = request.CategoryId;
        product.BrandId = request.BrandId;
        product.DisplayPrice = request.DisplayPrice;
        product.Thumbnail = request.Thumbnail;
        product.IsActive = request.IsActive;

        repo.Update(product);
        await uow.CommitAsync();
        return await GetByIdAsync(product.Id);
    }

    public async Task DeleteAsync(int id)
    {
        var repo = uow.Repository<Product>();
        var product = await repo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Không tìm thấy product id={id}");

        repo.Delete(product);   // soft-delete
        await uow.CommitAsync();
    }

    private static ProductDto ToDto(Product p) => new()
    {
        Id = p.Id,
        Name = p.Name,
        Slug = p.Slug,
        Description = p.Description,
        CategoryId = p.CategoryId,
        CategoryName = p.Category?.Name,   // ?. vì navigation có thể chưa nạp
        BrandId = p.BrandId,
        BrandName = p.Brand?.Name,
        DisplayPrice = p.DisplayPrice,
        ViewCount = p.ViewCount,
        SoldCount = p.SoldCount,
        Thumbnail = p.Thumbnail,
        IsActive = p.IsActive
    };
}