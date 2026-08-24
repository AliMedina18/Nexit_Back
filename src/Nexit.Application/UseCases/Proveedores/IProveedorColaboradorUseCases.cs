using Nexit.Application.DTOs.Proveedores;

namespace Nexit.Application.UseCases.Proveedores;

public interface IMarcarColaboradorProveedorUseCase
{
    /// <summary>"Estoy trabajando con este proveedor" -- cada quien se marca a sí mismo (docs/19). Sin efecto si ya estaba marcado.</summary>
    Task ExecuteAsync(Guid proveedorId, Guid usuarioId, CancellationToken cancellationToken = default);
}

public interface IQuitarColaboradorProveedorUseCase
{
    Task ExecuteAsync(Guid proveedorId, Guid usuarioId, CancellationToken cancellationToken = default);
}

public interface IListarMisProveedoresUseCase
{
    /// <summary>Los proveedores donde este usuario se marcó "trabajando con este proveedor".</summary>
    Task<IReadOnlyList<ProveedorResponseDto>> ExecuteAsync(Guid usuarioId, CancellationToken cancellationToken = default);
}
