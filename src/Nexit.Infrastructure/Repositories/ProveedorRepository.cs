using Microsoft.EntityFrameworkCore;
using Nexit.Core.Entities;
using Nexit.Core.Interfaces;
using Nexit.Infrastructure.Data;

namespace Nexit.Infrastructure.Repositories;

public class ProveedorRepository(NexitDbContext context) : Repository<Proveedor>(context), IProveedorRepository
{
    // Emails ahora es una lista (ProveedorEmail, ver docs/34) -- ver el comentario equivalente en ClienteRepository.
    public Task<bool> ExistsByEmailAsync(string email, Guid? excludedId = null, CancellationToken cancellationToken = default) =>
        DbSet.AnyAsync(x => x.Emails.Any(m => m.Email.ToLower() == email.ToLower()) && (!excludedId.HasValue || x.Id != excludedId), cancellationToken);
    // Mismo patrón que ClienteRepository.FindIdPorNombreAsync -- ver el comentario ahí (docs/35).
    public async Task<Guid?> FindIdPorNombreAsync(string nombre, CancellationToken cancellationToken = default) =>
        (await DbSet.AsNoTracking().FirstOrDefaultAsync(x => x.Nombre.ToLower() == nombre.Trim().ToLower(), cancellationToken))?.Id;
    public override Task<Proveedor?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        DbSet.Include(x => x.Telefonos).Include(x => x.Emails).Include(x => x.Servicios).Include(x => x.Colaboradores).ThenInclude(x => x.Usuario).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    // Include(x => x.Proyectos).ThenInclude(pp => pp.Proyecto) agregado para el endpoint de prioridad
    // (docs/24) -- necesita el CreatedAt del proyecto más reciente que usó a cada proveedor.
    public override async Task<IReadOnlyList<Proveedor>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await DbSet.AsNoTracking().Include(x => x.Telefonos).Include(x => x.Emails).Include(x => x.Servicios).Include(x => x.Colaboradores).ThenInclude(x => x.Usuario)
            .Include(x => x.Proyectos).ThenInclude(pp => pp.Proyecto).OrderBy(x => x.Nombre).ToListAsync(cancellationToken);
}
