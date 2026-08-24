using Nexit.Application.DTOs.Proyectos;

namespace Nexit.Application.UseCases.Proyectos;

public interface ICrearProyectoUseCase { Task<ProyectoResponseDto> ExecuteAsync(CrearProyectoDto input, Guid usuarioId, string? usuarioRol, CancellationToken cancellationToken = default); }
public interface IActualizarProyectoUseCase { Task<ProyectoResponseDto> ExecuteAsync(ActualizarProyectoDto input, Guid usuarioId, string? usuarioRol, CancellationToken cancellationToken = default); }
public interface IConsultarProyectosUseCase { Task<IReadOnlyList<ProyectoResponseDto>> ListAsync(CancellationToken cancellationToken = default); Task<ProyectoResponseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default); }
public interface IEliminarProyectoUseCase { Task ExecuteAsync(Guid id, Guid usuarioId, CancellationToken cancellationToken = default); }
public interface IAgregarSeguimientoProyectoUseCase { Task<SeguimientoProyectoDto> ExecuteAsync(Guid proyectoId, CrearSeguimientoProyectoDto input, Guid usuarioId, CancellationToken cancellationToken = default); }

/// <summary>"A qué proyecto atender primero" (docs/21, docs/22) -- proyectos activos (no finalizados/cancelados/facturados) puntuados con la rúbrica de <c>PrioridadProyectoCalculador</c>, de mayor a menor puntaje.</summary>
public interface IConsultarPrioridadProyectosUseCase { Task<IReadOnlyList<ProyectoPrioridadResponseDto>> ExecuteAsync(CancellationToken cancellationToken = default); }
