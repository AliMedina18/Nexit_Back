using Microsoft.EntityFrameworkCore;
using Nexit.Core.Entities;
using Nexit.Core.Interfaces;
using Nexit.Core.Utils;
using Nexit.Infrastructure.Data;

namespace Nexit.Infrastructure.Repositories;

public class ProyectoRepository(NexitDbContext context) : Repository<Proyecto>(context), IProyectoRepository
{
    public override Task<Proyecto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        DbSet.Include(x => x.Equipo).Include(x => x.Proveedores).Include(x => x.Seguimiento)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public override async Task<IReadOnlyList<Proyecto>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await DbSet.AsNoTracking().Include(x => x.Equipo).Include(x => x.Proveedores).Include(x => x.Seguimiento)
            .OrderByDescending(x => x.FechaEvento).ThenBy(x => x.Nombre).ToListAsync(cancellationToken);

    // Vista de calendario (ver docs/07 y docs/18-calendario-zona-horaria-por-sede.md): a propósito
    // estas 3 consultas NO usan Include ni traen la entidad Proyecto completa -- son
    // proyecciones/agregaciones livianas para que pintar un año completo del calendario no cargue
    // equipo/proveedores/seguimiento de cada proyecto, que la vista de calendario no necesita.
    //
    // Importante (docs/18): a qué mes/año pertenece un proyecto se decide por la hora LOCAL de su
    // sede (SedeNext -> SedeTimeZoneResolver), no por UTC ni por la zona horaria de sesión del
    // servidor -- extraer el año/mes directo de una columna timestamptz (EXTRACT/.Year/.Month) es
    // exactamente el bug que esto evita (un proyecto tarde en la noche cerca de fin de mes queda
    // contado en el mes siguiente). Por eso las 3 consultas traen primero un rango ancho (+/- 1 año,
    // usando el año en UTC, que nunca puede diferir del año local en más de un día) con un filtro
    // que sí es traducible a SQL y usa el índice de fecha_evento, y la fecha local exacta se
    // resuelve después en memoria -- no cambia el volumen de datos que se trae respecto a antes,
    // solo cuándo se decide el mes/año de cada uno.

    public async Task<IReadOnlyList<int>> ObtenerAniosConProyectosAsync(CancellationToken cancellationToken = default)
    {
        var candidatos = await DbSet.AsNoTracking().Where(x => x.FechaEvento.HasValue)
            .Select(x => new { x.FechaEvento, x.SedeNext }).ToListAsync(cancellationToken);

        return candidatos.Select(x => AnioLocal(x.FechaEvento!.Value, x.SedeNext))
            .Distinct().OrderByDescending(anio => anio).ToList();
    }

    public async Task<IReadOnlyList<ConteoMesProyectos>> ObtenerConteoPorMesAsync(int anio, CancellationToken cancellationToken = default)
    {
        var candidatos = await DbSet.AsNoTracking()
            .Where(x => x.FechaEvento.HasValue &&
                        (x.FechaEvento!.Value.Year == anio - 1 || x.FechaEvento!.Value.Year == anio || x.FechaEvento!.Value.Year == anio + 1))
            .Select(x => new { x.FechaEvento, x.SedeNext })
            .ToListAsync(cancellationToken);

        return candidatos
            .Select(x => (Anio: AnioLocal(x.FechaEvento!.Value, x.SedeNext), Mes: MesLocal(x.FechaEvento!.Value, x.SedeNext)))
            .Where(x => x.Anio == anio)
            .GroupBy(x => x.Mes)
            .Select(g => new ConteoMesProyectos(g.Key, g.Count()))
            .ToList();
    }

    public async Task<IReadOnlyList<ProyectoCalendarioItem>> ObtenerPorMesAsync(int anio, int mes, CancellationToken cancellationToken = default)
    {
        var candidatos = await (from p in DbSet.AsNoTracking()
                                 join e in Context.EstadosProyecto on p.EstadoId equals e.Id
                                 where p.FechaEvento.HasValue &&
                                       (p.FechaEvento!.Value.Year == anio - 1 || p.FechaEvento!.Value.Year == anio || p.FechaEvento!.Value.Year == anio + 1)
                                 select new ProyectoCalendarioItem(p.Id, p.Nombre, p.FechaEvento!.Value, p.ClienteId, p.Cliente != null ? p.Cliente.Nombre : null, e.Nombre, p.Prioridad, p.Ciudad, p.SedeNext))
            .ToListAsync(cancellationToken);

        return candidatos
            .Where(x => AnioLocal(x.FechaEvento, x.SedeNext) == anio && MesLocal(x.FechaEvento, x.SedeNext) == mes)
            .OrderBy(x => x.FechaEvento)
            .ToList();
    }

    private static int AnioLocal(DateTime fechaEventoUtc, string? sedeNext) => SedeTimeZoneResolver.ConvertirUtcALocal(fechaEventoUtc, sedeNext).Year;
    private static int MesLocal(DateTime fechaEventoUtc, string? sedeNext) => SedeTimeZoneResolver.ConvertirUtcALocal(fechaEventoUtc, sedeNext).Month;
}
