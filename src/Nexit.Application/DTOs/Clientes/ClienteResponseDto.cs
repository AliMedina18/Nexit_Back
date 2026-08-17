namespace Nexit.Application.DTOs.Clientes;

public class ClienteResponseDto : CreateClienteDto
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
