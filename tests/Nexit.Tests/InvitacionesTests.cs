using FluentValidation;
using FluentValidation.Results;
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

        var result = await new AceptarInvitacionUseCase(invitaciones.Object, usuarios.Object, Mock.Of<INotificacionRepository>(), uow.Object)
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
            new AceptarInvitacionUseCase(invitaciones.Object, usuarios.Object, Mock.Of<INotificacionRepository>(), uow.Object)
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
            new AceptarInvitacionUseCase(invitaciones.Object, usuarios.Object, Mock.Of<INotificacionRepository>(), uow.Object)
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
            new AceptarInvitacionUseCase(invitaciones.Object, usuarios.Object, Mock.Of<INotificacionRepository>(), uow.Object)
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

        await new RechazarInvitacionUseCase(invitaciones.Object, Mock.Of<INotificacionRepository>(), uow.Object).ExecuteAsync(invitacionId, "nueva@agencianextmkt.com");

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
            new RechazarInvitacionUseCase(invitaciones.Object, Mock.Of<INotificacionRepository>(), uow.Object).ExecuteAsync(invitacionId, "otro@agencianextmkt.com"));
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

    // --- Invitar a varios correos de una sola vez (lote). Lo importante acá no es que "funcione"
    // sino que NUNCA sea todo-o-nada: un correo malo dentro del lote no puede impedir que los
    // demás se inviten, porque quien invita escribe varios de corrido y un dedazo en uno solo
    // haría perder el envío entero.

    private static CrearInvitacionesLoteUseCase LoteConValidador(
        Mock<ICrearInvitacionUseCase> crear, Mock<IValidator<CrearInvitacionDto>> validador) =>
        new(crear.Object, validador.Object);

    private static Mock<IValidator<CrearInvitacionDto>> ValidadorQueAcepta(params string[] emailsInvalidos)
    {
        var validador = new Mock<IValidator<CrearInvitacionDto>>();
        validador.Setup(x => x.ValidateAsync(It.IsAny<CrearInvitacionDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CrearInvitacionDto dto, CancellationToken _) => emailsInvalidos.Contains(dto.Email)
                ? new ValidationResult([new ValidationFailure(nameof(dto.Email), "Correo no permitido.")])
                : new ValidationResult());
        return validador;
    }

    private static Mock<ICrearInvitacionUseCase> CrearQueDevuelveLaInvitacion()
    {
        var crear = new Mock<ICrearInvitacionUseCase>();
        crear.Setup(x => x.ExecuteAsync(It.IsAny<CrearInvitacionDto>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CrearInvitacionDto dto, Guid _, CancellationToken _) =>
                new InvitacionResponseDto { Email = dto.Email, Rol = dto.Rol, Estado = EstadosInvitacion.Pendiente });
        return crear;
    }

    [Fact]
    public async Task CrearInvitacionesLote_invita_a_todos_los_correos_validos()
    {
        var crear = CrearQueDevuelveLaInvitacion();
        var dto = new CrearInvitacionesLoteDto
        {
            Emails = ["una@agencianextmkt.com", "otra@agencianextmkt.com"],
            Rol = "manager",
            Mensaje = "bienvenidas"
        };

        var resultado = await LoteConValidador(crear, ValidadorQueAcepta()).ExecuteAsync(dto, AdminId);

        Assert.Equal(2, resultado.Enviadas.Count);
        Assert.Empty(resultado.Fallidas);
        Assert.All(resultado.Enviadas, i => Assert.Equal("manager", i.Rol));
    }

    [Fact]
    public async Task CrearInvitacionesLote_un_correo_invalido_no_impide_invitar_a_los_demas()
    {
        var crear = CrearQueDevuelveLaInvitacion();
        var dto = new CrearInvitacionesLoteDto
        {
            Emails = ["buena@agencianextmkt.com", "mala@otro-dominio.com", "otra@agencianextmkt.com"],
            Rol = "miembro"
        };

        var resultado = await LoteConValidador(crear, ValidadorQueAcepta("mala@otro-dominio.com")).ExecuteAsync(dto, AdminId);

        Assert.Equal(2, resultado.Enviadas.Count);
        var fallida = Assert.Single(resultado.Fallidas);
        Assert.Equal("mala@otro-dominio.com", fallida.Email);
        Assert.Contains("no permitido", fallida.Motivo);
        crear.Verify(x => x.ExecuteAsync(It.Is<CrearInvitacionDto>(d => d.Email == "mala@otro-dominio.com"), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CrearInvitacionesLote_una_falla_de_Supabase_en_un_correo_no_tumba_el_resto()
    {
        var crear = CrearQueDevuelveLaInvitacion();
        crear.Setup(x => x.ExecuteAsync(It.Is<CrearInvitacionDto>(d => d.Email == "repetida@agencianextmkt.com"), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new BusinessRuleException("Ese correo ya tiene una cuenta en Supabase Auth."));
        var dto = new CrearInvitacionesLoteDto
        {
            Emails = ["buena@agencianextmkt.com", "repetida@agencianextmkt.com"],
            Rol = "miembro"
        };

        var resultado = await LoteConValidador(crear, ValidadorQueAcepta()).ExecuteAsync(dto, AdminId);

        Assert.Single(resultado.Enviadas);
        Assert.Equal("buena@agencianextmkt.com", resultado.Enviadas[0].Email);
        var fallida = Assert.Single(resultado.Fallidas);
        Assert.Equal("repetida@agencianextmkt.com", fallida.Email);
        Assert.Contains("ya tiene una cuenta", fallida.Motivo);
    }

    [Fact]
    public async Task CrearInvitacionesLote_ignora_repetidos_y_espacios_del_mismo_envio()
    {
        var crear = CrearQueDevuelveLaInvitacion();
        var dto = new CrearInvitacionesLoteDto
        {
            Emails = ["  una@agencianextmkt.com ", "UNA@agencianextmkt.com", "", "   "],
            Rol = "miembro"
        };

        var resultado = await LoteConValidador(crear, ValidadorQueAcepta()).ExecuteAsync(dto, AdminId);

        Assert.Single(resultado.Enviadas);
        Assert.Empty(resultado.Fallidas);
        crear.Verify(x => x.ExecuteAsync(It.IsAny<CrearInvitacionDto>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    // --- Cancelar una invitación pendiente (2026-09-08). Distinto de rechazar: lo hace quien
    // invitó, no la persona invitada.

    [Fact]
    public async Task CancelarInvitacion_borra_la_pendiente_y_libera_el_correo()
    {
        var invitacionId = Guid.NewGuid();
        var repo = new Mock<IInvitacionEquipoRepository>();
        repo.Setup(x => x.GetByIdAsync(invitacionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InvitacionEquipo { Id = invitacionId, Email = "nueva@agencianextmkt.com", Estado = EstadosInvitacion.Pendiente });
        var uow = new Mock<IUnitOfWork>();

        await new CancelarInvitacionUseCase(repo.Object, uow.Object).ExecuteAsync(invitacionId);

        repo.Verify(x => x.DeleteAsync(invitacionId, It.IsAny<CancellationToken>()), Times.Once);
        uow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(EstadosInvitacion.Aceptada)]
    [InlineData(EstadosInvitacion.Rechazada)]
    public async Task CancelarInvitacion_rechaza_una_que_ya_se_respondio(string estado)
    {
        var invitacionId = Guid.NewGuid();
        var repo = new Mock<IInvitacionEquipoRepository>();
        repo.Setup(x => x.GetByIdAsync(invitacionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InvitacionEquipo { Id = invitacionId, Email = "nueva@agencianextmkt.com", Estado = estado });
        var uow = new Mock<IUnitOfWork>();

        await Assert.ThrowsAsync<BusinessRuleException>(() => new CancelarInvitacionUseCase(repo.Object, uow.Object).ExecuteAsync(invitacionId));

        repo.Verify(x => x.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CancelarInvitacion_falla_si_no_existe()
    {
        var repo = new Mock<IInvitacionEquipoRepository>();
        var uow = new Mock<IUnitOfWork>();

        await Assert.ThrowsAsync<EntityNotFoundException>(() => new CancelarInvitacionUseCase(repo.Object, uow.Object).ExecuteAsync(Guid.NewGuid()));
    }

    // --- La campanita (2026-09-08). Antes, quien invitaba solo se enteraba de la respuesta si
    // entraba a Usuarios y notaba que la invitación había desaparecido de la lista de pendientes.

    [Fact]
    public async Task AceptarInvitacion_le_avisa_a_quien_invito()
    {
        var invitacionId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var invitaciones = new Mock<IInvitacionEquipoRepository>();
        invitaciones.Setup(x => x.GetByIdAsync(invitacionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InvitacionEquipo { Id = invitacionId, Email = "nueva@agencianextmkt.com", Rol = "manager", Estado = EstadosInvitacion.Pendiente, InvitadoPorId = AdminId });
        var usuarios = new Mock<IUsuarioRepository>();
        var notificaciones = new Mock<INotificacionRepository>();

        await new AceptarInvitacionUseCase(invitaciones.Object, usuarios.Object, notificaciones.Object, Mock.Of<IUnitOfWork>())
            .ExecuteAsync(invitacionId, new AceptarInvitacionDto { Nombre = "Nueva", Apellido = "Persona" }, usuarioId, "nueva@agencianextmkt.com");

        notificaciones.Verify(x => x.AddAsync(
            It.Is<Notificacion>(n => n.UsuarioDestinatarioId == AdminId && n.Tipo == "invitacion_aceptada" && n.Mensaje.Contains("nueva@agencianextmkt.com")),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RechazarInvitacion_le_avisa_a_quien_invito()
    {
        var invitacionId = Guid.NewGuid();
        var invitaciones = new Mock<IInvitacionEquipoRepository>();
        invitaciones.Setup(x => x.GetByIdAsync(invitacionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InvitacionEquipo { Id = invitacionId, Email = "nueva@agencianextmkt.com", Estado = EstadosInvitacion.Pendiente, InvitadoPorId = AdminId });
        var notificaciones = new Mock<INotificacionRepository>();

        await new RechazarInvitacionUseCase(invitaciones.Object, notificaciones.Object, Mock.Of<IUnitOfWork>())
            .ExecuteAsync(invitacionId, "nueva@agencianextmkt.com");

        notificaciones.Verify(x => x.AddAsync(
            It.Is<Notificacion>(n => n.UsuarioDestinatarioId == AdminId && n.Tipo == "invitacion_rechazada"),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}

