using Microsoft.EntityFrameworkCore;
using Nexit.Core.Entities;
using Nexit.Core.Interfaces;
using Nexit.Infrastructure.Data;

namespace Nexit.Infrastructure.Repositories;

public class ProveedorColaboradorRepository(NexitDbContext context) : IProveedorColaboradorRepository
{
    public Task<bool> ExisteAsync(Guid proveedorId, Guid usuarioId, CancellationToken cancellationToken = default) =>
        context.ProveedorColaboradores.AsNoTracking().AnyAsync(x => x.ProveedorId == proveedorId && x.UsuarioId == usuarioId, cancellationToken);

    public async Task AgregarAsync(Guid proveedorId, Guid usuarioId, CancellationToken cancellationToken = default) =>
        await context.ProveedorColaboradores.AddAsync(new ProveedorColaborador { ProveedorId = proveedorId, UsuarioId = usuarioId }, cancellationToken);

    public async Task QuitarAsync(Guid proveedorId, Guid usuarioId, CancellationToken cancellationToken = default)
    {
        var registro = await context.ProveedorColaboradores.FirstOrDefaultAsync(x => x.ProveedorId == proveedorId && x.UsuarioId == usuarioId, cancellationToken);
        if (registro is not null) context.ProveedorColaboradores.Remove(registro);
    }

    public async Task<IReadOnlyList<ProveedorColaborador>> GetPorProveedorAsync(Guid proveedorId, CancellationToken cancellationToken = default) =>
        await context.ProveedorColaboradores.AsNoTracking().Include(x => x.Usuario)
            .Where(x => x.ProveedorId == proveedorId).OrderBy(x => x.FechaAgregado).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Guid>> GetProveedorIdsPorUsuarioAsync(Guid usuarioId, CancellationToken cancellationToken = default) =>
        await context.ProveedorColaboradores.AsNoTracking().Where(x => x.UsuarioId == usuarioId).Select(x => x.ProveedorId).ToListAsync(cancellationToken);
}
