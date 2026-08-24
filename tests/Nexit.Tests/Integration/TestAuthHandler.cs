using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Nexit.Tests.Integration;

/// <summary>
/// Reemplaza la autenticación JWT real de Supabase en las pruebas de integración/funcionales (H8,
/// ver docs/08-tipos-de-pruebas.md). No hay forma de generar tokens JWT firmados por un proyecto de
/// Supabase real en un entorno de pruebas, así que este handler simula el resultado de esa
/// validación a partir de dos cabeceras de prueba:
/// - <see cref="TestAuthHeader"/> ("X-Test-Role"): "super_admin"/"admin"/"manager"/"miembro", o
///   ausente para simular una petición sin token.
/// - <see cref="TestUserIdHeader"/> ("X-Test-UserId", opcional): fija el id de usuario autenticado
///   (el claim que lee BaseController.GetUserId()). Sin esta cabecera, cada petición recibe un id
///   aleatorio nuevo -- suficiente para pruebas de autorización que no dependen de una identidad
///   concreta, pero insuficiente para pruebas funcionales que necesitan actuar varias veces COMO EL
///   MISMO usuario (ej. un gerente que crea un proyecto y luego aprueba una solicitud sobre ese mismo
///   proyecto, o las protecciones de auto-bloqueo de usuarios).
/// </summary>
public class TestAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "TestScheme";
    public const string TestAuthHeader = "X-Test-Role";
    public const string TestUserIdHeader = "X-Test-UserId";

    /// <summary>
    /// ("X-Test-Active", opcional): pasar "false" para simular una cuenta desactivada (agrega el
    /// claim user_active=false, ver docs/17-eliminacion-automatica-usuarios.md). Sin esta cabecera
    /// no se agrega el claim -- mismo comportamiento de "activa" que tenían todas las pruebas antes
    /// de que existiera este chequeo, para no tener que tocar ninguna prueba ya escrita.
    /// </summary>
    public const string TestActiveHeader = "X-Test-Active";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(TestAuthHeader, out var role) || string.IsNullOrWhiteSpace(role))
            return Task.FromResult(AuthenticateResult.NoResult());

        var userId = Request.Headers.TryGetValue(TestUserIdHeader, out var userIdHeader) && Guid.TryParse(userIdHeader, out var parsed)
            ? parsed
            : Guid.NewGuid();
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new("user_role", role.ToString())
        };
        if (Request.Headers.TryGetValue(TestActiveHeader, out var active) && active == "false")
            claims.Add(new Claim("user_active", "false"));
        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
