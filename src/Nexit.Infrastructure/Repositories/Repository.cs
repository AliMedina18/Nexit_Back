using Microsoft.EntityFrameworkCore;
using Nexit.Core.Interfaces;
using Nexit.Infrastructure.Data;

namespace Nexit.Infrastructure.Repositories;

public class Repository<T>(NexitDbContext context) : IRepository<T> where T : class
{
    protected readonly NexitDbContext Context = context;
    protected readonly DbSet<T> DbSet = context.Set<T>();
    public virtual Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => DbSet.FindAsync([id], cancellationToken).AsTask();
    public virtual async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default) => await DbSet.AsNoTracking().ToListAsync(cancellationToken);
    public Task AddAsync(T entity, CancellationToken cancellationToken = default) => DbSet.AddAsync(entity, cancellationToken).AsTask();
    public void Update(T entity) => DbSet.Update(entity);
    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(id, cancellationToken);
        if (entity is not null) DbSet.Remove(entity);
    }
}
