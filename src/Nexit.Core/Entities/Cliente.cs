namespace Nexit.Core.Entities;

public class Cliente : BaseEntity
{
    public string Nombre { get; set; } = string.Empty;
    public string? Sector { get; set; }
    public string? Ciudad { get; set; }
    public string? Direccion { get; set; }
    public string? Web { get; set; }
    public string? Contacto { get; set; }
    public string? CargoContacto { get; set; }
    public string? Email { get; set; }
    public string? ValorReferencia { get; set; }
    public string? Notas { get; set; }
    public ICollection<ClienteTelefono> Telefonos { get; set; } = new List<ClienteTelefono>();
    public ICollection<Proyecto> Proyectos { get; set; } = new List<Proyecto>();
}
