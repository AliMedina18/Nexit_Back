namespace Nexit.Application.DTOs.Historial;

public class HistorialCambioResponseDto
{
    public Guid Id { get; set; }
    public Guid UsuarioId { get; set; }
    public string? UsuarioNombre { get; set; }
    public string Accion { get; set; } = string.Empty;
    public string? Campo { get; set; }
    public string? ValorAnterior { get; set; }
    public string? ValorNuevo { get; set; }
    public DateTime Fecha { get; set; }
}
