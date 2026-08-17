namespace Nexit.Core.Entities;

public class ProveedorServicio
{
    public Guid ProveedorId { get; set; }
    public Guid ServicioId { get; set; }
    public Proveedor Proveedor { get; set; } = null!;
    public Servicio Servicio { get; set; } = null!;
}
