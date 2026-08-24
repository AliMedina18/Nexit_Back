using Nexit.Core.Entities;

namespace Nexit.Core.Interfaces;

public interface IUsuarioRepository : IRepository<Usuario>
{
    Task<bool> ExistsByEmailAsync(string email, Guid? excludedId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cuentas desactivadas desde antes de <paramref name="limite"/> -- candidatas a la eliminación
    /// automática de 30 días (ver EliminarUsuariosInactivosUseCase).
    /// </summary>
    Task<IReadOnlyList<Usuario>> GetInactivosDesdeAsync(DateTime limite, CancellationToken cancellationToken = default);
}
