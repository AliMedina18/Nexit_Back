using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nexit.API.Filters;
using Nexit.Application.DTOs.Importacion;
using Nexit.Application.DTOs.Invitaciones;
using Nexit.Application.Services;
using Nexit.Application.UseCases.Invitaciones;
using Nexit.Core.Exceptions;

namespace Nexit.API.Controllers;

/// <summary>
/// Invitar y registrar a alguien del equipo en un solo paso (docs/10 sección 5, docs/25). Crear y
/// listar invitaciones es exclusivo de la super administradora (mismo criterio que
/// <see cref="UsuariosController"/>); ver/aceptar/rechazar "la mía" es de cualquier persona
/// autenticada -- incluso alguien que todavía no tiene fila en `usuarios`, por eso esas acciones
/// no llevan una política de rol más estricta que el <c>[Authorize]</c> de <see cref="BaseController"/>.
/// </summary>
public class InvitacionesController(
    ICrearInvitacionUseCase crear, ICrearInvitacionesLoteUseCase crearLote,
    IConsultarInvitacionesUseCase consultar, IConsultarMiInvitacionUseCase consultarMia,
    IAceptarInvitacionUseCase aceptar, IRechazarInvitacionUseCase rechazar, ICancelarInvitacionUseCase cancelar,
    IUsuariosImportExporter importExporter,
    IValidator<CrearInvitacionDto> createValidator, IValidator<CrearInvitacionesLoteDto> loteValidator,
    IValidator<AceptarInvitacionDto> aceptarValidator) : BaseController
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

    /// <summary>
    /// Invita a varios correos de una sola vez. Devuelve 200 aunque alguno falle: el cuerpo trae
    /// `enviadas` y `fallidas` por separado, porque un correo malo no debe cancelar el resto del
    /// lote (ver CrearInvitacionesLoteUseCase). Solo un 400 si el lote entero está mal formado.
    /// </summary>
    [HttpPost("lote"), Authorize(Policy = "SuperAdminOnly")]
    public async Task<ActionResult<InvitacionesLoteResponseDto>> CreateLote(CrearInvitacionesLoteDto dto, CancellationToken ct)
    {
        var validation = await loteValidator.ValidateAsync(dto, ct);
        if (!validation.IsValid) return BadRequest(new ValidationProblemDetails(validation.ToDictionary()));
        return Ok(await crearLote.ExecuteAsync(dto, GetUserId(), ct));
    }

    /// <summary>
    /// Invita a todos los correos de un .xlsx (columna "Correo", y "Rol" opcional -- sin ella entra
    /// como miembro). Es lo que el botón de Excel de la pantalla de Usuarios llama al "importar":
    /// importar usuarios es invitarlos, ver IUsuariosImportExporter para el porqué. Igual que los
    /// otros importadores, una fila mala no detiene el archivo.
    /// </summary>
    [HttpPost("importar"), Authorize(Policy = "SuperAdminOnly")]
    public async Task<ActionResult<ImportarResultadoDto>> ImportarInvitaciones(IFormFile? archivo, CancellationToken ct)
    {
        if (archivo is null || archivo.Length == 0) throw new BusinessRuleException("Debes adjuntar un archivo .xlsx.");
        await using var contenido = archivo.OpenReadStream();
        return Ok(await importExporter.ImportarInvitacionesAsync(contenido, GetUserId(), ct));
    }

    /// <summary>Cancela una invitación que sigue Pendiente -- ver ICancelarInvitacionUseCase.</summary>
    [HttpDelete("{id:guid}"), Authorize(Policy = "SuperAdminOnly")]
    public async Task<IActionResult> Cancelar(Guid id, CancellationToken ct)
    {
        await cancelar.ExecuteAsync(id, ct);
        return NoContent();
    }

    /// <summary>La invitación pendiente que le corresponde a quien está autenticado, según su correo -- 404 si no hay ninguna.</summary>
    [HttpGet("mia"), PermitirSinPerfil]
    public async Task<ActionResult<InvitacionResponseDto>> GetMia(CancellationToken ct)
    {
        var email = GetUserEmail();
        if (string.IsNullOrWhiteSpace(email)) return Unauthorized();
        var invitacion = await consultarMia.ExecuteAsync(email, ct);
        return invitacion is null ? NotFound() : Ok(invitacion);
    }

    [HttpPost("{id:guid}/aceptar"), PermitirSinPerfil]
    public async Task<ActionResult> Aceptar(Guid id, AceptarInvitacionDto dto, CancellationToken ct)
    {
        var validation = await aceptarValidator.ValidateAsync(dto, ct);
        if (!validation.IsValid) return BadRequest(new ValidationProblemDetails(validation.ToDictionary()));
        var email = GetUserEmail();
        if (string.IsNullOrWhiteSpace(email)) return Unauthorized();
        return Ok(await aceptar.ExecuteAsync(id, dto, GetUserId(), email, ct));
    }

    [HttpPost("{id:guid}/rechazar"), PermitirSinPerfil]
    public async Task<IActionResult> Rechazar(Guid id, CancellationToken ct)
    {
        var email = GetUserEmail();
        if (string.IsNullOrWhiteSpace(email)) return Unauthorized();
        await rechazar.ExecuteAsync(id, email, ct);
        return NoContent();
    }
}
