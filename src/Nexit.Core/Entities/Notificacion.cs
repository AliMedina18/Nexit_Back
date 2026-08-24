namespace Nexit.Core.Entities;

/// <summary>
/// Una notificación dentro del sistema, dirigida a un usuario concreto -- hoy solo se generan para
/// el flujo de solicitudes de eliminación (docs/19), pero el diseño (tipo + entidad genérica) no
/// asume que sea el único caso. No se borra nunca: "leída" es un estado, no una eliminación -- así
/// queda como historial permanente, tal como se pidió.
/// </summary>
public class Notificacion
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UsuarioDestinatarioId { get; set; }
    public string Tipo { get; set; } = string.Empty; // "solicitud_eliminacion_creada" | "solicitud_eliminacion_endosada" | "solicitud_eliminacion_decidida"
    public string Titulo { get; set; } = string.Empty;
    public string Mensaje { get; set; } = string.Empty;
    public string? TipoEntidad { get; set; } // "cliente" | "proveedor" | "proyecto" -- de qué trata, para poder llevar a la persona directo ahí
    public Guid? EntidadId { get; set; }
    public Guid? SolicitudId { get; set; }
    public bool Leida { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime? FechaLeida { get; set; }

    public Usuario UsuarioDestinatario { get; set; } = null!;
}
