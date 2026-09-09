using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Nexit.Core.Exceptions;
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

    public async Task InvitarUsuarioAsync(string email, CancellationToken cancellationToken = default)
    {
        var projectUrl = configuration["Supabase:ProjectUrl"];
        var serviceRoleKey = configuration["Supabase:ServiceRoleKey"];
        if (string.IsNullOrWhiteSpace(projectUrl) || string.IsNullOrWhiteSpace(serviceRoleKey))
        {
            // A diferencia de EliminarCuentaAsync, acá SÍ se lanza -- ver el comentario en
            // ISupabaseAuthAdminService sobre por qué invitar no puede fallar en silencio.
            throw new BusinessRuleException("No se pudo enviar la invitación: falta configurar Supabase:ProjectUrl y Supabase:ServiceRoleKey en el backend (mismas claves que usa la eliminación automática de cuentas, ver docs/17).");
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, $"{projectUrl.TrimEnd('/')}/auth/v1/invite");
        request.Headers.Add("apikey", serviceRoleKey);
        request.Headers.Add("Authorization", $"Bearer {serviceRoleKey}");
        request.Content = JsonContent.Create(new { email });

        HttpResponseMessage response;
        try
        {
            response = await Http.SendAsync(request, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error de red al invitar a {Email} desde Supabase Auth.", email);
            throw new BusinessRuleException("No se pudo contactar a Supabase para enviar la invitación. Intenta de nuevo en un momento.");
        }

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            logger.LogError("Supabase Auth respondió {StatusCode} al invitar a {Email}: {Body}", response.StatusCode, email, body);
            // 422 -- Supabase ya tiene una cuenta con ese correo (ya aceptó otra invitación antes,
            // o se dio de alta manual alguna vez) -- no es un error de configuración del backend.
            if (response.StatusCode == HttpStatusCode.UnprocessableEntity)
                throw new BusinessRuleException("Ese correo ya tiene una cuenta en Supabase Auth. Si ya es parte del equipo, créale el perfil directamente en vez de invitarlo de nuevo.");
            throw new BusinessRuleException("Supabase no pudo enviar la invitación. Intenta de nuevo o revisa el log del backend.");
        }
    }

    public async Task<Guid> CrearCuentaAsync(string email, CancellationToken cancellationToken = default)
    {
        var projectUrl = configuration["Supabase:ProjectUrl"];
        var serviceRoleKey = configuration["Supabase:ServiceRoleKey"];
        if (string.IsNullOrWhiteSpace(projectUrl) || string.IsNullOrWhiteSpace(serviceRoleKey))
            throw new BusinessRuleException("No se pudo crear la cuenta: falta configurar Supabase:ProjectUrl y Supabase:ServiceRoleKey en el backend (las mismas claves que usa invitar, ver docs/17).");

        using var request = new HttpRequestMessage(HttpMethod.Post, $"{projectUrl.TrimEnd('/')}/auth/v1/admin/users");
        request.Headers.Add("apikey", serviceRoleKey);
        request.Headers.Add("Authorization", $"Bearer {serviceRoleKey}");
        // Sin contraseña (ver ISupabaseAuthAdminService) y con el correo ya dado por bueno: la
        // persona no tiene que confirmar nada, entra directo con el código de un solo uso.
        request.Content = JsonContent.Create(new { email, email_confirm = true });

        HttpResponseMessage response;
        try
        {
            response = await Http.SendAsync(request, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error de red al crear la cuenta de {Email} en Supabase Auth.", email);
            throw new BusinessRuleException("No se pudo contactar a Supabase para crear la cuenta. Intenta de nuevo en un momento.");
        }

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            logger.LogError("Supabase Auth respondió {StatusCode} al crear la cuenta de {Email}: {Body}", response.StatusCode, email, body);
            if (response.StatusCode == HttpStatusCode.UnprocessableEntity)
                throw new BusinessRuleException("Ese correo ya tiene una cuenta en Supabase Auth. Si ya es parte del equipo, búscalo en la lista en vez de registrarlo otra vez.");
            throw new BusinessRuleException("Supabase no pudo crear la cuenta. Intenta de nuevo o revisa el log del backend.");
        }

        // La respuesta trae el usuario recién creado; su "id" es el UUID que necesita usuarios.id.
        using var documento = JsonDocument.Parse(body);
        if (!documento.RootElement.TryGetProperty("id", out var id) || !Guid.TryParse(id.GetString(), out var usuarioId))
        {
            logger.LogError("Supabase Auth creó la cuenta de {Email} pero la respuesta no traía un id utilizable: {Body}", email, body);
            throw new BusinessRuleException("Supabase creó la cuenta pero no devolvió su identificador. Revisa el log del backend antes de reintentar, para no crear una cuenta duplicada.");
        }
        return usuarioId;
    }
}
