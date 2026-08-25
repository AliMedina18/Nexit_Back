using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nexit.Application.DTOs.Presencia;
using Nexit.Application.UseCases.Presencia;

namespace Nexit.API.Controllers;

/// <summary>
/// Presencia en vivo (HU-12, docs/29): quién tiene el sistema abierto ahora mismo. Cualquier persona
/// autenticada puede hacer "ping" (registrar que sigue activa), pero solo admin/super_admin pueden
/// consultar el directorio completo -- ver la matriz de HU-12.
/// </summary>
public class PresenciaController(IRegistrarPresenciaUseCase ping, IConsultarPresenciaUseCase consultar) : BaseController
{
    /// <summary>El frontend llama esto cada 45-60 segundos mientras haya una sesión abierta (docs/29).</summary>
    [HttpPost("ping")]
    public async Task<IActionResult> Ping(CancellationToken ct)
    {
        await ping.ExecuteAsync(GetUserId(), ct);
        return NoContent();
    }

    [HttpGet, Authorize(Policy = "AdminOrAbove")]
    public async Task<ActionResult<IReadOnlyList<PresenciaResponseDto>>> Get(CancellationToken ct) => Ok(await consultar.ExecuteAsync(ct));
}
