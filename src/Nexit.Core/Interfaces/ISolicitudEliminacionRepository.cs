using Nexit.Core.Entities;

namespace Nexit.Core.Interfaces;

public interface ISolicitudEliminacionRepository : IRepository<SolicitudEliminacion>
{
    Task<IReadOnlyList<SolicitudEliminacion>> GetPendientesParaAdminAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SolicitudEliminacion>> GetPendientesParaGerenteAsync(Guid gerenteId, CancellationToken cancellationToken = default);
}
