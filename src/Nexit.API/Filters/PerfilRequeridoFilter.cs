using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Nexit.API.Models;
using Nexit.Core.Interfaces;

namespace Nexit.API.Filters;

/// <summary>
/// Marca una acción (o un controlador entero) como accesible para alguien que está autenticado en
/// Supabase Auth pero TODAVÍA NO tiene fila en <c>usuarios</c> -- es decir, alguien a mitad del
/// registro. Es la única excepción a <see cref="PerfilRequeridoFilter"/>, y por eso la lista de
/// endpoints marcados está fijada en una prueba (<c>PerfilRequeridoTests</c>): agregar uno nuevo
/// tiene que ser una decisión consciente, no algo que se cuele en un refactor.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class PermitirSinPerfilAttribute : Attribute;

/// <summary>
/// Cierra el hueco que dejaba el Auth Hook de Supabase (ver docs/schema/03_auth_hook_custom_claims.sql):
/// cuando una cuenta de Supabase Auth todavía no tiene fila en <c>usuarios</c>, el hook le pone
/// <c>user_role = "miembro"</c> por defecto para no romper las políticas que esperan ese claim. El
/// efecto secundario era que alguien recién invitado, con solo abrir sesión, ya pasaba el
/// <c>[Authorize]</c> de <see cref="Controllers.BaseController"/> y podía leer clientes, proveedores
/// y proyectos sin haberse registrado nunca. Este filtro exige que exista de verdad el perfil de
/// negocio antes de dejar pasar cualquier petición, salvo las marcadas con
/// <see cref="PermitirSinPerfilAttribute"/> (las que la propia persona necesita justamente PARA
/// registrarse).
///
/// De paso cierra la ventana de hasta una hora que documenta <c>Program.cs</c> para las cuentas
/// desactivadas: el claim <c>user_active</c> solo se actualiza cuando el token se renueva, así que
/// alguien recién desactivado conservaba acceso hasta que su token venciera. Acá se lee el estado
/// real de la fila, no el claim, así que la desactivación surte efecto en la siguiente petición --
/// que es lo que pide el criterio de aceptación de HU-06 ("una cuenta desactivada no puede usar
/// ningún endpoint del sistema").
///
/// Costo: una consulta por clave primaria por petición, que devuelve un solo booleano. Con el
/// volumen de Next (decenas de cuentas) es despreciable, y a cambio no depende de que el token se
/// haya renovado ni de correr SQL nuevo en Supabase.
/// </summary>
public class PerfilRequeridoFilter : IAsyncAuthorizationFilter
{
    /// <summary>Se devuelve en <c>codigo</c> para que el frontend mande a la pantalla de registro en vez de mostrar un "no tienes permiso" genérico.</summary>
    public const string CodigoPerfilRequerido = "perfil_requerido";

    /// <summary>Se devuelve en <c>codigo</c> cuando la fila existe pero está desactivada -- ahí el frontend cierra la sesión en vez de mandar a registrarse.</summary>
    public const string CodigoCuentaInactiva = "cuenta_inactiva";

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var endpoint = context.HttpContext.GetEndpoint();
        // Endpoints públicos (hoy solo GET /api/auth/estado-cuenta) y los explícitamente marcados
        // como parte del registro: ni siquiera se consulta la base.
        if (endpoint?.Metadata.GetMetadata<IAllowAnonymous>() is not null) return;
        if (endpoint?.Metadata.GetMetadata<PermitirSinPerfilAttribute>() is not null) return;

        // Sin autenticar: que responda el 401 de siempre, no un 403 confuso.
        if (context.HttpContext.User.Identity?.IsAuthenticated != true) return;

        var userId = LeerUserId(context);
        if (userId == Guid.Empty)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var repositorio = context.HttpContext.RequestServices.GetRequiredService<IUsuarioRepository>();
        var activo = await repositorio.EstaActivoAsync(userId, context.HttpContext.RequestAborted);

        if (activo is null)
        {
            context.Result = Rechazar(context, CodigoPerfilRequerido,
                "Todavía no completaste tu registro en Nexit. Acepta tu invitación para crear tu perfil.");
            return;
        }
        if (activo is false)
        {
            context.Result = Rechazar(context, CodigoCuentaInactiva,
                "Tu cuenta está desactivada. Contacta a la administradora del sistema.");
        }
    }

    private static Guid LeerUserId(AuthorizationFilterContext context)
    {
        var id = context.HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                 ?? context.HttpContext.User.FindFirst("sub")?.Value;
        return Guid.TryParse(id, out var userId) ? userId : Guid.Empty;
    }

    private static ObjectResult Rechazar(AuthorizationFilterContext context, string codigo, string mensaje) =>
        new(new ErrorResponse
        {
            StatusCode = StatusCodes.Status403Forbidden,
            Message = mensaje,
            Codigo = codigo,
            TraceId = context.HttpContext.TraceIdentifier
        })
        { StatusCode = StatusCodes.Status403Forbidden };
}
