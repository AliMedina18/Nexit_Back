namespace Nexit.Core.Entities;

public class ProyectoProveedor
{
    public Guid ProyectoId { get; set; }
    public Guid ProveedorId { get; set; }
    public Proyecto Proyecto { get; set; } = null!;
    public Proveedor Proveedor { get; set; } = null!;
}
