using Microsoft.EntityFrameworkCore;
using Nexit.Core.Entities;
using Nexit.Core.Interfaces;
using Nexit.Infrastructure.Data;

namespace Nexit.Infrastructure.Repositories;

public class ProveedorRepository(NexitDbContext context) : Repository<Proveedor>(context), IProveedorRepository
{
    public Task<bool> ExistsByEmailAsync(string email, Guid? excludedId = null, CancellationToken cancellationToken = default) =>
        DbSet.AnyAsync(x => x.Email != null && x.Email.ToLower() == email.ToLower() && (!excludedId.HasValue || x.Id != excludedId), cancellationToken);
    public override Task<Proveedor?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        DbSet.Include(x => x.Telefonos).Include(x => x.Servicios).Include(x => x.Colaboradores).ThenInclude(x => x.Usuario).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    // Include(x => x.Proyectos).ThenInclude(pp => pp.Proyecto) agregado para el endpoint de prioridad
    // (docs/24) -- necesita el CreatedAt del proyecto más reciente que usó a cada proveedor.
    public override async Task<IReadOnlyList<Proveedor>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await DbSet.AsNoTracking().Include(x => x.Telefonos).Include(x => x.Servicios).Include(x => x.Colaboradores).ThenInclude(x => x.Usuario)
            .Include(x => x.Proyectos).ThenInclude(pp => pp.Proyecto).OrderBy(x => x.Nombre).ToListAsync(cancellationToken);
}
