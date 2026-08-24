using Nexit.Application.DTOs.Proveedores;

namespace Nexit.Application.UseCases.Proveedores;

public interface ICrearProveedorUseCase { Task<ProveedorResponseDto> ExecuteAsync(CreateProveedorDto input, Guid usuarioId, CancellationToken cancellationToken = default); }
public interface IActualizarProveedorUseCase { Task<ProveedorResponseDto> ExecuteAsync(UpdateProveedorDto input, Guid usuarioId, CancellationToken cancellationToken = default); }
public interface IConsultarProveedoresUseCase { Task<IReadOnlyList<ProveedorResponseDto>> ListAsync(CancellationToken cancellationToken = default); Task<ProveedorResponseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default); }
public interface IEliminarProveedorUseCase { Task ExecuteAsync(Guid id, Guid usuarioId, CancellationToken cancellationToken = default); }
