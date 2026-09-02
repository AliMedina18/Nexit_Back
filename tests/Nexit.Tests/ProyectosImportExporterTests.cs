using ClosedXML.Excel;
using Moq;
using Nexit.Application.DTOs.Proyectos;
using Nexit.Application.Validators.Proyectos;
using Nexit.Core.Entities;
using Nexit.Core.Interfaces;
using Nexit.Infrastructure.Services;

namespace Nexit.Tests;

/// <summary>
/// Importar/exportar proyectos desde Excel (docs/31) -- la diferencia con clientes/proveedores es
/// que resuelve Cliente y Estado por NOMBRE (Estado es requerido, Cliente es opcional) y no incluye
/// equipo/proveedores/gerente (relaciones, se completan luego en la pantalla de edición).
/// </summary>
public class ProyectosImportExporterTests
{
    private static readonly Guid ClienteId = Guid.NewGuid();
    private static readonly Guid EstadoId = Guid.NewGuid();

    private static Mock<IClienteRepository> ClientesConAcme()
    {
        var clientes = new Mock<IClienteRepository>();
        clientes.Setup(x => x.FindIdPorNombreAsync("Acme S.A.", It.IsAny<CancellationToken>())).ReturnsAsync(ClienteId);
        clientes.Setup(x => x.FindIdPorNombreAsync(It.Is<string>(n => n != "Acme S.A."), It.IsAny<CancellationToken>())).ReturnsAsync((Guid?)null);
        return clientes;
    }

    private static Mock<ICatalogosRepository> CatalogosConEstadoPropuesta()
    {
        var catalogos = new Mock<ICatalogosRepository>();
        catalogos.Setup(x => x.FindEstadoIdPorNombreAsync("Propuesta enviada", It.IsAny<CancellationToken>())).ReturnsAsync(EstadoId);
        catalogos.Setup(x => x.FindEstadoIdPorNombreAsync(It.Is<string>(n => n != "Propuesta enviada"), It.IsAny<CancellationToken>())).ReturnsAsync((Guid?)null);
        catalogos.Setup(x => x.GetEstadosAsync(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<EstadoProyecto> { new() { Id = EstadoId, Nombre = "Propuesta enviada", Fase = 1, Orden = 1 } });
        return catalogos;
    }

    private static Stream LibroConFila(params string?[] valores)
    {
        using var workbook = new XLWorkbook();
        var hoja = workbook.Worksheets.Add("Proyectos");
        string[] columnas = ["Nombre", "Cliente", "Contacto del proyecto", "Tipo de proyecto", "Prioridad", "Ciudad", "Sede Next", "Fecha de solicitud", "Fecha del evento", "Estado", "% de avance", "Estado del brief", "Estado de la propuesta", "N.º de factura", "Pagado (Sí/No)", "Fecha de pago", "Notas"];
        for (var i = 0; i < columnas.Length; i++) hoja.Cell(1, i + 1).Value = columnas[i];
        for (var i = 0; i < valores.Length; i++) if (valores[i] is not null) hoja.Cell(2, i + 1).Value = valores[i];
        var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;
        return stream;
    }

    [Fact]
    public async Task Importar_crea_una_fila_valida_resolviendo_cliente_y_estado_por_nombre()
    {
        var crear = new Mock<Nexit.Application.UseCases.Proyectos.ICrearProyectoUseCase>();
        crear.Setup(x => x.ExecuteAsync(It.IsAny<CrearProyectoDto>(), It.IsAny<Guid>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CrearProyectoDto dto, Guid _, string? _, CancellationToken _) => new ProyectoResponseDto { Nombre = dto.Nombre });
        var validator = new CrearProyectoValidator();
        var importer = new ProyectosImportExporter(crear.Object, validator, ClientesConAcme().Object, CatalogosConEstadoPropuesta().Object);

        using var archivo = LibroConFila("Lanzamiento producto", "Acme S.A.", null, null, null, null, null, null, null, "Propuesta enviada", null, null, null, null, null, null, null);
        var resultado = await importer.ImportarAsync(archivo, Guid.NewGuid(), "admin");

        Assert.Equal(1, resultado.Creados);
        Assert.Empty(resultado.Errores);
        crear.Verify(x => x.ExecuteAsync(It.Is<CrearProyectoDto>(d => d.Nombre == "Lanzamiento producto" && d.ClienteId == ClienteId && d.EstadoId == EstadoId), It.IsAny<Guid>(), "admin", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Importar_reporta_error_cuando_el_estado_no_existe()
    {
        var crear = new Mock<Nexit.Application.UseCases.Proyectos.ICrearProyectoUseCase>();
        var validator = new CrearProyectoValidator();
        var importer = new ProyectosImportExporter(crear.Object, validator, ClientesConAcme().Object, CatalogosConEstadoPropuesta().Object);

        using var archivo = LibroConFila("Lanzamiento producto", null, null, null, null, null, null, null, null, "Estado inventado", null, null, null, null, null, null, null);
        var resultado = await importer.ImportarAsync(archivo, Guid.NewGuid(), "admin");

        Assert.Equal(0, resultado.Creados);
        Assert.Single(resultado.Errores);
        Assert.Contains("Estado inventado", resultado.Errores[0].Mensaje);
    }

    [Fact]
    public async Task Importar_reporta_error_cuando_falta_el_estado()
    {
        var crear = new Mock<Nexit.Application.UseCases.Proyectos.ICrearProyectoUseCase>();
        var validator = new CrearProyectoValidator();
        var importer = new ProyectosImportExporter(crear.Object, validator, ClientesConAcme().Object, CatalogosConEstadoPropuesta().Object);

        using var archivo = LibroConFila("Lanzamiento producto", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null);
        var resultado = await importer.ImportarAsync(archivo, Guid.NewGuid(), "admin");

        Assert.Equal(0, resultado.Creados);
        Assert.Single(resultado.Errores);
        Assert.Contains("requerido", resultado.Errores[0].Mensaje);
    }

    [Fact]
    public async Task Importar_reporta_error_cuando_el_cliente_nombrado_no_existe()
    {
        var crear = new Mock<Nexit.Application.UseCases.Proyectos.ICrearProyectoUseCase>();
        var validator = new CrearProyectoValidator();
        var importer = new ProyectosImportExporter(crear.Object, validator, ClientesConAcme().Object, CatalogosConEstadoPropuesta().Object);

        using var archivo = LibroConFila("Lanzamiento producto", "Cliente inexistente", null, null, null, null, null, null, null, "Propuesta enviada", null, null, null, null, null, null, null);
        var resultado = await importer.ImportarAsync(archivo, Guid.NewGuid(), "admin");

        Assert.Equal(0, resultado.Creados);
        Assert.Single(resultado.Errores);
        Assert.Contains("Cliente inexistente", resultado.Errores[0].Mensaje);
        crear.Verify(x => x.ExecuteAsync(It.IsAny<CrearProyectoDto>(), It.IsAny<Guid>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Importar_interpreta_Si_como_pagado_true()
    {
        var crear = new Mock<Nexit.Application.UseCases.Proyectos.ICrearProyectoUseCase>();
        CrearProyectoDto? capturado = null;
        crear.Setup(x => x.ExecuteAsync(It.IsAny<CrearProyectoDto>(), It.IsAny<Guid>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .Callback<CrearProyectoDto, Guid, string?, CancellationToken>((dto, _, _, _) => capturado = dto)
            .ReturnsAsync((CrearProyectoDto dto, Guid _, string? _, CancellationToken _) => new ProyectoResponseDto { Nombre = dto.Nombre });
        var validator = new CrearProyectoValidator();
        var importer = new ProyectosImportExporter(crear.Object, validator, ClientesConAcme().Object, CatalogosConEstadoPropuesta().Object);

        using var archivo = LibroConFila("Lanzamiento producto", null, null, null, null, null, null, null, null, "Propuesta enviada", null, null, null, null, "Sí", "2026-09-01", null);
        var resultado = await importer.ImportarAsync(archivo, Guid.NewGuid(), "admin");

        Assert.Equal(1, resultado.Creados);
        Assert.True(capturado!.Pagado);
        Assert.NotNull(capturado.FechaPago);
    }

    [Fact]
    public async Task Exportar_escribe_el_encabezado_resolviendo_nombre_de_cliente_y_estado()
    {
        var clientes = new Mock<IClienteRepository>();
        clientes.Setup(x => x.GetByIdAsync(ClienteId, It.IsAny<CancellationToken>())).ReturnsAsync(new Cliente { Id = ClienteId, Nombre = "Acme S.A." });
        var importer = new ProyectosImportExporter(Mock.Of<Nexit.Application.UseCases.Proyectos.ICrearProyectoUseCase>(), Mock.Of<FluentValidation.IValidator<CrearProyectoDto>>(), clientes.Object, CatalogosConEstadoPropuesta().Object);
        var proyectos = new List<ProyectoResponseDto> { new() { Nombre = "Lanzamiento producto", ClienteId = ClienteId, EstadoId = EstadoId, EstadoBrief = "Pendiente por enviar", PropuestaEstado = "No enviada" } };

        var bytes = await importer.ExportarAsync(proyectos);

        using var workbook = new XLWorkbook(new MemoryStream(bytes));
        var hoja = workbook.Worksheet(1);
        Assert.Equal("Lanzamiento producto", hoja.Cell(2, 1).GetString());
        Assert.Equal("Acme S.A.", hoja.Cell(2, 2).GetString());
        Assert.Equal("Propuesta enviada", hoja.Cell(2, 10).GetString());
    }
}
