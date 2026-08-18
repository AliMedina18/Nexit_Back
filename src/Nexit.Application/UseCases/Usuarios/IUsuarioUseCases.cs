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
