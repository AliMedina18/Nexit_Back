using Nexit.Application.DTOs.Notificaciones;
using Nexit.Core.Entities;
using Nexit.Core.Exceptions;
using Nexit.Core.Interfaces;

namespace Nexit.Application.UseCases.Notificaciones;

public class ListarMisNotificacionesUseCase(INotificacionRepository repository) : IListarMisNotificacionesUseCase
{
    public async Task<IReadOnlyList<NotificacionResponseDto>> ExecuteAsync(Guid usuarioId, CancellationToken cancellationToken = default) =>
        (await repository.GetPorUsuarioAsync(usuarioId, cancellationToken)).Select(NotificacionMapper.ToResponse).ToList();
}

public class MarcarNotificacionLeidaUseCase(INotificacionRepository repository, IUnitOfWork unitOfWork) : IMarcarNotificacionLeidaUseCase
{
    public async Task ExecuteAsync(Guid notificacionId, Guid usuarioId, CancellationToken cancellationToken = default)
    {
        var notificacion = await repository.GetByIdAsync(notificacionId, cancellationToken) ?? throw new EntityNotFoundException("Notificacion", notificacionId);
        if (notificacion.UsuarioDestinatarioId != usuarioId) throw new ForbiddenOperationException("Esta notificación no es tuya.");
        if (!notificacion.Leida) { notificacion.Leida = true; notificacion.FechaLeida = DateTime.UtcNow; repository.Update(notificacion); await unitOfWork.SaveChangesAsync(cancellationToken); }
    }
}

internal static class NotificacionMapper
{
    public static NotificacionResponseDto ToResponse(Notificacion n) => new()
    {
        Id = n.Id, Tipo = n.Tipo, Titulo = n.Titulo, Mensaje = n.Mensaje, TipoEntidad = n.TipoEntidad,
        EntidadId = n.EntidadId, SolicitudId = n.SolicitudId, Leida = n.Leida, FechaCreacion = n.FechaCreacion, FechaLeida = n.FechaLeida
    };
}

/// <summary>
/// Construye las notificaciones del flujo de solicitudes de eliminación (docs/19) -- centralizado
/// acá para que los 5 casos de uso de <c>SolicitudEliminacionUseCases.cs</c> que las disparan no
/// repitan la construcción del texto cada uno por su lado.
/// </summary>
internal static class NotificacionFactory
{
    public static Notificacion SolicitudCreadaParaGerente(Guid gerenteId, SolicitudEliminacion solicitud) => new()
    {
        UsuarioDestinatarioId = gerenteId, Tipo = "solicitud_eliminacion_creada",
        Titulo = $"Te pidieron eliminar un {solicitud.TipoEntidad}",
        Mensaje = $"Alguien de tu equipo solicitó eliminar un {solicitud.TipoEntidad} que lideras. Motivo: {solicitud.Motivo ?? "(sin motivo indicado)"}.",
        TipoEntidad = solicitud.TipoEntidad, EntidadId = solicitud.EntidadId, SolicitudId = solicitud.Id
    };

    public static Notificacion SolicitudCreadaParaAdmin(Guid adminId, SolicitudEliminacion solicitud, int totalPendientesParaEstaEntidad) => new()
    {
        UsuarioDestinatarioId = adminId, Tipo = "solicitud_eliminacion_creada",
        Titulo = $"Solicitud para eliminar un {solicitud.TipoEntidad}",
        Mensaje = totalPendientesParaEstaEntidad > 1
            ? $"Motivo: {solicitud.Motivo ?? "(sin motivo indicado)"}. Ya van {totalPendientesParaEstaEntidad} solicitudes pendientes para este mismo {solicitud.TipoEntidad}."
            : $"Motivo: {solicitud.Motivo ?? "(sin motivo indicado)"}.",
        TipoEntidad = solicitud.TipoEntidad, EntidadId = solicitud.EntidadId, SolicitudId = solicitud.Id
    };

    public static Notificacion GerenteEndoso(Guid adminId, SolicitudEliminacion solicitud) => new()
    {
        UsuarioDestinatarioId = adminId, Tipo = "solicitud_eliminacion_endosada",
        Titulo = $"El gerente responsable ya aprobó eliminar un {solicitud.TipoEntidad}",
        Mensaje = $"Falta tu decisión final para completar la eliminación de este {solicitud.TipoEntidad}.",
        TipoEntidad = solicitud.TipoEntidad, EntidadId = solicitud.EntidadId, SolicitudId = solicitud.Id
    };

    public static Notificacion DecisionParaSolicitante(SolicitudEliminacion solicitud, bool aprobada, string? comentario) => new()
    {
        UsuarioDestinatarioId = solicitud.SolicitadoPorId, Tipo = "solicitud_eliminacion_decidida",
        Titulo = aprobada ? $"Tu solicitud de eliminar un {solicitud.TipoEntidad} fue aprobada" : $"Tu solicitud de eliminar un {solicitud.TipoEntidad} fue rechazada",
        Mensaje = string.IsNullOrWhiteSpace(comentario) ? (aprobada ? "Se eliminó según lo solicitado." : "No se eliminó.") : comentario,
        TipoEntidad = solicitud.TipoEntidad, EntidadId = solicitud.EntidadId, SolicitudId = solicitud.Id
    };
}
