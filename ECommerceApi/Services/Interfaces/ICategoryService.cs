using ECommerceApi.DTOs.Catalog;

namespace ECommerceApi.Services
{
    public interface ICategoryService
    {
        Task<IEnumerable<CategoryDto>> GetAllAsync();
        Task<CategoryDto> GetByIdAsync(int id);
        Task<CategoryDto> CreateAsync(CreateCategoryRequest request);
        Task<CategoryDto> UpdateAsync(int id, UpdateCategoryRequest request);
        Task DeleteAsync(int id);
    }
}
