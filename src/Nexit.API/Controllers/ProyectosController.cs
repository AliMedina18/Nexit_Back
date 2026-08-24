using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nexit.Application.DTOs.Proyectos;
using Nexit.Application.UseCases.Proyectos;

namespace Nexit.API.Controllers;

public class ProyectosController(ICrearProyectoUseCase crear, IActualizarProyectoUseCase actualizar, IConsultarProyectosUseCase consultar, IEliminarProyectoUseCase eliminar, IAgregarSeguimientoProyectoUseCase agregarSeguimiento, IConsultarPrioridadProyectosUseCase consultarPrioridad, IValidator<CrearProyectoDto> createValidator, IValidator<ActualizarProyectoDto> updateValidator, IValidator<CrearSeguimientoProyectoDto> seguimientoValidator) : BaseController
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProyectoResponseDto>>> GetAll(CancellationToken ct) => Ok(await consultar.ListAsync(ct));

    // Antes de "{id:guid}" a propósito -- si no, ASP.NET Core intenta parsear "prioridad" como Guid y falla con 404.
    /// <summary>"A qué proyecto atender primero" (docs/21, docs/22) -- puntuado y ordenado, con las razones de cada puntaje.</summary>
    [HttpGet("prioridad")]
    public async Task<ActionResult<IReadOnlyList<ProyectoPrioridadResponseDto>>> GetPrioridad(CancellationToken ct) => Ok(await consultarPrioridad.ExecuteAsync(ct));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProyectoResponseDto>> GetById(Guid id, CancellationToken ct) => Ok(await consultar.GetByIdAsync(id, ct));

    [HttpPost]
    public async Task<ActionResult<ProyectoResponseDto>> Create(CrearProyectoDto dto, CancellationToken ct)
    {
        var validation = await createValidator.ValidateAsync(dto, ct);
        if (!validation.IsValid) return BadRequest(new ValidationProblemDetails(validation.ToDictionary()));
        var userId = GetUserId(); if (userId == Guid.Empty) return Unauthorized();
        var result = await crear.ExecuteAsync(dto, userId, GetUserRole(), ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ProyectoResponseDto>> Update(Guid id, ActualizarProyectoDto dto, CancellationToken ct)
    {
        dto.Id = id; var validation = await updateValidator.ValidateAsync(dto, ct);
        if (!validation.IsValid) return BadRequest(new ValidationProblemDetails(validation.ToDictionary()));
        var userId = GetUserId(); if (userId == Guid.Empty) return Unauthorized();
        return Ok(await actualizar.ExecuteAsync(dto, userId, GetUserRole(), ct));
    }

    [HttpDelete("{id:guid}"), Authorize(Policy = "AdminOrAbove")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct) { await eliminar.ExecuteAsync(id, GetUserId(), ct); return NoContent(); }

    [HttpPost("{id:guid}/seguimiento")]
    public async Task<ActionResult<SeguimientoProyectoDto>> AddSeguimiento(Guid id, CrearSeguimientoProyectoDto dto, CancellationToken ct)
    {
        var validation = await seguimientoValidator.ValidateAsync(dto, ct);
        if (!validation.IsValid) return BadRequest(new ValidationProblemDetails(validation.ToDictionary()));
        var userId = GetUserId(); if (userId == Guid.Empty) return Unauthorized();
        return Ok(await agregarSeguimiento.ExecuteAsync(id, dto, userId, ct));
    }
}
