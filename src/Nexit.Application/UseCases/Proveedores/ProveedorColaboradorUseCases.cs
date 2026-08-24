using Nexit.Application.DTOs.Proveedores;
using Nexit.Core.Exceptions;
using Nexit.Core.Interfaces;

namespace Nexit.Application.UseCases.Proveedores;

public class MarcarColaboradorProveedorUseCase(IProveedorColaboradorRepository colaboradores, IProveedorRepository proveedores, IUnitOfWork unitOfWork) : IMarcarColaboradorProveedorUseCase
{
    public async Task ExecuteAsync(Guid proveedorId, Guid usuarioId, CancellationToken cancellationToken = default)
    {
        if (await proveedores.GetByIdAsync(proveedorId, cancellationToken) is null) throw new EntityNotFoundException("Proveedor", proveedorId);
        if (await colaboradores.ExisteAsync(proveedorId, usuarioId, cancellationToken)) return; // ya estaba marcado, sin efecto
        await colaboradores.AgregarAsync(proveedorId, usuarioId, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

public class QuitarColaboradorProveedorUseCase(IProveedorColaboradorRepository colaboradores, IUnitOfWork unitOfWork) : IQuitarColaboradorProveedorUseCase
{
    public async Task ExecuteAsync(Guid proveedorId, Guid usuarioId, CancellationToken cancellationToken = default)
    {
        await colaboradores.QuitarAsync(proveedorId, usuarioId, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

public class ListarMisProveedoresUseCase(IProveedorColaboradorRepository colaboradores, IProveedorRepository proveedores) : IListarMisProveedoresUseCase
{
    public async Task<IReadOnlyList<ProveedorResponseDto>> ExecuteAsync(Guid usuarioId, CancellationToken cancellationToken = default)
    {
        var idsAsignados = (await colaboradores.GetProveedorIdsPorUsuarioAsync(usuarioId, cancellationToken)).ToHashSet();
        if (idsAsignados.Count == 0) return [];
        return (await proveedores.GetAllAsync(cancellationToken)).Where(p => idsAsignados.Contains(p.Id)).Select(ProveedorMapper.ToResponse).ToList();
    }
}
