using Nexit.Application.DTOs.Proveedores;
using Nexit.Core.Entities;
using Nexit.Core.Exceptions;
using Nexit.Core.Interfaces;

namespace Nexit.Application.UseCases.Proveedores;

public interface IProveedorAdjuntosUseCase
{
    Task<IReadOnlyList<ProveedorAdjuntoDto>> ListAsync(Guid proveedorId, CancellationToken cancellationToken = default);
    Task<ProveedorAdjuntoDto> CrearAsync(Guid proveedorId, CrearProveedorAdjuntoDto input, CancellationToken cancellationToken = default);
    Task EliminarAsync(Guid proveedorId, Guid id, CancellationToken cancellationToken = default);
}

public class ProveedorAdjuntosUseCase(IProveedorRepository proveedores, IProveedorAdjuntoRepository adjuntos, IUnitOfWork unitOfWork) : IProveedorAdjuntosUseCase
{
    public async Task<IReadOnlyList<ProveedorAdjuntoDto>> ListAsync(Guid proveedorId, CancellationToken ct = default)
    {
        await AsegurarProveedor(proveedorId, ct);
        return (await adjuntos.GetByProveedorIdAsync(proveedorId, ct)).Select(Map).ToList();
    }

    public async Task<ProveedorAdjuntoDto> CrearAsync(Guid proveedorId, CrearProveedorAdjuntoDto input, CancellationToken ct = default)
    {
        await AsegurarProveedor(proveedorId, ct); Validar(input);
        var adjunto = new ProveedorAdjunto { ProveedorId = proveedorId, Tipo = input.Tipo, Nombre = input.Nombre.Trim(), Url = input.Url?.Trim(), StoragePath = input.StoragePath?.Trim(), Meta = input.Meta?.Trim(), Fecha = input.Fecha ?? DateTime.UtcNow };
        await adjuntos.AddAsync(adjunto, ct); await unitOfWork.SaveChangesAsync(ct);
        return Map(adjunto);
    }

    public async Task EliminarAsync(Guid proveedorId, Guid id, CancellationToken ct = default)
    {
        await AsegurarProveedor(proveedorId, ct);
        var adjunto = await adjuntos.GetByIdAsync(id, ct);
        if (adjunto is null || adjunto.ProveedorId != proveedorId) throw new EntityNotFoundException("ProveedorAdjunto", id);
        await adjuntos.DeleteAsync(id, ct); await unitOfWork.SaveChangesAsync(ct);
    }

    private async Task AsegurarProveedor(Guid id, CancellationToken ct) { if (await proveedores.GetByIdAsync(id, ct) is null) throw new EntityNotFoundException("Proveedor", id); }
    private static void Validar(CrearProveedorAdjuntoDto input)
    {
        if (string.IsNullOrWhiteSpace(input.Nombre)) throw new BusinessRuleException("El nombre del adjunto es requerido.");
        if (input.Tipo == "link" && string.IsNullOrWhiteSpace(input.Url)) throw new BusinessRuleException("Un adjunto de tipo link requiere una URL.");
        if (input.Tipo == "file" && string.IsNullOrWhiteSpace(input.StoragePath)) throw new BusinessRuleException("Un adjunto de tipo file requiere una ruta de almacenamiento.");
        if (input.Tipo is not ("link" or "file")) throw new BusinessRuleException("El tipo de adjunto debe ser link o file.");
    }
    private static ProveedorAdjuntoDto Map(ProveedorAdjunto x) => new() { Id = x.Id, ProveedorId = x.ProveedorId, Tipo = x.Tipo, Nombre = x.Nombre, Url = x.Url, StoragePath = x.StoragePath, Meta = x.Meta, Fecha = x.Fecha, CreatedAt = x.CreatedAt };
}
