using Nexit.Core.Entities;

namespace Nexit.Core.Interfaces;

public interface IHistorialCambioRepository
{
    Task AddAsync(HistorialCambio registro, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<HistorialCambio> registros, CancellationToken cancellationToken = default);

    /// <summary>Historial de un registro puntual (proyecto/proveedor/cliente), más reciente primero.</summary>
    Task<IReadOnlyList<HistorialCambio>> GetPorEntidadAsync(string tipoEntidad, Guid entidadId, CancellationToken cancellationToken = default);
}
