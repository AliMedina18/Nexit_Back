using Nexit.Application.DTOs.Proyectos;
using Nexit.Core.Exceptions;
using Nexit.Core.Interfaces;

namespace Nexit.Application.UseCases.Proyectos;

public class ConsultarCalendarioProyectosUseCase(IProyectoRepository repository) : IConsultarCalendarioProyectosUseCase
{
    public async Task<IReadOnlyList<int>> ListarAniosAsync(CancellationToken cancellationToken = default) =>
        await repository.ObtenerAniosConProyectosAsync(cancellationToken);

    public async Task<CalendarioAnioDto> ObtenerResumenAnioAsync(int anio, CancellationToken cancellationToken = default)
    {
        if (anio is < 1900 or > 2999) throw new BusinessRuleException("El año indicado no es válido.");
        var conteos = (await repository.ObtenerConteoPorMesAsync(anio, cancellationToken)).ToDictionary(x => x.Mes, x => x.Cantidad);
        // Se rellenan los 12 meses aunque el GROUP BY solo haya devuelto los que tienen proyectos --
        // así el frontend siempre recibe una grilla completa de enero a diciembre, sin tener que
        // adivinar qué meses faltan.
        var meses = Enumerable.Range(1, 12).Select(mes => new CalendarioMesDto { Mes = mes, Cantidad = conteos.GetValueOrDefault(mes) }).ToList();
        return new CalendarioAnioDto { Anio = anio, TotalProyectos = meses.Sum(x => x.Cantidad), Meses = meses };
    }

    public async Task<IReadOnlyList<ProyectoCalendarioItemDto>> ObtenerProyectosDelMesAsync(int anio, int mes, CancellationToken cancellationToken = default)
    {
        if (anio is < 1900 or > 2999) throw new BusinessRuleException("El año indicado no es válido.");
        if (mes is < 1 or > 12) throw new BusinessRuleException("El mes debe estar entre 1 y 12.");
        return (await repository.ObtenerPorMesAsync(anio, mes, cancellationToken)).Select(CalendarioMapper.ToDto).ToList();
    }
}

internal static class CalendarioMapper
{
    public static ProyectoCalendarioItemDto ToDto(Core.Interfaces.ProyectoCalendarioItem item) => new()
    {
        Id = item.Id, Nombre = item.Nombre, FechaEvento = item.FechaEvento, ClienteId = item.ClienteId,
        ClienteNombre = item.ClienteNombre, EstadoNombre = item.EstadoNombre, Prioridad = item.Prioridad,
        Ciudad = item.Ciudad, SedeNext = item.SedeNext
    };
}
