using Microsoft.EntityFrameworkCore;
using LiveNow.CRM.Core.Interfaces;
using LiveNow.CRM.Infrastructure.Data;

namespace LiveNow.CRM.Infrastructure.Repositories;

public class Repository<T> : IRepository<T>
    where T : class
{
    private readonly LiveNowDbContext _context;

    public Repository(LiveNowDbContext context)
    {
        _context = context;
    }

    public async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Set<T>().FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Set<T>().ToListAsync(cancellationToken);
    }

    public async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await _context.Set<T>().AddAsync(entity, cancellationToken);
        return entity;
    }

    public async Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        _context.Set<T>().Update(entity);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
    {
        _context.Set<T>().Remove(entity);
        await Task.CompletedTask;
    }

    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Set<T>().CountAsync(cancellationToken);
    }
}