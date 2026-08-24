using Microsoft.EntityFrameworkCore;
using Nexit.Core.Entities;
using Nexit.Core.Interfaces;
using Nexit.Infrastructure.Data;

namespace Nexit.Infrastructure.Repositories;

public class NotificacionRepository(NexitDbContext context) : Repository<Notificacion>(context), INotificacionRepository
{
    public async Task<IReadOnlyList<Notificacion>> GetPorUsuarioAsync(Guid usuarioId, CancellationToken cancellationToken = default) =>
        await DbSet.AsNoTracking().Where(x => x.UsuarioDestinatarioId == usuarioId)
            .OrderByDescending(x => x.FechaCreacion).ToListAsync(cancellationToken);
}
