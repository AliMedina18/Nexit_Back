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

    // Mismo patrón que ClienteRepository.FindIdPorNombreAsync/ProveedorRepository.FindIdPorNombreAsync
    // (docs/35), pero con la pareja (ClienteId, Nombre) como llave -- ver el comentario en la interfaz.
    // La comparación `x.ClienteId == clienteId` con ambos nullable es segura tal cual (EF Core la
    // traduce a SQL que trata NULL = NULL como verdadero para este propósito, no hace falta un caso
    // especial para "sin cliente").
    public async Task<Guid?> FindIdPorClienteYNombreAsync(Guid? clienteId, string nombre, CancellationToken cancellationToken = default) =>
        (await DbSet.AsNoTracking().FirstOrDefaultAsync(x => x.ClienteId == clienteId && x.Nombre.ToLower() == nombre.Trim().ToLower(), cancellationToken))?.Id;

}
