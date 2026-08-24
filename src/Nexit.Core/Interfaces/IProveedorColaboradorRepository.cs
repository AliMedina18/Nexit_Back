using Nexit.Core.Entities;

namespace Nexit.Core.Interfaces;

public interface IProveedorColaboradorRepository
{
    Task<bool> ExisteAsync(Guid proveedorId, Guid usuarioId, CancellationToken cancellationToken = default);
    Task AgregarAsync(Guid proveedorId, Guid usuarioId, CancellationToken cancellationToken = default);
    Task QuitarAsync(Guid proveedorId, Guid usuarioId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProveedorColaborador>> GetPorProveedorAsync(Guid proveedorId, CancellationToken cancellationToken = default);

    /// <summary>Los proveedores donde este usuario se marcó "trabajando con este proveedor" -- la sección "mis proveedores" (docs/19).</summary>
    Task<IReadOnlyList<Guid>> GetProveedorIdsPorUsuarioAsync(Guid usuarioId, CancellationToken cancellationToken = default);
}
