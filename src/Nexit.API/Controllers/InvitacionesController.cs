using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nexit.Application.DTOs.Invitaciones;
using Nexit.Application.UseCases.Invitaciones;

namespace Nexit.API.Controllers;

/// <summary>
/// Invitar y registrar a alguien del equipo en un solo paso (docs/10 sección 5, docs/25). Crear y
/// listar invitaciones es exclusivo de la super administradora (mismo criterio que
/// <see cref="UsuariosController"/>); ver/aceptar/rechazar "la mía" es de cualquier persona
/// autenticada -- incluso alguien que todavía no tiene fila en `usuarios`, por eso esas acciones
/// no llevan una política de rol más estricta que el <c>[Authorize]</c> de <see cref="BaseController"/>.
/// </summary>
public class InvitacionesController(
    ICrearInvitacionUseCase crear, IConsultarInvitacionesUseCase consultar, IConsultarMiInvitacionUseCase consultarMia,
    IAceptarInvitacionUseCase aceptar, IRechazarInvitacionUseCase rechazar,
    IValidator<CrearInvitacionDto> createValidator, IValidator<AceptarInvitacionDto> aceptarValidator) : BaseController
{
    [HttpGet, Authorize(Policy = "SuperAdminOnly")]
    public async Task<ActionResult<IReadOnlyList<InvitacionResponseDto>>> GetAll(CancellationToken ct) => Ok(await consultar.ListAsync(ct));

    [HttpPost, Authorize(Policy = "SuperAdminOnly")]
    public async Task<ActionResult<InvitacionResponseDto>> Create(CrearInvitacionDto dto, CancellationToken ct)
    {
        var validation = await createValidator.ValidateAsync(dto, ct);
        if (!validation.IsValid) return BadRequest(new ValidationProblemDetails(validation.ToDictionary()));
        return Ok(await crear.ExecuteAsync(dto, GetUserId(), ct));
    }

    /// <summary>La invitación pendiente que le corresponde a quien está autenticado, según su correo -- 404 si no hay ninguna.</summary>
    [HttpGet("mia")]
    public async Task<ActionResult<InvitacionResponseDto>> GetMia(CancellationToken ct)
    {
        var email = GetUserEmail();
        if (string.IsNullOrWhiteSpace(email)) return Unauthorized();
        var invitacion = await consultarMia.ExecuteAsync(email, ct);
        return invitacion is null ? NotFound() : Ok(invitacion);
    }

    [HttpPost("{id:guid}/aceptar")]
    public async Task<ActionResult> Aceptar(Guid id, AceptarInvitacionDto dto, CancellationToken ct)
    {
        var validation = await aceptarValidator.ValidateAsync(dto, ct);
        if (!validation.IsValid) return BadRequest(new ValidationProblemDetails(validation.ToDictionary()));
        var email = GetUserEmail();
        if (string.IsNullOrWhiteSpace(email)) return Unauthorized();
        return Ok(await aceptar.ExecuteAsync(id, dto, GetUserId(), email, ct));
    }

    [HttpPost("{id:guid}/rechazar")]
    public async Task<IActionResult> Rechazar(Guid id, CancellationToken ct)
    {
        var email = GetUserEmail();
        if (string.IsNullOrWhiteSpace(email)) return Unauthorized();
        await rechazar.ExecuteAsync(id, email, ct);
        return NoContent();
    }
}
