namespace Nexit.Application.DTOs.Clientes;

/// <summary>"A qué cliente prestarle atención" (docs/21, docs/24) -- puntuado con las razones de cada puntaje.</summary>
public class ClientePrioridadResponseDto
{
    public Guid ClienteId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public int Puntaje { get; set; }
    public List<string> Razones { get; set; } = [];
}
