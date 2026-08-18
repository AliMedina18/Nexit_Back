using Microsoft.EntityFrameworkCore;
using Nexit.Core.Entities;
using Nexit.Core.Interfaces;
using Nexit.Infrastructure.Data;

namespace Nexit.Infrastructure.Repositories;

public class UsuarioRepository(NexitDbContext context) : Repository<Usuario>(context), IUsuarioRepository
{
    public Task<bool> ExistsByEmailAsync(string email, Guid? excludedId = null, CancellationToken cancellationToken = default) =>
        DbSet.AnyAsync(x => x.Email.ToLower() == email.ToLower() && (!excludedId.HasValue || x.Id != excludedId), cancellationToken);
    public override async Task<IReadOnlyList<Usuario>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await DbSet.AsNoTracking().OrderBy(x => x.Nombre).ToListAsync(cancellationToken);
}
