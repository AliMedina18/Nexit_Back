using Microsoft.EntityFrameworkCore;
using Nexit.Core.Entities;
using Nexit.Core.Interfaces;
using Nexit.Infrastructure.Data;

namespace Nexit.Infrastructure.Repositories;

public class ClienteRepository(NexitDbContext context) : Repository<Cliente>(context), IClienteRepository
{
    public Task<bool> ExistsByEmailAsync(string email, Guid? excludedId = null, CancellationToken cancellationToken = default) =>
        DbSet.AnyAsync(x => x.Email != null && x.Email.ToLower() == email.ToLower() && (!excludedId.HasValue || x.Id != excludedId), cancellationToken);
    public Task<Cliente?> GetByEmailAsync(string email, CancellationToken cancellationToken = default) =>
        DbSet.Include(x => x.Telefonos).FirstOrDefaultAsync(x => x.Email != null && x.Email.ToLower() == email.ToLower(), cancellationToken);
    public override Task<Cliente?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        DbSet.Include(x => x.Telefonos).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    public override async Task<IReadOnlyList<Cliente>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await DbSet.AsNoTracking().Include(x => x.Telefonos).OrderBy(x => x.Nombre).ToListAsync(cancellationToken);
}
