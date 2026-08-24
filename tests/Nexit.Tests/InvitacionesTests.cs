using Moq;
using Nexit.Application.DTOs.Invitaciones;
using Nexit.Application.UseCases.Invitaciones;
using Nexit.Core.Constants;
using Nexit.Core.Entities;
using Nexit.Core.Exceptions;
using Nexit.Core.Interfaces;

namespace Nexit.Tests;

/// <summary>
/// Invitar y registrar a alguien del equipo en un solo paso (docs/10 sección 5, docs/25).
/// </summary>
public class InvitacionesTests
{
    private static readonly Guid AdminId = Guid.NewGuid();

    [Fact]
    public async Task CrearInvitacion_dispara_la_invitacion_real_antes_de_guardarla()
    {
        var repo = new Mock<IInvitacionEquipoRepository>();
        var authAdmin = new Mock<ISupabaseAuthAdminService>();
        var uow = new Mock<IUnitOfWork>();
        var dto = new CrearInvitacionDto { Email = "nueva@agencianextmkt.com", Rol = "miembro", Mensaje = "bienvenida" };

        var result = await new CrearInvitacionUseCase(repo.Object, authAdmin.Object, uow.Object).ExecuteAsync(dto, AdminId);

        authAdmin.Verify(x => x.InvitarUsuarioAsync("nueva@agencianextmkt.com", It.IsAny<CancellationToken>()), Times.Once);
        repo.Verify(x => x.AddAsync(It.Is<InvitacionEquipo>(i => i.Estado == EstadosInvitacion.Pendiente && i.InvitadoPorId == AdminId), It.IsAny<CancellationToken>()), Times.Once);
        Assert.Equal(EstadosInvitacion.Pendiente, result.Estado);
    }

    [Fact]
    public async Task CrearInvitacion_no_guarda_nada_si_Supabase_falla()
    {
        var repo = new Mock<IInvitacionEquipoRepository>();
        var authAdmin = new Mock<ISupabaseAuthAdminService>();
        authAdmin.Setup(x => x.InvitarUsuarioAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new BusinessRuleException("no configurado"));
        var uow = new Mock<IUnitOfWork>();
        var dto = new CrearInvitacionDto { Email = "nueva@agencianextmkt.com", Rol = "miembro" };

        await Assert.ThrowsAsync<BusinessRuleException>(() => new CrearInvitacionUseCase(repo.Object, authAdmin.Object, uow.Object).ExecuteAsync(dto, AdminId));

        repo.Verify(x => x.AddAsync(It.IsAny<InvitacionEquipo>(), It.IsAny<CancellationToken>()), Times.Never);
        uow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ConsultarMiInvitacion_devuelve_null_si_no_hay_ninguna_pendiente()
    {
        var repo = new Mock<IInvitacionEquipoRepository>();
        repo.Setup(x => x.GetPendientePorEmailAsync("alguien@agencianextmkt.com", It.IsAny<CancellationToken>())).ReturnsAsync((InvitacionEquipo?)null);

        var result = await new ConsultarMiInvitacionUseCase(repo.Object).ExecuteAsync("alguien@agencianextmkt.com");

        Assert.Null(result);
    }

    [Fact]
    public async Task ConsultarMiInvitacion_devuelve_la_pendiente_con_su_mensaje()
    {
        var repo = new Mock<IInvitacionEquipoRepository>();
        var invitacion = new InvitacionEquipo { Email = "alguien@agencianextmkt.com", Rol = "manager", Mensaje = "bienvenido", Estado = EstadosInvitacion.Pendiente };
        repo.Setup(x => x.GetPendientePorEmailAsync("alguien@agencianextmkt.com", It.IsAny<CancellationToken>())).ReturnsAsync(invitacion);

        var result = await new ConsultarMiInvitacionUseCase(repo.Object).ExecuteAsync("alguien@agencianextmkt.com");

        Assert.NotNull(result);
        Assert.Equal("manager", result!.Rol);
        Assert.Equal("bienvenido", result.Mensaje);
    }

    [Fact]
    public async Task AceptarInvitacion_crea_el_perfil_con_el_rol_propuesto_y_el_propio_uuid_de_quien_acepta()
    {
        var invitacionId = Guid.NewGuid();
        var nuevaId = Guid.NewGuid();
        var invitacion = new InvitacionEquipo { Id = invitacionId, Email = "nueva@agencianextmkt.com", Rol = "manager", Estado = EstadosInvitacion.Pendiente };
        var invitaciones = new Mock<IInvitacionEquipoRepository>();
        invitaciones.Setup(x => x.GetByIdAsync(invitacionId, It.IsAny<CancellationToken>())).ReturnsAsync(invitacion);
        var usuarios = new Mock<IUsuarioRepository>();
        usuarios.Setup(x => x.GetByIdAsync(nuevaId, It.IsAny<CancellationToken>())).ReturnsAsync((Usuario?)null);
        var uow = new Mock<IUnitOfWork>();
        var dto = new AceptarInvitacionDto { Nombre = "Ana", Apellido = "Pérez" };

        var result = await new AceptarInvitacionUseCase(invitaciones.Object, usuarios.Object, uow.Object)
            .ExecuteAsync(invitacionId, dto, nuevaId, "nueva@agencianextmkt.com");

        Assert.Equal("manager", result.Rol);
        Assert.Equal(EstadosInvitacion.Aceptada, invitacion.Estado);
        Assert.NotNull(invitacion.FechaRespuesta);
        usuarios.Verify(x => x.AddAsync(It.Is<Usuario>(u => u.Id == nuevaId && u.Rol == "manager" && u.Nombre == "Ana"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AceptarInvitacion_rechaza_si_el_correo_no_coincide()
    {
        var invitacionId = Guid.NewGuid();
        var invitacion = new InvitacionEquipo { Id = invitacionId, Email = "nueva@agencianextmkt.com", Rol = "miembro", Estado = EstadosInvitacion.Pendiente };
        var invitaciones = new Mock<IInvitacionEquipoRepository>();
        invitaciones.Setup(x => x.GetByIdAsync(invitacionId, It.IsAny<CancellationToken>())).ReturnsAsync(invitacion);
        var usuarios = new Mock<IUsuarioRepository>();
        var uow = new Mock<IUnitOfWork>();

        await Assert.ThrowsAsync<ForbiddenOperationException>(() =>
            new AceptarInvitacionUseCase(invitaciones.Object, usuarios.Object, uow.Object)
                .ExecuteAsync(invitacionId, new AceptarInvitacionDto { Nombre = "X", Apellido = "Y" }, Guid.NewGuid(), "otro@agencianextmkt.com"));
    }

    [Fact]
    public async Task AceptarInvitacion_rechaza_si_ya_se_respondio_antes()
    {
        var invitacionId = Guid.NewGuid();
        var invitacion = new InvitacionEquipo { Id = invitacionId, Email = "nueva@agencianextmkt.com", Rol = "miembro", Estado = EstadosInvitacion.Rechazada };
        var invitaciones = new Mock<IInvitacionEquipoRepository>();
        invitaciones.Setup(x => x.GetByIdAsync(invitacionId, It.IsAny<CancellationToken>())).ReturnsAsync(invitacion);
        var usuarios = new Mock<IUsuarioRepository>();
        var uow = new Mock<IUnitOfWork>();

        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            new AceptarInvitacionUseCase(invitaciones.Object, usuarios.Object, uow.Object)
                .ExecuteAsync(invitacionId, new AceptarInvitacionDto { Nombre = "X", Apellido = "Y" }, Guid.NewGuid(), "nueva@agencianextmkt.com"));
    }

    [Fact]
    public async Task AceptarInvitacion_rechaza_si_quien_acepta_ya_tiene_perfil()
    {
        var invitacionId = Guid.NewGuid();
        var yaExisteId = Guid.NewGuid();
        var invitacion = new InvitacionEquipo { Id = invitacionId, Email = "nueva@agencianextmkt.com", Rol = "miembro", Estado = EstadosInvitacion.Pendiente };
        var invitaciones = new Mock<IInvitacionEquipoRepository>();
        invitaciones.Setup(x => x.GetByIdAsync(invitacionId, It.IsAny<CancellationToken>())).ReturnsAsync(invitacion);
        var usuarios = new Mock<IUsuarioRepository>();
        usuarios.Setup(x => x.GetByIdAsync(yaExisteId, It.IsAny<CancellationToken>())).ReturnsAsync(new Usuario { Id = yaExisteId });
        var uow = new Mock<IUnitOfWork>();

        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            new AceptarInvitacionUseCase(invitaciones.Object, usuarios.Object, uow.Object)
                .ExecuteAsync(invitacionId, new AceptarInvitacionDto { Nombre = "X", Apellido = "Y" }, yaExisteId, "nueva@agencianextmkt.com"));
    }

    [Fact]
    public async Task RechazarInvitacion_marca_rechazada_sin_crear_ningun_perfil()
    {
        var invitacionId = Guid.NewGuid();
        var invitacion = new InvitacionEquipo { Id = invitacionId, Email = "nueva@agencianextmkt.com", Rol = "miembro", Estado = EstadosInvitacion.Pendiente };
        var invitaciones = new Mock<IInvitacionEquipoRepository>();
        invitaciones.Setup(x => x.GetByIdAsync(invitacionId, It.IsAny<CancellationToken>())).ReturnsAsync(invitacion);
        var uow = new Mock<IUnitOfWork>();

        await new RechazarInvitacionUseCase(invitaciones.Object, uow.Object).ExecuteAsync(invitacionId, "nueva@agencianextmkt.com");

        Assert.Equal(EstadosInvitacion.Rechazada, invitacion.Estado);
        Assert.NotNull(invitacion.FechaRespuesta);
    }

    [Fact]
    public async Task RechazarInvitacion_rechaza_si_el_correo_no_coincide()
    {
        var invitacionId = Guid.NewGuid();
        var invitacion = new InvitacionEquipo { Id = invitacionId, Email = "nueva@agencianextmkt.com", Rol = "miembro", Estado = EstadosInvitacion.Pendiente };
        var invitaciones = new Mock<IInvitacionEquipoRepository>();
        invitaciones.Setup(x => x.GetByIdAsync(invitacionId, It.IsAny<CancellationToken>())).ReturnsAsync(invitacion);
        var uow = new Mock<IUnitOfWork>();

        await Assert.ThrowsAsync<ForbiddenOperationException>(() =>
            new RechazarInvitacionUseCase(invitaciones.Object, uow.Object).ExecuteAsync(invitacionId, "otro@agencianextmkt.com"));
    }

    [Fact]
    public async Task ConsultarInvitaciones_lista_todas_con_el_nombre_de_quien_invito()
    {
        var repo = new Mock<IInvitacionEquipoRepository>();
        var admin = new Usuario { Nombre = "Alicia", Apellido = "Medina" };
        repo.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(
        [
            new InvitacionEquipo { Email = "a@agencianextmkt.com", Rol = "miembro", Estado = EstadosInvitacion.Pendiente, InvitadoPor = admin },
        ]);

        var result = await new ConsultarInvitacionesUseCase(repo.Object).ListAsync();

        var item = Assert.Single(result);
        Assert.Equal("Alicia Medina", item.InvitadoPorNombre);
    }
}
