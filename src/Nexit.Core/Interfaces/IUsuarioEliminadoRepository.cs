using Nexit.Core.Entities;

namespace Nexit.Core.Interfaces;

/// <summary>Respaldo de usuarios eliminados -- ver UsuarioEliminado.</summary>
public interface IUsuarioEliminadoRepository
{
    Task AddAsync(UsuarioEliminado registro, CancellationToken cancellationToken = default);

    /// <summary>
    /// Un respaldo por cada id original que sí se encontró (el más reciente, si por algún motivo hay
    /// más de uno). Se usa para completar el nombre de solicitudes de eliminación de cuentas viejas,
    /// creadas antes de que SolicitudEliminacion guardara su propia fotografía del nombre.
    /// </summary>
    Task<IReadOnlyDictionary<Guid, UsuarioEliminado>> GetByUsuarioIdsOriginalAsync(IReadOnlyCollection<Guid> usuarioIdsOriginal, CancellationToken cancellationToken = default);
}
