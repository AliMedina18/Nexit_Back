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

/// <summary>
/// Dar de alta a alguien SIN pasar por el correo de invitación (2026-09-08). A diferencia de
/// <see cref="CreateUsuarioDto"/>, acá no se manda ningún Id: el backend crea la cuenta en Supabase
/// Auth y usa el UUID que Supabase le asigne. Es el camino para cuando quien administra prefiere
/// dejar todo listo de una vez en vez de esperar a que la persona abra un correo y responda.
/// </summary>
public class RegistrarUsuarioDto
{
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Rol { get; set; } = "miembro";
    public string? Iniciales { get; set; }
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

/// <summary>
/// Version liviana para armar el equipo de un proyecto (Alicia 2026-09-09): a diferencia de
/// <see cref="UsuarioResponseDto"/> (directorio completo, admin/super_admin), esto lo puede pedir
/// CUALQUIER autenticado con perfil -- crear/editar un proyecto no es exclusivo de admin+, así que
/// buscar a quién agregar al equipo tampoco puede serlo. Sin correo ni datos de cuenta a propósito:
/// es solo lo que hace falta para mostrar un nombre en un buscador, no el directorio entero.
/// </summary>
public class UsuarioEquipoDto
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
}
