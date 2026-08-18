using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Nexit.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public abstract class BaseController : ControllerBase
{
    protected Guid GetUserId()
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        return Guid.TryParse(id, out var userId) ? userId : Guid.Empty;
    }

    /// <summary>
    /// Rol de negocio del usuario autenticado (super_admin/admin/manager/miembro), leído del mismo
    /// claim que usan las políticas de autorización ("app_role"/"user_role", inyectado por el Auth
    /// Hook de Supabase — ver docs/schema/03_auth_hook_custom_claims.sql). Para reglas que dependen
    /// de datos en tiempo de ejecución y no se pueden expresar como una política estática (por
    /// ejemplo, "solo un administrador puede reasignar el gerente de un proyecto").
    /// </summary>
    protected string? GetUserRole() => User.FindFirstValue("app_role") ?? User.FindFirstValue("user_role") ?? User.FindFirstValue(ClaimTypes.Role);
}
