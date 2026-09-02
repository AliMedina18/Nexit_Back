namespace Nexit.Application.DTOs.Auth;

/// <summary>
/// Respuesta de GET /api/auth/estado-cuenta (ver AuthController/docs/30). Un solo campo a
/// propósito -- lo mínimo que el login necesita para decidir el siguiente paso, sin filtrar nada
/// más sobre la cuenta (ni si existe, ni el rol, ni nada) a alguien que todavía no se autenticó.
/// </summary>
public class EstadoCuentaResponseDto
{
    public bool TieneContrasena { get; set; }
}
