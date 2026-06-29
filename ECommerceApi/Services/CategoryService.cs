using ECommerceApi.DTOs.Catalog;
using ECommerceApi.Models;
using ECommerceApi.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using KeyNotFoundException = System.Collections.Generic.KeyNotFoundException;


namespace ECommerceApi.Services
{
    public class CategoryService(IUnitOfWork uow) : ICategoryService
    {
        public async Task<IEnumerable<CategoryDto>> GetAllAsync()
        {
            var categories = await uow.Repository<Category>().Query()
                .OrderBy(c => c.SortOrder)
                .ToListAsync();
            return categories.Select(ToDto);
        }

        public async Task<CategoryDto> GetByIdAsync(int id)
        {
            var category = await uow.Repository<Category>().GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"Không tìm thấy category id={id}");
            return ToDto(category);
        }

        public async Task<CategoryDto> CreateAsync(CreateCategoryRequest request)
        {
            var repo = uow.Repository<Category>();

            // check slug trùng (UX tốt hơn để DB ném lỗi)
            if (await repo.Query().AnyAsync(c => c.Slug == request.Slug))
                throw new InvalidOperationException($"Slug '{request.Slug}' đã tồn tại");

            var category = new Category
            {
                Name = request.Name,
                Slug = request.Slug,
                Description = request.Description,
                ParentId = request.ParentId,
                SortOrder = request.SortOrder,
                IsActive = request.IsActive
            };

            await repo.AddAsync(category);
            await uow.CommitAsync();
            return ToDto(category);
        }

        public async Task<CategoryDto> UpdateAsync(int id, UpdateCategoryRequest request)
        {
            var repo = uow.Repository<Category>();
            var category = await repo.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"Không tìm thấy category id={id}");

            category.Name = request.Name;
            category.Slug = request.Slug;
            category.Description = request.Description;
            category.ParentId = request.ParentId;
            category.SortOrder = request.SortOrder;
            category.IsActive = request.IsActive;

            repo.Update(category);
            await uow.CommitAsync();
            return ToDto(category);
        }

        public async Task DeleteAsync(int id)
        {
            var repo = uow.Repository<Category>();
            var category = await repo.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"Không tìm thấy category id={id}");

            repo.Delete(category);   // soft-delete (set DeletedAt) vì Category kế thừa BaseEntity
            await uow.CommitAsync();
        }

        // mapping thủ công entity -> DTO
        private static CategoryDto ToDto(Category c) => new()
        {
            Id = c.Id,
            Name = c.Name,
            Slug = c.Slug,
            Description = c.Description,
            ParentId = c.ParentId,
            SortOrder = c.SortOrder,
            IsActive = c.IsActive
        };
    }
}
