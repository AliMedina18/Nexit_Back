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
