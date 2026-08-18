using Moq;
using Nexit.Application.UseCases.Proyectos;
using Nexit.Core.Exceptions;
using Nexit.Core.Interfaces;

namespace Nexit.Tests;

/// <summary>
/// Vista de calendario de proyectos (enero a diciembre, por año) -- ver
/// docs/07-calendario-e-informes-excel.md. El foco de estas pruebas es el relleno de los 12 meses
/// (el repositorio, mockeado aquí, solo devuelve los meses que SÍ tienen proyectos -- el caso de uso
/// es quien completa la grilla) y la validación de año/mes.
/// </summary>
public class CalendarioProyectosTests
{
    [Fact]
    public async Task ObtenerResumenAnio_fills_in_the_12_months_even_when_only_some_have_projects()
    {
        var repository = new Mock<IProyectoRepository>();
        repository.Setup(x => x.ObtenerConteoPorMesAsync(2026, It.IsAny<CancellationToken>()))
            .ReturnsAsync([new ConteoMesProyectos(3, 5), new ConteoMesProyectos(8, 2)]);

        var result = await new ConsultarCalendarioProyectosUseCase(repository.Object).ObtenerResumenAnioAsync(2026);

        Assert.Equal(2026, result.Anio);
        Assert.Equal(12, result.Meses.Count);
        Assert.Equal(7, result.TotalProyectos);
        Assert.Equal(5, result.Meses.Single(m => m.Mes == 3).Cantidad);
        Assert.Equal(2, result.Meses.Single(m => m.Mes == 8).Cantidad);
        Assert.All(result.Meses.Where(m => m.Mes != 3 && m.Mes != 8), m => Assert.Equal(0, m.Cantidad));
    }

    [Fact]
    public async Task ObtenerResumenAnio_returns_all_zeros_for_a_year_with_no_projects()
    {
        var repository = new Mock<IProyectoRepository>();
        repository.Setup(x => x.ObtenerConteoPorMesAsync(1999, It.IsAny<CancellationToken>())).ReturnsAsync([]);

        var result = await new ConsultarCalendarioProyectosUseCase(repository.Object).ObtenerResumenAnioAsync(1999);

        Assert.Equal(0, result.TotalProyectos);
        Assert.All(result.Meses, m => Assert.Equal(0, m.Cantidad));
    }

    [Theory]
    [InlineData(1800)]
    [InlineData(3000)]
    public async Task ObtenerResumenAnio_rejects_an_out_of_range_year(int anio)
    {
        await Assert.ThrowsAsync<BusinessRuleException>(() => new ConsultarCalendarioProyectosUseCase(Mock.Of<IProyectoRepository>()).ObtenerResumenAnioAsync(anio));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(13)]
    public async Task ObtenerProyectosDelMes_rejects_an_out_of_range_month(int mes)
    {
        await Assert.ThrowsAsync<BusinessRuleException>(() => new ConsultarCalendarioProyectosUseCase(Mock.Of<IProyectoRepository>()).ObtenerProyectosDelMesAsync(2026, mes));
    }

    [Fact]
    public async Task ObtenerProyectosDelMes_maps_the_lightweight_projection()
    {
        var repository = new Mock<IProyectoRepository>();
        var proyectoId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        repository.Setup(x => x.ObtenerPorMesAsync(2026, 8, It.IsAny<CancellationToken>()))
            .ReturnsAsync([new ProyectoCalendarioItem(proyectoId, "Lanzamiento", new DateTime(2026, 8, 15), clienteId, "Acme", "En curso", "Alta", "Bogotá", "Bogotá")]);

        var result = await new ConsultarCalendarioProyectosUseCase(repository.Object).ObtenerProyectosDelMesAsync(2026, 8);

        Assert.Single(result);
        Assert.Equal("Lanzamiento", result[0].Nombre);
        Assert.Equal("Acme", result[0].ClienteNombre);
        Assert.Equal("En curso", result[0].EstadoNombre);
    }

    [Fact]
    public async Task ListarAnios_returns_whatever_the_repository_reports()
    {
        var repository = new Mock<IProyectoRepository>();
        repository.Setup(x => x.ObtenerAniosConProyectosAsync(It.IsAny<CancellationToken>())).ReturnsAsync([2026, 2025, 2023]);

        var result = await new ConsultarCalendarioProyectosUseCase(repository.Object).ListarAniosAsync();

        Assert.Equal([2026, 2025, 2023], result);
    }
}
