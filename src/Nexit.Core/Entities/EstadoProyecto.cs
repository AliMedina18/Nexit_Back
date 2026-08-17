namespace Nexit.Core.Entities;

public class EstadoProyecto
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Nombre { get; set; } = string.Empty;
    public short Fase { get; set; }
    public short Orden { get; set; }
    public FaseProyecto FaseProyecto { get; set; } = null!;
}
