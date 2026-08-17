namespace Nexit.Core.Entities;

public class Ciudad
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid RegionId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public Region Region { get; set; } = null!;
}
