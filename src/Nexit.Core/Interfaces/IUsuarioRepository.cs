using Nexit.Core.Entities;

namespace Nexit.Core.Interfaces;

public interface IUsuarioRepository : IRepository<Usuario>
{
    Task<bool> ExistsByEmailAsync(string email, Guid? excludedId = null, CancellationToken cancellationToken = default);
}
