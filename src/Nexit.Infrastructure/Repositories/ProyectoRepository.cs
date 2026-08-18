using Microsoft.EntityFrameworkCore;
using Nexit.Core.Entities;
using Nexit.Core.Interfaces;
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

    // Vista de calendario (docs/06-modelo-permisos-roles.md no aplica aquí, pero ver el nuevo doc de
    // esta ronda de cambios): a propósito estas 3 consultas NO usan Include ni traen la entidad
    // Proyecto completa -- son proyecciones/agregaciones resueltas en SQL (GROUP BY, DISTINCT) para
    // que pintar un año completo del calendario no cargue equipo/proveedores/seguimiento de cada
    // proyecto, que la vista de calendario no necesita.

    public async Task<IReadOnlyList<int>> ObtenerAniosConProyectosAsync(CancellationToken cancellationToken = default) =>
        await DbSet.AsNoTracking().Where(x => x.FechaEvento.HasValue)
            .Select(x => x.FechaEvento!.Value.Year).Distinct()
            .OrderByDescending(anio => anio).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<ConteoMesProyectos>> ObtenerConteoPorMesAsync(int anio, CancellationToken cancellationToken = default) =>
        await DbSet.AsNoTracking()
            .Where(x => x.FechaEvento.HasValue && x.FechaEvento.Value.Year == anio)
            .GroupBy(x => x.FechaEvento!.Value.Month)
            .Select(g => new ConteoMesProyectos(g.Key, g.Count()))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<ProyectoCalendarioItem>> ObtenerPorMesAsync(int anio, int mes, CancellationToken cancellationToken = default) =>
        await (from p in DbSet.AsNoTracking()
               join e in Context.EstadosProyecto on p.EstadoId equals e.Id
               where p.FechaEvento.HasValue && p.FechaEvento.Value.Year == anio && p.FechaEvento.Value.Month == mes
               orderby p.FechaEvento
               select new ProyectoCalendarioItem(p.Id, p.Nombre, p.FechaEvento!.Value, p.ClienteId, p.Cliente != null ? p.Cliente.Nombre : null, e.Nombre, p.Prioridad, p.Ciudad, p.SedeNext))
            .ToListAsync(cancellationToken);
}
