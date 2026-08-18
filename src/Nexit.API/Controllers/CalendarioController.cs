using Microsoft.AspNetCore.Mvc;
using Nexit.Application.DTOs.Proyectos;
using Nexit.Application.UseCases.Proyectos;

namespace Nexit.API.Controllers;

/// <summary>
/// Vista de calendario de proyectos (enero a diciembre, cualquier año) -- ver docs/07-calendario-e-
/// informes-excel.md. Cualquier usuario autenticado puede verla (igual que la lista normal de
/// proyectos): no está restringida como Informes. A propósito son 3 endpoints separados en vez de
/// uno solo que traiga "todo": pintar la grilla de un año solo pide el conteo por mes (rápido, sin
/// cargar proyectos completos); la lista de un mes específico solo se pide cuando alguien entra a
/// verlo.
/// </summary>
public class CalendarioController(IConsultarCalendarioProyectosUseCase consultar) : BaseController
{
    [HttpGet("anios")]
    public async Task<ActionResult<IReadOnlyList<int>>> GetAnios(CancellationToken ct) => Ok(await consultar.ListarAniosAsync(ct));

    [HttpGet("{anio:int}")]
    public async Task<ActionResult<CalendarioAnioDto>> GetResumenAnio(int anio, CancellationToken ct) => Ok(await consultar.ObtenerResumenAnioAsync(anio, ct));

    [HttpGet("{anio:int}/{mes:int}")]
    public async Task<ActionResult<IReadOnlyList<ProyectoCalendarioItemDto>>> GetProyectosDelMes(int anio, int mes, CancellationToken ct) => Ok(await consultar.ObtenerProyectosDelMesAsync(anio, mes, ct));
}
