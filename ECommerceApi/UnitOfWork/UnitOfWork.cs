using ECommerceApi.Data;
using ECommerceApi.Repositories.Base;
using ECommerceApi.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;

namespace ECommerceApi.UnitOfWork
{
    public class UnitOfWork(AppDbContext db) : IUnitOfWork
    {
        private readonly Dictionary<Type, object> _repositories = [];
        private IDbContextTransaction? _transaction;

        public IRepository<T> Repository<T>() where T : class
        {
            if (_repositories.TryGetValue(typeof(T), out var existing))
                return (IRepository<T>)existing;

            var repository = new Repository<T>(db);
            _repositories[typeof(T)] = repository;
            return repository;
        }

        // Lưu thay đổi nhưng KHÔNG commit transaction (để lấy Id giữa chừng)
        public Task<int> SaveChangesAsync() => db.SaveChangesAsync();

        // Transaction methods
        public async Task BeginAsync() =>
            _transaction = await db.Database.BeginTransactionAsync();

        public async Task<int> CommitAsync()
        {
            var rows = await db.SaveChangesAsync();
            if (_transaction is not null)
                await _transaction.CommitAsync();
            return rows;
        }

        public async Task RollbackAsync()
        {
            if (_transaction is not null)
                await _transaction.RollbackAsync();
        }

        public async ValueTask DisposeAsync()
        {
            if (_transaction is not null)
                await _transaction.DisposeAsync();
            await db.DisposeAsync();
        }
    }
}
