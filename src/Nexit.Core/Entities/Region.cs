namespace Nexit.Core.Entities;

public class Region
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PaisId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public Pais Pais { get; set; } = null!;
    public ICollection<Ciudad> Ciudades { get; set; } = new List<Ciudad>();
}
