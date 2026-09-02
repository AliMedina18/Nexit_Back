using Nexit.Core.Entities;

namespace Nexit.Core.Interfaces;

public interface IClienteRepository : IRepository<Cliente>
{
    Task<bool> ExistsByEmailAsync(string email, Guid? excludedId = null, CancellationToken cancellationToken = default);
    Task<Cliente?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    /// <summary>Busca un cliente por nombre exacto (sin distinguir mayúsculas) -- para la importación masiva de proyectos (docs/31), donde el Excel trae el nombre del cliente, no su Id.</summary>
    Task<Guid?> FindIdPorNombreAsync(string nombre, CancellationToken cancellationToken = default);
}
