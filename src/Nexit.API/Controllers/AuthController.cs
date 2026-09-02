using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Nexit.Application.DTOs.Auth;
using Nexit.Application.UseCases.Auth;

namespace Nexit.API.Controllers;

/// <summary>
/// Apoyo al login del frontend (ver docs/30) -- nada de esto reemplaza a Supabase Auth, que sigue
/// siendo quien de verdad autentica; esto solo le da a la pantalla de login la información mínima
/// para decidir qué paso mostrar primero.
///
/// <see cref="EstadoCuenta"/> es, a propósito, el ÚNICO endpoint público (sin sesión) de toda esta
/// API -- BaseController exige [Authorize] a nivel de clase, así que aquí se anula explícitamente
/// con [AllowAnonymous] en esa acción (comportamiento estándar de ASP.NET Core: un [AllowAnonymous]
/// en la acción gana sobre el [Authorize] de la clase). Va con su propio límite de tasa, mucho más
/// estricto que el resto de la API ("auth-anon" en Program.cs, por IP) -- un endpoint público que
/// confirma/descarta correos es, por naturaleza, un vector de enumeración de cuentas, y el límite
/// por defecto (100/min, pensado para gente ya autenticada) sería demasiado permisivo para eso.
/// </summary>
public class AuthController(
    IConsultarEstadoCuentaUseCase consultarEstadoCuenta,
    IConfirmarContrasenaConfiguradaUseCase confirmarContrasenaConfigurada) : BaseController
{
    /// <summary>
    /// ¿Esta cuenta ya tiene contraseña configurada? Pública, sin autenticación -- por diseño, la
    /// respuesta nunca distingue "no existe" de "existe pero aún no tiene contraseña" (ver el
    /// comentario de ConsultarEstadoCuentaUseCase).
    /// </summary>
    [HttpGet("estado-cuenta"), AllowAnonymous, EnableRateLimiting("auth-anon")]
    public async Task<ActionResult<EstadoCuentaResponseDto>> EstadoCuenta([FromQuery] string email, CancellationToken ct) =>
        Ok(await consultarEstadoCuenta.ExecuteAsync(email ?? string.Empty, ct));

    /// <summary>
    /// Nexit_Front la llama justo después de crear/restablecer la contraseña en Supabase Auth --
    /// cualquier autenticado, sin política de rol: es sobre la propia cuenta de quien llama.
    /// </summary>
    [HttpPost("confirmar-contrasena")]
    public async Task<IActionResult> ConfirmarContrasena(CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty) return Unauthorized();
        await confirmarContrasenaConfigurada.ExecuteAsync(userId, ct);
        return NoContent();
    }
}
