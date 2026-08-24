using Nexit.Application.DTOs.Clientes;

namespace Nexit.Application.UseCases.Clientes;

public interface ICrearClienteUseCase { Task<ClienteResponseDto> ExecuteAsync(CreateClienteDto input, Guid usuarioId, CancellationToken cancellationToken = default); }
public interface IActualizarClienteUseCase { Task<ClienteResponseDto> ExecuteAsync(UpdateClienteDto input, Guid usuarioId, CancellationToken cancellationToken = default); }
public interface IConsultarClientesUseCase
{
    Task<IReadOnlyList<ClienteResponseDto>> ListAsync(CancellationToken cancellationToken = default);
    Task<ClienteResponseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
public interface IEliminarClienteUseCase { Task ExecuteAsync(Guid id, Guid usuarioId, CancellationToken cancellationToken = default); }
/// <summary>"A qué cliente prestarle atención" (docs/21, docs/24) -- todos los clientes puntuados con la rúbrica de <c>PrioridadClienteCalculador</c>, de mayor a menor puntaje.</summary>
public interface IConsultarPrioridadClientesUseCase { Task<IReadOnlyList<ClientePrioridadResponseDto>> ExecuteAsync(CancellationToken cancellationToken = default); }
