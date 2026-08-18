using Moq;
using Nexit.Application.DTOs.Catalogos;
using Nexit.Application.UseCases.Catalogos;
using Nexit.Core.Entities;
using Nexit.Core.Exceptions;
using Nexit.Core.Interfaces;

namespace Nexit.Tests;

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
