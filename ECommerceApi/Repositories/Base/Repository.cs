using ECommerceApi.Data;
using ECommerceApi.Models;
using ECommerceApi.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ECommerceApi.Repositories.Base
{
    public class Repository<T>(AppDbContext db) : IRepository<T> where T : class
    {
        protected readonly AppDbContext _db = db;
        protected readonly DbSet<T> _set = db.Set<T>();

        public async Task<IEnumerable<T>> GetAllAsync() =>
            await _set.ToListAsync();

        public async Task<T?> GetByIdAsync(int id) =>
            await _set.FindAsync(id);

        public async Task AddAsync(T entity) =>
            await _set.AddAsync(entity);

        public void Update(T entity) =>
            _set.Update(entity);

        public void Delete(T entity)
        {
            // Kế thừa BaseEntity → soft-delete (set DeletedAt). Còn lại → xóa cứng.
            if (entity is BaseEntity softDeletable)
            {
                softDeletable.DeletedAt = DateTime.UtcNow;
                _set.Update(entity);
            }
            else
            {
                _set.Remove(entity);
            }
        }

        public void HardDelete(T entity) =>
            _set.Remove(entity);

        // Lưu ý: query mặc định đã tự ẩn bản ghi soft-deleted nhờ HasQueryFilter.
        // Cần lấy cả bản đã xóa thì gọi .IgnoreQueryFilters() trên kết quả Query().
        public IQueryable<T> Query() => _set.AsQueryable();
    }
}
