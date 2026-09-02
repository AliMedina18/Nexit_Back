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

    // --- GET /api/usuarios/me (agregado 2026-08-26): cualquier autenticado ve su propio perfil,
    // no solo super_admin -- antes esto era imposible porque toda la clase exigía SuperAdminOnly.

    [Fact]
    public async Task Un_miembro_puede_ver_su_propio_perfil_con_me()
    {
        var client = ClientAs("miembro");

        var response = await client.GetAsync("/api/usuarios/me");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var perfil = await response.Content.ReadFromJsonAsync<UsuarioResponseDto>();
        Assert.Equal(UsuarioSembradoId("miembro"), perfil!.Id);
    }

    [Fact]
    public async Task Un_gerente_puede_ver_su_propio_perfil_con_me()
    {
        var client = ClientAs("manager");

        var response = await client.GetAsync("/api/usuarios/me");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var perfil = await response.Content.ReadFromJsonAsync<UsuarioResponseDto>();
        Assert.Equal(UsuarioSembradoId("manager"), perfil!.Id);
    }

    [Fact]
    public async Task Un_miembro_no_puede_listar_todos_los_usuarios()
    {
        var client = ClientAs("miembro");

        var response = await client.GetAsync("/api/usuarios");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // --- Ampliado 2026-08-26: la usuaria pidió que un administrador (no solo super_admin) pueda ver
    // el directorio completo, y que cualquiera pueda mirar (sin editar) el perfil de otra persona,
    // como el directorio de Microsoft Teams -- ver docs/06, sección 6.

    [Fact]
    public async Task Un_administrador_puede_listar_todos_los_usuarios()
    {
        var client = ClientAs("admin");

        var response = await client.GetAsync("/api/usuarios");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var lista = await response.Content.ReadFromJsonAsync<List<UsuarioResponseDto>>();
        Assert.NotEmpty(lista!);
    }

    [Fact]
    public async Task Un_miembro_puede_ver_el_perfil_de_otra_persona_por_id_pero_solo_para_mirar()
    {
        var client = ClientAs("miembro");
        var otroId = UsuarioSembradoId("admin");

        var response = await client.GetAsync($"/api/usuarios/{otroId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var perfil = await response.Content.ReadFromJsonAsync<UsuarioResponseDto>();
        Assert.Equal(otroId, perfil!.Id);
    }

    [Fact]
    public async Task Un_miembro_no_puede_editar_el_perfil_de_otra_persona()
    {
        var client = ClientAs("miembro");
        var otroId = UsuarioSembradoId("admin");

        var response = await client.PutAsJsonAsync($"/api/usuarios/{otroId}", new UpdateUsuarioDto
        {
            Nombre = "Intento",
            Apellido = "De edición",
            Rol = "admin",
            Activo = true,
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Un_miembro_no_puede_eliminar_a_otro_usuario()
    {
        var client = ClientAs("miembro");
        var otroId = UsuarioSembradoId("admin");

        var response = await client.DeleteAsync($"/api/usuarios/{otroId}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // --- Confirmación explícita 2026-08-26: "ver el directorio completo" NO es lo mismo que
    // "gestionar usuarios" -- un admin puede listar (arriba) pero, igual que un miembro, sigue sin
    // poder crear, editar ni eliminar a nadie. Eso sigue siendo exclusivo de super_admin.

    [Fact]
    public async Task Un_administrador_no_puede_crear_un_usuario()
    {
        var client = ClientAs("admin");

        var response = await client.PostAsJsonAsync("/api/usuarios", new CreateUsuarioDto
        {
            Id = Guid.NewGuid(),
            Nombre = "Persona",
            Apellido = "Nueva",
            Email = $"{Guid.NewGuid():N}@nexit-test.com",
            Rol = "miembro",
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Un_administrador_no_puede_editar_el_perfil_de_otra_persona()
    {
        var client = ClientAs("admin");
        var otroId = UsuarioSembradoId("miembro");

        var response = await client.PutAsJsonAsync($"/api/usuarios/{otroId}", new UpdateUsuarioDto
        {
            Nombre = "Intento",
            Apellido = "De edición",
            Rol = "miembro",
            Activo = true,
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Un_administrador_no_puede_eliminar_a_otro_usuario()
    {
        var client = ClientAs("admin");
        var otroId = UsuarioSembradoId("miembro");

        var response = await client.DeleteAsync($"/api/usuarios/{otroId}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
