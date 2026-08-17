namespace Nexit.Application.DTOs.Clientes;

public class ClienteTelefonoDto
{
    public Guid? Id { get; set; }
    public string Telefono { get; set; } = string.Empty;
    public string? Etiqueta { get; set; }
}
