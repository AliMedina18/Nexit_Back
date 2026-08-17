namespace Nexit.Core.Entities;

public class Servicio : BaseEntity
{
    public string Nombre { get; set; } = string.Empty;
    public ICollection<ProveedorServicio> Proveedores { get; set; } = new List<ProveedorServicio>();
}
