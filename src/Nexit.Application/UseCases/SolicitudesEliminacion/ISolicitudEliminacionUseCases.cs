using Nexit.Application.DTOs.SolicitudesEliminacion;

namespace Nexit.Application.UseCases.SolicitudesEliminacion;

public interface ISolicitarEliminacionUseCase { Task<SolicitudEliminacionResponseDto> ExecuteAsync(CrearSolicitudEliminacionDto input, Guid solicitanteId, CancellationToken cancellationToken = default); }
public interface IAprobarComoGerenteUseCase { Task<SolicitudEliminacionResponseDto> ExecuteAsync(Guid solicitudId, Guid gerenteId, CancellationToken cancellationToken = default); }
public interface IRechazarComoGerenteUseCase { Task<SolicitudEliminacionResponseDto> ExecuteAsync(Guid solicitudId, Guid gerenteId, RevisionSolicitudDto input, CancellationToken cancellationToken = default); }
public interface IAprobarComoAdminUseCase { Task<SolicitudEliminacionResponseDto> ExecuteAsync(Guid solicitudId, Guid adminId, RevisionSolicitudDto input, CancellationToken cancellationToken = default); }
public interface IRechazarComoAdminUseCase { Task<SolicitudEliminacionResponseDto> ExecuteAsync(Guid solicitudId, Guid adminId, RevisionSolicitudDto input, CancellationToken cancellationToken = default); }
public interface IConsultarSolicitudesEliminacionUseCase
{
    Task<IReadOnlyList<SolicitudEliminacionResponseDto>> ListAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SolicitudEliminacionResponseDto>> ListPendientesParaGerenteAsync(Guid gerenteId, CancellationToken cancellationToken = default);
    Task<SolicitudEliminacionResponseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
