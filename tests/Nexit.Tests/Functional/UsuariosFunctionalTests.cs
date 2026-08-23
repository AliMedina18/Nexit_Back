using System.Net;
using System.Net.Http.Json;
using Nexit.Application.DTOs.Usuarios;

namespace Nexit.Tests.Functional;

/// <summary>
/// Pruebas funcionales de la validación de dominio de correo al registrar el perfil de negocio de
/// un usuario (ver docs/09-crear-proyecto-supabase-paso-a-paso.md) -- el respaldo de aplicación al
/// trigger de Postgres check_usuario_dominio_correo.
/// </summary>
public class UsuariosFunctionalTests(NexitFunctionalApiFactory factory) : FunctionalTestBase(factory)
{
    [Fact]
    public async Task No_se_puede_registrar_un_usuario_con_correo_de_dominio_no_permitido()
    {
        var client = ClientAs("super_admin");

        var response = await client.PostAsJsonAsync("/api/usuarios", new CreateUsuarioDto
        {
            Id = Guid.NewGuid(),
            Nombre = "Persona",
            Apellido = "Externa",
            Email = $"{Guid.NewGuid():N}@dominio-no-permitido.com",
            Rol = "miembro"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Si_se_puede_registrar_un_usuario_con_correo_de_dominio_permitido()
    {
        var client = ClientAs("super_admin");

        var response = await client.PostAsJsonAsync("/api/usuarios", new CreateUsuarioDto
        {
            Id = Guid.NewGuid(),
            Nombre = "Persona",
            Apellido = "Del equipo",
            Email = $"{Guid.NewGuid():N}@nexit-test.com",
            Rol = "miembro"
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }
}
