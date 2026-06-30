using ECommerceApi.Repositories.Interfaces;

namespace ECommerceApi.UnitOfWork
{
    public interface IUnitOfWork : IAsyncDisposable
    {
        // Lấy generic repository cho bất kỳ entity nào (cache theo type)
        IRepository<T> Repository<T>() where T : class;

        // Lưu thay đổi (chưa commit transaction)
        Task<int> SaveChangesAsync();

        // Transaction
        Task BeginAsync();
        Task<int> CommitAsync();
        Task RollbackAsync();
    }
}
