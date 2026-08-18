using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Nexit.Core.Constants;
using Nexit.Core.Entities;
using Nexit.Infrastructure.Data;
using Nexit.Tests.Integration;
using Testcontainers.PostgreSql;

namespace Nexit.Tests.Functional;

/// <summary>
/// Fixture de pruebas FUNCIONALES (ver docs/08-tipos-de-pruebas.md) -- a diferencia de
/// <see cref="Nexit.Tests.Integration.NexitApiFactory"/> (que solo verifica el pipeline de
/// autenticación/autorización, sin base de datos real), esta levanta un contenedor Postgres real
/// con Testcontainers, le aplica las migraciones REALES de EF Core (las mismas que se aplicarían en
/// producción), y deja la aplicación completa lista para peticiones HTTP de extremo a extremo: la
/// petición pasa por el controlador, el caso de uso, EF Core y termina escribiendo/leyendo filas de
/// verdad en Postgres. Esto es justamente lo que las pruebas unitarias (mockean los repositorios) y
/// las de integración (sin base de datos) no pueden detectar -- por ejemplo, un tipo de columna que
/// no cuadra con Postgres, o una migración que en realidad no aplica limpio.
///
/// Requiere Docker corriendo en la máquina que ejecuta las pruebas (local o CI) -- si Docker no está
/// disponible, InitializeAsync falla con un mensaje claro en vez de colgarse.
/// </summary>
public class NexitFunctionalApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer dbContainer = new PostgreSqlBuilder("postgres:16-alpine")
        .WithDatabase("nexit_functional_tests")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    /// <summary>Id del único EstadoProyecto sembrado al iniciar -- los proyectos requieren un EstadoId válido (FK NOT NULL) y el trigger que lo autocompleta (ver nexus_schema_v2.sql) es SQL crudo, no una migración de EF, así que aquí hay que indicarlo explícitamente.</summary>
    public Guid EstadoProyectoSembradoId { get; private set; }

    /// <summary>
    /// Un Usuario real sembrado por cada rol (super_admin/admin/manager/miembro). Varias tablas
    /// (clientes.created_by, proveedores.created_by, proyectos.gerente_id, etc.) tienen FK NOT NULL
    /// hacia usuarios -- a diferencia de las pruebas de integración (sin base de datos, donde un
    /// Guid.NewGuid() cualquiera sirve como "usuario autenticado"), aquí ese id tiene que existir de
    /// verdad en la tabla o Postgres rechaza el INSERT. <see cref="FunctionalTestBase.ClientAs"/> usa
    /// este diccionario por defecto para que las pruebas no tengan que sembrar un usuario a mano cada vez.
    /// </summary>
    public IReadOnlyDictionary<string, Guid> UsuariosSembradosPorRol { get; private set; } = new Dictionary<string, Guid>();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureServices(services =>
        {
            // No basta con sobrescribir ConnectionStrings:DefaultConnection vía configuración: Program.cs
            // llama a AddInfrastructure(builder.Configuration), que lee y resuelve la cadena de conexión
            // de forma síncrona en ese mismo momento (para armar las DbContextOptions), antes de que este
            // hook de WebApplicationFactory tenga oportunidad de aplicarse -- así que ese registro ya quedó
            // apuntando al Postgres local de appsettings.Development.json (que no existe en este entorno).
            // En vez de pelear con el orden de configuración, se quita ese registro y se vuelve a registrar
            // el DbContext apuntando directo al contenedor real de Testcontainers.
            services.RemoveAll<DbContextOptions<NexitDbContext>>();
            services.RemoveAll<NexitDbContext>();
            services.AddDbContext<NexitDbContext>(options => options
                .UseNpgsql(dbContainer.GetConnectionString(), npgsql => npgsql.MigrationsAssembly(typeof(NexitDbContext).Assembly.FullName))
                .UseSnakeCaseNamingConvention());

            services.AddAuthentication(TestAuthHandler.SchemeName)
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, _ => { });
        });
    }

    public async Task InitializeAsync()
    {
        await dbContainer.StartAsync();

        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NexitDbContext>();
        await db.Database.MigrateAsync();

        // El trigger check_usuario_dominio_correo() (SQL crudo, ver docs/schema/nexus_schema_v2.sql y
        // la migración AddLocalDatabaseRules) exige que el email de cada usuario termine en un dominio
        // de la tabla dominios_correo_permitidos -- hay que sembrarlo ANTES de insertar usuarios (en su
        // propio SaveChanges, porque el trigger no conoce las relaciones de EF Core, solo ve filas ya
        // confirmadas en la base).
        db.DominiosCorreoPermitidos.Add(new DominioCorreoPermitido { Dominio = "nexit-test.com" });
        await db.SaveChangesAsync();

        var fase = new FaseProyecto { Fase = 1, Nombre = "Planeación" };
        db.FasesProyecto.Add(fase);
        var estado = new EstadoProyecto { Nombre = "Planeación interna", Fase = 1, Orden = 1 };
        db.EstadosProyecto.Add(estado);

        var usuariosPorRol = new Dictionary<string, Guid>();
        foreach (var rol in Roles.Todos)
        {
            var usuario = new Usuario { Nombre = "Usuario", Apellido = rol, Email = $"{rol}@nexit-test.com", Rol = rol, Activo = true };
            db.Usuarios.Add(usuario);
            usuariosPorRol[rol] = usuario.Id;
        }
        UsuariosSembradosPorRol = usuariosPorRol;

        await db.SaveChangesAsync();
        EstadoProyectoSembradoId = estado.Id;
    }

    async Task IAsyncLifetime.DisposeAsync() => await dbContainer.DisposeAsync();
}
