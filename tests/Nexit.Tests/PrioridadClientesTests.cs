using Moq;
using Nexit.Application.UseCases.Clientes;
using Nexit.Core.Entities;
using Nexit.Core.Interfaces;
using Nexit.Core.Utils;

namespace Nexit.Tests;

/// <summary>
/// Extensión de la rúbrica de prioridad (docs/21, docs/24) a clientes: el calculador puro
/// (<see cref="PrioridadClienteCalculador"/>) y el caso de uso que lo aplica a todos los clientes.
/// </summary>
public class PrioridadClientesTests
{
    private static readonly DateTime Ahora = new(2026, 8, 24, 12, 0, 0, DateTimeKind.Utc);

    private static Cliente ClienteBase() => new() { Nombre = "Acme" };

    [Fact]
    public void Calcular_cliente_con_actividad_reciente_y_pocos_proyectos_no_da_puntaje()
    {
        var cliente = ClienteBase();
        var resultado = PrioridadClienteCalculador.Calcular(cliente, ultimoProyecto: Ahora.AddDays(-10), proyectosActivos: 1, ahora: Ahora);

        Assert.Equal(0, resultado.Puntaje);
        Assert.Empty(resultado.Razones);
    }

    [Fact]
    public void Calcular_marca_sin_actividad_reciente()
    {
        var cliente = ClienteBase();
        var resultado = PrioridadClienteCalculador.Calcular(cliente, ultimoProyecto: Ahora.AddDays(-100), proyectosActivos: 0, ahora: Ahora);

        Assert.Equal(PrioridadClienteCalculador.PuntosSinActividadReciente, resultado.Puntaje);
        Assert.Contains(resultado.Razones, r => r.Contains("Sin proyectos nuevos hace 100 días"));
    }

    [Fact]
    public void Calcular_marca_cliente_sin_ningun_proyecto_todavia()
    {
        var cliente = ClienteBase();
        var resultado = PrioridadClienteCalculador.Calcular(cliente, ultimoProyecto: null, proyectosActivos: 0, ahora: Ahora);

        Assert.Equal(PrioridadClienteCalculador.PuntosClienteSinProyectosAun, resultado.Puntaje);
        Assert.Contains(resultado.Razones, r => r.Contains("sin ningún proyecto todavía"));
    }

    [Fact]
    public void Calcular_marca_cliente_frecuente()
    {
        var cliente = ClienteBase();
        var resultado = PrioridadClienteCalculador.Calcular(cliente, ultimoProyecto: Ahora.AddDays(-2), proyectosActivos: 3, ahora: Ahora);

        Assert.Equal(PrioridadClienteCalculador.PuntosClienteFrecuente, resultado.Puntaje);
        Assert.Contains(resultado.Razones, r => r.Contains("Cliente frecuente: 3 proyectos activos"));
    }

    [Fact]
    public void Calcular_suma_sin_actividad_reciente_y_frecuente()
    {
        // Un cliente puede tener varios proyectos activos (frecuente) y aun así llevar tiempo sin
        // proyectos NUEVOS -- las dos señales no son excluyentes entre sí.
        var cliente = ClienteBase();
        var resultado = PrioridadClienteCalculador.Calcular(cliente, ultimoProyecto: Ahora.AddDays(-95), proyectosActivos: 4, ahora: Ahora);

        Assert.Equal(PrioridadClienteCalculador.PuntosSinActividadReciente + PrioridadClienteCalculador.PuntosClienteFrecuente, resultado.Puntaje);
        Assert.Equal(2, resultado.Razones.Count);
    }

    [Fact]
    public async Task ConsultarPrioridad_orders_by_score_descending_and_includes_reasons()
    {
        var clientes = new Mock<IClienteRepository>();
        var catalogos = new Mock<ICatalogosRepository>();
        var estadoActivo = new EstadoProyecto { Id = Guid.NewGuid(), Nombre = "En curso", Fase = 2 };
        catalogos.Setup(x => x.GetEstadosAsync(null, It.IsAny<CancellationToken>())).ReturnsAsync([estadoActivo]);
        clientes.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(
        [
            new Cliente { Nombre = "Sin proyectos", Proyectos = [] },
            new Cliente { Nombre = "Con actividad", Proyectos = [new Proyecto { EstadoId = estadoActivo.Id, CreatedAt = DateTime.UtcNow.AddDays(-1) }] },
        ]);

        var result = await new ConsultarPrioridadClientesUseCase(clientes.Object, catalogos.Object).ExecuteAsync();

        Assert.Equal("Sin proyectos", result[0].Nombre); // sin ningún proyecto todavía puntúa, con actividad reciente no
        Assert.Equal("Con actividad", result[1].Nombre);
        Assert.NotEmpty(result[0].Razones);
        Assert.Empty(result[1].Razones);
    }

    [Fact]
    public async Task ConsultarPrioridad_counts_only_non_terminal_projects_as_active()
    {
        var clientes = new Mock<IClienteRepository>();
        var catalogos = new Mock<ICatalogosRepository>();
        var estadoActivo = new EstadoProyecto { Id = Guid.NewGuid(), Nombre = "En curso", Fase = 2 };
        var estadoFinalizado = new EstadoProyecto { Id = Guid.NewGuid(), Nombre = "Finalizado", Fase = 2 };
        catalogos.Setup(x => x.GetEstadosAsync(null, It.IsAny<CancellationToken>())).ReturnsAsync([estadoActivo, estadoFinalizado]);
        var cliente = new Cliente
        {
            Nombre = "Con historial", Proyectos =
            [
                new Proyecto { EstadoId = estadoActivo.Id, CreatedAt = DateTime.UtcNow.AddDays(-1) },
                new Proyecto { EstadoId = estadoActivo.Id, CreatedAt = DateTime.UtcNow.AddDays(-2) },
                new Proyecto { EstadoId = estadoActivo.Id, CreatedAt = DateTime.UtcNow.AddDays(-3) },
                new Proyecto { EstadoId = estadoFinalizado.Id, CreatedAt = DateTime.UtcNow.AddDays(-4) }, // no debe contar como "activo"
            ]
        };
        clientes.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync([cliente]);

        var result = await new ConsultarPrioridadClientesUseCase(clientes.Object, catalogos.Object).ExecuteAsync();

        var item = Assert.Single(result);
        Assert.Equal(PrioridadClienteCalculador.PuntosClienteFrecuente, item.Puntaje); // 3 activos, no 4
    }
}
