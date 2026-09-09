namespace Nexit.API.Models;

public class ErrorResponse
{
    public int StatusCode { get; init; }
    public string Message { get; init; } = string.Empty;

    /// <summary>
    /// Código estable de la causa, para los pocos errores en los que el frontend tiene que hacer algo
    /// distinto y no solo mostrar el mensaje (hoy: "perfil_requerido" y "cuenta_inactiva", ver
    /// PerfilRequeridoFilter). Null en el resto -- el middleware de excepciones no lo llena.
    /// </summary>
    public string? Codigo { get; init; }
    public string TraceId { get; init; } = string.Empty;
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}
