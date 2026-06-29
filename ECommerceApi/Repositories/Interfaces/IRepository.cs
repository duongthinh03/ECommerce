namespace ECommerceApi.Repositories.Interfaces
{
    public interface IRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<T?> GetByIdAsync(int id);
        Task AddAsync(T entity);
        void Update(T entity);
        void Delete(T entity);        // soft-delete nếu kế thừa BaseEntity, ngược lại xóa cứng
        void HardDelete(T entity);    // luôn xóa cứng khỏi DB

        IQueryable<T> Query();        // để build query (filter/paging) ở tầng service
    }
}
