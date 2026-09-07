namespace Nexit.Core.Entities;

public class ClienteEmail : BaseEntity
{
    public Guid ClienteId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? Etiqueta { get; set; }
    public Cliente Cliente { get; set; } = null!;
}
