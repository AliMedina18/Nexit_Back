using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nexit.Application.DTOs.Usuarios;
using Nexit.Application.UseCases.Usuarios;

namespace Nexit.API.Controllers;

/// <summary>
/// Gestión de cuentas de usuario (rol, activo/inactivo, alta y baja) — exclusiva del super
/// administrador. Ver docs/06-modelo-permisos-roles.md. Crear un usuario aquí solo registra su
/// perfil de negocio; la cuenta de acceso (correo, contraseña) se invita primero desde Supabase Auth.
/// </summary>
[Authorize(Policy = "SuperAdminOnly")]
public class UsuariosController(
    ICrearUsuarioUseCase crear,
    IActualizarUsuarioUseCase actualizar,
    IConsultarUsuariosUseCase consultar,
    IEliminarUsuarioUseCase eliminar,
    IValidator<CreateUsuarioDto> createValidator,
    IValidator<UpdateUsuarioDto> updateValidator) : BaseController
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<UsuarioResponseDto>>> GetAll(CancellationToken ct) => Ok(await consultar.ListAsync(ct));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UsuarioResponseDto>> GetById(Guid id, CancellationToken ct) => Ok(await consultar.GetByIdAsync(id, ct));

    [HttpPost]
    public async Task<ActionResult<UsuarioResponseDto>> Create(CreateUsuarioDto dto, CancellationToken ct)
    {
        var validation = await createValidator.ValidateAsync(dto, ct);
        if (!validation.IsValid) return BadRequest(new ValidationProblemDetails(validation.ToDictionary()));
        var result = await crear.ExecuteAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<UsuarioResponseDto>> Update(Guid id, UpdateUsuarioDto dto, CancellationToken ct)
    {
        var validation = await updateValidator.ValidateAsync(dto, ct);
        if (!validation.IsValid) return BadRequest(new ValidationProblemDetails(validation.ToDictionary()));
        return Ok(await actualizar.ExecuteAsync(id, dto, GetUserId(), ct));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct) { await eliminar.ExecuteAsync(id, GetUserId(), ct); return NoContent(); }
}
