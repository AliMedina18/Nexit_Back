using Microsoft.EntityFrameworkCore;
using Nexit.Core.Entities;
using Nexit.Core.Interfaces;
using Nexit.Infrastructure.Data;

namespace Nexit.Infrastructure.Repositories;

public class ProyectoRepository(NexitDbContext context) : Repository<Proyecto>(context), IProyectoRepository
{
    public override Task<Proyecto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        DbSet.Include(x => x.Equipo).Include(x => x.Proveedores).Include(x => x.Seguimiento)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public override async Task<IReadOnlyList<Proyecto>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await DbSet.AsNoTracking().Include(x => x.Equipo).Include(x => x.Proveedores).Include(x => x.Seguimiento)
            .OrderByDescending(x => x.FechaEvento).ThenBy(x => x.Nombre).ToListAsync(cancellationToken);
}
