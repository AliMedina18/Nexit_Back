namespace Nexit.Core.Entities;

public class CategoriaProveedor
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Nombre { get; set; } = string.Empty;
}
