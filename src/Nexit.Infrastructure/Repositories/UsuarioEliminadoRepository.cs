using Microsoft.EntityFrameworkCore;
using Nexit.Core.Entities;
using Nexit.Core.Interfaces;
using Nexit.Infrastructure.Data;

namespace Nexit.Infrastructure.Repositories;

public class UsuarioEliminadoRepository(NexitDbContext context) : IUsuarioEliminadoRepository
{
    public Task AddAsync(UsuarioEliminado registro, CancellationToken cancellationToken = default) =>
        context.UsuariosEliminados.AddAsync(registro, cancellationToken).AsTask();

    public async Task<IReadOnlyDictionary<Guid, UsuarioEliminado>> GetByUsuarioIdsOriginalAsync(IReadOnlyCollection<Guid> usuarioIdsOriginal, CancellationToken cancellationToken = default)
    {
        if (usuarioIdsOriginal.Count == 0) return new Dictionary<Guid, UsuarioEliminado>();
        var registros = await context.UsuariosEliminados
            .Where(x => usuarioIdsOriginal.Contains(x.UsuarioIdOriginal))
            .OrderByDescending(x => x.FechaEliminacion)
            .ToListAsync(cancellationToken);
        // Si por algún motivo hay más de un respaldo para el mismo id original (ver el comentario de
        // UsuarioEliminado.Id), se queda con el más reciente -- ya vienen ordenados arriba.
        return registros.GroupBy(x => x.UsuarioIdOriginal).ToDictionary(g => g.Key, g => g.First());
    }
}
