namespace Nexit.Core.Entities;

public class ProyectoSeguimiento : BaseEntity
{
    public Guid ProyectoId { get; set; }
    public Guid? AutorId { get; set; }
    public string Area { get; set; } = "General";
    public DateTime Fecha { get; set; } = DateTime.UtcNow;
    public string Nota { get; set; } = string.Empty;
    public Proyecto Proyecto { get; set; } = null!;
    public Usuario? Autor { get; set; }
}
