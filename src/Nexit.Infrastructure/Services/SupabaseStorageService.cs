using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Nexit.Core.Exceptions;
using Nexit.Core.Interfaces;

namespace Nexit.Infrastructure.Services;

/// <summary>Ver ISupabaseStorageService. Llama directo a la API REST de Supabase Storage (sin el SDK oficial, mismo patrón que SupabaseAuthAdminService).</summary>
public class SupabaseStorageService(IConfiguration configuration, ILogger<SupabaseStorageService> logger) : ISupabaseStorageService
{
    private static readonly HttpClient Http = new();

    /// <summary>Bucket único para esta pieza (docs/28) -- privado, creado a mano con límite de 20 MB y solo PDF/Excel permitidos.</summary>
    private const string Bucket = "adjuntos-proveedores";

    public async Task<string> SubirAsync(string rutaDestino, Stream contenido, string contentType, CancellationToken cancellationToken = default)
    {
        var (projectUrl, serviceRoleKey) = ObtenerConfiguracion();
        if (projectUrl is null || serviceRoleKey is null)
            throw new BusinessRuleException("No se pudo subir el archivo: falta configurar Supabase:ProjectUrl y Supabase:ServiceRoleKey en el backend (mismas claves que usa docs/17 y docs/25).");

        using var request = new HttpRequestMessage(HttpMethod.Post, $"{projectUrl}/storage/v1/object/{Bucket}/{rutaDestino}");
        request.Headers.Add("apikey", serviceRoleKey);
        request.Headers.Add("Authorization", $"Bearer {serviceRoleKey}");
        using var streamContent = new StreamContent(contenido);
        streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
        request.Content = streamContent;

        HttpResponseMessage response;
        try
        {
            response = await Http.SendAsync(request, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error de red al subir {RutaDestino} a Supabase Storage.", rutaDestino);
            throw new BusinessRuleException("No se pudo contactar a Supabase para subir el archivo. Intenta de nuevo en un momento.");
        }

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            logger.LogError("Supabase Storage respondió {StatusCode} al subir {RutaDestino}: {Body}", response.StatusCode, rutaDestino, body);
            throw new BusinessRuleException("Supabase no pudo guardar el archivo. Intenta de nuevo o revisa el log del backend.");
        }

        return rutaDestino;
    }

    public async Task<string> ObtenerUrlFirmadaAsync(string storagePath, TimeSpan vigencia, CancellationToken cancellationToken = default)
    {
        var (projectUrl, serviceRoleKey) = ObtenerConfiguracion();
        if (projectUrl is null || serviceRoleKey is null)
            throw new BusinessRuleException("No se pudo generar el enlace de descarga: falta configurar Supabase:ProjectUrl y Supabase:ServiceRoleKey en el backend.");

        using var request = new HttpRequestMessage(HttpMethod.Post, $"{projectUrl}/storage/v1/object/sign/{Bucket}/{storagePath}");
        request.Headers.Add("apikey", serviceRoleKey);
        request.Headers.Add("Authorization", $"Bearer {serviceRoleKey}");
        request.Content = JsonContent.Create(new { expiresIn = (int)vigencia.TotalSeconds });

        HttpResponseMessage response;
        try
        {
            response = await Http.SendAsync(request, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error de red al firmar la descarga de {StoragePath}.", storagePath);
            throw new BusinessRuleException("No se pudo contactar a Supabase para generar el enlace de descarga. Intenta de nuevo en un momento.");
        }

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            logger.LogError("Supabase Storage respondió {StatusCode} al firmar {StoragePath}: {Body}", response.StatusCode, storagePath, body);
            throw new BusinessRuleException("Supabase no pudo generar el enlace de descarga. Intenta de nuevo o revisa el log del backend.");
        }

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
        var signedUrl = doc.RootElement.GetProperty("signedURL").GetString()
            ?? throw new BusinessRuleException("Supabase no devolvió un enlace de descarga válido.");
        return $"{projectUrl}/storage/v1{signedUrl}";
    }

    public async Task EliminarAsync(string storagePath, CancellationToken cancellationToken = default)
    {
        var (projectUrl, serviceRoleKey) = ObtenerConfiguracion();
        if (projectUrl is null || serviceRoleKey is null)
        {
            // Igual que SupabaseAuthAdminService.EliminarCuentaAsync: limpieza de mejor esfuerzo, no
            // debe bloquear que se borre la fila de la base si Supabase todavía no está configurado.
            logger.LogWarning("No se pudo eliminar el archivo {StoragePath} de Supabase Storage: falta configurar Supabase:ProjectUrl / Supabase:ServiceRoleKey.", storagePath);
            return;
        }

        using var request = new HttpRequestMessage(HttpMethod.Delete, $"{projectUrl}/storage/v1/object/{Bucket}");
        request.Headers.Add("apikey", serviceRoleKey);
        request.Headers.Add("Authorization", $"Bearer {serviceRoleKey}");
        request.Content = JsonContent.Create(new { prefixes = new[] { storagePath } });

        try
        {
            var response = await Http.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                logger.LogError("Supabase Storage respondió {StatusCode} al eliminar {StoragePath}: {Body}", response.StatusCode, storagePath, body);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error de red al intentar eliminar {StoragePath} de Supabase Storage.", storagePath);
        }
    }

    private (string? projectUrl, string? serviceRoleKey) ObtenerConfiguracion()
    {
        var projectUrl = configuration["Supabase:ProjectUrl"]?.TrimEnd('/');
        var serviceRoleKey = configuration["Supabase:ServiceRoleKey"];
        if (string.IsNullOrWhiteSpace(projectUrl) || string.IsNullOrWhiteSpace(serviceRoleKey)) return (null, null);
        return (projectUrl, serviceRoleKey);
    }
}
