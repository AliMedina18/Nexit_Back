using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nexit.Application.DTOs.Informes;
using Nexit.Application.Services;
using Nexit.Application.UseCases.Informes;

namespace Nexit.API.Controllers;

/// <summary>
/// Informes semanal/mensual — exclusivo de administradores (ver docs/06-modelo-permisos-roles.md):
/// solo super_admin y admin pueden ver o exportar esta información, gerentes y miembros no.
/// </summary>
[Authorize(Policy = "AdminOrAbove")]
public class InformesController(IConsultarInformesUseCase consultar, IGenerarInformeSnapshotUseCase generar, IInformeExcelExporter exportador) : BaseController
{
    [HttpGet("resumen")]
    public async Task<ActionResult<InformeResumenDto>> GetResumen(CancellationToken ct) => Ok(await consultar.ObtenerResumenAsync(ct));

    [HttpGet("resumen/exportar")]
    public async Task<IActionResult> ExportarResumen(CancellationToken ct)
    {
        var datos = await consultar.ObtenerResumenAsync(ct);
        var bytes = exportador.Exportar($"Informe general — {DateTime.UtcNow:yyyy-MM-dd}", datos);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"informe-general-{DateTime.UtcNow:yyyyMMdd}.xlsx");
    }

    [HttpGet("snapshots/{tipo}/{periodoKey}")]
    public async Task<ActionResult<InformeSnapshotDto>> GetSnapshot(string tipo, string periodoKey, CancellationToken ct) => Ok(await consultar.ObtenerSnapshotAsync(tipo, periodoKey, ct));

    [HttpGet("snapshots/{tipo}/{periodoKey}/exportar")]
    public async Task<IActionResult> ExportarSnapshot(string tipo, string periodoKey, CancellationToken ct)
    {
        var snapshot = await consultar.ObtenerSnapshotAsync(tipo, periodoKey, ct);
        var bytes = exportador.Exportar($"Informe {tipo} — {periodoKey}", snapshot);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"informe-{tipo}-{periodoKey}.xlsx");
    }

    [HttpPost("snapshots")]
    public async Task<ActionResult<InformeSnapshotDto>> CrearSnapshot(CrearInformeSnapshotDto dto, CancellationToken ct)
    {
        var userId = GetUserId(); if (userId == Guid.Empty) return Unauthorized();
        return Ok(await generar.ExecuteAsync(dto, userId, ct));
    }
}
