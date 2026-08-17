using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Nexit.Application.DTOs.Proveedores;
using Nexit.Application.UseCases.Proveedores;

namespace Nexit.API.Controllers;

public class ProveedoresController(ICrearProveedorUseCase crear, IActualizarProveedorUseCase actualizar, IConsultarProveedoresUseCase consultar, IEliminarProveedorUseCase eliminar, IValidator<CreateProveedorDto> createValidator, IValidator<UpdateProveedorDto> updateValidator) : BaseController
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProveedorResponseDto>>> GetAll(CancellationToken ct) => Ok(await consultar.ListAsync(ct));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProveedorResponseDto>> GetById(Guid id, CancellationToken ct) => Ok(await consultar.GetByIdAsync(id, ct));

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
        return Ok(await actualizar.ExecuteAsync(dto, ct));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await eliminar.ExecuteAsync(id, ct);
        return NoContent();
    }
}
