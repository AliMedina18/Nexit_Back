namespace Nexit.Application.DTOs.SolicitudesEliminacion;

public class CrearSolicitudEliminacionDto
{
    public string TipoEntidad { get; set; } = string.Empty; // "cliente" | "proveedor" | "proyecto"
    public Guid EntidadId { get; set; }
    public string? Motivo { get; set; }
}

public class RevisionSolicitudDto
{
    public string? Comentario { get; set; }
}

public class SolicitudEliminacionResponseDto
{
    public Guid Id { get; set; }
    public string TipoEntidad { get; set; } = string.Empty;
    public Guid EntidadId { get; set; }
    public Guid SolicitadoPorId { get; set; }
    public string? Motivo { get; set; }
    public string Estado { get; set; } = string.Empty;
    public Guid? GerenteResponsableId { get; set; }
    public Guid? AprobadoPorGerenteId { get; set; }
    public DateTime? AprobadoPorGerenteEn { get; set; }
    public Guid? RevisadoPorId { get; set; }
    public DateTime? RevisadoEn { get; set; }
    public string? ComentarioRevision { get; set; }
    public DateTime CreatedAt { get; set; }
}
