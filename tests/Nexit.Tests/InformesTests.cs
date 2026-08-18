using Moq;
using Nexit.Application.DTOs.Informes;
using Nexit.Application.UseCases.Informes;
using Nexit.Core.Entities;
using Nexit.Core.Exceptions;
using Nexit.Core.Interfaces;

namespace Nexit.Tests;

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
