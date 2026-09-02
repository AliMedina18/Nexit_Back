using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nexit.Application.DTOs.Usuarios;
using Nexit.Application.UseCases.Usuarios;

namespace Nexit.API.Controllers;

/// <summary>
/// Gestión de cuentas de usuario. Tres niveles de acceso dentro del mismo controlador (actualizado
/// 2026-08-26, ver docs/06-modelo-permisos-roles.md sección 6):
///
///  - <b>Crear/editar/eliminar</b> (<see cref="Create"/>, <see cref="Update"/>, <see cref="Delete"/>):
///    exclusivo de <c>super_admin</c> (<c>SuperAdminOnly</c>) -- sin cambios.
///  - <b>Listar a todos</b> (<see cref="GetAll"/>): ahora también <c>admin</c>, no solo
///    <c>super_admin</c> (<c>AdminOrAbove</c>) -- la administradora operativa necesita ver el
///    directorio completo, aunque no pueda tocarlo.
///  - <b>Ver un perfil individual</b> (<see cref="GetById"/>, <see cref="GetMe"/>): cualquier persona
///    autenticada, sin importar el rol -- solo lectura, como el directorio de personas de Microsoft
///    Teams: cualquiera puede mirar el perfil de un compañero, pero editarlo/eliminarlo sigue siendo
///    exclusivo de super_admin.
///
/// Crear un usuario aquí solo registra su perfil de negocio; la cuenta de acceso (correo, contraseña)
/// se invita primero desde Supabase Auth.
/// </summary>
public class UsuariosController(
    ICrearUsuarioUseCase crear,
    IActualizarUsuarioUseCase actualizar,
    IConsultarUsuariosUseCase consultar,
    IEliminarUsuarioUseCase eliminar,
    IValidator<CreateUsuarioDto> createValidator,
    IValidator<UpdateUsuarioDto> updateValidator) : BaseController
{
    /// <summary>Directorio completo -- admin/super_admin (ver el resumen de la clase).</summary>
    [HttpGet, Authorize(Policy = "AdminOrAbove")]
    public async Task<ActionResult<IReadOnlyList<UsuarioResponseDto>>> GetAll(CancellationToken ct) => Ok(await consultar.ListAsync(ct));

    // Antes de "{id:guid}" a propósito -- "me" no es un Guid válido, así que no compite con esa ruta,
    // pero se pone primero para que quede junto al resto de rutas estáticas por convención del repo.
    /// <summary>Perfil propio -- cualquier autenticado, no exclusivo de super_admin (ver el resumen de la clase).</summary>
    [HttpGet("me")]
    public async Task<ActionResult<UsuarioResponseDto>> GetMe(CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty) return Unauthorized();
        return Ok(await consultar.GetByIdAsync(userId, ct));
    }

    /// <summary>
    /// Perfil de OTRA persona, solo lectura -- cualquier autenticado (agregado 2026-08-26, ver el
    /// resumen de la clase). No expone nada que <see cref="Update"/>/<see cref="Delete"/> dejen
    /// modificar: quien llama esto no puede editar ni eliminar, solo mirar.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UsuarioResponseDto>> GetById(Guid id, CancellationToken ct) => Ok(await consultar.GetByIdAsync(id, ct));

    [HttpPost, Authorize(Policy = "SuperAdminOnly")]
    public async Task<ActionResult<UsuarioResponseDto>> Create(CreateUsuarioDto dto, CancellationToken ct)
    {
        var validation = await createValidator.ValidateAsync(dto, ct);
        if (!validation.IsValid) return BadRequest(new ValidationProblemDetails(validation.ToDictionary()));
        var result = await crear.ExecuteAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}"), Authorize(Policy = "SuperAdminOnly")]
    public async Task<ActionResult<UsuarioResponseDto>> Update(Guid id, UpdateUsuarioDto dto, CancellationToken ct)
    {
        var validation = await updateValidator.ValidateAsync(dto, ct);
        if (!validation.IsValid) return BadRequest(new ValidationProblemDetails(validation.ToDictionary()));
        return Ok(await actualizar.ExecuteAsync(id, dto, GetUserId(), ct));
    }

    [HttpDelete("{id:guid}"), Authorize(Policy = "SuperAdminOnly")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct) { await eliminar.ExecuteAsync(id, GetUserId(), ct); return NoContent(); }
}
