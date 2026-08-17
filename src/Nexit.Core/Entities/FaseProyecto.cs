namespace Nexit.Core.Entities;

public class FaseProyecto
{
    public short Fase { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public ICollection<EstadoProyecto> Estados { get; set; } = new List<EstadoProyecto>();
}
