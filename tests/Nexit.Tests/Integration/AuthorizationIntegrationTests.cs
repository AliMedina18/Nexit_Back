using System.Net;
using System.Net.Http.Json;

namespace Nexit.Tests.Integration;

/// <summary>
/// H8 — pruebas de integración que levantan la aplicación completa (pipeline de middleware real,
/// incluida autenticación y autorización) en vez de invocar casos de uso de forma aislada.
/// El objetivo es detectar, por ejemplo, que alguien quite un [Authorize] por accidente en un
/// refactor futuro, algo que las pruebas unitarias existentes no pueden ver.
/// </summary>
public class AuthorizationIntegrationTests(NexitApiFactory factory) : IClassFixture<NexitApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task GetClientes_without_a_token_returns_401()
    {
        var response = await _client.GetAsync("/api/clientes");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetClientes_with_any_authenticated_role_is_not_blocked_by_authorization()
    {
        _client.DefaultRequestHeaders.Remove(TestAuthHandler.TestAuthHeader);
        _client.DefaultRequestHeaders.Add(TestAuthHandler.TestAuthHeader, "miembro");

        var response = await _client.GetAsync("/api/clientes");

        Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.NotEqual(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task DeleteCliente_without_a_token_returns_401()
    {
        var response = await _client.DeleteAsync($"/api/clientes/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [InlineData("miembro")]
    [InlineData("manager")]
    public async Task DeleteCliente_with_a_non_admin_role_returns_403(string role)
    {
        _client.DefaultRequestHeaders.Remove(TestAuthHandler.TestAuthHeader);
        _client.DefaultRequestHeaders.Add(TestAuthHandler.TestAuthHeader, role);

        var response = await _client.DeleteAsync($"/api/clientes/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task DeleteCliente_with_admin_role_passes_authorization()
    {
        _client.DefaultRequestHeaders.Remove(TestAuthHandler.TestAuthHeader);
        _client.DefaultRequestHeaders.Add(TestAuthHandler.TestAuthHeader, "admin");

        var response = await _client.DeleteAsync($"/api/clientes/{Guid.NewGuid()}");

        // No hay base de datos real en el entorno de pruebas, así que la petición puede fallar más
        // adelante en el pipeline (p. ej. 500 al no poder conectar a Postgres) — lo que importa aquí
        // es que la autorización ya no la bloquea (nunca 401/403), a diferencia de "miembro"/"manager".
        Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.NotEqual(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Theory]
    [InlineData("miembro")]
    [InlineData("manager")]
    public async Task CrearPais_catalogo_with_a_non_admin_role_returns_403(string role)
    {
        _client.DefaultRequestHeaders.Remove(TestAuthHandler.TestAuthHeader);
        _client.DefaultRequestHeaders.Add(TestAuthHandler.TestAuthHeader, role);

        var response = await _client.PostAsJsonAsync("/api/catalogos/paises", new { nombre = "Colombia", tipoDivision = "Departamento" });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Every_response_still_carries_the_security_headers_even_on_401()
    {
        var response = await _client.GetAsync("/api/clientes");

        Assert.Equal("nosniff", response.Headers.GetValues("X-Content-Type-Options").First());
        Assert.Equal("DENY", response.Headers.GetValues("X-Frame-Options").First());
    }

    // Modelo de 4 roles (docs/06-modelo-permisos-roles.md) — la gestión de usuarios es exclusiva del
    // super administrador; ni siquiera un administrador normal puede ver esa parte.
    [Theory]
    [InlineData("miembro")]
    [InlineData("manager")]
    [InlineData("admin")]
    public async Task GetUsuarios_with_anything_less_than_super_admin_returns_403(string role)
    {
        _client.DefaultRequestHeaders.Remove(TestAuthHandler.TestAuthHeader);
        _client.DefaultRequestHeaders.Add(TestAuthHandler.TestAuthHeader, role);

        var response = await _client.GetAsync("/api/usuarios");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetUsuarios_with_super_admin_role_passes_authorization()
    {
        _client.DefaultRequestHeaders.Remove(TestAuthHandler.TestAuthHeader);
        _client.DefaultRequestHeaders.Add(TestAuthHandler.TestAuthHeader, "super_admin");

        var response = await _client.GetAsync("/api/usuarios");

        Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.NotEqual(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetUsuarios_without_a_token_returns_401()
    {
        var response = await _client.GetAsync("/api/usuarios");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // Cualquier usuario autenticado (gerente o miembro) puede pedir una eliminación — la restricción
    // está en quién la aprueba, no en quién la solicita.
    [Theory]
    [InlineData("miembro")]
    [InlineData("manager")]
    public async Task CrearSolicitudEliminacion_with_any_authenticated_role_is_not_blocked_by_authorization(string role)
    {
        _client.DefaultRequestHeaders.Remove(TestAuthHandler.TestAuthHeader);
        _client.DefaultRequestHeaders.Add(TestAuthHandler.TestAuthHeader, role);

        var response = await _client.PostAsJsonAsync("/api/solicitudeseliminacion", new { tipoEntidad = "cliente", entidadId = Guid.NewGuid() });

        Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.NotEqual(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Theory]
    [InlineData("miembro")]
    [InlineData("manager")]
    public async Task AprobarSolicitudEliminacion_como_admin_with_a_non_admin_role_returns_403(string role)
    {
        _client.DefaultRequestHeaders.Remove(TestAuthHandler.TestAuthHeader);
        _client.DefaultRequestHeaders.Add(TestAuthHandler.TestAuthHeader, role);

        var response = await _client.PutAsJsonAsync($"/api/solicitudeseliminacion/{Guid.NewGuid()}/aprobar", new { });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task AprobarGerente_endpoint_is_reachable_by_any_authenticated_role()
    {
        // La verificación de que sea EL gerente responsable ocurre dentro del caso de uso (403 vía
        // ForbiddenOperationException), no en la política estática de autorización — cualquier
        // gerente autenticado debe poder llegar al endpoint.
        _client.DefaultRequestHeaders.Remove(TestAuthHandler.TestAuthHeader);
        _client.DefaultRequestHeaders.Add(TestAuthHandler.TestAuthHeader, "manager");

        var response = await _client.PutAsJsonAsync($"/api/solicitudeseliminacion/{Guid.NewGuid()}/aprobar-gerente", new { });

        Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.NotEqual(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
