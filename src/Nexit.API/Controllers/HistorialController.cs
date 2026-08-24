using Microsoft.AspNetCore.Mvc;
using Nexit.Application.DTOs.Historial;
using Nexit.Application.UseCases.Historial;

namespace Nexit.API.Controllers;

/// <summary>
/// Historial de cambios (docs/19) de un proyecto, proveedor o cliente -- quién cambió qué campo y
/// cuándo, tipo Google Docs/Excel. Cualquier autenticado puede consultarlo, igual que puede ver el
/// registro al que pertenece (no hay ningún dato aquí que no se pueda inferir viendo el registro
/// actual más quién lo creó/editó por última vez, que ya es visible para cualquiera).
/// </summary>
[Route("api/historial")]
public class HistorialController(IConsultarHistorialCambiosUseCase consultar) : BaseController
{
    private static readonly HashSet<string> TiposValidos = new(StringComparer.Ordinal) { "proyecto", "proveedor", "cliente" };

    [HttpGet("{tipoEntidad}/{entidadId:guid}")]
    public async Task<ActionResult<IReadOnlyList<HistorialCambioResponseDto>>> GetPorEntidad(string tipoEntidad, Guid entidadId, CancellationToken ct)
    {
        if (!TiposValidos.Contains(tipoEntidad)) return BadRequest("tipoEntidad debe ser 'proyecto', 'proveedor' o 'cliente'.");
        return Ok(await consultar.ExecuteAsync(tipoEntidad, entidadId, ct));
    }
}
