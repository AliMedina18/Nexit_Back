namespace Nexit.Application.DTOs.Notificaciones;

public class NotificacionResponseDto
{
    public Guid Id { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public string Titulo { get; set; } = string.Empty;
    public string Mensaje { get; set; } = string.Empty;
    public string? TipoEntidad { get; set; }
    public Guid? EntidadId { get; set; }
    public Guid? SolicitudId { get; set; }
    public bool Leida { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaLeida { get; set; }
}
