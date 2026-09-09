namespace Nexit.Application.DTOs.Invitaciones;

public class CrearInvitacionDto
{
    public string Email { get; set; } = string.Empty;
    public string Rol { get; set; } = "miembro";

    /// <summary>Mensaje corto y opcional de bienvenida, para que la persona lo vea al aceptar.</summary>
    public string? Mensaje { get; set; }
}

/// <summary>Lo que completa la propia persona invitada al aceptar -- el correo y el rol ya vienen de la invitación, no se repiten acá.</summary>
public class AceptarInvitacionDto
{
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string? Iniciales { get; set; }
}

public class InvitacionResponseDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
    public string? Mensaje { get; set; }
    public string Estado { get; set; } = string.Empty;
    public string? InvitadoPorNombre { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? FechaRespuesta { get; set; }
}

/// <summary>Un destinatario del lote, con su propio rol -- Alicia 2026-09-09: puede invitar en un
/// mismo envío a gente con roles distintos (varios miembros, un admin, un directivo).</summary>
public class InvitacionLoteDestinatarioDto
{
    public string Email { get; set; } = string.Empty;
    public string Rol { get; set; } = "miembro";
}

/// <summary>
/// Invitar a varias personas de una sola vez (estilo Gmail: se escriben varios correos separados y
/// se manda una sola vez). Cada destinatario lleva su propio rol -- se pueden mezclar roles
/// distintos en un mismo envío -- pero todos comparten el mismo mensaje de bienvenida.
/// </summary>
public class CrearInvitacionesLoteDto
{
    public List<InvitacionLoteDestinatarioDto> Destinatarios { get; set; } = [];
    public string? Mensaje { get; set; }
}

/// <summary>Un correo del lote que no se pudo invitar, con el motivo exacto -- ver <see cref="InvitacionesLoteResponseDto"/>.</summary>
public class InvitacionFallidaDto
{
    public string Email { get; set; } = string.Empty;
    public string Motivo { get; set; } = string.Empty;
}

/// <summary>
/// Resultado de un envío por lote. A propósito NO es "todo o nada": que un correo esté repetido o
/// sea de un dominio ajeno no debe impedir que los otros cinco se inviten -- se devuelve qué salió
/// y qué no, para que la pantalla lo muestre correo por correo.
/// </summary>
public class InvitacionesLoteResponseDto
{
    public List<InvitacionResponseDto> Enviadas { get; set; } = [];
    public List<InvitacionFallidaDto> Fallidas { get; set; } = [];
}
