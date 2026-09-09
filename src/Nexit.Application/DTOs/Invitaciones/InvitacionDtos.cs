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

/// <summary>
/// Invitar a varias personas de una sola vez (estilo Gmail: se escriben varios correos separados y
/// se manda una sola vez). Todas comparten el mismo rol propuesto y el mismo mensaje -- si hicieran
/// falta roles distintos, son dos envíos distintos, que es más claro que una tabla de correo+rol
/// dentro de un modal.
/// </summary>
public class CrearInvitacionesLoteDto
{
    public List<string> Emails { get; set; } = [];
    public string Rol { get; set; } = "miembro";
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
