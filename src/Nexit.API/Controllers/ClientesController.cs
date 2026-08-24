using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nexit.Application.DTOs.Clientes;
using Nexit.Application.UseCases.Clientes;

namespace Nexit.API.Controllers;

public class ClientesController(ICrearClienteUseCase crear, IActualizarClienteUseCase actualizar, IConsultarClientesUseCase consultar, IEliminarClienteUseCase eliminar, IConsultarPrioridadClientesUseCase consultarPrioridad, IValidator<CreateClienteDto> createValidator, IValidator<UpdateClienteDto> updateValidator) : BaseController
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ClienteResponseDto>>> GetAll(CancellationToken cancellationToken) => Ok(await consultar.ListAsync(cancellationToken));

    // Antes de "{id:guid}" a propósito -- si no, ASP.NET Core intenta parsear "prioridad" como Guid y falla con 404.
    /// <summary>"A qué cliente prestarle atención" (docs/21, docs/24) -- puntuado y ordenado, con las razones de cada puntaje.</summary>
    [HttpGet("prioridad")]
    public async Task<ActionResult<IReadOnlyList<ClientePrioridadResponseDto>>> GetPrioridad(CancellationToken cancellationToken) => Ok(await consultarPrioridad.ExecuteAsync(cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ClienteResponseDto>> GetById(Guid id, CancellationToken cancellationToken) => Ok(await consultar.GetByIdAsync(id, cancellationToken));
    [HttpPost]
    public async Task<ActionResult<ClienteResponseDto>> Create(CreateClienteDto dto, CancellationToken cancellationToken)
    {
        var validation = await createValidator.ValidateAsync(dto, cancellationToken);
        if (!validation.IsValid) return BadRequest(new ValidationProblemDetails(validation.ToDictionary()));
        var userId = GetUserId();
        if (userId == Guid.Empty) return Unauthorized();
        var result = await crear.ExecuteAsync(dto, userId, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ClienteResponseDto>> Update(Guid id, UpdateClienteDto dto, CancellationToken cancellationToken)
    {
        dto.Id = id;
        var validation = await updateValidator.ValidateAsync(dto, cancellationToken);
        if (!validation.IsValid) return BadRequest(new ValidationProblemDetails(validation.ToDictionary()));
        var userId = GetUserId(); if (userId == Guid.Empty) return Unauthorized();
        return Ok(await actualizar.ExecuteAsync(dto, userId, cancellationToken));
    }
    [HttpDelete("{id:guid}"), Authorize(Policy = "AdminOrAbove")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await eliminar.ExecuteAsync(id, GetUserId(), cancellationToken);
        return NoContent();
    }
}
