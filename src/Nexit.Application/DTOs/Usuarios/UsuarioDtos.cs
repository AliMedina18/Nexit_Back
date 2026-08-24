namespace Nexit.Application.DTOs.Usuarios;

/// <summary>
/// Crea el perfil de negocio (tabla usuarios) para una cuenta que YA existe en Supabase Auth.
/// El super administrador primero invita a la persona por correo desde el dashboard de Supabase
/// (Authentication → Users → Invite), y usa aquí el UUID que Supabase le asigna — este backend
/// no crea contraseñas ni envía invitaciones, eso lo administra Supabase Auth.
/// </summary>
public class CreateUsuarioDto
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Rol { get; set; } = "miembro";
    public string? Iniciales { get; set; }
    public bool Activo { get; set; } = true;
}

public class UpdateUsuarioDto
{
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Rol { get; set; } = "miembro";
    public string? Iniciales { get; set; }
    public bool Activo { get; set; } = true;
}

public class UsuarioResponseDto
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
    public string? Iniciales { get; set; }
    public bool Activo { get; set; }

    /// <summary>
    /// Cuándo se desactivó (null si está activa). El frontend puede usarlo para mostrar "se elimina
    /// automáticamente el [FechaDesactivacion + 30 días]" -- ver docs/17-eliminacion-automatica-usuarios.md.
    /// </summary>
    public DateTime? FechaDesactivacion { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
