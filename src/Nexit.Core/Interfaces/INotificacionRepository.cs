using Nexit.Core.Entities;

namespace Nexit.Core.Interfaces;

public interface INotificacionRepository : IRepository<Notificacion>
{
    /// <summary>Bandeja de un usuario, más recientes primero -- incluye leídas y no leídas (es el historial permanente, docs/19).</summary>
    Task<IReadOnlyList<Notificacion>> GetPorUsuarioAsync(Guid usuarioId, CancellationToken cancellationToken = default);
}
