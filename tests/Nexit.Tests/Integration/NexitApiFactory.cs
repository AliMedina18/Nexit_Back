using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace Nexit.Tests.Integration;

/// <summary>
/// Levanta la aplicación completa (pipeline de middleware real, incluida la autorización) para las
/// pruebas H8, reemplazando únicamente la autenticación JWT de Supabase por <see cref="TestAuthHandler"/>.
/// Todo lo demás (políticas de autorización, orden del pipeline, rate limiting, etc.) es el mismo código
/// que corre en producción.
/// </summary>
public class NexitApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureServices(services =>
        {
            services.AddAuthentication(TestAuthHandler.SchemeName)
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, _ => { });
        });
    }
}
