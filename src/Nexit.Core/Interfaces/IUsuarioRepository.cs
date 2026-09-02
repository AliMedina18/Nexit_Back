using Nexit.Core.Entities;

namespace Nexit.Core.Interfaces;

public interface IUsuarioRepository : IRepository<Usuario>
{
    Task<bool> ExistsByEmailAsync(string email, Guid? excludedId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca por correo (sin distinguir mayúsculas/minúsculas), sin filtrar por <c>Activo</c> --
    /// usado por ConsultarEstadoCuentaUseCase (ver AuthController/docs/30) para saber, antes del
    /// login, si esa persona ya configuró su contraseña; ese use case es quien decide qué hacer con
    /// una cuenta inactiva, no este repositorio.
    /// </summary>
    Task<Usuario?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cuentas desactivadas desde antes de <paramref name="limite"/> -- candidatas a la eliminación
    /// automática de 30 días (ver EliminarUsuariosInactivosUseCase).
    /// </summary>
    Task<IReadOnlyList<Usuario>> GetInactivosDesdeAsync(DateTime limite, CancellationToken cancellationToken = default);
}
