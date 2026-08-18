using Microsoft.EntityFrameworkCore;
using Nexit.Core.Entities;
using Nexit.Core.Interfaces;
using Nexit.Infrastructure.Data;

namespace Nexit.Infrastructure.Repositories;

public class SolicitudEliminacionRepository(NexitDbContext context) : Repository<SolicitudEliminacion>(context), ISolicitudEliminacionRepository
{
    public override Task<SolicitudEliminacion?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        DbSet.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public override async Task<IReadOnlyList<SolicitudEliminacion>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await DbSet.AsNoTracking().OrderByDescending(x => x.CreatedAt).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<SolicitudEliminacion>> GetPendientesParaAdminAsync(CancellationToken cancellationToken = default) =>
        await DbSet.AsNoTracking().Where(x => x.Estado == "pendiente_admin").OrderBy(x => x.CreatedAt).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<SolicitudEliminacion>> GetPendientesParaGerenteAsync(Guid gerenteId, CancellationToken cancellationToken = default) =>
        await DbSet.AsNoTracking().Where(x => x.Estado == "pendiente_gerente" && x.GerenteResponsableId == gerenteId).OrderBy(x => x.CreatedAt).ToListAsync(cancellationToken);
}
