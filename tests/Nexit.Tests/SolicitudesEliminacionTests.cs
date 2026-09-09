using Moq;
using Nexit.Application.DTOs.SolicitudesEliminacion;
using Nexit.Application.UseCases.SolicitudesEliminacion;
using Nexit.Core.Constants;
using Nexit.Core.Entities;
using Nexit.Core.Exceptions;
using Nexit.Core.Interfaces;

namespace Nexit.Tests;

/// <summary>
/// Flujo de solicitudes de eliminación (ver docs/06-modelo-permisos-roles.md): un gerente o miembro
/// no puede eliminar directamente un cliente, proveedor o proyecto. Si el proyecto tiene un gerente
/// responsable distinto de quien solicita, la solicitud pasa primero por ese gerente
/// (pendiente_gerente); si no, o si es clientes/proveedores, va directo a un administrador
/// (pendiente_admin), quien ejecuta el borrado real al aprobar.
/// </summary>
public class SolicitudesEliminacionTests
{
    [Fact]
    public async Task Solicitar_eliminar_proyecto_con_gerente_distinto_queda_pendiente_de_ese_gerente()
    {
        var solicitudes = new Mock<ISolicitudEliminacionRepository>();
        var proyectos = new Mock<IProyectoRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var proyectoId = Guid.NewGuid();
        var gerenteId = Guid.NewGuid();
        var solicitanteId = Guid.NewGuid();
        proyectos.Setup(x => x.GetByIdAsync(proyectoId, It.IsAny<CancellationToken>())).ReturnsAsync(new Proyecto { Id = proyectoId, GerenteId = gerenteId });

        var result = await new SolicitarEliminacionUseCase(solicitudes.Object, proyectos.Object, Mock.Of<IUsuarioRepository>(), Mock.Of<INotificacionRepository>(), unitOfWork.Object)
            .ExecuteAsync(new CrearSolicitudEliminacionDto { TipoEntidad = "proyecto", EntidadId = proyectoId }, solicitanteId);

        Assert.Equal("pendiente_gerente", result.Estado);
        Assert.Equal(gerenteId, result.GerenteResponsableId);
    }

    [Fact]
    public async Task Solicitar_eliminar_proyecto_sin_gerente_asignado_va_directo_al_administrador()
    {
        var solicitudes = new Mock<ISolicitudEliminacionRepository>();
        solicitudes.Setup(x => x.GetOtrasPendientesPorEntidadAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync([]);
        var proyectos = new Mock<IProyectoRepository>();
        var usuarios = new Mock<IUsuarioRepository>();
        usuarios.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync([]);
        var proyectoId = Guid.NewGuid();
        proyectos.Setup(x => x.GetByIdAsync(proyectoId, It.IsAny<CancellationToken>())).ReturnsAsync(new Proyecto { Id = proyectoId, GerenteId = null });

        var result = await new SolicitarEliminacionUseCase(solicitudes.Object, proyectos.Object, usuarios.Object, Mock.Of<INotificacionRepository>(), Mock.Of<IUnitOfWork>())
            .ExecuteAsync(new CrearSolicitudEliminacionDto { TipoEntidad = "proyecto", EntidadId = proyectoId }, Guid.NewGuid());

        Assert.Equal("pendiente_admin", result.Estado);
        Assert.Null(result.GerenteResponsableId);
    }

    [Fact]
    public async Task Solicitar_eliminar_proyecto_propio_como_su_gerente_va_directo_al_administrador()
    {
        var solicitudes = new Mock<ISolicitudEliminacionRepository>();
        solicitudes.Setup(x => x.GetOtrasPendientesPorEntidadAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync([]);
        var proyectos = new Mock<IProyectoRepository>();
        var usuarios = new Mock<IUsuarioRepository>();
        usuarios.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync([]);
        var proyectoId = Guid.NewGuid();
        var gerenteId = Guid.NewGuid();
        proyectos.Setup(x => x.GetByIdAsync(proyectoId, It.IsAny<CancellationToken>())).ReturnsAsync(new Proyecto { Id = proyectoId, GerenteId = gerenteId });

        var result = await new SolicitarEliminacionUseCase(solicitudes.Object, proyectos.Object, usuarios.Object, Mock.Of<INotificacionRepository>(), Mock.Of<IUnitOfWork>())
            .ExecuteAsync(new CrearSolicitudEliminacionDto { TipoEntidad = "proyecto", EntidadId = proyectoId }, gerenteId);

        Assert.Equal("pendiente_admin", result.Estado);
    }

    [Theory]
    [InlineData("cliente")]
    [InlineData("proveedor")]
    public async Task Solicitar_eliminar_cliente_o_proveedor_va_directo_al_administrador(string tipo)
    {
        var solicitudes = new Mock<ISolicitudEliminacionRepository>();
        solicitudes.Setup(x => x.GetOtrasPendientesPorEntidadAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync([]);
        var usuarios = new Mock<IUsuarioRepository>();
        usuarios.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync([]);
        var result = await new SolicitarEliminacionUseCase(solicitudes.Object, Mock.Of<IProyectoRepository>(), usuarios.Object, Mock.Of<INotificacionRepository>(), Mock.Of<IUnitOfWork>())
            .ExecuteAsync(new CrearSolicitudEliminacionDto { TipoEntidad = tipo, EntidadId = Guid.NewGuid() }, Guid.NewGuid());

        Assert.Equal("pendiente_admin", result.Estado);
        Assert.Null(result.GerenteResponsableId);
    }

    [Fact]
    public async Task Solicitar_eliminar_proyecto_inexistente_throws()
    {
        var solicitudes = new Mock<ISolicitudEliminacionRepository>();
        var proyectos = new Mock<IProyectoRepository>();
        proyectos.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Proyecto?)null);

        await Assert.ThrowsAsync<EntityNotFoundException>(() => new SolicitarEliminacionUseCase(solicitudes.Object, proyectos.Object, Mock.Of<IUsuarioRepository>(), Mock.Of<INotificacionRepository>(), Mock.Of<IUnitOfWork>())
            .ExecuteAsync(new CrearSolicitudEliminacionDto { TipoEntidad = "proyecto", EntidadId = Guid.NewGuid() }, Guid.NewGuid()));
    }

    [Fact]
    public async Task AprobarComoGerente_rejects_a_gerente_who_is_not_the_owner()
    {
        var solicitudes = new Mock<ISolicitudEliminacionRepository>();
        var solicitudId = Guid.NewGuid();
        var gerenteResponsable = Guid.NewGuid();
        solicitudes.Setup(x => x.GetByIdAsync(solicitudId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SolicitudEliminacion { Id = solicitudId, Estado = "pendiente_gerente", GerenteResponsableId = gerenteResponsable, TipoEntidad = "proyecto", EntidadId = Guid.NewGuid(), SolicitadoPorId = Guid.NewGuid() });

        await Assert.ThrowsAsync<ForbiddenOperationException>(() => new AprobarComoGerenteUseCase(solicitudes.Object, Mock.Of<IUsuarioRepository>(), Mock.Of<INotificacionRepository>(), Mock.Of<IUnitOfWork>())
            .ExecuteAsync(solicitudId, Guid.NewGuid(), CancellationToken.None));
    }

    [Fact]
    public async Task AprobarComoGerente_rejects_a_request_not_awaiting_a_gerente()
    {
        var solicitudes = new Mock<ISolicitudEliminacionRepository>();
        var solicitudId = Guid.NewGuid();
        var gerenteId = Guid.NewGuid();
        solicitudes.Setup(x => x.GetByIdAsync(solicitudId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SolicitudEliminacion { Id = solicitudId, Estado = "pendiente_admin", GerenteResponsableId = gerenteId, TipoEntidad = "proyecto", EntidadId = Guid.NewGuid(), SolicitadoPorId = Guid.NewGuid() });

        await Assert.ThrowsAsync<BusinessRuleException>(() => new AprobarComoGerenteUseCase(solicitudes.Object, Mock.Of<IUsuarioRepository>(), Mock.Of<INotificacionRepository>(), Mock.Of<IUnitOfWork>())
            .ExecuteAsync(solicitudId, gerenteId, CancellationToken.None));
    }

    [Fact]
    public async Task AprobarComoGerente_moves_the_owner_endorsed_request_to_pendiente_admin()
    {
        var solicitudes = new Mock<ISolicitudEliminacionRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var usuarios = new Mock<IUsuarioRepository>();
        usuarios.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync([]);
        var solicitudId = Guid.NewGuid();
        var gerenteId = Guid.NewGuid();
        var solicitud = new SolicitudEliminacion { Id = solicitudId, Estado = "pendiente_gerente", GerenteResponsableId = gerenteId, TipoEntidad = "proyecto", EntidadId = Guid.NewGuid(), SolicitadoPorId = Guid.NewGuid() };
        solicitudes.Setup(x => x.GetByIdAsync(solicitudId, It.IsAny<CancellationToken>())).ReturnsAsync(solicitud);

        var result = await new AprobarComoGerenteUseCase(solicitudes.Object, usuarios.Object, Mock.Of<INotificacionRepository>(), unitOfWork.Object).ExecuteAsync(solicitudId, gerenteId, CancellationToken.None);

        Assert.Equal("pendiente_admin", result.Estado);
        Assert.Equal(gerenteId, result.AprobadoPorGerenteId);
        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RechazarComoGerente_sets_rechazada_with_comment()
    {
        var solicitudes = new Mock<ISolicitudEliminacionRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var solicitudId = Guid.NewGuid();
        var gerenteId = Guid.NewGuid();
        var solicitud = new SolicitudEliminacion { Id = solicitudId, Estado = "pendiente_gerente", GerenteResponsableId = gerenteId, TipoEntidad = "proyecto", EntidadId = Guid.NewGuid(), SolicitadoPorId = Guid.NewGuid() };
        solicitudes.Setup(x => x.GetByIdAsync(solicitudId, It.IsAny<CancellationToken>())).ReturnsAsync(solicitud);

        var result = await new RechazarComoGerenteUseCase(solicitudes.Object, Mock.Of<INotificacionRepository>(), unitOfWork.Object)
            .ExecuteAsync(solicitudId, gerenteId, new RevisionSolicitudDto { Comentario = "No procede" }, CancellationToken.None);

        Assert.Equal("rechazada", result.Estado);
        Assert.Equal("No procede", result.ComentarioRevision);
    }

    [Fact]
    public async Task AprobarComoAdmin_deletes_the_underlying_client_and_marks_aprobada()
    {
        var solicitudes = new Mock<ISolicitudEliminacionRepository>();
        var clientes = new Mock<IClienteRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var solicitudId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var solicitud = new SolicitudEliminacion { Id = solicitudId, Estado = "pendiente_admin", TipoEntidad = "cliente", EntidadId = clienteId, SolicitadoPorId = Guid.NewGuid() };
        solicitudes.Setup(x => x.GetByIdAsync(solicitudId, It.IsAny<CancellationToken>())).ReturnsAsync(solicitud);
        solicitudes.Setup(x => x.GetOtrasPendientesPorEntidadAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync([]);
        clientes.Setup(x => x.GetByIdAsync(clienteId, It.IsAny<CancellationToken>())).ReturnsAsync(new Cliente { Id = clienteId });

        var adminId = Guid.NewGuid();
        var result = await new AprobarComoAdminUseCase(solicitudes.Object, clientes.Object, Mock.Of<IProveedorRepository>(), Mock.Of<IProyectoRepository>(), Mock.Of<IUsuarioRepository>(), Mock.Of<IUsuarioEliminadoRepository>(), Mock.Of<ISupabaseAuthAdminService>(), Mock.Of<INotificacionRepository>(), unitOfWork.Object)
            .ExecuteAsync(solicitudId, adminId, new RevisionSolicitudDto(), CancellationToken.None);

        Assert.Equal("aprobada", result.Estado);
        Assert.Equal(adminId, result.RevisadoPorId);
        clientes.Verify(x => x.DeleteAsync(clienteId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AprobarComoAdmin_skips_the_delete_when_the_entity_is_already_gone()
    {
        var solicitudes = new Mock<ISolicitudEliminacionRepository>();
        var proveedores = new Mock<IProveedorRepository>();
        var solicitudId = Guid.NewGuid();
        var proveedorId = Guid.NewGuid();
        var solicitud = new SolicitudEliminacion { Id = solicitudId, Estado = "pendiente_admin", TipoEntidad = "proveedor", EntidadId = proveedorId, SolicitadoPorId = Guid.NewGuid() };
        solicitudes.Setup(x => x.GetByIdAsync(solicitudId, It.IsAny<CancellationToken>())).ReturnsAsync(solicitud);
        solicitudes.Setup(x => x.GetOtrasPendientesPorEntidadAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync([]);
        proveedores.Setup(x => x.GetByIdAsync(proveedorId, It.IsAny<CancellationToken>())).ReturnsAsync((Proveedor?)null);

        var result = await new AprobarComoAdminUseCase(solicitudes.Object, Mock.Of<IClienteRepository>(), proveedores.Object, Mock.Of<IProyectoRepository>(), Mock.Of<IUsuarioRepository>(), Mock.Of<IUsuarioEliminadoRepository>(), Mock.Of<ISupabaseAuthAdminService>(), Mock.Of<INotificacionRepository>(), Mock.Of<IUnitOfWork>())
            .ExecuteAsync(solicitudId, Guid.NewGuid(), new RevisionSolicitudDto(), CancellationToken.None);

        Assert.Equal("aprobada", result.Estado);
        proveedores.Verify(x => x.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AprobarComoAdmin_cascades_to_resolve_other_pending_requests_for_the_same_entity()
    {
        // "El administrador decide UNA vez por la entidad, no solicitud por solicitud" (docs/19):
        // si fulanito y María pidieron por separado eliminar el mismo proveedor, aprobar la de
        // fulanito debe resolver también la de María -- con la misma decisión, mismo revisor, mismo
        // comentario -- y notificar a cada una por separado.
        var solicitudes = new Mock<ISolicitudEliminacionRepository>();
        var proveedores = new Mock<IProveedorRepository>();
        var notificaciones = new Mock<INotificacionRepository>();
        var solicitudId = Guid.NewGuid();
        var proveedorId = Guid.NewGuid();
        var solicitanteFulanito = Guid.NewGuid();
        var solicitanteMaria = Guid.NewGuid();
        var otraSolicitudId = Guid.NewGuid();
        var solicitud = new SolicitudEliminacion { Id = solicitudId, Estado = "pendiente_admin", TipoEntidad = "proveedor", EntidadId = proveedorId, SolicitadoPorId = solicitanteFulanito };
        var otraSolicitud = new SolicitudEliminacion { Id = otraSolicitudId, Estado = "pendiente_admin", TipoEntidad = "proveedor", EntidadId = proveedorId, SolicitadoPorId = solicitanteMaria };
        solicitudes.Setup(x => x.GetByIdAsync(solicitudId, It.IsAny<CancellationToken>())).ReturnsAsync(solicitud);
        solicitudes.Setup(x => x.GetOtrasPendientesPorEntidadAsync("proveedor", proveedorId, solicitudId, It.IsAny<CancellationToken>())).ReturnsAsync([otraSolicitud]);
        proveedores.Setup(x => x.GetByIdAsync(proveedorId, It.IsAny<CancellationToken>())).ReturnsAsync(new Proveedor { Id = proveedorId });
        var enviadas = new List<Notificacion>();
        notificaciones.Setup(x => x.AddAsync(It.IsAny<Notificacion>(), It.IsAny<CancellationToken>()))
            .Callback<Notificacion, CancellationToken>((n, _) => enviadas.Add(n)).Returns(Task.CompletedTask);

        var adminId = Guid.NewGuid();
        await new AprobarComoAdminUseCase(solicitudes.Object, Mock.Of<IClienteRepository>(), proveedores.Object, Mock.Of<IProyectoRepository>(), Mock.Of<IUsuarioRepository>(), Mock.Of<IUsuarioEliminadoRepository>(), Mock.Of<ISupabaseAuthAdminService>(), notificaciones.Object, Mock.Of<IUnitOfWork>())
            .ExecuteAsync(solicitudId, adminId, new RevisionSolicitudDto { Comentario = "Aprobado" }, CancellationToken.None);

        Assert.Equal("aprobada", otraSolicitud.Estado);
        Assert.Equal(adminId, otraSolicitud.RevisadoPorId);
        Assert.Equal("Aprobado", otraSolicitud.ComentarioRevision);
        solicitudes.Verify(x => x.Update(otraSolicitud), Times.Once);
        Assert.Equal(2, enviadas.Count);
        Assert.Contains(enviadas, n => n.UsuarioDestinatarioId == solicitanteFulanito);
        Assert.Contains(enviadas, n => n.UsuarioDestinatarioId == solicitanteMaria);
    }

    [Fact]
    public async Task RechazarComoAdmin_cascades_to_resolve_other_pending_requests_for_the_same_entity()
    {
        var solicitudes = new Mock<ISolicitudEliminacionRepository>();
        var notificaciones = new Mock<INotificacionRepository>();
        var solicitudId = Guid.NewGuid();
        var entidadId = Guid.NewGuid();
        var otraSolicitudId = Guid.NewGuid();
        var solicitud = new SolicitudEliminacion { Id = solicitudId, Estado = "pendiente_admin", TipoEntidad = "cliente", EntidadId = entidadId, SolicitadoPorId = Guid.NewGuid() };
        var otraSolicitud = new SolicitudEliminacion { Id = otraSolicitudId, Estado = "pendiente_admin", TipoEntidad = "cliente", EntidadId = entidadId, SolicitadoPorId = Guid.NewGuid() };
        solicitudes.Setup(x => x.GetByIdAsync(solicitudId, It.IsAny<CancellationToken>())).ReturnsAsync(solicitud);
        solicitudes.Setup(x => x.GetOtrasPendientesPorEntidadAsync("cliente", entidadId, solicitudId, It.IsAny<CancellationToken>())).ReturnsAsync([otraSolicitud]);

        await new RechazarComoAdminUseCase(solicitudes.Object, notificaciones.Object, Mock.Of<IUnitOfWork>())
            .ExecuteAsync(solicitudId, Guid.NewGuid(), new RevisionSolicitudDto { Comentario = "No procede" }, CancellationToken.None);

        Assert.Equal("rechazada", otraSolicitud.Estado);
        Assert.Equal("No procede", otraSolicitud.ComentarioRevision);
        solicitudes.Verify(x => x.Update(otraSolicitud), Times.Once);
    }

    [Fact]
    public async Task Solicitar_eliminar_notifies_the_responsible_gerente_when_pendiente_gerente()
    {
        var solicitudes = new Mock<ISolicitudEliminacionRepository>();
        var proyectos = new Mock<IProyectoRepository>();
        var notificaciones = new Mock<INotificacionRepository>();
        var proyectoId = Guid.NewGuid();
        var gerenteId = Guid.NewGuid();
        proyectos.Setup(x => x.GetByIdAsync(proyectoId, It.IsAny<CancellationToken>())).ReturnsAsync(new Proyecto { Id = proyectoId, GerenteId = gerenteId });
        Notificacion? enviada = null;
        notificaciones.Setup(x => x.AddAsync(It.IsAny<Notificacion>(), It.IsAny<CancellationToken>()))
            .Callback<Notificacion, CancellationToken>((n, _) => enviada = n).Returns(Task.CompletedTask);

        await new SolicitarEliminacionUseCase(solicitudes.Object, proyectos.Object, Mock.Of<IUsuarioRepository>(), notificaciones.Object, Mock.Of<IUnitOfWork>())
            .ExecuteAsync(new CrearSolicitudEliminacionDto { TipoEntidad = "proyecto", EntidadId = proyectoId }, Guid.NewGuid());

        Assert.NotNull(enviada);
        Assert.Equal(gerenteId, enviada!.UsuarioDestinatarioId);
        Assert.Equal("solicitud_eliminacion_creada", enviada.Tipo);
    }

    [Fact]
    public async Task Solicitar_eliminar_notifies_only_active_admins_and_mentions_how_many_are_pending()
    {
        // "que sepa cuántas solicitudes tiene" (docs/19): si ya había 2 solicitudes pendientes para
        // este mismo proveedor, la notificación al administrador debe mencionar que ya van 3 (la que
        // se acaba de crear cuenta también) -- y solo debe notificarse a administradores activos.
        var solicitudes = new Mock<ISolicitudEliminacionRepository>();
        solicitudes.Setup(x => x.GetOtrasPendientesPorEntidadAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([new SolicitudEliminacion(), new SolicitudEliminacion()]);
        var usuarios = new Mock<IUsuarioRepository>();
        var adminActivoId = Guid.NewGuid();
        usuarios.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(
        [
            new Usuario { Id = adminActivoId, Rol = Roles.Admin, Activo = true },
            new Usuario { Id = Guid.NewGuid(), Rol = Roles.Admin, Activo = false },
            new Usuario { Id = Guid.NewGuid(), Rol = Roles.Manager, Activo = true },
        ]);
        var notificaciones = new Mock<INotificacionRepository>();
        var enviadas = new List<Notificacion>();
        notificaciones.Setup(x => x.AddAsync(It.IsAny<Notificacion>(), It.IsAny<CancellationToken>()))
            .Callback<Notificacion, CancellationToken>((n, _) => enviadas.Add(n)).Returns(Task.CompletedTask);

        await new SolicitarEliminacionUseCase(solicitudes.Object, Mock.Of<IProyectoRepository>(), usuarios.Object, notificaciones.Object, Mock.Of<IUnitOfWork>())
            .ExecuteAsync(new CrearSolicitudEliminacionDto { TipoEntidad = "proveedor", EntidadId = Guid.NewGuid() }, Guid.NewGuid());

        var enviada = Assert.Single(enviadas);
        Assert.Equal(adminActivoId, enviada.UsuarioDestinatarioId);
        Assert.Contains("3 solicitudes", enviada.Mensaje);
    }

    [Fact]
    public async Task AprobarComoAdmin_rejects_a_request_not_awaiting_an_admin()
    {
        var solicitudes = new Mock<ISolicitudEliminacionRepository>();
        var solicitudId = Guid.NewGuid();
        solicitudes.Setup(x => x.GetByIdAsync(solicitudId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SolicitudEliminacion { Id = solicitudId, Estado = "pendiente_gerente", TipoEntidad = "cliente", EntidadId = Guid.NewGuid(), SolicitadoPorId = Guid.NewGuid() });

        await Assert.ThrowsAsync<BusinessRuleException>(() => new AprobarComoAdminUseCase(solicitudes.Object, Mock.Of<IClienteRepository>(), Mock.Of<IProveedorRepository>(), Mock.Of<IProyectoRepository>(), Mock.Of<IUsuarioRepository>(), Mock.Of<IUsuarioEliminadoRepository>(), Mock.Of<ISupabaseAuthAdminService>(), Mock.Of<INotificacionRepository>(), Mock.Of<IUnitOfWork>())
            .ExecuteAsync(solicitudId, Guid.NewGuid(), new RevisionSolicitudDto(), CancellationToken.None));
    }

    [Fact]
    public async Task RechazarComoAdmin_sets_rechazada_without_deleting_anything()
    {
        var solicitudes = new Mock<ISolicitudEliminacionRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var solicitudId = Guid.NewGuid();
        var solicitud = new SolicitudEliminacion { Id = solicitudId, Estado = "pendiente_admin", TipoEntidad = "proyecto", EntidadId = Guid.NewGuid(), SolicitadoPorId = Guid.NewGuid() };
        solicitudes.Setup(x => x.GetByIdAsync(solicitudId, It.IsAny<CancellationToken>())).ReturnsAsync(solicitud);
        solicitudes.Setup(x => x.GetOtrasPendientesPorEntidadAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync([]);

        var result = await new RechazarComoAdminUseCase(solicitudes.Object, Mock.Of<INotificacionRepository>(), unitOfWork.Object)
            .ExecuteAsync(solicitudId, Guid.NewGuid(), new RevisionSolicitudDto { Comentario = "Aún se necesita" }, CancellationToken.None);

        Assert.Equal("rechazada", result.Estado);
    }

    [Fact]
    public async Task ConsultarSolicitudes_lists_pending_for_a_specific_gerente()
    {
        var solicitudes = new Mock<ISolicitudEliminacionRepository>();
        var gerenteId = Guid.NewGuid();
        solicitudes.Setup(x => x.GetPendientesParaGerenteAsync(gerenteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([new SolicitudEliminacion { TipoEntidad = "proyecto", EntidadId = Guid.NewGuid(), SolicitadoPorId = Guid.NewGuid(), Estado = "pendiente_gerente", GerenteResponsableId = gerenteId }]);

        var result = await new ConsultarSolicitudesEliminacionUseCase(solicitudes.Object).ListPendientesParaGerenteAsync(gerenteId);

        Assert.Single(result);
        Assert.Equal(gerenteId, result[0].GerenteResponsableId);
    }

    // ---------------------------------------------------------------------
    // Eliminar personas por solicitud (docs/40) -- antes era DELETE /api/usuarios/{id} directo.
    // ---------------------------------------------------------------------

    /// <summary>Arma el caso de uso con lo mínimo, para no repetir nueve argumentos en cada test.</summary>
    private static SolicitarEliminacionUseCase Solicitador(
        ISolicitudEliminacionRepository solicitudes, IUsuarioRepository usuarios, INotificacionRepository? notificaciones = null) =>
        new(solicitudes, Mock.Of<IProyectoRepository>(), usuarios, notificaciones ?? Mock.Of<INotificacionRepository>(), Mock.Of<IUnitOfWork>());

    private static Mock<IUsuarioRepository> UsuariosCon(params Usuario[] usuarios)
    {
        var repo = new Mock<IUsuarioRepository>();
        repo.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(usuarios);
        foreach (var u in usuarios) repo.Setup(x => x.GetByIdAsync(u.Id, It.IsAny<CancellationToken>())).ReturnsAsync(u);
        return repo;
    }

    [Fact]
    public async Task SolicitarEliminacion_de_usuario_notifica_a_admins_y_super_admin_menos_al_solicitante_y_al_afectado()
    {
        // Lo que pidió Alicia: "le llega una notificación al administrador y al super administrador".
        // Y lo que no pidió pero sobra: que le llegue a ella misma por pedirlo, o a la persona que
        // están pidiendo eliminar.
        var superAdmin = new Usuario { Id = Guid.NewGuid(), Rol = Roles.SuperAdmin, Activo = true };
        var admin = new Usuario { Id = Guid.NewGuid(), Rol = Roles.Admin, Activo = true };
        var otroAdmin = new Usuario { Id = Guid.NewGuid(), Rol = Roles.Admin, Activo = true };
        var objetivo = new Usuario { Id = Guid.NewGuid(), Rol = Roles.Miembro, Activo = true };
        var usuarios = UsuariosCon(superAdmin, admin, otroAdmin, objetivo);
        var solicitudes = new Mock<ISolicitudEliminacionRepository>();
        solicitudes.Setup(x => x.GetOtrasPendientesPorEntidadAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync([]);
        var enviadas = new List<Notificacion>();
        var notificaciones = new Mock<INotificacionRepository>();
        notificaciones.Setup(x => x.AddAsync(It.IsAny<Notificacion>(), It.IsAny<CancellationToken>()))
            .Callback<Notificacion, CancellationToken>((n, _) => enviadas.Add(n)).Returns(Task.CompletedTask);

        var result = await Solicitador(solicitudes.Object, usuarios.Object, notificaciones.Object).ExecuteAsync(
            new CrearSolicitudEliminacionDto { TipoEntidad = TiposEntidadEliminable.Usuario, EntidadId = objetivo.Id, Motivo = "Ya no trabaja aquí" },
            admin.Id);

        Assert.Equal("pendiente_admin", result.Estado);
        Assert.Equal(2, enviadas.Count);
        Assert.Contains(enviadas, n => n.UsuarioDestinatarioId == superAdmin.Id);
        Assert.Contains(enviadas, n => n.UsuarioDestinatarioId == otroAdmin.Id);
        Assert.DoesNotContain(enviadas, n => n.UsuarioDestinatarioId == admin.Id);
        Assert.DoesNotContain(enviadas, n => n.UsuarioDestinatarioId == objetivo.Id);
        Assert.All(enviadas, n => Assert.Contains("cuenta de usuario", n.Titulo));
    }

    [Fact]
    public async Task SolicitarEliminacion_de_usuario_rechaza_pedir_la_propia()
    {
        var admin = new Usuario { Id = Guid.NewGuid(), Rol = Roles.Admin, Activo = true };
        var usuarios = UsuariosCon(admin);
        await Assert.ThrowsAsync<ForbiddenOperationException>(() => Solicitador(Mock.Of<ISolicitudEliminacionRepository>(), usuarios.Object)
            .ExecuteAsync(new CrearSolicitudEliminacionDto { TipoEntidad = TiposEntidadEliminable.Usuario, EntidadId = admin.Id }, admin.Id));
    }

    [Fact]
    public async Task SolicitarEliminacion_de_usuario_rechaza_al_super_admin_como_objetivo()
    {
        var admin = new Usuario { Id = Guid.NewGuid(), Rol = Roles.Admin, Activo = true };
        var superAdmin = new Usuario { Id = Guid.NewGuid(), Rol = Roles.SuperAdmin, Activo = true };
        var usuarios = UsuariosCon(admin, superAdmin);
        await Assert.ThrowsAsync<ForbiddenOperationException>(() => Solicitador(Mock.Of<ISolicitudEliminacionRepository>(), usuarios.Object)
            .ExecuteAsync(new CrearSolicitudEliminacionDto { TipoEntidad = TiposEntidadEliminable.Usuario, EntidadId = superAdmin.Id }, admin.Id));
    }

    [Fact]
    public async Task SolicitarEliminacion_de_usuario_rechaza_a_quien_no_es_administrador()
    {
        var miembro = new Usuario { Id = Guid.NewGuid(), Rol = Roles.Miembro, Activo = true };
        var otro = new Usuario { Id = Guid.NewGuid(), Rol = Roles.Miembro, Activo = true };
        var usuarios = UsuariosCon(miembro, otro);
        await Assert.ThrowsAsync<ForbiddenOperationException>(() => Solicitador(Mock.Of<ISolicitudEliminacionRepository>(), usuarios.Object)
            .ExecuteAsync(new CrearSolicitudEliminacionDto { TipoEntidad = TiposEntidadEliminable.Usuario, EntidadId = otro.Id }, miembro.Id));
    }

    [Fact]
    public async Task AprobarComoAdmin_de_usuario_archiva_borra_el_perfil_y_la_cuenta_de_supabase()
    {
        var solicitudes = new Mock<ISolicitudEliminacionRepository>();
        var usuarios = new Mock<IUsuarioRepository>();
        var archivo = new Mock<IUsuarioEliminadoRepository>();
        var authAdmin = new Mock<ISupabaseAuthAdminService>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var solicitudId = Guid.NewGuid();
        var objetivoId = Guid.NewGuid();
        var adminId = Guid.NewGuid();
        var solicitud = new SolicitudEliminacion { Id = solicitudId, Estado = "pendiente_admin", TipoEntidad = TiposEntidadEliminable.Usuario, EntidadId = objetivoId, SolicitadoPorId = Guid.NewGuid() };
        solicitudes.Setup(x => x.GetByIdAsync(solicitudId, It.IsAny<CancellationToken>())).ReturnsAsync(solicitud);
        solicitudes.Setup(x => x.GetOtrasPendientesPorEntidadAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync([]);
        usuarios.Setup(x => x.GetByIdAsync(objetivoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Usuario { Id = objetivoId, Nombre = "Ana", Apellido = "Ruiz", Email = "ana@agencianextmkt.com", Rol = Roles.Miembro });
        UsuarioEliminado? archivado = null;
        archivo.Setup(x => x.AddAsync(It.IsAny<UsuarioEliminado>(), It.IsAny<CancellationToken>()))
            .Callback<UsuarioEliminado, CancellationToken>((u, _) => archivado = u).Returns(Task.CompletedTask);

        var result = await new AprobarComoAdminUseCase(solicitudes.Object, Mock.Of<IClienteRepository>(), Mock.Of<IProveedorRepository>(),
                Mock.Of<IProyectoRepository>(), usuarios.Object, archivo.Object, authAdmin.Object, Mock.Of<INotificacionRepository>(), unitOfWork.Object)
            .ExecuteAsync(solicitudId, adminId, new RevisionSolicitudDto(), CancellationToken.None);

        Assert.Equal("aprobada", result.Estado);
        Assert.NotNull(archivado);
        Assert.Equal(objetivoId, archivado!.UsuarioIdOriginal);
        Assert.Equal(adminId, archivado.EliminadoPorId);
        usuarios.Verify(x => x.DeleteAsync(objetivoId, It.IsAny<CancellationToken>()), Times.Once);
        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        authAdmin.Verify(x => x.EliminarCuentaAsync(objetivoId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AprobarComoAdmin_de_usuario_no_toca_supabase_si_el_perfil_ya_no_existe()
    {
        var solicitudes = new Mock<ISolicitudEliminacionRepository>();
        var usuarios = new Mock<IUsuarioRepository>();
        var authAdmin = new Mock<ISupabaseAuthAdminService>();
        var solicitudId = Guid.NewGuid();
        var solicitud = new SolicitudEliminacion { Id = solicitudId, Estado = "pendiente_admin", TipoEntidad = TiposEntidadEliminable.Usuario, EntidadId = Guid.NewGuid(), SolicitadoPorId = Guid.NewGuid() };
        solicitudes.Setup(x => x.GetByIdAsync(solicitudId, It.IsAny<CancellationToken>())).ReturnsAsync(solicitud);
        solicitudes.Setup(x => x.GetOtrasPendientesPorEntidadAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync([]);
        usuarios.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Usuario?)null);

        var result = await new AprobarComoAdminUseCase(solicitudes.Object, Mock.Of<IClienteRepository>(), Mock.Of<IProveedorRepository>(),
                Mock.Of<IProyectoRepository>(), usuarios.Object, Mock.Of<IUsuarioEliminadoRepository>(), authAdmin.Object, Mock.Of<INotificacionRepository>(), Mock.Of<IUnitOfWork>())
            .ExecuteAsync(solicitudId, Guid.NewGuid(), new RevisionSolicitudDto(), CancellationToken.None);

        Assert.Equal("aprobada", result.Estado);
        usuarios.Verify(x => x.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        authAdmin.Verify(x => x.EliminarCuentaAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AprobarComoAdmin_de_usuario_rechaza_aprobar_la_propia_cuenta()
    {
        var solicitudes = new Mock<ISolicitudEliminacionRepository>();
        var usuarios = new Mock<IUsuarioRepository>();
        var solicitudId = Guid.NewGuid();
        var adminId = Guid.NewGuid();
        solicitudes.Setup(x => x.GetByIdAsync(solicitudId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SolicitudEliminacion { Id = solicitudId, Estado = "pendiente_admin", TipoEntidad = TiposEntidadEliminable.Usuario, EntidadId = adminId, SolicitadoPorId = Guid.NewGuid() });
        usuarios.Setup(x => x.GetByIdAsync(adminId, It.IsAny<CancellationToken>())).ReturnsAsync(new Usuario { Id = adminId, Rol = Roles.Admin });

        await Assert.ThrowsAsync<ForbiddenOperationException>(() => new AprobarComoAdminUseCase(solicitudes.Object, Mock.Of<IClienteRepository>(), Mock.Of<IProveedorRepository>(),
                Mock.Of<IProyectoRepository>(), usuarios.Object, Mock.Of<IUsuarioEliminadoRepository>(), Mock.Of<ISupabaseAuthAdminService>(), Mock.Of<INotificacionRepository>(), Mock.Of<IUnitOfWork>())
            .ExecuteAsync(solicitudId, adminId, new RevisionSolicitudDto(), CancellationToken.None));
    }

    [Fact]
    public async Task DecisionParaSolicitante_no_estalla_cuando_quien_pidio_ya_no_existe()
    {
        // SolicitadoPorId queda en null cuando esa cuenta se elimina (FK ON DELETE SET NULL). Antes de
        // eso el FK era RESTRICT y la fila ni siquiera podía quedar huérfana; ahora sí, y decidir esa
        // solicitud no debe reventar por no tener a quién notificar.
        var solicitudes = new Mock<ISolicitudEliminacionRepository>();
        var notificaciones = new Mock<INotificacionRepository>();
        var solicitudId = Guid.NewGuid();
        solicitudes.Setup(x => x.GetByIdAsync(solicitudId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SolicitudEliminacion { Id = solicitudId, Estado = "pendiente_admin", TipoEntidad = TiposEntidadEliminable.Cliente, EntidadId = Guid.NewGuid(), SolicitadoPorId = null });
        solicitudes.Setup(x => x.GetOtrasPendientesPorEntidadAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync([]);

        var result = await new RechazarComoAdminUseCase(solicitudes.Object, notificaciones.Object, Mock.Of<IUnitOfWork>())
            .ExecuteAsync(solicitudId, Guid.NewGuid(), new RevisionSolicitudDto(), CancellationToken.None);

        Assert.Equal("rechazada", result.Estado);
        notificaciones.Verify(x => x.AddAsync(It.IsAny<Notificacion>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
