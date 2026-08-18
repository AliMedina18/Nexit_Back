using Nexit.Application.DTOs.Clientes;
using Nexit.Core.Entities;
using Nexit.Core.Exceptions;
using Nexit.Core.Interfaces;

namespace Nexit.Application.UseCases.Clientes;

public class CrearClienteUseCase(IClienteRepository repository, IUnitOfWork unitOfWork) : ICrearClienteUseCase
{
    public async Task<ClienteResponseDto> ExecuteAsync(CreateClienteDto input, Guid usuarioId, CancellationToken cancellationToken = default)
    {
        var cliente = ClienteMapper.ToEntity(input);
        cliente.CreatedBy = usuarioId;
        cliente.Telefonos = input.Telefonos.Select(phone => new ClienteTelefono
        {
            Id = phone.Id ?? Guid.NewGuid(), ClienteId = cliente.Id, Telefono = phone.Telefono, Etiqueta = phone.Etiqueta
        }).ToList();
        await repository.AddAsync(cliente, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return ClienteMapper.ToResponse(cliente);
    }
}

public class ActualizarClienteUseCase(IClienteRepository repository, IUnitOfWork unitOfWork) : IActualizarClienteUseCase
{
    public async Task<ClienteResponseDto> ExecuteAsync(UpdateClienteDto input, Guid usuarioId, CancellationToken cancellationToken = default)
    {
        var cliente = await repository.GetByIdAsync(input.Id, cancellationToken) ?? throw new EntityNotFoundException("Cliente", input.Id);
        ClienteMapper.Apply(input, cliente);
        cliente.Telefonos.Clear();
        foreach (var phone in input.Telefonos)
        {
            cliente.Telefonos.Add(new ClienteTelefono
            {
                Id = phone.Id ?? Guid.NewGuid(), ClienteId = cliente.Id, Telefono = phone.Telefono, Etiqueta = phone.Etiqueta
            });
        }
        cliente.UpdatedAt = DateTime.UtcNow;
        cliente.UpdatedBy = usuarioId;
        repository.Update(cliente);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return ClienteMapper.ToResponse(cliente);
    }
}

public class ConsultarClientesUseCase(IClienteRepository repository) : IConsultarClientesUseCase
{
    public async Task<IReadOnlyList<ClienteResponseDto>> ListAsync(CancellationToken cancellationToken = default) => (await repository.GetAllAsync(cancellationToken)).Select(ClienteMapper.ToResponse).ToList();
    public async Task<ClienteResponseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => ClienteMapper.ToResponse(await repository.GetByIdAsync(id, cancellationToken) ?? throw new EntityNotFoundException("Cliente", id));
}

internal static class ClienteMapper
{
    public static Cliente ToEntity(CreateClienteDto input) => new()
    {
        Nombre = input.Nombre, Sector = input.Sector, Ciudad = input.Ciudad, Direccion = input.Direccion, Web = input.Web,
        Contacto = input.Contacto, CargoContacto = input.CargoContacto, Email = input.Email, ValorReferencia = input.ValorReferencia, Notas = input.Notas
    };
    public static void Apply(CreateClienteDto input, Cliente cliente)
    {
        cliente.Nombre = input.Nombre; cliente.Sector = input.Sector; cliente.Ciudad = input.Ciudad; cliente.Direccion = input.Direccion;
        cliente.Web = input.Web; cliente.Contacto = input.Contacto; cliente.CargoContacto = input.CargoContacto; cliente.Email = input.Email;
        cliente.ValorReferencia = input.ValorReferencia; cliente.Notas = input.Notas;
    }
    public static ClienteResponseDto ToResponse(Cliente cliente) => new()
    {
        Id = cliente.Id, Nombre = cliente.Nombre, Sector = cliente.Sector, Ciudad = cliente.Ciudad, Direccion = cliente.Direccion,
        Web = cliente.Web, Contacto = cliente.Contacto, CargoContacto = cliente.CargoContacto, Email = cliente.Email,
        ValorReferencia = cliente.ValorReferencia, Notas = cliente.Notas, CreatedAt = cliente.CreatedAt, UpdatedAt = cliente.UpdatedAt,
        Telefonos = cliente.Telefonos.Select(phone => new ClienteTelefonoDto { Id = phone.Id, Telefono = phone.Telefono, Etiqueta = phone.Etiqueta }).ToList()
    };
}

public class EliminarClienteUseCase(IClienteRepository repository, IUnitOfWork unitOfWork) : IEliminarClienteUseCase
{
    public async Task ExecuteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (await repository.GetByIdAsync(id, cancellationToken) is null) throw new EntityNotFoundException("Cliente", id);
        await repository.DeleteAsync(id, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
