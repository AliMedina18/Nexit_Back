using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Nexit.Tests.Integration;

namespace Nexit.Tests.Functional;

/// <summary>
/// El candado de "sin perfil no entras" (PerfilRequeridoFilter), contra la aplicación real y
/// Postgres real -- que es donde se puede probar de verdad, porque la regla depende de si existe
/// o no la fila en <c>usuarios</c>, no de un claim del token.
///
/// El agujero que cierra: el Auth Hook de Supabase le pone <c>user_role = "miembro"</c> por
/// defecto a cualquier cuenta de Supabase Auth que todavía no tenga fila en <c>usuarios</c>
/// (docs/schema/03_auth_hook_custom_claims.sql), así que alguien recién invitado pasaba el
/// [Authorize] y podía leer clientes, proveedores y proyectos sin haberse registrado nunca.
/// </summary>
public class PerfilRequeridoFunctionalTests(NexitFunctionalApiFactory factory) : FunctionalTestBase(factory)
{
    /// <summary>Cliente autenticado como una cuenta de Supabase Auth que NO tiene fila en `usuarios` -- alguien recién invitado que aún no aceptó.</summary>
    private HttpClient ClientSinPerfil() => ClientAs("miembro", Guid.NewGuid());

    private static async Task<string?> CodigoDeError(HttpResponseMessage response)
    {
        using var documento = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return documento.RootElement.TryGetProperty("codigo", out var codigo) ? codigo.GetString() : null;
    }

    [Fact]
    public async Task Sin_perfil_no_se_puede_leer_ningun_dato_del_sistema()
    {
        var client = ClientSinPerfil();

        var response = await client.GetAsync("/api/clientes");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Equal("perfil_requerido", await CodigoDeError(response));
    }

    [Theory]
    [InlineData("/api/proveedores")]
    [InlineData("/api/proyectos")]
    [InlineData("/api/catalogos/paises")]
    [InlineData("/api/notificaciones")]
    public async Task Sin_perfil_el_bloqueo_aplica_parejo_a_todo_el_sistema(string ruta)
    {
        var client = ClientSinPerfil();

        var response = await client.GetAsync(ruta);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Sin_perfil_si_se_puede_consultar_la_propia_invitacion()
    {
        // Es justamente el camino para conseguir un perfil (docs/25): si esto quedara bloqueado,
        // nadie podría registrarse nunca. 404 = no hay ninguna pendiente para ese correo, que es
        // una respuesta legítima; lo que no puede pasar es un 403.
        var client = ClientSinPerfil();

        var response = await client.GetAsync("/api/invitaciones/mia");

        Assert.NotEqual(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Sin_perfil_me_responde_404_y_no_403()
    {
        // El frontend distingue "todavía no te registraste" de "no tienes permiso" por este 404
        // (ver auth-store.ts / EstadoPerfil en Nexit_Front) -- si acá saliera 403, mandaría a la
        // gente a la pantalla equivocada.
        var client = ClientSinPerfil();

        var response = await client.GetAsync("/api/usuarios/me");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Una_cuenta_desactivada_pierde_el_acceso_en_la_siguiente_peticion()
    {
        // Criterio de aceptación de HU-06 ("una cuenta desactivada no puede usar ningún endpoint
        // del sistema"). El token de esta cuenta NO trae el claim user_active=false -- se emitió
        // antes de desactivarla, que es exactamente el caso real: el claim solo se actualiza al
        // renovar el token, hasta una hora después. El filtro lee el estado real de la fila.
        var desactivada = await CrearUsuarioDesactivadoAsync();
        var client = ClientAs("miembro", desactivada);

        var response = await client.GetAsync("/api/clientes");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Equal("cuenta_inactiva", await CodigoDeError(response));
    }
}
