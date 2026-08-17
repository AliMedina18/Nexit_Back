using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nexit.Application.DTOs.Informes;
using Nexit.Application.UseCases.Informes;

namespace Nexit.API.Controllers;

public class InformesController(IConsultarInformesUseCase consultar, IGenerarInformeSnapshotUseCase generar) : BaseController
{
    [HttpGet("resumen")]
    public async Task<ActionResult<InformeResumenDto>> GetResumen(CancellationToken ct) => Ok(await consultar.ObtenerResumenAsync(ct));

    [HttpGet("snapshots/{tipo}/{periodoKey}")]
    public async Task<ActionResult<InformeSnapshotDto>> GetSnapshot(string tipo, string periodoKey, CancellationToken ct) => Ok(await consultar.ObtenerSnapshotAsync(tipo, periodoKey, ct));

    [HttpPost("snapshots"), Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<InformeSnapshotDto>> CrearSnapshot(CrearInformeSnapshotDto dto, CancellationToken ct)
    {
        var userId = GetUserId(); if (userId == Guid.Empty) return Unauthorized();
        return Ok(await generar.ExecuteAsync(dto, userId, ct));
    }
}
