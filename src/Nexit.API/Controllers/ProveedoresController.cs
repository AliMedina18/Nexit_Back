using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nexit.Application.DTOs.Proveedores;
using Nexit.Application.UseCases.Proveedores;

namespace Nexit.API.Controllers;

public class ProveedoresController(
    ICrearProveedorUseCase crear, IActualizarProveedorUseCase actualizar, IConsultarProveedoresUseCase consultar, IEliminarProveedorUseCase eliminar,
    IMarcarColaboradorProveedorUseCase marcarColaborador, IQuitarColaboradorProveedorUseCase quitarColaborador, IListarMisProveedoresUseCase listarMios,
    IValidator<CreateProveedorDto> createValidator, IValidator<UpdateProveedorDto> updateValidator) : BaseController
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProveedorResponseDto>>> GetAll(CancellationToken ct) => Ok(await consultar.ListAsync(ct));

    // Antes de "{id:guid}" a propósito -- si no, ASP.NET Core intenta parsear "mios" como Guid y falla con 404.
    [HttpGet("mios")]
    public async Task<ActionResult<IReadOnlyList<ProveedorResponseDto>>> GetMisProveedores(CancellationToken ct) => Ok(await listarMios.ExecuteAsync(GetUserId(), ct));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProveedorResponseDto>> GetById(Guid id, CancellationToken ct) => Ok(await consultar.GetByIdAsync(id, ct));

    /// <summary>"Estoy trabajando con este proveedor" (docs/19) -- cada quien se marca a sí mismo, no un administrador.</summary>
    [HttpPost("{id:guid}/colaboradores")]
    public async Task<IActionResult> MarcarColaborador(Guid id, CancellationToken ct)
    {
        await marcarColaborador.ExecuteAsync(id, GetUserId(), ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}/colaboradores")]
    public async Task<IActionResult> QuitarColaborador(Guid id, CancellationToken ct)
    {
        await quitarColaborador.ExecuteAsync(id, GetUserId(), ct);
        return NoContent();
    }

    [HttpPost]
    public async Task<ActionResult<ProveedorResponseDto>> Create(CreateProveedorDto dto, CancellationToken ct)
    {
        var validation = await createValidator.ValidateAsync(dto, ct);
        if (!validation.IsValid) return BadRequest(new ValidationProblemDetails(validation.ToDictionary()));
        var userId = GetUserId();
        if (userId == Guid.Empty) return Unauthorized();
        var result = await crear.ExecuteAsync(dto, userId, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ProveedorResponseDto>> Update(Guid id, UpdateProveedorDto dto, CancellationToken ct)
    {
        dto.Id = id;
        var validation = await updateValidator.ValidateAsync(dto, ct);
        if (!validation.IsValid) return BadRequest(new ValidationProblemDetails(validation.ToDictionary()));
        var userId = GetUserId(); if (userId == Guid.Empty) return Unauthorized();
        return Ok(await actualizar.ExecuteAsync(dto, userId, ct));
    }

    [HttpDelete("{id:guid}"), Authorize(Policy = "AdminOrAbove")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await eliminar.ExecuteAsync(id, GetUserId(), ct);
        return NoContent();
    }
}
