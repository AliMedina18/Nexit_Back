namespace Nexit.Core.Entities;

public class DominioCorreoPermitido
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Dominio { get; set; } = string.Empty;
}
