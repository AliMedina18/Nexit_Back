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

    /// <summary>
    /// ¿Existe el perfil de negocio de esta cuenta y está activo? <c>null</c> si no existe (cuenta de
    /// Supabase Auth sin fila en <c>usuarios</c>, o sea alguien a mitad del registro), <c>true</c>/<c>false</c>
    /// según <c>Activo</c> si existe. Devuelve solo el booleano, sin materializar la entidad, porque
    /// PerfilRequeridoFilter la llama en CADA petición autenticada.
    /// </summary>
    Task<bool?> EstaActivoAsync(Guid id, CancellationToken cancellationToken = default);
}
