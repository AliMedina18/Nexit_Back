using Nexit.Core.Entities;

namespace Nexit.Core.Interfaces;

public interface IProveedorRepository : IRepository<Proveedor>
{
    Task<bool> ExistsByEmailAsync(string email, Guid? excludedId = null, CancellationToken cancellationToken = default);
    /// <summary>Busca un proveedor por nombre exacto (sin distinguir mayúsculas) -- para la importación masiva (docs/31, docs/35): así "reimportar" un proveedor ya existente lo actualiza en vez de duplicarlo.</summary>
    Task<Guid?> FindIdPorNombreAsync(string nombre, CancellationToken cancellationToken = default);
}
