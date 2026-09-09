using Nexit.Application.DTOs.Usuarios;

namespace Nexit.Application.UseCases.Usuarios;

public interface ICrearUsuarioUseCase { Task<UsuarioResponseDto> ExecuteAsync(CreateUsuarioDto input, CancellationToken cancellationToken = default); }
/// <summary>
/// Crea la cuenta de acceso en Supabase Auth Y el perfil de negocio, en un solo paso -- ver
/// <see cref="Nexit.Core.Interfaces.ISupabaseAuthAdminService.CrearCuentaAsync"/>. La persona entra
/// la primera vez con el código de un solo uso del login y ahí crea su contraseña, igual que quien
/// llega por invitación.
/// </summary>
public interface IRegistrarUsuarioUseCase { Task<UsuarioResponseDto> ExecuteAsync(RegistrarUsuarioDto input, Guid callerId, CancellationToken cancellationToken = default); }

public interface IActualizarUsuarioUseCase { Task<UsuarioResponseDto> ExecuteAsync(Guid id, UpdateUsuarioDto input, Guid callerId, CancellationToken cancellationToken = default); }
public interface IConsultarUsuariosUseCase
{
    Task<IReadOnlyList<UsuarioResponseDto>> ListAsync(CancellationToken cancellationToken = default);
    Task<UsuarioResponseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
// IEliminarUsuarioUseCase se eliminó el 2026-09-08 (docs/40): ya no hay borrado directo de personas.
// Lo hace AprobarComoAdminUseCase al aprobar una solicitud de tipo "usuario", y el proceso automático
// de los 30 días vía IEliminarUsuariosInactivosUseCase.

/// <summary>
/// Barrido automático (ver docs/17-eliminacion-automatica-usuarios.md): elimina a quien lleva 30
/// días o más desactivado, dejando un respaldo en `usuarios_eliminados` antes de borrar. La dispara
/// el background service, no un endpoint HTTP -- por eso no recibe un callerId como la eliminación manual.
/// </summary>
public interface IEliminarUsuariosInactivosUseCase
{
    /// <returns>Cuántas cuentas se eliminaron en esta corrida.</returns>
    Task<int> ExecuteAsync(CancellationToken cancellationToken = default);
}
