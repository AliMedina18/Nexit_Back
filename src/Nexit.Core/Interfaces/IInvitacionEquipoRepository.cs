using Nexit.Core.Entities;

namespace Nexit.Core.Interfaces;

public interface IInvitacionEquipoRepository : IRepository<InvitacionEquipo>
{
    Task<InvitacionEquipo?> GetPendientePorEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> ExistePendientePorEmailAsync(string email, CancellationToken cancellationToken = default);
}
