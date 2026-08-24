using Microsoft.EntityFrameworkCore;
using Nexit.Core.Constants;
using Nexit.Core.Entities;
using Nexit.Core.Interfaces;
using Nexit.Infrastructure.Data;

namespace Nexit.Infrastructure.Repositories;

public class InvitacionEquipoRepository(NexitDbContext context) : Repository<InvitacionEquipo>(context), IInvitacionEquipoRepository
{
    public override Task<InvitacionEquipo?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        DbSet.Include(x => x.InvitadoPor).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public override async Task<IReadOnlyList<InvitacionEquipo>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await DbSet.AsNoTracking().Include(x => x.InvitadoPor).OrderByDescending(x => x.CreatedAt).ToListAsync(cancellationToken);

    public Task<InvitacionEquipo?> GetPendientePorEmailAsync(string email, CancellationToken cancellationToken = default) =>
        DbSet.Include(x => x.InvitadoPor)
            .FirstOrDefaultAsync(x => x.Email.ToLower() == email.ToLower() && x.Estado == EstadosInvitacion.Pendiente, cancellationToken);

    public Task<bool> ExistePendientePorEmailAsync(string email, CancellationToken cancellationToken = default) =>
        DbSet.AnyAsync(x => x.Email.ToLower() == email.ToLower() && x.Estado == EstadosInvitacion.Pendiente, cancellationToken);
}
