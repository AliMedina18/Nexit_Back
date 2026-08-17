namespace Nexit.Core.Entities;

public class ProyectoEquipo : BaseEntity
{
    public Guid ProyectoId { get; set; }
    public string Rol { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public Proyecto Proyecto { get; set; } = null!;
}
