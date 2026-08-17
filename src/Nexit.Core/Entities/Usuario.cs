namespace Nexit.Core.Entities;

public class Usuario : BaseEntity
{
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Rol { get; set; } = "miembro";
    public string? Iniciales { get; set; }
    public bool Activo { get; set; } = true;
    public ICollection<Cliente> ClientesCreados { get; set; } = new List<Cliente>();
    public ICollection<Proveedor> ProveedoresCreados { get; set; } = new List<Proveedor>();
    public ICollection<Proyecto> ProyectosCreados { get; set; } = new List<Proyecto>();
    public ICollection<ProyectoSeguimiento> SeguimientosEscritos { get; set; } = new List<ProyectoSeguimiento>();
}
