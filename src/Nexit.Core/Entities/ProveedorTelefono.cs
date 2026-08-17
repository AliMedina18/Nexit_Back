namespace Nexit.Core.Entities;

public class ProveedorTelefono : BaseEntity
{
    public Guid ProveedorId { get; set; }
    public string Telefono { get; set; } = string.Empty;
    public string? Etiqueta { get; set; }
    public Proveedor Proveedor { get; set; } = null!;
}
