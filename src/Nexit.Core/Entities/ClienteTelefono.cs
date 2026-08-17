namespace Nexit.Core.Entities;

public class ClienteTelefono : BaseEntity
{
    public Guid ClienteId { get; set; }
    public string Telefono { get; set; } = string.Empty;
    public string? Etiqueta { get; set; }
    public Cliente Cliente { get; set; } = null!;
}
