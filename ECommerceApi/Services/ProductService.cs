using ECommerceApi.Common;
using ECommerceApi.DTOs.Catalog;
using ECommerceApi.Models;
using ECommerceApi.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using KeyNotFoundException = System.Collections.Generic.KeyNotFoundException;  // alias (bẫy GreenDonut)

namespace ECommerceApi.Services;

public class ProductService(IUnitOfWork uow) : IProductService
{
    public async Task<PagedResult<ProductDto>> SearchAsync(ProductSearchQuery query)
    {
        IQueryable<Product> q = uow.Repository<Product>()
            .Query()
            .Include(p => p.Category)        // nạp navigation để lấy CategoryName
            .Include(p => p.Brand)
            .Include(p => p.Variants);       // để tính còn hàng (InStock)

        // ----- Lọc -----
        if (!string.IsNullOrWhiteSpace(query.Q))
        {
            var term = query.Q.Trim();
            q = q.Where(p => p.Name.Contains(term));
        }
        if (query.CategoryId is int cid)
            q = q.Where(p => p.CategoryId == cid);
        if (query.MinPrice is decimal min)
            q = q.Where(p => p.DisplayPrice >= min);
        if (query.MaxPrice is decimal max)
            q = q.Where(p => p.DisplayPrice <= max);
        if (query.InStock == true)
            q = q.Where(p => p.Variants.Any(v => v.Stock > 0 && v.IsActive));

        // ----- Sắp xếp -----
        q = query.Sort switch
        {
            "price_asc" => q.OrderBy(p => p.DisplayPrice).ThenByDescending(p => p.Id),
            "price_desc" => q.OrderByDescending(p => p.DisplayPrice).ThenByDescending(p => p.Id),
            _ => q.OrderByDescending(p => p.Id),   // newest (mặc định)
        };

        // ----- Phân trang ----- (dùng helper sẵn có; query : PageRequest nên truyền thẳng)
        var paged = await q.ToPagedResultAsync(query);

        var dtos = paged.Items.Select(ToDto).ToList();
        await AttachRatingsAsync(dtos);

        return new PagedResult<ProductDto>
        {
            Items = dtos,
            Page = paged.Page,
            PageSize = paged.PageSize,
            TotalCount = paged.TotalCount
        };
    }

    public async Task<ProductDto> GetByIdAsync(int id)
    {
        var product = await uow.Repository<Product>().Query()
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Include(p => p.Variants)
            .FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new KeyNotFoundException($"Không tìm thấy product id={id}");
        var dto = ToDto(product);
        await AttachRatingsAsync([dto]);
        return dto;
    }

    public async Task<List<ProductDto>> GetByIdsAsync(IReadOnlyList<int> ids)
    {
        if (ids.Count == 0) return [];
        var products = await uow.Repository<Product>().Query()
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Include(p => p.Variants)
            .Where(p => ids.Contains(p.Id))
            .ToListAsync();

        var dtos = products.Select(ToDto).ToList();
        await AttachRatingsAsync(dtos);

        // giữ đúng thứ tự truyền vào (vd wishlist: mới thêm lên đầu)
        var map = dtos.ToDictionary(d => d.Id);
        return ids.Where(map.ContainsKey).Select(id => map[id]).ToList();
    }

    // Nạp điểm đánh giá TB + số lượt cho danh sách DTO bằng 1 query gộp (tránh N+1).
    private async Task AttachRatingsAsync(List<ProductDto> dtos)
    {
        if (dtos.Count == 0) return;
        var ids = dtos.Select(d => d.Id).ToList();
        var stats = await uow.Repository<ProductReview>().Query()
            .Where(r => ids.Contains(r.ProductId))
            .GroupBy(r => r.ProductId)
            .Select(g => new { ProductId = g.Key, Avg = g.Average(x => (double)x.Rating), Count = g.Count() })
            .ToDictionaryAsync(x => x.ProductId);

        foreach (var d in dtos)
        {
            if (stats.TryGetValue(d.Id, out var s))
            {
                d.AvgRating = Math.Round(s.Avg, 1);
                d.ReviewCount = s.Count;
            }
        }
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
        IsActive = p.IsActive,
        InStock = p.Variants.Any(v => v.Stock > 0 && v.IsActive)
    };
}