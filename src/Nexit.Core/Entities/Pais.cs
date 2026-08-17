namespace Nexit.Core.Entities;

public class Pais
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Nombre { get; set; } = string.Empty;
    public string EtiquetaRegion { get; set; } = "Departamento";
    public ICollection<Region> Regiones { get; set; } = new List<Region>();
}
