namespace Nexit.Core.Entities;

public class Proveedor : BaseEntity
{
    public string Nombre { get; set; } = string.Empty;
    public Guid PaisId { get; set; }
    public Guid? RegionId { get; set; }
    public Guid? CiudadId { get; set; }
    public Guid CategoriaId { get; set; }
    public string Estado { get; set; } = "Activo";
    public string? Contacto { get; set; }
    public string? CargoContacto { get; set; }
    public string? Email { get; set; }
    public string? Web { get; set; }
    public string? Direccion { get; set; }
    public int? Aforo { get; set; }
    public string? CostoReferencia { get; set; }
    public int? Score { get; set; }
    public string? Presupuesto { get; set; }
    public string? Cobertura { get; set; }
    public string? Notas { get; set; }
    public ICollection<ProveedorTelefono> Telefonos { get; set; } = new List<ProveedorTelefono>();
    public ICollection<ProveedorServicio> Servicios { get; set; } = new List<ProveedorServicio>();
    public ICollection<ProveedorAdjunto> Adjuntos { get; set; } = new List<ProveedorAdjunto>();
    public ICollection<ProyectoProveedor> Proyectos { get; set; } = new List<ProyectoProveedor>();
    /// <summary>Quiénes se marcaron "trabajando con este proveedor" -- ver <see cref="ProveedorColaborador"/> y docs/19.</summary>
    public ICollection<ProveedorColaborador> Colaboradores { get; set; } = new List<ProveedorColaborador>();
}
