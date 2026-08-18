using Microsoft.Extensions.DependencyInjection;
using Nexit.Core.Entities;
using Nexit.Infrastructure.Data;
using Nexit.Tests.Integration;

namespace Nexit.Tests.Functional;

/// <summary>Punto común para todas las pruebas funcionales: crea un HttpClient autenticado como un rol (y, opcionalmente, un id de usuario fijo -- ver TestAuthHandler) contra la app real levantada sobre Postgres real.</summary>
[Collection("Funcional")]
public abstract class FunctionalTestBase(NexitFunctionalApiFactory factory)
{
    /// <summary>
    /// Cliente HTTP autenticado como el rol dado. Por defecto actúa como el Usuario real sembrado
    /// para ese rol (ver <see cref="NexitFunctionalApiFactory.UsuariosSembradosPorRol"/>) -- necesario
    /// porque created_by/gerente_id, etc. tienen FK reales hacia usuarios. Pasa userId explícito solo
    /// cuando la prueba necesita actuar como un usuario adicional/distinto (ej. dos gerentes distintos).
    /// </summary>
    protected HttpClient ClientAs(string role, Guid? userId = null)
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.TestAuthHeader, role);
        var idAUsar = userId ?? (factory.UsuariosSembradosPorRol.TryGetValue(role, out var sembrado) ? sembrado : (Guid?)null);
        if (idAUsar.HasValue) client.DefaultRequestHeaders.Add(TestAuthHandler.TestUserIdHeader, idAUsar.Value.ToString());
        return client;
    }

    protected Guid EstadoProyectoId => factory.EstadoProyectoSembradoId;

    /// <summary>Id del Usuario real sembrado para ese rol (ver <see cref="NexitFunctionalApiFactory.UsuariosSembradosPorRol"/>) -- para pruebas que necesitan el id sin pasar por ClientAs (ej. comprobar que NO quedó asignado a otro usuario).</summary>
    protected Guid UsuarioSembradoId(string rol) => factory.UsuariosSembradosPorRol[rol];

    /// <summary>
    /// Siembra un Usuario real ADICIONAL con el rol dado -- para pruebas que necesitan dos identidades
    /// distintas del mismo rol (ej. dos gerentes distintos, para probar que uno no puede aprobar la
    /// solicitud de eliminación de un proyecto cuyo gerente responsable es el OTRO -- BOLA/autorización
    /// a nivel de objeto, ver docs/08-tipos-de-pruebas.md). Los usuarios de <see cref="ClientAs"/> por
    /// defecto solo cubren un usuario por rol, insuficiente para este tipo de prueba.
    /// </summary>
    protected async Task<Guid> CrearUsuarioAdicionalAsync(string rol)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NexitDbContext>();
        var usuario = new Usuario { Nombre = "Usuario adicional", Apellido = rol, Email = $"{Guid.NewGuid():N}@nexit-test.com", Rol = rol, Activo = true };
        db.Usuarios.Add(usuario);
        await db.SaveChangesAsync();
        return usuario.Id;
    }
}
