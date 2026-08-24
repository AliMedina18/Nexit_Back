using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Nexit.Core.Interfaces;

namespace Nexit.Infrastructure.Services;

/// <summary>Ver ISupabaseAuthAdminService. Llama a la Admin API de Supabase Auth (DELETE /auth/v1/admin/users/{id}).</summary>
public class SupabaseAuthAdminService(IConfiguration configuration, ILogger<SupabaseAuthAdminService> logger) : ISupabaseAuthAdminService
{
    private static readonly HttpClient Http = new();

    public async Task EliminarCuentaAsync(Guid usuarioId, CancellationToken cancellationToken = default)
    {
        var projectUrl = configuration["Supabase:ProjectUrl"];
        var serviceRoleKey = configuration["Supabase:ServiceRoleKey"];
        if (string.IsNullOrWhiteSpace(projectUrl) || string.IsNullOrWhiteSpace(serviceRoleKey))
        {
            // No configurado todavía -- se documenta como paso pendiente en docs/17. No lanza
            // excepción: la eliminación del perfil de negocio (usuarios/usuarios_eliminados) no debe
            // fallar por esto, solo queda una cuenta de Supabase Auth huérfana hasta que se configure.
            logger.LogWarning("No se pudo eliminar la cuenta de Supabase Auth de {UsuarioId}: falta configurar Supabase:ProjectUrl / Supabase:ServiceRoleKey.", usuarioId);
            return;
        }

        using var request = new HttpRequestMessage(HttpMethod.Delete, $"{projectUrl.TrimEnd('/')}/auth/v1/admin/users/{usuarioId}");
        request.Headers.Add("apikey", serviceRoleKey);
        request.Headers.Add("Authorization", $"Bearer {serviceRoleKey}");

        try
        {
            var response = await Http.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode && response.StatusCode != System.Net.HttpStatusCode.NotFound)
            {
                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                logger.LogError("Supabase Auth respondió {StatusCode} al eliminar la cuenta {UsuarioId}: {Body}", response.StatusCode, usuarioId, body);
            }
        }
        catch (Exception ex)
        {
            // Igual que arriba: un fallo de red hacia Supabase no debe tumbar la eliminación del
            // perfil de negocio, que ya se guardó. Queda una cuenta de Auth huérfana por revisar.
            logger.LogError(ex, "Error de red al intentar eliminar la cuenta de Supabase Auth {UsuarioId}.", usuarioId);
        }
    }
}
