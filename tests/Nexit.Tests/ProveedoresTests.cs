using FluentValidation.TestHelper;
using Moq;
using Nexit.Application.DTOs.Proveedores;
using Nexit.Application.UseCases.Proveedores;
using Nexit.Application.Validators.Proveedores;
using Nexit.Core.Entities;
using Nexit.Core.Exceptions;
using Nexit.Core.Interfaces;

namespace Nexit.Tests;

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
        var result = await new CrearProveedorUseCase(proveedores.Object, catalogos.Object, Mock.Of<IHistorialCambioRepository>(), Mock.Of<IUnitOfWork>()).ExecuteAsync(new CreateProveedorDto { Nombre = "Venue Central", PaisId = paisId, CategoriaId = categoriaId }, Guid.NewGuid());
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
        await Assert.ThrowsAsync<BusinessRuleException>(() => new CrearProveedorUseCase(Mock.Of<IProveedorRepository>(), catalogos.Object, Mock.Of<IHistorialCambioRepository>(), Mock.Of<IUnitOfWork>()).ExecuteAsync(new CreateProveedorDto { Nombre = "Venue", PaisId = paisId, CategoriaId = categoriaId, RegionId = regionId }, Guid.NewGuid()));
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

    [Fact]
    public async Task ActualizarProveedor_registers_who_made_the_edit()
    {
        var repository = new Mock<IProveedorRepository>();
        var catalogos = new Mock<ICatalogosRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var proveedorId = Guid.NewGuid();
        var editorId = Guid.NewGuid();
        var paisId = Guid.NewGuid();
        var categoriaId = Guid.NewGuid();
        var proveedor = new Proveedor { Id = proveedorId, Nombre = "Venue Central", PaisId = paisId, CategoriaId = categoriaId };
        repository.Setup(x => x.GetByIdAsync(proveedorId, It.IsAny<CancellationToken>())).ReturnsAsync(proveedor);
        catalogos.Setup(x => x.GetPaisAsync(paisId, It.IsAny<CancellationToken>())).ReturnsAsync(new Pais { Id = paisId });
        catalogos.Setup(x => x.GetCategoriaAsync(categoriaId, It.IsAny<CancellationToken>())).ReturnsAsync(new CategoriaProveedor { Id = categoriaId });

        await new ActualizarProveedorUseCase(repository.Object, catalogos.Object, Mock.Of<IHistorialCambioRepository>(), unitOfWork.Object).ExecuteAsync(new UpdateProveedorDto { Id = proveedorId, Nombre = "Venue Central Renovado", PaisId = paisId, CategoriaId = categoriaId }, editorId);

        Assert.Equal(editorId, proveedor.UpdatedBy);
        Assert.NotNull(proveedor.UpdatedAt);
    }
}
