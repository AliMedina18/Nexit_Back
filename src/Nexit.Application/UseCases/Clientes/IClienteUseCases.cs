using Nexit.Application.DTOs.Clientes;

namespace Nexit.Application.UseCases.Clientes;

public interface ICrearClienteUseCase { Task<ClienteResponseDto> ExecuteAsync(CreateClienteDto input, Guid usuarioId, CancellationToken cancellationToken = default); }
public interface IActualizarClienteUseCase { Task<ClienteResponseDto> ExecuteAsync(UpdateClienteDto input, Guid usuarioId, CancellationToken cancellationToken = default); }
public interface IConsultarClientesUseCase
{
    Task<IReadOnlyList<ClienteResponseDto>> ListAsync(CancellationToken cancellationToken = default);
    Task<ClienteResponseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
public interface IEliminarClienteUseCase { Task ExecuteAsync(Guid id, CancellationToken cancellationToken = default); }
