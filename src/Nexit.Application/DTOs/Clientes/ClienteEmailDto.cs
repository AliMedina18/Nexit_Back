namespace Nexit.Application.DTOs.Clientes;

public class ClienteEmailDto
{
    public Guid? Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? Etiqueta { get; set; }
}
