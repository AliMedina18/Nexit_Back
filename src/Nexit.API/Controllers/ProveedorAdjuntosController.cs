using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nexit.Application.DTOs.Proveedores;
using Nexit.Application.UseCases.Proveedores;
using Nexit.Core.Exceptions;

namespace Nexit.API.Controllers;

[ApiController]
[Authorize]
[Route("api/proveedores/{proveedorId:guid}/adjuntos")]
public class ProveedorAdjuntosController(IProveedorAdjuntosUseCase adjuntos) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProveedorAdjuntoDto>>> GetAll(Guid proveedorId, CancellationToken ct) => Ok(await adjuntos.ListAsync(proveedorId, ct));

    [HttpPost]
    public async Task<ActionResult<ProveedorAdjuntoDto>> Create(Guid proveedorId, CrearProveedorAdjuntoDto dto, CancellationToken ct) => Ok(await adjuntos.CrearAsync(proveedorId, dto, ct));

    /// <summary>Sube un archivo real (docs/28) -- solo PDF/Excel, máximo 20 MB (mismo límite del bucket de Supabase Storage).</summary>
    [HttpPost("subir")]
    [RequestSizeLimit(20 * 1024 * 1024)]
    public async Task<ActionResult<ProveedorAdjuntoDto>> Subir(Guid proveedorId, IFormFile? archivo, CancellationToken ct)
    {
        if (archivo is null || archivo.Length == 0) throw new BusinessRuleException("Debes adjuntar un archivo.");
        await using var contenido = archivo.OpenReadStream();
        var resultado = await adjuntos.SubirAsync(proveedorId, archivo.FileName, archivo.ContentType, archivo.Length, contenido, ct);
        return Ok(resultado);
    }

    /// <summary>Devuelve la URL para descargar este adjunto (el link tal cual, o una URL firmada temporal si es un archivo real en Storage) -- el frontend abre esa URL directamente, este endpoint no transmite el archivo.</summary>
    [HttpGet("{id:guid}/descargar")]
    public async Task<ActionResult> Descargar(Guid proveedorId, Guid id, CancellationToken ct) => Ok(new { url = await adjuntos.ObtenerUrlDescargaAsync(proveedorId, id, ct) });

    [HttpDelete("{id:guid}"), Authorize(Policy = "AdminOrAbove")]
    public async Task<IActionResult> Delete(Guid proveedorId, Guid id, CancellationToken ct) { await adjuntos.EliminarAsync(proveedorId, id, ct); return NoContent(); }
}
