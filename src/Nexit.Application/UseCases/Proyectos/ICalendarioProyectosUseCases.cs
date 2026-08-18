using Nexit.Application.DTOs.Proyectos;

namespace Nexit.Application.UseCases.Proyectos;

public interface IConsultarCalendarioProyectosUseCase
{
    /// <summary>Años que tienen al menos un proyecto con fecha_evento -- para el selector de año del calendario.</summary>
    Task<IReadOnlyList<int>> ListarAniosAsync(CancellationToken cancellationToken = default);

    /// <summary>Resumen de un año completo (enero a diciembre) con el conteo de proyectos por mes.</summary>
    Task<CalendarioAnioDto> ObtenerResumenAnioAsync(int anio, CancellationToken cancellationToken = default);

    /// <summary>Proyectos (proyección liviana) de un mes específico -- se piden solo cuando alguien entra a ver ese mes.</summary>
    Task<IReadOnlyList<ProyectoCalendarioItemDto>> ObtenerProyectosDelMesAsync(int anio, int mes, CancellationToken cancellationToken = default);
}
