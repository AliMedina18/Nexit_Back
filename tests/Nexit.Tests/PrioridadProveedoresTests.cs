using Moq;
using Nexit.Application.UseCases.Proveedores;
using Nexit.Core.Entities;
using Nexit.Core.Interfaces;
using Nexit.Core.Utils;

namespace Nexit.Tests;

/// <summary>
/// Extensión de la rúbrica de prioridad (docs/21, docs/24) a proveedores: el calculador puro
/// (<see cref="PrioridadProveedorCalculador"/>) y el caso de uso que lo aplica a los proveedores
/// no bloqueados.
/// </summary>
public class PrioridadProveedoresTests
{
    private static readonly DateTime Ahora = new(2026, 8, 24, 12, 0, 0, DateTimeKind.Utc);

    private static Proveedor ProveedorBase() => new() { Nombre = "Salón Azul" };

    [Fact]
    public void Calcular_sin_score_y_sin_uso_no_da_puntaje()
    {
        var proveedor = ProveedorBase();
        var resultado = PrioridadProveedorCalculador.Calcular(proveedor, ultimoProyectoAsignado: null, ahora: Ahora);

        Assert.Equal(0, resultado.Puntaje);
        Assert.Empty(resultado.Razones);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public void Calcular_marca_score_bajo(int score)
    {
        var proveedor = ProveedorBase();
        proveedor.Score = score;

        var resultado = PrioridadProveedorCalculador.Calcular(proveedor, ultimoProyectoAsignado: null, ahora: Ahora);

        Assert.Equal(PrioridadProveedorCalculador.PuntosScoreBajo, resultado.Puntaje);
        Assert.Contains(resultado.Razones, r => r.Contains("Score bajo"));
    }

    [Fact]
    public void Calcular_no_marca_score_medio_como_bajo_ni_como_bueno()
    {
        var proveedor = ProveedorBase();
        proveedor.Score = 3;

        var resultado = PrioridadProveedorCalculador.Calcular(proveedor, ultimoProyectoAsignado: null, ahora: Ahora);

        Assert.Equal(0, resultado.Puntaje);
    }

    [Fact]
    public void Calcular_marca_sin_calificar_si_ya_se_uso()
    {
        var proveedor = ProveedorBase();

        var resultado = PrioridadProveedorCalculador.Calcular(proveedor, ultimoProyectoAsignado: Ahora.AddDays(-10), ahora: Ahora);

        Assert.Equal(PrioridadProveedorCalculador.PuntosSinCalificarYaUsado, resultado.Puntaje);
        Assert.Contains(resultado.Razones, r => r.Contains("nunca se le asignó un Score"));
    }

    [Fact]
    public void Calcular_no_marca_sin_calificar_si_nunca_se_ha_usado()
    {
        // Un proveedor nuevo sin Score y sin ningún proyecto todavía no es "falta calificarlo" --
        // simplemente no se ha usado aún, no hay nada raro que señalar.
        var proveedor = ProveedorBase();

        var resultado = PrioridadProveedorCalculador.Calcular(proveedor, ultimoProyectoAsignado: null, ahora: Ahora);

        Assert.Equal(0, resultado.Puntaje);
    }

    [Fact]
    public void Calcular_marca_buen_proveedor_sin_uso_reciente()
    {
        var proveedor = ProveedorBase();
        proveedor.Score = 5;

        var resultado = PrioridadProveedorCalculador.Calcular(proveedor, ultimoProyectoAsignado: Ahora.AddDays(-100), ahora: Ahora);

        Assert.Equal(PrioridadProveedorCalculador.PuntosBuenProveedorSinUsoReciente, resultado.Puntaje);
        Assert.Contains(resultado.Razones, r => r.Contains("sin proyectos en los últimos 100 días"));
    }

    [Fact]
    public void Calcular_marca_buen_proveedor_nunca_asignado()
    {
        var proveedor = ProveedorBase();
        proveedor.Score = 4;

        var resultado = PrioridadProveedorCalculador.Calcular(proveedor, ultimoProyectoAsignado: null, ahora: Ahora);

        Assert.Equal(PrioridadProveedorCalculador.PuntosBuenProveedorSinUsoReciente, resultado.Puntaje);
        Assert.Contains(resultado.Razones, r => r.Contains("nunca se le ha asignado un proyecto"));
    }

    [Fact]
    public void Calcular_no_marca_buen_proveedor_con_uso_reciente()
    {
        var proveedor = ProveedorBase();
        proveedor.Score = 5;

        var resultado = PrioridadProveedorCalculador.Calcular(proveedor, ultimoProyectoAsignado: Ahora.AddDays(-10), ahora: Ahora);

        Assert.Equal(0, resultado.Puntaje);
    }

    [Fact]
    public void Calcular_marca_colaboradores_sin_proyecto_formal()
    {
        var proveedor = ProveedorBase();
        proveedor.Colaboradores = [new ProveedorColaborador(), new ProveedorColaborador()];

        var resultado = PrioridadProveedorCalculador.Calcular(proveedor, ultimoProyectoAsignado: null, ahora: Ahora);

        Assert.Equal(PrioridadProveedorCalculador.PuntosColaboradoresSinProyectoFormal, resultado.Puntaje);
        Assert.Contains(resultado.Razones, r => r.Contains("2 personas del equipo"));
    }

    [Fact]
    public void Calcular_no_marca_colaboradores_si_ya_tiene_proyecto_formal()
    {
        // Score = 3 (ni bajo ni alto) para aislar la señal de colaboradores de las otras dos.
        var proveedor = ProveedorBase();
        proveedor.Score = 3;
        proveedor.Colaboradores = [new ProveedorColaborador(), new ProveedorColaborador()];

        var resultado = PrioridadProveedorCalculador.Calcular(proveedor, ultimoProyectoAsignado: Ahora.AddDays(-1), ahora: Ahora);

        Assert.Equal(0, resultado.Puntaje);
    }

    [Fact]
    public void Calcular_suma_score_bajo_y_colaboradores_sin_proyecto()
    {
        var proveedor = ProveedorBase();
        proveedor.Score = 1;
        proveedor.Colaboradores = [new ProveedorColaborador(), new ProveedorColaborador(), new ProveedorColaborador()];

        var resultado = PrioridadProveedorCalculador.Calcular(proveedor, ultimoProyectoAsignado: null, ahora: Ahora);

        Assert.Equal(PrioridadProveedorCalculador.PuntosScoreBajo + PrioridadProveedorCalculador.PuntosColaboradoresSinProyectoFormal, resultado.Puntaje);
        Assert.Equal(2, resultado.Razones.Count);
    }

    [Fact]
    public async Task ConsultarPrioridad_excludes_blocked_providers()
    {
        var proveedores = new Mock<IProveedorRepository>();
        proveedores.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(
        [
            new Proveedor { Nombre = "Activo", Estado = "Activo", Score = 1 },
            new Proveedor { Nombre = "Bloqueado", Estado = "Bloqueado", Score = 1 },
        ]);

        var result = await new ConsultarPrioridadProveedoresUseCase(proveedores.Object).ExecuteAsync();

        var item = Assert.Single(result);
        Assert.Equal("Activo", item.Nombre);
    }

    [Fact]
    public async Task ConsultarPrioridad_orders_by_score_descending_and_includes_reasons()
    {
        var proveedores = new Mock<IProveedorRepository>();
        proveedores.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(
        [
            new Proveedor { Nombre = "Sin señales", Estado = "Activo", Score = 3 },
            new Proveedor { Nombre = "Score bajo", Estado = "Activo", Score = 1 },
        ]);

        var result = await new ConsultarPrioridadProveedoresUseCase(proveedores.Object).ExecuteAsync();

        Assert.Equal("Score bajo", result[0].Nombre);
        Assert.Equal("Sin señales", result[1].Nombre);
        Assert.NotEmpty(result[0].Razones);
        Assert.Empty(result[1].Razones);
    }

    [Fact]
    public async Task ConsultarPrioridad_uses_the_most_recent_project_assigned()
    {
        var proveedores = new Mock<IProveedorRepository>();
        var proyectoViejo = new Proyecto { Nombre = "Viejo", CreatedAt = DateTime.UtcNow.AddDays(-200) };
        var proyectoReciente = new Proyecto { Nombre = "Reciente", CreatedAt = DateTime.UtcNow.AddDays(-5) };
        var proveedor = new Proveedor
        {
            Nombre = "Con historial", Estado = "Activo", Score = 5,
            Proyectos =
            [
                new ProyectoProveedor { Proyecto = proyectoViejo },
                new ProyectoProveedor { Proyecto = proyectoReciente },
            ]
        };
        proveedores.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync([proveedor]);

        var result = await new ConsultarPrioridadProveedoresUseCase(proveedores.Object).ExecuteAsync();

        // Se usó el proyecto MÁS reciente (hace 5 días, no 200) -- no debería marcarse "sin uso reciente".
        var item = Assert.Single(result);
        Assert.Equal(0, item.Puntaje);
    }
}
