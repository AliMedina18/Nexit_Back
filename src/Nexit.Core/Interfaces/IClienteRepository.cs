using Nexit.Core.Entities;

namespace Nexit.Core.Interfaces;

public interface IClienteRepository : IRepository<Cliente>
{
    Task<bool> ExistsByEmailAsync(string email, Guid? excludedId = null, CancellationToken cancellationToken = default);
    Task<Cliente?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
}
