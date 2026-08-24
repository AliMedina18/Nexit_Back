using Moq;
using Nexit.Application.UseCases.Historial;
using Nexit.Core.Entities;
using Nexit.Core.Interfaces;
using Nexit.Core.Utils;

namespace Nexit.Tests;

/// <summary>
/// Historial de cambios de Proyecto/Proveedor/Cliente (docs/19 -- "tipo Google Docs/Excel, que se
/// sepa quién hizo el cambio"): el detector de diffs por reflexión y el registrador que lo usa desde
/// los casos de uso de Crear/Actualizar/Eliminar.
/// </summary>
public class HistorialCambiosTests
{
    private class Muestra
    {
        public Guid Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid? UpdatedBy { get; set; }
        public List<string> Etiquetas { get; set; } = [];
    }

    [Fact]
    public void Comparar_detects_a_changed_field()
    {
        var antes = CambioDetector.Snapshot(new Muestra { Nombre = "Acme" });
        var cambios = CambioDetector.Comparar(antes, new Muestra { Nombre = "Acme Corp" });

        var cambio = Assert.Single(cambios);
        Assert.Equal("Nombre", cambio.Campo);
        Assert.Equal("Acme", cambio.ValorAnterior);
        Assert.Equal("Acme Corp", cambio.ValorNuevo);
    }

    [Fact]
    public void Comparar_ignores_fields_that_did_not_change()
    {
        var antes = CambioDetector.Snapshot(new Muestra { Nombre = "Acme", Telefono = "555" });
        var cambios = CambioDetector.Comparar(antes, new Muestra { Nombre = "Acme", Telefono = "555" });

        Assert.Empty(cambios);
    }

    [Fact]
    public void Comparar_reports_a_field_becoming_null()
    {
        var antes = CambioDetector.Snapshot(new Muestra { Nombre = "Acme", Telefono = "555" });
        var cambios = CambioDetector.Comparar(antes, new Muestra { Nombre = "Acme", Telefono = null });

        var cambio = Assert.Single(cambios);
        Assert.Equal("Telefono", cambio.Campo);
        Assert.Equal("555", cambio.ValorAnterior);
        Assert.Null(cambio.ValorNuevo);
    }

    [Fact]
    public void Comparar_never_reports_audit_metadata_fields()
    {
        // Id/UpdatedAt/UpdatedBy son metadata de auditoría, no un dato que alguien haya editado a
        // propósito -- si se reportaran, cada edición generaría "ruido" en el historial (docs/19).
        var updatedBy1 = Guid.NewGuid();
        var updatedBy2 = Guid.NewGuid();
        var antes = CambioDetector.Snapshot(new Muestra { Id = Guid.NewGuid(), Nombre = "Acme", UpdatedAt = DateTime.UtcNow, UpdatedBy = updatedBy1 });
        var cambios = CambioDetector.Comparar(antes, new Muestra { Id = Guid.NewGuid(), Nombre = "Acme", UpdatedAt = DateTime.UtcNow.AddMinutes(5), UpdatedBy = updatedBy2 });

        Assert.Empty(cambios);
    }

    [Fact]
    public void Comparar_ignores_collection_properties()
    {
        // Las colecciones/navegaciones (Etiquetas, Telefonos, Equipo...) no son "campos simples" --
        // compararlas por reflexión rompería o generaría falsos positivos por referencia.
        var antes = CambioDetector.Snapshot(new Muestra { Nombre = "Acme", Etiquetas = ["a"] });
        var cambios = CambioDetector.Comparar(antes, new Muestra { Nombre = "Acme", Etiquetas = ["a", "b"] });

        Assert.Empty(cambios);
    }

    [Fact]
    public async Task HistorialRegistrador_RegistrarEdicionAsync_writes_nothing_when_there_are_no_changes()
    {
        var historial = new Mock<IHistorialCambioRepository>();
        var antes = CambioDetector.Snapshot(new Muestra { Nombre = "Acme" });

        await HistorialRegistrador.RegistrarEdicionAsync(historial.Object, "cliente", Guid.NewGuid(), Guid.NewGuid(), antes, new Muestra { Nombre = "Acme" }, CancellationToken.None);

        historial.Verify(x => x.AddRangeAsync(It.IsAny<IEnumerable<HistorialCambio>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HistorialRegistrador_RegistrarEdicionAsync_writes_one_row_per_changed_field()
    {
        var historial = new Mock<IHistorialCambioRepository>();
        IEnumerable<HistorialCambio>? registrados = null;
        historial.Setup(x => x.AddRangeAsync(It.IsAny<IEnumerable<HistorialCambio>>(), It.IsAny<CancellationToken>()))
            .Callback<IEnumerable<HistorialCambio>, CancellationToken>((r, _) => registrados = r)
            .Returns(Task.CompletedTask);
        var entidadId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var antes = CambioDetector.Snapshot(new Muestra { Nombre = "Acme", Telefono = "555" });

        await HistorialRegistrador.RegistrarEdicionAsync(historial.Object, "proveedor", entidadId, usuarioId, antes, new Muestra { Nombre = "Acme Corp", Telefono = "556" }, CancellationToken.None);

        Assert.NotNull(registrados);
        var lista = registrados!.ToList();
        Assert.Equal(2, lista.Count);
        Assert.All(lista, r => Assert.Equal("edicion", r.Accion));
        Assert.All(lista, r => Assert.Equal("proveedor", r.TipoEntidad));
        Assert.All(lista, r => Assert.Equal(entidadId, r.EntidadId));
        Assert.All(lista, r => Assert.Equal(usuarioId, r.UsuarioId));
        Assert.Contains(lista, r => r.Campo == "Nombre" && r.ValorAnterior == "Acme" && r.ValorNuevo == "Acme Corp");
        Assert.Contains(lista, r => r.Campo == "Telefono" && r.ValorAnterior == "555" && r.ValorNuevo == "556");
    }

    [Fact]
    public async Task HistorialRegistrador_RegistrarCreacionAsync_writes_a_creacion_row_without_a_campo()
    {
        var historial = new Mock<IHistorialCambioRepository>();
        HistorialCambio? registrado = null;
        historial.Setup(x => x.AddAsync(It.IsAny<HistorialCambio>(), It.IsAny<CancellationToken>()))
            .Callback<HistorialCambio, CancellationToken>((r, _) => registrado = r)
            .Returns(Task.CompletedTask);
        var entidadId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();

        await HistorialRegistrador.RegistrarCreacionAsync(historial.Object, "proyecto", entidadId, usuarioId, CancellationToken.None);

        Assert.NotNull(registrado);
        Assert.Equal("creacion", registrado!.Accion);
        Assert.Null(registrado.Campo);
        Assert.Equal(entidadId, registrado.EntidadId);
        Assert.Equal(usuarioId, registrado.UsuarioId);
    }

    [Fact]
    public async Task HistorialRegistrador_RegistrarEliminacionAsync_writes_an_eliminacion_row()
    {
        var historial = new Mock<IHistorialCambioRepository>();
        HistorialCambio? registrado = null;
        historial.Setup(x => x.AddAsync(It.IsAny<HistorialCambio>(), It.IsAny<CancellationToken>()))
            .Callback<HistorialCambio, CancellationToken>((r, _) => registrado = r)
            .Returns(Task.CompletedTask);

        await HistorialRegistrador.RegistrarEliminacionAsync(historial.Object, "cliente", Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);

        Assert.Equal("eliminacion", registrado!.Accion);
    }

    [Fact]
    public async Task ConsultarHistorial_maps_the_authors_full_name()
    {
        var repository = new Mock<IHistorialCambioRepository>();
        var entidadId = Guid.NewGuid();
        var usuario = new Usuario { Nombre = "Ana", Apellido = "Ríos" };
        repository.Setup(x => x.GetPorEntidadAsync("proveedor", entidadId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([new HistorialCambio { TipoEntidad = "proveedor", EntidadId = entidadId, Accion = "edicion", Campo = "Nombre", Usuario = usuario }]);

        var result = await new ConsultarHistorialCambiosUseCase(repository.Object).ExecuteAsync("proveedor", entidadId);

        Assert.Single(result);
        Assert.Equal("Ana Ríos", result[0].UsuarioNombre);
    }
}
