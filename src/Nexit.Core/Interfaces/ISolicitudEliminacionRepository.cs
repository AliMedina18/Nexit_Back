using Nexit.Core.Entities;

namespace Nexit.Core.Interfaces;

public interface ISolicitudEliminacionRepository : IRepository<SolicitudEliminacion>
{
    Task<IReadOnlyList<SolicitudEliminacion>> GetPendientesParaAdminAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SolicitudEliminacion>> GetPendientesParaGerenteAsync(Guid gerenteId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Otras solicitudes pendientes (cualquier estado, "pendiente_gerente" o "pendiente_admin") para
    /// el mismo tipo+entidad -- para el conteo que ve un administrador (docs/19) y para resolverlas
    /// todas juntas cuando decide una sola vez sobre esa entidad (ver AprobarComoAdminUseCase /
    /// RechazarComoAdminUseCase). Excluye la solicitud que ya se está procesando.
    /// </summary>
    Task<IReadOnlyList<SolicitudEliminacion>> GetOtrasPendientesPorEntidadAsync(string tipoEntidad, Guid entidadId, Guid excluirSolicitudId, CancellationToken cancellationToken = default);
}
