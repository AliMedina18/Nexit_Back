using Microsoft.EntityFrameworkCore;
using Nexit.Core.Entities;
using Nexit.Core.Interfaces;
using Nexit.Infrastructure.Data;

namespace Nexit.Infrastructure.Repositories;

public class HistorialCambioRepository(NexitDbContext context) : IHistorialCambioRepository
{
    public async Task AddAsync(HistorialCambio registro, CancellationToken cancellationToken = default) =>
        await context.HistorialCambios.AddAsync(registro, cancellationToken);

    public async Task AddRangeAsync(IEnumerable<HistorialCambio> registros, CancellationToken cancellationToken = default) =>
        await context.HistorialCambios.AddRangeAsync(registros, cancellationToken);

    public async Task<IReadOnlyList<HistorialCambio>> GetPorEntidadAsync(string tipoEntidad, Guid entidadId, CancellationToken cancellationToken = default) =>
        await context.HistorialCambios.AsNoTracking().Include(x => x.Usuario)
            .Where(x => x.TipoEntidad == tipoEntidad && x.EntidadId == entidadId)
            .OrderByDescending(x => x.Fecha).ToListAsync(cancellationToken);
}
