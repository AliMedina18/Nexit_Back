using Nexit.Application.DTOs.Proyectos;

namespace Nexit.Application.UseCases.Proyectos;

public interface ICrearProyectoUseCase { Task<ProyectoResponseDto> ExecuteAsync(CrearProyectoDto input, Guid usuarioId, CancellationToken cancellationToken = default); }
public interface IActualizarProyectoUseCase { Task<ProyectoResponseDto> ExecuteAsync(ActualizarProyectoDto input, CancellationToken cancellationToken = default); }
public interface IConsultarProyectosUseCase { Task<IReadOnlyList<ProyectoResponseDto>> ListAsync(CancellationToken cancellationToken = default); Task<ProyectoResponseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default); }
public interface IEliminarProyectoUseCase { Task ExecuteAsync(Guid id, CancellationToken cancellationToken = default); }
public interface IAgregarSeguimientoProyectoUseCase { Task<SeguimientoProyectoDto> ExecuteAsync(Guid proyectoId, CrearSeguimientoProyectoDto input, Guid usuarioId, CancellationToken cancellationToken = default); }
