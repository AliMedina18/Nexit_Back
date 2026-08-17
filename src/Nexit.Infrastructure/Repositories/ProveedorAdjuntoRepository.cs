using Microsoft.EntityFrameworkCore;
using Nexit.Core.Entities;
using Nexit.Core.Interfaces;
using Nexit.Infrastructure.Data;

namespace Nexit.Infrastructure.Repositories;

public class ProveedorAdjuntoRepository(NexitDbContext context) : Repository<ProveedorAdjunto>(context), IProveedorAdjuntoRepository
{
    public async Task<IReadOnlyList<ProveedorAdjunto>> GetByProveedorIdAsync(Guid proveedorId, CancellationToken cancellationToken = default) =>
        await DbSet.AsNoTracking().Where(x => x.ProveedorId == proveedorId).OrderByDescending(x => x.Fecha).ToListAsync(cancellationToken);
}
