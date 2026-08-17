using Nexit.Core.Entities;

namespace Nexit.Core.Interfaces;

public interface IProveedorAdjuntoRepository : IRepository<ProveedorAdjunto>
{
    Task<IReadOnlyList<ProveedorAdjunto>> GetByProveedorIdAsync(Guid proveedorId, CancellationToken cancellationToken = default);
}
