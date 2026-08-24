namespace Nexit.Core.Entities;

/// <summary>
/// Invitar y registrar a alguien del equipo en un solo paso, desde dentro de Nexit (docs/10,
/// sección 5; docs/25) -- reemplaza el proceso manual de dos pasos separados (invitar en el
/// dashboard de Supabase, y aparte crear el perfil con <c>POST /api/usuarios</c>).
///
/// El correo real que recibe la persona lo sigue mandando Supabase (este backend nunca envía
/// correos, ver docs/10), pero ahora se dispara solo, con un mensaje personalizado opcional del
/// invitador. Cuando la persona invitada inicia sesión por primera vez, ve esta invitación
/// pendiente y decide aceptarla (lo que crea su perfil automáticamente, con el rol que se le
/// propuso) o rechazarla.
/// </summary>
public class InvitacionEquipo : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public string Rol { get; set; } = "miembro";

    /// <summary>Mensaje corto y opcional de quien invita (ej. "bienvenido al equipo, nos vemos el lunes").</summary>
    public string? Mensaje { get; set; }

    /// <summary>"Pendiente" / "Aceptada" / "Rechazada" -- ver <c>Nexit.Core.Constants.EstadosInvitacion</c>.</summary>
    public string Estado { get; set; } = "Pendiente";

    public Guid InvitadoPorId { get; set; }
    public Usuario? InvitadoPor { get; set; }

    /// <summary>Cuándo se aceptó o rechazó -- null mientras sigue Pendiente.</summary>
    public DateTime? FechaRespuesta { get; set; }
}
