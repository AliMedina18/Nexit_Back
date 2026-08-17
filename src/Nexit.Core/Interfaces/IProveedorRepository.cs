using Nexit.Core.Entities;

namespace Nexit.Core.Interfaces;

public interface IProveedorRepository : IRepository<Proveedor>
{
    Task<bool> ExistsByEmailAsync(string email, Guid? excludedId = null, CancellationToken cancellationToken = default);
}
