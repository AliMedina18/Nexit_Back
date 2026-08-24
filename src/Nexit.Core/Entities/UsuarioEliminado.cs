namespace Nexit.Core.Entities;

/// <summary>
/// Respaldo de un usuario justo antes de eliminarlo de <c>usuarios</c> (automáticamente, tras 30
/// días desactivado, o manualmente vía <c>DELETE /api/usuarios/{id}</c>). No es la tabla de negocio
/// -- nadie la consulta desde la aplicación normal, es puramente un archivo de auditoría/recuperación
/// por si hace falta reconstruir quién era esa cuenta. Ver docs/17-eliminacion-automatica-usuarios.md.
/// </summary>
public class UsuarioEliminado
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>El Id original en `usuarios` (y en Supabase Auth) -- no se reutiliza como PK de esta tabla para poder guardar más de un registro si algún día se reutiliza un Id (no debería pasar, pero no se depende de eso).</summary>
    public Guid UsuarioIdOriginal { get; set; }

    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
    public string? Iniciales { get; set; }

    /// <summary>CreatedAt original del usuario (cuándo se dio de alta la primera vez), no de este respaldo.</summary>
    public DateTime FechaAltaOriginal { get; set; }

    public DateTime? FechaDesactivacion { get; set; }

    /// <summary>Cuándo se archivó/eliminó de verdad.</summary>
    public DateTime FechaEliminacion { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Quién ejecutó la eliminación (super_admin que usó DELETE) -- null cuando la disparó el
    /// proceso automático de 30 días, para distinguir los dos casos al revisar este respaldo.
    /// </summary>
    public Guid? EliminadoPorId { get; set; }
}
