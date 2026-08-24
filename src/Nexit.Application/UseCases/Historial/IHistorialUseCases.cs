using Nexit.Application.DTOs.Historial;

namespace Nexit.Application.UseCases.Historial;

public interface IConsultarHistorialCambiosUseCase
{
    /// <summary>tipoEntidad: "proyecto" | "proveedor" | "cliente".</summary>
    Task<IReadOnlyList<HistorialCambioResponseDto>> ExecuteAsync(string tipoEntidad, Guid entidadId, CancellationToken cancellationToken = default);
}
