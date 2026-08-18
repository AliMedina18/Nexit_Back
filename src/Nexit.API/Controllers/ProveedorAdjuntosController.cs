using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nexit.Application.DTOs.Proveedores;
using Nexit.Application.UseCases.Proveedores;

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

    [HttpDelete("{id:guid}"), Authorize(Policy = "AdminOrAbove")]
    public async Task<IActionResult> Delete(Guid proveedorId, Guid id, CancellationToken ct) { await adjuntos.EliminarAsync(proveedorId, id, ct); return NoContent(); }
}
