namespace Nexit.Application.DTOs.Proyectos;

public class CalendarioMesDto
{
    public int Mes { get; init; }
    public int Cantidad { get; init; }
}

public class CalendarioAnioDto
{
    public int Anio { get; init; }
    public int TotalProyectos { get; init; }
    /// <summary>Siempre trae los 12 meses (enero a diciembre), con Cantidad = 0 para los que no tienen proyectos -- así el calendario pinta la grilla completa sin huecos.</summary>
    public IReadOnlyList<CalendarioMesDto> Meses { get; init; } = [];
}

public class ProyectoCalendarioItemDto
{
    public Guid Id { get; init; }
    public string Nombre { get; init; } = string.Empty;
    public DateTime FechaEvento { get; init; }
    /// <summary>"yyyy-MM-dd" -- fecha del evento YA convertida a la hora local de la sede del proyecto
    /// (ver SedeTimeZoneResolver, docs/18). Usala para decidir en que dia pintar el proyecto en el
    /// calendario; NO recalcules el dia cortando FechaEvento (que sigue en UTC).</summary>
    public string FechaEventoLocal { get; init; } = string.Empty;
    public Guid? ClienteId { get; init; }
    public string? ClienteNombre { get; init; }
    public string EstadoNombre { get; init; } = string.Empty;
    public string? Prioridad { get; init; }
    public string? Ciudad { get; init; }
    public string? SedeNext { get; init; }
}
