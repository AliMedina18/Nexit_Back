using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nexit.Application.DTOs.Catalogos;
using Nexit.Application.UseCases.Catalogos;

namespace Nexit.API.Controllers;

public class CatalogosController(ICatalogosService catalogos) : BaseController
{
    [HttpGet("paises")]
    public Task<IReadOnlyList<PaisDto>> GetPaises(CancellationToken ct) => catalogos.GetPaisesAsync(ct);
    [HttpPost("paises"), Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<PaisDto>> CrearPais(CrearPaisDto dto, CancellationToken ct) => CreatedAtAction(nameof(GetPaises), await catalogos.CrearPaisAsync(dto, ct));
    [HttpPut("paises/{id:guid}"), Authorize(Policy = "AdminOnly")]
    public Task<PaisDto> ActualizarPais(Guid id, CrearPaisDto dto, CancellationToken ct) => catalogos.ActualizarPaisAsync(id, dto, ct);

    [HttpGet("regiones")]
    public Task<IReadOnlyList<RegionDto>> GetRegiones([FromQuery] Guid paisId, CancellationToken ct) => catalogos.GetRegionesAsync(paisId, ct);
    [HttpPost("regiones"), Authorize(Policy = "AdminOnly")]
    public Task<RegionDto> CrearRegion(CrearRegionDto dto, CancellationToken ct) => catalogos.CrearRegionAsync(dto, ct);
    [HttpPut("regiones/{id:guid}"), Authorize(Policy = "AdminOnly")]
    public Task<RegionDto> ActualizarRegion(Guid id, CrearRegionDto dto, CancellationToken ct) => catalogos.ActualizarRegionAsync(id, dto, ct);

    [HttpGet("ciudades")]
    public Task<IReadOnlyList<CiudadDto>> GetCiudades([FromQuery] Guid regionId, CancellationToken ct) => catalogos.GetCiudadesAsync(regionId, ct);
    [HttpPost("ciudades"), Authorize(Policy = "AdminOnly")]
    public Task<CiudadDto> CrearCiudad(CrearCiudadDto dto, CancellationToken ct) => catalogos.CrearCiudadAsync(dto, ct);
    [HttpPut("ciudades/{id:guid}"), Authorize(Policy = "AdminOnly")]
    public Task<CiudadDto> ActualizarCiudad(Guid id, CrearCiudadDto dto, CancellationToken ct) => catalogos.ActualizarCiudadAsync(id, dto, ct);

    [HttpGet("categorias-proveedor")]
    public Task<IReadOnlyList<ItemCatalogoDto>> GetCategorias(CancellationToken ct) => catalogos.GetCategoriasAsync(ct);
    [HttpPost("categorias-proveedor"), Authorize(Policy = "AdminOnly")]
    public Task<ItemCatalogoDto> CrearCategoria(NombreDto dto, CancellationToken ct) => catalogos.CrearCategoriaAsync(dto, ct);
    [HttpPut("categorias-proveedor/{id:guid}"), Authorize(Policy = "AdminOnly")]
    public Task<ItemCatalogoDto> ActualizarCategoria(Guid id, NombreDto dto, CancellationToken ct) => catalogos.ActualizarCategoriaAsync(id, dto, ct);

    [HttpGet("servicios")]
    public Task<IReadOnlyList<ItemCatalogoDto>> GetServicios(CancellationToken ct) => catalogos.GetServiciosAsync(ct);
    [HttpPost("servicios"), Authorize(Policy = "AdminOnly")]
    public Task<ItemCatalogoDto> CrearServicio(NombreDto dto, CancellationToken ct) => catalogos.CrearServicioAsync(dto, ct);
    [HttpPut("servicios/{id:guid}"), Authorize(Policy = "AdminOnly")]
    public Task<ItemCatalogoDto> ActualizarServicio(Guid id, NombreDto dto, CancellationToken ct) => catalogos.ActualizarServicioAsync(id, dto, ct);

    [HttpGet("fases-proyecto")]
    public Task<IReadOnlyList<FaseProyectoDto>> GetFases(CancellationToken ct) => catalogos.GetFasesAsync(ct);
    [HttpPut("fases-proyecto/{fase:int}"), Authorize(Policy = "AdminOnly")]
    public Task<FaseProyectoDto> ActualizarFase(short fase, NombreDto dto, CancellationToken ct) => catalogos.ActualizarFaseAsync(fase, dto, ct);

    [HttpGet("estados-proyecto")]
    public Task<IReadOnlyList<EstadoProyectoDto>> GetEstados([FromQuery] short? fase, CancellationToken ct) => catalogos.GetEstadosAsync(fase, ct);
    [HttpPost("estados-proyecto"), Authorize(Policy = "AdminOnly")]
    public Task<EstadoProyectoDto> CrearEstado(CrearEstadoProyectoDto dto, CancellationToken ct) => catalogos.CrearEstadoAsync(dto, ct);
    [HttpPut("estados-proyecto/{id:guid}"), Authorize(Policy = "AdminOnly")]
    public Task<EstadoProyectoDto> ActualizarEstado(Guid id, CrearEstadoProyectoDto dto, CancellationToken ct) => catalogos.ActualizarEstadoAsync(id, dto, ct);

    [HttpDelete("{tipo}/{id:guid}"), Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Eliminar(string tipo, Guid id, CancellationToken ct)
    {
        await catalogos.EliminarAsync(tipo, id, ct);
        return NoContent();
    }
}
