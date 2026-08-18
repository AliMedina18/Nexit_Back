namespace Nexit.Core.Entities;

/// <summary>
/// Solicitud para eliminar un cliente, proveedor o proyecto, hecha por un gerente o miembro
/// (quienes no pueden eliminar directamente — ver auditoría de seguridad, docs/06-modelo-permisos-roles.md).
/// Para proyectos con un gerente responsable (<see cref="Entities.Proyecto.GerenteId"/>) asignado,
/// distinto de quien solicita, primero debe endosarla ese gerente (estado "pendiente_gerente") antes
/// de llegar a un administrador. Para clientes y proveedores, y para proyectos sin gerente asignado
/// o solicitados por su propio gerente, va directo a "pendiente_admin".
/// </summary>
public class SolicitudEliminacion : BaseEntity
{
    public string TipoEntidad { get; set; } = string.Empty; // "cliente" | "proveedor" | "proyecto"
    public Guid EntidadId { get; set; }
    public Guid SolicitadoPorId { get; set; }
    public string? Motivo { get; set; }
    public string Estado { get; set; } = "pendiente_admin"; // "pendiente_gerente" | "pendiente_admin" | "aprobada" | "rechazada"
    public Guid? GerenteResponsableId { get; set; }
    public Guid? AprobadoPorGerenteId { get; set; }
    public DateTime? AprobadoPorGerenteEn { get; set; }
    public Guid? RevisadoPorId { get; set; }
    public DateTime? RevisadoEn { get; set; }
    public string? ComentarioRevision { get; set; }

    public Usuario SolicitadoPor { get; set; } = null!;
    public Usuario? GerenteResponsable { get; set; }
    public Usuario? AprobadoPorGerente { get; set; }
    public Usuario? RevisadoPor { get; set; }
}
