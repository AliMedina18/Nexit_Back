using System.Net;
using System.Net.Http.Json;
using Nexit.Application.DTOs.Clientes;
using Nexit.Application.DTOs.Proyectos;
using Nexit.Application.DTOs.SolicitudesEliminacion;

namespace Nexit.Tests.Functional;

/// <summary>
/// Pruebas de seguridad de extremo a extremo contra Postgres real (ver docs/08-tipos-de-pruebas.md).
/// A diferencia de <see cref="Nexit.Tests.Security.ControllersRequierenAutorizacionTests"/> (reflexión,
/// sin HTTP) y <see cref="Nexit.Tests.Security.RateLimitingIntegrationTests"/> (integración, sin base de
/// datos), estas necesitan una base de datos real porque prueban reglas de negocio que dependen de
/// datos guardados: que el texto se guarde tal cual (nunca se ejecute como SQL), que un rol bajo no
/// pueda auto-asignarse privilegios vía el cuerpo de la petición (mass assignment), y que solo el
/// gerente responsable de un proyecto -- ni cualquier otro gerente -- pueda aprobar su eliminación
/// (autorización a nivel de objeto / BOLA, OWASP API1:2023).
/// </summary>
public class SeguridadFunctionalTests(NexitFunctionalApiFactory factory) : FunctionalTestBase(factory)
{
    [Fact]
    public async Task Un_intento_de_inyeccion_SQL_en_el_nombre_se_guarda_como_texto_literal_y_no_como_SQL()
    {
        var client = ClientAs("admin");
        // Un payload clásico de inyección SQL -- si el backend concatenara SQL a mano en vez de usar
        // EF Core/parámetros, esto podría borrar datos o romper la consulta. Con EF Core LINQ (como usa
        // este proyecto en todos los repositorios) el texto siempre viaja como parámetro, nunca como
        // SQL ejecutable -- esta prueba lo demuestra contra Postgres de verdad, no lo asume.
        var payload = "Acme'; DROP TABLE clientes; --";

        var creado = await client.PostAsJsonAsync("/api/clientes", new CreateClienteDto { Nombre = payload, Telefonos = [new ClienteTelefonoDto { Telefono = "3000000000" }] });
        Assert.Equal(HttpStatusCode.Created, creado.StatusCode);
        var cliente = await creado.Content.ReadFromJsonAsync<ClienteResponseDto>();

        // Se relee con OTRA petición -- si la tabla hubiera sido borrada, esto fallaría con 500, no
        // devolvería el texto tal cual.
        var releido = await (await client.GetAsync($"/api/clientes/{cliente!.Id}")).Content.ReadFromJsonAsync<ClienteResponseDto>();
        Assert.Equal(payload, releido!.Nombre);

        var listaSigueViva = await client.GetAsync("/api/clientes");
        Assert.Equal(HttpStatusCode.OK, listaSigueViva.StatusCode);
    }

    [Fact]
    public async Task Un_miembro_no_puede_auto_asignarse_como_gerente_de_un_proyecto_mandando_gerenteId_en_el_cuerpo()
    {
        // Mass assignment / escalamiento de privilegios (OWASP API3:2023): un "miembro" no tiene por
        // qué poder volverse el gerente responsable de un proyecto solo porque lo puso en el JSON de la
        // petición -- la regla de negocio (ProyectoRules, ver CrearProyectoUseCase) debe ignorarlo.
        var miembroId = UsuarioSembradoId("miembro");
        var otroUsuarioId = UsuarioSembradoId("admin"); // intenta autoasignarse el id de otra persona real
        var client = ClientAs("miembro");

        var respuesta = await client.PostAsJsonAsync("/api/proyectos", new CrearProyectoDto
        {
            Nombre = "Proyecto de prueba de mass assignment", EstadoId = EstadoProyectoId, GerenteId = otroUsuarioId
        });

        Assert.Equal(HttpStatusCode.Created, respuesta.StatusCode);
        var proyecto = await respuesta.Content.ReadFromJsonAsync<ProyectoResponseDto>();
        // Ni el id que mandó ("otroUsuarioId") ni el suyo propio -- un miembro nunca queda como gerente.
        Assert.Null(proyecto!.GerenteId);
        Assert.NotEqual(miembroId, proyecto.GerenteId);
    }

    [Fact]
    public async Task Un_gerente_que_NO_es_el_responsable_del_proyecto_no_puede_aprobar_su_solicitud_de_eliminacion()
    {
        // Autorización a nivel de objeto (BOLA, OWASP API1:2023): la política estática de autorización
        // (basada en rol) no alcanza aquí -- CUALQUIER gerente autenticado puede llegar al endpoint
        // aprobar-gerente (ver AuthorizationIntegrationTests), pero el caso de uso debe rechazar a
        // cualquiera que no sea EL gerente responsable de ESE proyecto en particular.
        var gerenteResponsable = ClientAs("manager"); // usuario sembrado por rol -- dueño real del proyecto
        var crearProyecto = await gerenteResponsable.PostAsJsonAsync("/api/proyectos", new CrearProyectoDto
        {
            Nombre = "Proyecto con gerente responsable", EstadoId = EstadoProyectoId
        });
        Assert.Equal(HttpStatusCode.Created, crearProyecto.StatusCode);
        var proyecto = await crearProyecto.Content.ReadFromJsonAsync<ProyectoResponseDto>();
        Assert.Equal(UsuarioSembradoId("manager"), proyecto!.GerenteId); // se auto-asignó al crear, como dueño

        var otroGerenteId = await CrearUsuarioAdicionalAsync("manager");
        var otroGerente = ClientAs("manager", otroGerenteId);
        // El otro gerente solicita la eliminación (no es el responsable -> pendiente_gerente).
        var solicitud = await otroGerente.PostAsJsonAsync("/api/solicitudeseliminacion", new CrearSolicitudEliminacionDto
        {
            TipoEntidad = "proyecto", EntidadId = proyecto.Id, Motivo = "Ya no se necesita"
        });
        Assert.Equal(HttpStatusCode.Created, solicitud.StatusCode);
        var solicitudCreada = await solicitud.Content.ReadFromJsonAsync<SolicitudEliminacionResponseDto>();
        Assert.Equal("pendiente_gerente", solicitudCreada!.Estado);

        // El MISMO gerente que la solicitó (no el responsable) intenta aprobarla como si fuera el
        // gerente del proyecto -- debe ser rechazado, aunque su rol técnicamente sí pueda llegar al endpoint.
        var intentoIndebido = await otroGerente.PutAsJsonAsync($"/api/solicitudeseliminacion/{solicitudCreada.Id}/aprobar-gerente", new { });
        Assert.Equal(HttpStatusCode.Forbidden, intentoIndebido.StatusCode);

        // El gerente responsable de verdad sí puede.
        var aprobacionValida = await gerenteResponsable.PutAsJsonAsync($"/api/solicitudeseliminacion/{solicitudCreada.Id}/aprobar-gerente", new { });
        Assert.Equal(HttpStatusCode.OK, aprobacionValida.StatusCode);
        var solicitudAprobada = await aprobacionValida.Content.ReadFromJsonAsync<SolicitudEliminacionResponseDto>();
        Assert.Equal("pendiente_admin", solicitudAprobada!.Estado);
    }
}
