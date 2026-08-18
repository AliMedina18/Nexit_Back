using Nexit.Application.DTOs.SolicitudesEliminacion;
using Nexit.Core.Entities;
using Nexit.Core.Exceptions;
using Nexit.Core.Interfaces;

namespace Nexit.Application.UseCases.SolicitudesEliminacion;

public class SolicitarEliminacionUseCase(ISolicitudEliminacionRepository solicitudes, IProyectoRepository proyectos, IUnitOfWork unitOfWork) : ISolicitarEliminacionUseCase
{
    public async Task<SolicitudEliminacionResponseDto> ExecuteAsync(CrearSolicitudEliminacionDto input, Guid solicitanteId, CancellationToken cancellationToken = default)
    {
        Guid? gerenteResponsableId = null;
        var estado = "pendiente_admin";
        if (input.TipoEntidad == "proyecto")
        {
            var proyecto = await proyectos.GetByIdAsync(input.EntidadId, cancellationToken) ?? throw new EntityNotFoundException("Proyecto", input.EntidadId);
            // Si el proyecto tiene un gerente responsable distinto de quien solicita, primero debe
            // endosarla ese gerente. Si el solicitante ES el gerente responsable, o el proyecto todavía
            // no tiene gerente asignado, la solicitud va directo al administrador.
            if (proyecto.GerenteId.HasValue && proyecto.GerenteId.Value != solicitanteId)
            {
                gerenteResponsableId = proyecto.GerenteId;
                estado = "pendiente_gerente";
            }
        }
        var solicitud = new SolicitudEliminacion
        {
            TipoEntidad = input.TipoEntidad, EntidadId = input.EntidadId, SolicitadoPorId = solicitanteId,
            Motivo = input.Motivo, Estado = estado, GerenteResponsableId = gerenteResponsableId
        };
        await solicitudes.AddAsync(solicitud, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return SolicitudEliminacionMapper.ToResponse(solicitud);
    }
}

public class AprobarComoGerenteUseCase(ISolicitudEliminacionRepository solicitudes, IUnitOfWork unitOfWork) : IAprobarComoGerenteUseCase
{
    public async Task<SolicitudEliminacionResponseDto> ExecuteAsync(Guid solicitudId, Guid gerenteId, CancellationToken cancellationToken = default)
    {
        var solicitud = await solicitudes.GetByIdAsync(solicitudId, cancellationToken) ?? throw new EntityNotFoundException("SolicitudEliminacion", solicitudId);
        if (solicitud.Estado != "pendiente_gerente") throw new BusinessRuleException("Esta solicitud no está esperando la aprobación de un gerente.");
        if (solicitud.GerenteResponsableId != gerenteId) throw new ForbiddenOperationException("Solo el gerente responsable de este proyecto puede aprobar esta solicitud.");
        solicitud.Estado = "pendiente_admin";
        solicitud.AprobadoPorGerenteId = gerenteId;
        solicitud.AprobadoPorGerenteEn = DateTime.UtcNow;
        solicitudes.Update(solicitud);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return SolicitudEliminacionMapper.ToResponse(solicitud);
    }
}

public class RechazarComoGerenteUseCase(ISolicitudEliminacionRepository solicitudes, IUnitOfWork unitOfWork) : IRechazarComoGerenteUseCase
{
    public async Task<SolicitudEliminacionResponseDto> ExecuteAsync(Guid solicitudId, Guid gerenteId, RevisionSolicitudDto input, CancellationToken cancellationToken = default)
    {
        var solicitud = await solicitudes.GetByIdAsync(solicitudId, cancellationToken) ?? throw new EntityNotFoundException("SolicitudEliminacion", solicitudId);
        if (solicitud.Estado != "pendiente_gerente") throw new BusinessRuleException("Esta solicitud no está esperando la aprobación de un gerente.");
        if (solicitud.GerenteResponsableId != gerenteId) throw new ForbiddenOperationException("Solo el gerente responsable de este proyecto puede rechazar esta solicitud.");
        solicitud.Estado = "rechazada";
        solicitud.RevisadoPorId = gerenteId;
        solicitud.RevisadoEn = DateTime.UtcNow;
        solicitud.ComentarioRevision = input.Comentario;
        solicitudes.Update(solicitud);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return SolicitudEliminacionMapper.ToResponse(solicitud);
    }
}

public class AprobarComoAdminUseCase(
    ISolicitudEliminacionRepository solicitudes,
    IClienteRepository clientes,
    IProveedorRepository proveedores,
    IProyectoRepository proyectos,
    IUnitOfWork unitOfWork) : IAprobarComoAdminUseCase
{
    public async Task<SolicitudEliminacionResponseDto> ExecuteAsync(Guid solicitudId, Guid adminId, RevisionSolicitudDto input, CancellationToken cancellationToken = default)
    {
        var solicitud = await solicitudes.GetByIdAsync(solicitudId, cancellationToken) ?? throw new EntityNotFoundException("SolicitudEliminacion", solicitudId);
        if (solicitud.Estado != "pendiente_admin") throw new BusinessRuleException("Esta solicitud no está esperando la aprobación de un administrador.");
        // Si la entidad ya no existe (por ejemplo, alguien más ya la eliminó), simplemente se marca
        // la solicitud como aprobada sin volver a intentar borrarla.
        switch (solicitud.TipoEntidad)
        {
            case "cliente":
                if (await clientes.GetByIdAsync(solicitud.EntidadId, cancellationToken) is not null) await clientes.DeleteAsync(solicitud.EntidadId, cancellationToken);
                break;
            case "proveedor":
                if (await proveedores.GetByIdAsync(solicitud.EntidadId, cancellationToken) is not null) await proveedores.DeleteAsync(solicitud.EntidadId, cancellationToken);
                break;
            case "proyecto":
                if (await proyectos.GetByIdAsync(solicitud.EntidadId, cancellationToken) is not null) await proyectos.DeleteAsync(solicitud.EntidadId, cancellationToken);
                break;
        }
        solicitud.Estado = "aprobada";
        solicitud.RevisadoPorId = adminId;
        solicitud.RevisadoEn = DateTime.UtcNow;
        solicitud.ComentarioRevision = input.Comentario;
        solicitudes.Update(solicitud);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return SolicitudEliminacionMapper.ToResponse(solicitud);
    }
}

public class RechazarComoAdminUseCase(ISolicitudEliminacionRepository solicitudes, IUnitOfWork unitOfWork) : IRechazarComoAdminUseCase
{
    public async Task<SolicitudEliminacionResponseDto> ExecuteAsync(Guid solicitudId, Guid adminId, RevisionSolicitudDto input, CancellationToken cancellationToken = default)
    {
        var solicitud = await solicitudes.GetByIdAsync(solicitudId, cancellationToken) ?? throw new EntityNotFoundException("SolicitudEliminacion", solicitudId);
        if (solicitud.Estado != "pendiente_admin") throw new BusinessRuleException("Esta solicitud no está esperando la aprobación de un administrador.");
        solicitud.Estado = "rechazada";
        solicitud.RevisadoPorId = adminId;
        solicitud.RevisadoEn = DateTime.UtcNow;
        solicitud.ComentarioRevision = input.Comentario;
        solicitudes.Update(solicitud);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return SolicitudEliminacionMapper.ToResponse(solicitud);
    }
}

public class ConsultarSolicitudesEliminacionUseCase(ISolicitudEliminacionRepository solicitudes) : IConsultarSolicitudesEliminacionUseCase
{
    public async Task<IReadOnlyList<SolicitudEliminacionResponseDto>> ListAsync(CancellationToken cancellationToken = default) =>
        (await solicitudes.GetAllAsync(cancellationToken)).Select(SolicitudEliminacionMapper.ToResponse).ToList();

    public async Task<IReadOnlyList<SolicitudEliminacionResponseDto>> ListPendientesParaGerenteAsync(Guid gerenteId, CancellationToken cancellationToken = default) =>
        (await solicitudes.GetPendientesParaGerenteAsync(gerenteId, cancellationToken)).Select(SolicitudEliminacionMapper.ToResponse).ToList();

    public async Task<SolicitudEliminacionResponseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        SolicitudEliminacionMapper.ToResponse(await solicitudes.GetByIdAsync(id, cancellationToken) ?? throw new EntityNotFoundException("SolicitudEliminacion", id));
}

internal static class SolicitudEliminacionMapper
{
    public static SolicitudEliminacionResponseDto ToResponse(SolicitudEliminacion solicitud) => new()
    {
        Id = solicitud.Id, TipoEntidad = solicitud.TipoEntidad, EntidadId = solicitud.EntidadId, SolicitadoPorId = solicitud.SolicitadoPorId,
        Motivo = solicitud.Motivo, Estado = solicitud.Estado, GerenteResponsableId = solicitud.GerenteResponsableId,
        AprobadoPorGerenteId = solicitud.AprobadoPorGerenteId, AprobadoPorGerenteEn = solicitud.AprobadoPorGerenteEn,
        RevisadoPorId = solicitud.RevisadoPorId, RevisadoEn = solicitud.RevisadoEn, ComentarioRevision = solicitud.ComentarioRevision,
        CreatedAt = solicitud.CreatedAt
    };
}
