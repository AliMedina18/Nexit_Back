using Nexit.Application.DTOs.Usuarios;

namespace Nexit.Application.UseCases.Usuarios;

public interface ICrearUsuarioUseCase { Task<UsuarioResponseDto> ExecuteAsync(CreateUsuarioDto input, CancellationToken cancellationToken = default); }
public interface IActualizarUsuarioUseCase { Task<UsuarioResponseDto> ExecuteAsync(Guid id, UpdateUsuarioDto input, Guid callerId, CancellationToken cancellationToken = default); }
public interface IConsultarUsuariosUseCase
{
    Task<IReadOnlyList<UsuarioResponseDto>> ListAsync(CancellationToken cancellationToken = default);
    Task<UsuarioResponseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
public interface IEliminarUsuarioUseCase { Task ExecuteAsync(Guid id, Guid callerId, CancellationToken cancellationToken = default); }

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
