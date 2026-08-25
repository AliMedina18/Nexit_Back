namespace Nexit.Core.Entities;

public class ProveedorAdjunto
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ProveedorId { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Url { get; set; }
    public string? StoragePath { get; set; }
    public string? Meta { get; set; }
    public string? ContentType { get; set; }
    public long? TamanoBytes { get; set; }
    public DateTime Fecha { get; set; } = DateTime.UtcNow;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Proveedor Proveedor { get; set; } = null!;
}
