using Nexit.Application.DTOs.SolicitudesEliminacion;
using Nexit.Application.UseCases.Notificaciones;
using Nexit.Application.UseCases.Usuarios;
using Nexit.Core.Constants;
using Nexit.Core.Entities;
using Nexit.Core.Exceptions;
using Nexit.Core.Interfaces;

namespace Nexit.Application.UseCases.SolicitudesEliminacion;

public class SolicitarEliminacionUseCase(
    ISolicitudEliminacionRepository solicitudes, IClienteRepository clientes, IProveedorRepository proveedores,
    IProyectoRepository proyectos, IUsuarioRepository usuarios,
    INotificacionRepository notificaciones, IUnitOfWork unitOfWork) : ISolicitarEliminacionUseCase
{
    public async Task<SolicitudEliminacionResponseDto> ExecuteAsync(CrearSolicitudEliminacionDto input, Guid solicitanteId, CancellationToken cancellationToken = default)
    {
        // Pedir eliminar a una PERSONA no es lo mismo que pedir eliminar un registro (docs/40): solo
        // admin/super_admin llegan siquiera a la pantalla donde se pide, nadie puede pedir su propia
        // eliminación (sería una forma rara de darse de baja saltándose la desactivación), y la cuenta
        // del super_admin no se puede pedir en absoluto -- si el sistema se quedara sin ella, no habría
        // quien vuelva a dar de alta a nadie.
        Usuario? usuarioObjetivo = null;
        if (input.TipoEntidad == TiposEntidadEliminable.Usuario)
        {
            var solicitante = await usuarios.GetByIdAsync(solicitanteId, cancellationToken)
                ?? throw new EntityNotFoundException("Usuario", solicitanteId);
            if (solicitante.Rol != Roles.Admin && solicitante.Rol != Roles.SuperAdmin)
                throw new ForbiddenOperationException("Solo un administrador puede pedir que se elimine una cuenta.");
            if (input.EntidadId == solicitanteId)
                throw new ForbiddenOperationException("No puedes pedir que eliminen tu propia cuenta.");
            usuarioObjetivo = await usuarios.GetByIdAsync(input.EntidadId, cancellationToken)
                ?? throw new EntityNotFoundException("Usuario", input.EntidadId);
            if (usuarioObjetivo.Rol == Roles.SuperAdmin)
                throw new ForbiddenOperationException("La cuenta del super administrador no se puede eliminar.");
        }

        Guid? gerenteResponsableId = null;
        var estado = "pendiente_admin";
        Proyecto? proyecto = null;
        if (input.TipoEntidad == TiposEntidadEliminable.Proyecto)
        {
            proyecto = await proyectos.GetByIdAsync(input.EntidadId, cancellationToken) ?? throw new EntityNotFoundException("Proyecto", input.EntidadId);
            // Si el proyecto tiene un gerente responsable distinto de quien solicita, primero debe
            // endosarla ese gerente. Si el solicitante ES el gerente responsable, o el proyecto todavía
            // no tiene gerente asignado, la solicitud va directo al administrador.
            if (proyecto.GerenteId.HasValue && proyecto.GerenteId.Value != solicitanteId)
            {
                gerenteResponsableId = proyecto.GerenteId;
                estado = "pendiente_gerente";
            }
        }

        // Fotografía del nombre al momento de pedirla (ver SolicitudEliminacion.EntidadNombre): para
        // cuando alguien la revise -- a veces días después, y a veces ya aprobada por otra vía -- la
        // entidad puede llevar rato borrada, y sin esto no había forma de saber qué (o a quién) se
        // había pedido eliminar, más allá de un id sin nombre.
        var entidadNombre = input.TipoEntidad switch
        {
            TiposEntidadEliminable.Usuario => usuarioObjetivo is { } u ? $"{u.Nombre} {u.Apellido}".Trim() : null,
            TiposEntidadEliminable.Proyecto => proyecto?.Nombre,
            TiposEntidadEliminable.Cliente => (await clientes.GetByIdAsync(input.EntidadId, cancellationToken))?.Nombre,
            TiposEntidadEliminable.Proveedor => (await proveedores.GetByIdAsync(input.EntidadId, cancellationToken))?.Nombre,
            _ => null,
        };

        var solicitud = new SolicitudEliminacion
        {
            TipoEntidad = input.TipoEntidad, EntidadId = input.EntidadId, EntidadNombre = entidadNombre, SolicitadoPorId = solicitanteId,
            Motivo = input.Motivo, Estado = estado, GerenteResponsableId = gerenteResponsableId
        };
        await solicitudes.AddAsync(solicitud, cancellationToken);

        if (estado == "pendiente_gerente")
        {
            await notificaciones.AddAsync(NotificacionFactory.SolicitudCreadaParaGerente(gerenteResponsableId!.Value, solicitud), cancellationToken);
        }
        else
        {
            // Cuántas solicitudes pendientes ya existían para esta misma entidad, para que el
            // administrador vea de una vez cuántas personas la están pidiendo (docs/19) -- +1 porque
            // esta que se acaba de crear también cuenta, y GetOtrasPendientes... la excluye a propósito.
            var totalPendientes = (await solicitudes.GetOtrasPendientesPorEntidadAsync(input.TipoEntidad, input.EntidadId, solicitud.Id, cancellationToken)).Count + 1;
            // Le llega a TODOS los administradores y al super administrador -- son quienes pueden decidir.
            // Con una cuenta de por medio se salta a dos: a quien la pidió (ya sabe) y a la persona en
            // cuestión (avisarle "pidieron eliminarte" desde su propia campana sería cruel e inútil: no
            // puede hacer nada al respecto).
            var destinatarios = await IdsAdministradoresAsync(usuarios, cancellationToken);
            if (usuarioObjetivo is not null)
                destinatarios = destinatarios.Where(id => id != solicitanteId && id != usuarioObjetivo.Id).ToList();
            foreach (var adminId in destinatarios)
                await notificaciones.AddAsync(NotificacionFactory.SolicitudCreadaParaAdmin(adminId, solicitud, totalPendientes), cancellationToken);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return SolicitudEliminacionMapper.ToResponse(solicitud);
    }

    internal static async Task<IReadOnlyList<Guid>> IdsAdministradoresAsync(IUsuarioRepository usuarios, CancellationToken ct) =>
        (await usuarios.GetAllAsync(ct)).Where(u => u.Activo && (u.Rol == Roles.Admin || u.Rol == Roles.SuperAdmin)).Select(u => u.Id).ToList();
}

public class AprobarComoGerenteUseCase(ISolicitudEliminacionRepository solicitudes, IUsuarioRepository usuarios, INotificacionRepository notificaciones, IUnitOfWork unitOfWork) : IAprobarComoGerenteUseCase
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
        foreach (var adminId in await SolicitarEliminacionUseCase.IdsAdministradoresAsync(usuarios, cancellationToken))
            await notificaciones.AddAsync(NotificacionFactory.GerenteEndoso(adminId, solicitud), cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return SolicitudEliminacionMapper.ToResponse(solicitud);
    }
}

public class RechazarComoGerenteUseCase(ISolicitudEliminacionRepository solicitudes, INotificacionRepository notificaciones, IUnitOfWork unitOfWork) : IRechazarComoGerenteUseCase
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
        if (NotificacionFactory.DecisionParaSolicitante(solicitud, aprobada: false, input.Comentario) is { } aviso)
            await notificaciones.AddAsync(aviso, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return SolicitudEliminacionMapper.ToResponse(solicitud);
    }
}

public class AprobarComoAdminUseCase(
    ISolicitudEliminacionRepository solicitudes,
    IClienteRepository clientes,
    IProveedorRepository proveedores,
    IProyectoRepository proyectos,
    IUsuarioRepository usuarios,
    IUsuarioEliminadoRepository usuariosEliminados,
    ISupabaseAuthAdminService authAdmin,
    INotificacionRepository notificaciones,
    IUnitOfWork unitOfWork) : IAprobarComoAdminUseCase
{
    public async Task<SolicitudEliminacionResponseDto> ExecuteAsync(Guid solicitudId, Guid adminId, RevisionSolicitudDto input, CancellationToken cancellationToken = default)
    {
        var solicitud = await solicitudes.GetByIdAsync(solicitudId, cancellationToken) ?? throw new EntityNotFoundException("SolicitudEliminacion", solicitudId);
        if (solicitud.Estado != "pendiente_admin") throw new BusinessRuleException("Esta solicitud no está esperando la aprobación de un administrador.");
        // Si la entidad ya no existe (por ejemplo, alguien más ya la eliminó), simplemente se marca
        // la solicitud como aprobada sin volver a intentar borrarla.
        Guid? cuentaAuthPorEliminar = null;
        switch (solicitud.TipoEntidad)
        {
            case TiposEntidadEliminable.Cliente:
                if (await clientes.GetByIdAsync(solicitud.EntidadId, cancellationToken) is not null) await clientes.DeleteAsync(solicitud.EntidadId, cancellationToken);
                break;
            case TiposEntidadEliminable.Proveedor:
                if (await proveedores.GetByIdAsync(solicitud.EntidadId, cancellationToken) is not null) await proveedores.DeleteAsync(solicitud.EntidadId, cancellationToken);
                break;
            case TiposEntidadEliminable.Proyecto:
                if (await proyectos.GetByIdAsync(solicitud.EntidadId, cancellationToken) is not null) await proyectos.DeleteAsync(solicitud.EntidadId, cancellationToken);
                break;
            case TiposEntidadEliminable.Usuario:
                // Mismo procedimiento que la eliminación automática de los 30 días (docs/17): respaldo en
                // `usuarios_eliminados`, borrado del perfil, y por último la cuenta de Supabase Auth --
                // esa va DESPUÉS de SaveChangesAsync, porque es lo único de aquí que no se puede deshacer.
                if (await usuarios.GetByIdAsync(solicitud.EntidadId, cancellationToken) is { } usuario)
                {
                    if (usuario.Rol == Roles.SuperAdmin) throw new ForbiddenOperationException("La cuenta del super administrador no se puede eliminar.");
                    if (usuario.Id == adminId) throw new ForbiddenOperationException("No puedes aprobar la eliminación de tu propia cuenta.");
                    await usuariosEliminados.AddAsync(UsuarioMapper.ToArchivo(usuario, eliminadoPorId: adminId), cancellationToken);
                    await usuarios.DeleteAsync(usuario.Id, cancellationToken);
                    cuentaAuthPorEliminar = usuario.Id;
                }
                break;
        }
        solicitud.Estado = "aprobada";
        solicitud.RevisadoPorId = adminId;
        solicitud.RevisadoEn = DateTime.UtcNow;
        solicitud.ComentarioRevision = input.Comentario;
        solicitudes.Update(solicitud);
        if (NotificacionFactory.DecisionParaSolicitante(solicitud, aprobada: true, input.Comentario) is { } aviso)
            await notificaciones.AddAsync(aviso, cancellationToken);

        // El administrador decide UNA vez por la entidad, no solicitud por solicitud (docs/19): como
        // la entidad ya se eliminó, cualquier otra solicitud todavía abierta para ella (la haya hecho
        // quien la haya hecho, en cualquiera de las dos etapas) queda resuelta con la misma decisión y
        // el mismo comentario -- nadie se queda con una solicitud pendiente apuntando a algo que ya no existe.
        foreach (var otra in await solicitudes.GetOtrasPendientesPorEntidadAsync(solicitud.TipoEntidad, solicitud.EntidadId, solicitud.Id, cancellationToken))
        {
            otra.Estado = "aprobada"; otra.RevisadoPorId = adminId; otra.RevisadoEn = DateTime.UtcNow; otra.ComentarioRevision = input.Comentario;
            solicitudes.Update(otra);
            if (NotificacionFactory.DecisionParaSolicitante(otra, aprobada: true, input.Comentario) is { } avisoOtra)
                await notificaciones.AddAsync(avisoOtra, cancellationToken);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        if (cuentaAuthPorEliminar is not null) await authAdmin.EliminarCuentaAsync(cuentaAuthPorEliminar.Value, cancellationToken);
        return SolicitudEliminacionMapper.ToResponse(solicitud);
    }
}

public class RechazarComoAdminUseCase(ISolicitudEliminacionRepository solicitudes, INotificacionRepository notificaciones, IUnitOfWork unitOfWork) : IRechazarComoAdminUseCase
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
        if (NotificacionFactory.DecisionParaSolicitante(solicitud, aprobada: false, input.Comentario) is { } aviso)
            await notificaciones.AddAsync(aviso, cancellationToken);

        // Misma razón que en AprobarComoAdminUseCase: una sola decisión del administrador resuelve
        // TODAS las solicitudes pendientes de esa misma entidad, no solo la que se revisó primero.
        foreach (var otra in await solicitudes.GetOtrasPendientesPorEntidadAsync(solicitud.TipoEntidad, solicitud.EntidadId, solicitud.Id, cancellationToken))
        {
            otra.Estado = "rechazada"; otra.RevisadoPorId = adminId; otra.RevisadoEn = DateTime.UtcNow; otra.ComentarioRevision = input.Comentario;
            solicitudes.Update(otra);
            if (NotificacionFactory.DecisionParaSolicitante(otra, aprobada: false, input.Comentario) is { } avisoOtra)
                await notificaciones.AddAsync(avisoOtra, cancellationToken);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return SolicitudEliminacionMapper.ToResponse(solicitud);
    }
}

public class ConsultarSolicitudesEliminacionUseCase(ISolicitudEliminacionRepository solicitudes, IUsuarioEliminadoRepository usuariosEliminados) : IConsultarSolicitudesEliminacionUseCase
{
    public async Task<IReadOnlyList<SolicitudEliminacionResponseDto>> ListAsync(CancellationToken cancellationToken = default)
    {
        var dtos = (await solicitudes.GetAllAsync(cancellationToken)).Select(SolicitudEliminacionMapper.ToResponse).ToList();
        await CompletarNombresDeCuentasEliminadasAsync(dtos, cancellationToken);
        return dtos;
    }

    // Las que le tocan a un gerente son siempre de proyecto (ver SolicitarEliminacionUseCase), nunca
    // de una cuenta -- no hace falta completar nada acá.
    public async Task<IReadOnlyList<SolicitudEliminacionResponseDto>> ListPendientesParaGerenteAsync(Guid gerenteId, CancellationToken cancellationToken = default) =>
        (await solicitudes.GetPendientesParaGerenteAsync(gerenteId, cancellationToken)).Select(SolicitudEliminacionMapper.ToResponse).ToList();

    public async Task<SolicitudEliminacionResponseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var dto = SolicitudEliminacionMapper.ToResponse(await solicitudes.GetByIdAsync(id, cancellationToken) ?? throw new EntityNotFoundException("SolicitudEliminacion", id));
        await CompletarNombresDeCuentasEliminadasAsync([dto], cancellationToken);
        return dto;
    }

    /// <summary>
    /// Respaldo para solicitudes de eliminar una CUENTA creadas antes del 2026-09-09, cuando
    /// SolicitudEliminacion todavía no guardaba su propia fotografía del nombre (EntidadNombre): sin
    /// eso, y con la cuenta ya borrada, la única forma de saber quién era esa persona es el respaldo
    /// de <c>usuarios_eliminados</c> que ya se guarda desde antes al eliminarla (ver UsuarioEliminado).
    /// </summary>
    private async Task CompletarNombresDeCuentasEliminadasAsync(List<SolicitudEliminacionResponseDto> dtos, CancellationToken cancellationToken)
    {
        var idsFaltantes = dtos
            .Where(d => d.TipoEntidad == TiposEntidadEliminable.Usuario && string.IsNullOrEmpty(d.EntidadNombre))
            .Select(d => d.EntidadId)
            .Distinct()
            .ToList();
        if (idsFaltantes.Count == 0) return;

        var respaldos = await usuariosEliminados.GetByUsuarioIdsOriginalAsync(idsFaltantes, cancellationToken);
        foreach (var dto in dtos)
        {
            if (dto.TipoEntidad == TiposEntidadEliminable.Usuario && string.IsNullOrEmpty(dto.EntidadNombre)
                && respaldos.TryGetValue(dto.EntidadId, out var respaldo))
                dto.EntidadNombre = $"{respaldo.Nombre} {respaldo.Apellido}".Trim();
        }
    }
}

internal static class SolicitudEliminacionMapper
{
    public static SolicitudEliminacionResponseDto ToResponse(SolicitudEliminacion solicitud) => new()
    {
        Id = solicitud.Id, TipoEntidad = solicitud.TipoEntidad, EntidadId = solicitud.EntidadId, EntidadNombre = solicitud.EntidadNombre,
        SolicitadoPorId = solicitud.SolicitadoPorId,
        Motivo = solicitud.Motivo, Estado = solicitud.Estado, GerenteResponsableId = solicitud.GerenteResponsableId,
        AprobadoPorGerenteId = solicitud.AprobadoPorGerenteId, AprobadoPorGerenteEn = solicitud.AprobadoPorGerenteEn,
        RevisadoPorId = solicitud.RevisadoPorId, RevisadoEn = solicitud.RevisadoEn, ComentarioRevision = solicitud.ComentarioRevision,
        CreatedAt = solicitud.CreatedAt
    };
}
