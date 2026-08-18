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
                // Guid.Empty (no Guid.NewGuid()) a propósito para los teléfonos nuevos: "cliente" ya está
                // rastreado por el DbContext (viene de GetByIdAsync arriba), así que estos ClienteTelefono
                // solo se descubren por fixup de navegación, no por un Add() explícito. EF Core solo puede
                // distinguir "entidad nueva" de "entidad existente" para una clave generada por la base
                // (Id tiene HasDefaultValueSql("gen_random_uuid()")) cuando el valor de esa clave es el
                // default de CLR -- con cualquier otro valor asume que ya existe y genera un UPDATE en vez
                // de un INSERT, que falla porque esa fila no existe todavía (ver docs/08-tipos-de-pruebas.md).
                Id = phone.Id ?? Guid.Empty, ClienteId = cliente.Id, Telefono = phone.Telefono, Etiqueta = phone.Etiqueta
            });
        }
        cliente.UpdatedAt = DateTime.UtcNow;
        cliente.UpdatedBy = usuarioId;
        // OJO: NO llamar repository.Update(cliente) aquí -- "cliente" ya está siendo rastreado por el
        // DbContext (se obtuvo con GetByIdAsync en este mismo scope), así que la llamada es redundante Y,
        // peor, DbSet.Update() recorre TODO el grafo y marca como Modified cualquier entidad hija con un Id
        // ya asignado (no default) que encuentre -- incluyendo los ClienteTelefono nuevos de arriba, a los
        // que aquí se les asigna un Guid explícito. El resultado es que EF genera un UPDATE en vez de un
        // INSERT para esos teléfonos nuevos, que falla con DbUpdateConcurrencyException (0 filas afectadas,
        // porque esa fila todavía no existe) -- un bug real que solo aparece contra Postgres de verdad, no
        // con los repositorios mockeados de las pruebas unitarias (ver docs/08-tipos-de-pruebas.md).
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
