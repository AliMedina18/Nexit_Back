using FluentValidation.TestHelper;
using Moq;
using Nexit.Application.DTOs.Clientes;
using Nexit.Application.UseCases.Clientes;
using Nexit.Application.Validators.Clientes;
using Nexit.Core.Entities;
using Nexit.Core.Exceptions;
using Nexit.Core.Interfaces;

namespace Nexit.Tests;

public class ClientesTests
{
    [Fact]
    public async Task CreateValidator_rejects_empty_name()
    {
        var repository = new Mock<IClienteRepository>();
        var result = await new CreateClienteValidator(repository.Object).TestValidateAsync(new CreateClienteDto { Telefonos = [new ClienteTelefonoDto { Telefono = "3000000000" }] });
        result.ShouldHaveValidationErrorFor(x => x.Nombre);
    }

    [Fact]
    public async Task CreateValidator_rejects_duplicate_email()
    {
        var repository = new Mock<IClienteRepository>();
        repository.Setup(x => x.ExistsByEmailAsync("contacto@nexit.com", null, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var dto = new CreateClienteDto { Nombre = "Nexit", Email = "contacto@nexit.com", Telefonos = [new ClienteTelefonoDto { Telefono = "3000000000" }] };
        var result = await new CreateClienteValidator(repository.Object).TestValidateAsync(dto);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public async Task CrearCliente_assigns_author_and_phones()
    {
        var repository = new Mock<IClienteRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        Cliente? saved = null;
        repository.Setup(x => x.AddAsync(It.IsAny<Cliente>(), It.IsAny<CancellationToken>())).Callback<Cliente, CancellationToken>((client, _) => saved = client).Returns(Task.CompletedTask);
        var authorId = Guid.NewGuid();
        var result = await new CrearClienteUseCase(repository.Object, Mock.Of<IHistorialCambioRepository>(), unitOfWork.Object).ExecuteAsync(new CreateClienteDto { Nombre = "Acme", Telefonos = [new ClienteTelefonoDto { Telefono = "555-0100" }] }, authorId);
        Assert.Equal("Acme", result.Nombre);
        Assert.NotNull(saved);
        Assert.Equal(authorId, saved!.CreatedBy);
        Assert.Single(saved.Telefonos);
        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ActualizarCliente_throws_when_client_does_not_exist()
    {
        var repository = new Mock<IClienteRepository>();
        repository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Cliente?)null);
        await Assert.ThrowsAsync<EntityNotFoundException>(() => new ActualizarClienteUseCase(repository.Object, Mock.Of<IHistorialCambioRepository>(), Mock.Of<IUnitOfWork>()).ExecuteAsync(new UpdateClienteDto { Id = Guid.NewGuid() }, Guid.NewGuid()));
    }

    [Fact]
    public async Task ActualizarCliente_registers_who_made_the_edit()
    {
        var repository = new Mock<IClienteRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var clienteId = Guid.NewGuid();
        var editorId = Guid.NewGuid();
        var cliente = new Cliente { Id = clienteId, Nombre = "Acme" };
        repository.Setup(x => x.GetByIdAsync(clienteId, It.IsAny<CancellationToken>())).ReturnsAsync(cliente);

        await new ActualizarClienteUseCase(repository.Object, Mock.Of<IHistorialCambioRepository>(), unitOfWork.Object).ExecuteAsync(new UpdateClienteDto { Id = clienteId, Nombre = "Acme Corp" }, editorId);

        Assert.Equal(editorId, cliente.UpdatedBy);
        Assert.NotNull(cliente.UpdatedAt);
    }

    [Fact]
    public async Task ConsultarClientes_returns_mapped_clients()
    {
        var repository = new Mock<IClienteRepository>();
        repository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync([new Cliente { Nombre = "Acme" }]);
        var result = await new ConsultarClientesUseCase(repository.Object).ListAsync();
        Assert.Single(result);
        Assert.Equal("Acme", result[0].Nombre);
    }
}
