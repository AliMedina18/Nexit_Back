using FluentValidation.TestHelper;
using Moq;
using Nexit.Application.DTOs.Clientes;
using Nexit.Application.UseCases.Clientes;
using Nexit.Application.DTOs.Catalogos;
using Nexit.Application.UseCases.Catalogos;
using Nexit.Application.DTOs.Proveedores;
using Nexit.Application.UseCases.Proveedores;
using Nexit.Application.DTOs.Proyectos;
using Nexit.Application.UseCases.Proyectos;
using Nexit.Application.DTOs.Informes;
using Nexit.Application.UseCases.Informes;
using Nexit.Application.Validators.Proveedores;
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
        var result = await new CrearClienteUseCase(repository.Object, unitOfWork.Object).ExecuteAsync(new CreateClienteDto { Nombre = "Acme", Telefonos = [new ClienteTelefonoDto { Telefono = "555-0100" }] }, authorId);
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
        await Assert.ThrowsAsync<EntityNotFoundException>(() => new ActualizarClienteUseCase(repository.Object, Mock.Of<IUnitOfWork>()).ExecuteAsync(new UpdateClienteDto { Id = Guid.NewGuid() }));
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

public class ProveedoresTests
{
    [Fact]
    public async Task CrearProveedor_persists_a_valid_provider()
    {
        var proveedores = new Mock<IProveedorRepository>();
        var catalogos = new Mock<ICatalogosRepository>();
        var paisId = Guid.NewGuid();
        var categoriaId = Guid.NewGuid();
        catalogos.Setup(x => x.GetPaisAsync(paisId, It.IsAny<CancellationToken>())).ReturnsAsync(new Pais { Id = paisId, Nombre = "Colombia" });
        catalogos.Setup(x => x.GetCategoriaAsync(categoriaId, It.IsAny<CancellationToken>())).ReturnsAsync(new CategoriaProveedor { Id = categoriaId, Nombre = "Hotel" });
        var result = await new CrearProveedorUseCase(proveedores.Object, catalogos.Object, Mock.Of<IUnitOfWork>()).ExecuteAsync(new CreateProveedorDto { Nombre = "Venue Central", PaisId = paisId, CategoriaId = categoriaId }, Guid.NewGuid());
        Assert.Equal("Venue Central", result.Nombre);
        proveedores.Verify(x => x.AddAsync(It.IsAny<Proveedor>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CrearProveedor_rejects_region_from_another_country()
    {
        var catalogos = new Mock<ICatalogosRepository>();
        var paisId = Guid.NewGuid();
        var categoriaId = Guid.NewGuid();
        var regionId = Guid.NewGuid();
        catalogos.Setup(x => x.GetPaisAsync(paisId, It.IsAny<CancellationToken>())).ReturnsAsync(new Pais { Id = paisId });
        catalogos.Setup(x => x.GetCategoriaAsync(categoriaId, It.IsAny<CancellationToken>())).ReturnsAsync(new CategoriaProveedor { Id = categoriaId });
        catalogos.Setup(x => x.GetRegionAsync(regionId, It.IsAny<CancellationToken>())).ReturnsAsync(new Region { Id = regionId, PaisId = Guid.NewGuid() });
        await Assert.ThrowsAsync<BusinessRuleException>(() => new CrearProveedorUseCase(Mock.Of<IProveedorRepository>(), catalogos.Object, Mock.Of<IUnitOfWork>()).ExecuteAsync(new CreateProveedorDto { Nombre = "Venue", PaisId = paisId, CategoriaId = categoriaId, RegionId = regionId }, Guid.NewGuid()));
    }

    [Fact]
    public async Task UpdateProveedorValidator_allows_its_own_email()
    {
        var repository = new Mock<IProveedorRepository>();
        var id = Guid.NewGuid();
        repository.Setup(x => x.ExistsByEmailAsync("venue@nexit.com", id, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        var result = await new UpdateProveedorValidator(repository.Object).TestValidateAsync(new UpdateProveedorDto { Id = id, Nombre = "Venue", PaisId = Guid.NewGuid(), CategoriaId = Guid.NewGuid(), Email = "venue@nexit.com" });
        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }
}

public class CatalogosTests
{
    [Fact]
    public async Task CrearPais_persists_a_unique_catalog_entry()
    {
        var repository = new Mock<ICatalogosRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        repository.Setup(x => x.NombreExisteAsync<Pais>("Colombia", null, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        var result = await new CatalogosService(repository.Object, unitOfWork.Object).CrearPaisAsync(new CrearPaisDto("Colombia", "Departamento"));
        Assert.Equal("Colombia", result.Nombre);
        repository.Verify(x => x.AddAsync(It.Is<Pais>(pais => pais.Nombre == "Colombia"), It.IsAny<CancellationToken>()), Times.Once);
        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetRegiones_returns_only_the_requested_country_regions()
    {
        var repository = new Mock<ICatalogosRepository>();
        var paisId = Guid.NewGuid();
        repository.Setup(x => x.GetRegionesAsync(paisId, It.IsAny<CancellationToken>())).ReturnsAsync([new Region { PaisId = paisId, Nombre = "Antioquia" }]);
        var result = await new CatalogosService(repository.Object, Mock.Of<IUnitOfWork>()).GetRegionesAsync(paisId);
        Assert.Single(result);
        Assert.Equal("Antioquia", result[0].Nombre);
    }

    [Fact]
    public async Task CrearEstado_rejects_an_unknown_phase()
    {
        var repository = new Mock<ICatalogosRepository>();
        repository.Setup(x => x.GetFaseAsync(3, It.IsAny<CancellationToken>())).ReturnsAsync((FaseProyecto?)null);
        await Assert.ThrowsAsync<BusinessRuleException>(() => new CatalogosService(repository.Object, Mock.Of<IUnitOfWork>()).CrearEstadoAsync(new CrearEstadoProyectoDto("Facturado", 3, 9)));
    }
}

public class ProyectosTests
{
    [Fact]
    public async Task CrearProyecto_persists_project_with_team_and_providers()
    {
        var proyectos = new Mock<IProyectoRepository>();
        var clientes = new Mock<IClienteRepository>();
        var proveedores = new Mock<IProveedorRepository>();
        var catalogos = new Mock<ICatalogosRepository>();
        var clienteId = Guid.NewGuid();
        var estadoId = Guid.NewGuid();
        var proveedorId = Guid.NewGuid();
        clientes.Setup(x => x.GetByIdAsync(clienteId, It.IsAny<CancellationToken>())).ReturnsAsync(new Cliente { Id = clienteId });
        catalogos.Setup(x => x.GetEstadoAsync(estadoId, It.IsAny<CancellationToken>())).ReturnsAsync(new EstadoProyecto { Id = estadoId });
        proveedores.Setup(x => x.GetByIdAsync(proveedorId, It.IsAny<CancellationToken>())).ReturnsAsync(new Proveedor { Id = proveedorId });

        var result = await new CrearProyectoUseCase(proyectos.Object, clientes.Object, proveedores.Object, catalogos.Object, Mock.Of<IUnitOfWork>()).ExecuteAsync(
            new CrearProyectoDto { Nombre = "Lanzamiento", ClienteId = clienteId, EstadoId = estadoId, Equipo = [new ProyectoEquipoDto { Nombre = "Ana", Rol = "Ejecutivo" }], ProveedorIds = [proveedorId] }, Guid.NewGuid());

        Assert.Equal("Lanzamiento", result.Nombre);
        proyectos.Verify(x => x.AddAsync(It.Is<Proyecto>(p => p.Equipo.Count == 1 && p.Proveedores.Count == 1), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CrearProyecto_rejects_unknown_state()
    {
        var estadoId = Guid.NewGuid();
        var catalogos = new Mock<ICatalogosRepository>();
        catalogos.Setup(x => x.GetEstadoAsync(estadoId, It.IsAny<CancellationToken>())).ReturnsAsync((EstadoProyecto?)null);

        await Assert.ThrowsAsync<BusinessRuleException>(() => new CrearProyectoUseCase(Mock.Of<IProyectoRepository>(), Mock.Of<IClienteRepository>(), Mock.Of<IProveedorRepository>(), catalogos.Object, Mock.Of<IUnitOfWork>())
            .ExecuteAsync(new CrearProyectoDto { Nombre = "Lanzamiento", EstadoId = estadoId }, Guid.NewGuid()));
    }

    [Fact]
    public async Task AgregarSeguimiento_assigns_author_and_saves()
    {
        var proyectoId = Guid.NewGuid();
        var autorId = Guid.NewGuid();
        var proyectos = new Mock<IProyectoRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        proyectos.Setup(x => x.GetByIdAsync(proyectoId, It.IsAny<CancellationToken>())).ReturnsAsync(new Proyecto { Id = proyectoId });

        var result = await new AgregarSeguimientoProyectoUseCase(proyectos.Object, unitOfWork.Object).ExecuteAsync(proyectoId, new CrearSeguimientoProyectoDto { Nota = "Proveedor confirmado" }, autorId);

        Assert.Equal(autorId, result.AutorId);
        Assert.Equal("General", result.Area);
        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}

public class InformesTests
{
    [Fact]
    public async Task GenerarSnapshot_persists_current_totals()
    {
        var repository = new Mock<IInformesRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        repository.Setup(x => x.GetByPeriodoAsync("mensual", "2026-08", It.IsAny<CancellationToken>())).ReturnsAsync((InformeSnapshot?)null);
        repository.Setup(x => x.ObtenerDatosAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new InformeDatos(3, 2, 4, 1, new Dictionary<string, int> { ["En curso"] = 2 }, new Dictionary<string, int> { ["Aprobado"] = 4 }));

        var result = await new GenerarInformeSnapshotUseCase(repository.Object, unitOfWork.Object).ExecuteAsync(new CrearInformeSnapshotDto { Tipo = "mensual", PeriodoKey = "2026-08" }, Guid.NewGuid());

        Assert.Equal(4, result.TotalProyectos);
        Assert.Equal(2, result.PorEstado["En curso"]);
        repository.Verify(x => x.AddAsync(It.Is<InformeSnapshot>(s => s.TotalClientes == 2 && s.PeriodoKey == "2026-08"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GenerarSnapshot_rejects_an_invalid_type()
    {
        await Assert.ThrowsAsync<BusinessRuleException>(() => new GenerarInformeSnapshotUseCase(Mock.Of<IInformesRepository>(), Mock.Of<IUnitOfWork>())
            .ExecuteAsync(new CrearInformeSnapshotDto { Tipo = "diario", PeriodoKey = "2026-08-17" }, Guid.NewGuid()));
    }
}
