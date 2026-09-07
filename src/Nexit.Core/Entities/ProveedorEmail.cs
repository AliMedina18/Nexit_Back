namespace Nexit.Core.Entities;

public class ProveedorEmail : BaseEntity
{
    public Guid ProveedorId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? Etiqueta { get; set; }
    public Proveedor Proveedor { get; set; } = null!;
}
