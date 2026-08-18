using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nexit.Application.DTOs.SolicitudesEliminacion;
using Nexit.Application.UseCases.SolicitudesEliminacion;

namespace Nexit.API.Controllers;

/// <summary>
/// Un gerente o miembro no puede eliminar directamente un cliente, proveedor o proyecto (ver
/// docs/06-modelo-permisos-roles.md) — en su lugar solicita la eliminación aquí. Si es un proyecto
/// con gerente responsable distinto de quien solicita, primero la endosa ese gerente; de ahí (o
/// directo, para clientes/proveedores y proyectos sin gerente o solicitados por su propio gerente)
/// pasa a un administrador para la decisión final, que ejecuta el borrado real.
/// </summary>
public class SolicitudesEliminacionController(
    ISolicitarEliminacionUseCase solicitar,
    IAprobarComoGerenteUseCase aprobarGerente,
    IRechazarComoGerenteUseCase rechazarGerente,
    IAprobarComoAdminUseCase aprobarAdmin,
    IRechazarComoAdminUseCase rechazarAdmin,
    IConsultarSolicitudesEliminacionUseCase consultar,
    IValidator<CrearSolicitudEliminacionDto> createValidator) : BaseController
{
    [HttpPost]
    public async Task<ActionResult<SolicitudEliminacionResponseDto>> Create(CrearSolicitudEliminacionDto dto, CancellationToken ct)
    {
        var validation = await createValidator.ValidateAsync(dto, ct);
        if (!validation.IsValid) return BadRequest(new ValidationProblemDetails(validation.ToDictionary()));
        var userId = GetUserId(); if (userId == Guid.Empty) return Unauthorized();
        var result = await solicitar.ExecuteAsync(dto, userId, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet, Authorize(Policy = "AdminOrAbove")]
    public async Task<ActionResult<IReadOnlyList<SolicitudEliminacionResponseDto>>> GetAll(CancellationToken ct) => Ok(await consultar.ListAsync(ct));

    [HttpGet("pendientes-para-mi")]
    public async Task<ActionResult<IReadOnlyList<SolicitudEliminacionResponseDto>>> GetPendientesParaMi(CancellationToken ct) => Ok(await consultar.ListPendientesParaGerenteAsync(GetUserId(), ct));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SolicitudEliminacionResponseDto>> GetById(Guid id, CancellationToken ct) => Ok(await consultar.GetByIdAsync(id, ct));

    [HttpPut("{id:guid}/aprobar-gerente")]
    public async Task<ActionResult<SolicitudEliminacionResponseDto>> AprobarGerente(Guid id, CancellationToken ct) => Ok(await aprobarGerente.ExecuteAsync(id, GetUserId(), ct));

    [HttpPut("{id:guid}/rechazar-gerente")]
    public async Task<ActionResult<SolicitudEliminacionResponseDto>> RechazarGerente(Guid id, RevisionSolicitudDto dto, CancellationToken ct) => Ok(await rechazarGerente.ExecuteAsync(id, GetUserId(), dto, ct));

    [HttpPut("{id:guid}/aprobar"), Authorize(Policy = "AdminOrAbove")]
    public async Task<ActionResult<SolicitudEliminacionResponseDto>> Aprobar(Guid id, RevisionSolicitudDto dto, CancellationToken ct) => Ok(await aprobarAdmin.ExecuteAsync(id, GetUserId(), dto, ct));

    [HttpPut("{id:guid}/rechazar"), Authorize(Policy = "AdminOrAbove")]
    public async Task<ActionResult<SolicitudEliminacionResponseDto>> Rechazar(Guid id, RevisionSolicitudDto dto, CancellationToken ct) => Ok(await rechazarAdmin.ExecuteAsync(id, GetUserId(), dto, ct));
}
