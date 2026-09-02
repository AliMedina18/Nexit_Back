using ClosedXML.Excel;
using Moq;
using Nexit.Application.DTOs.Clientes;
using Nexit.Application.Validators.Clientes;
using Nexit.Core.Entities;
using Nexit.Core.Exceptions;
using Nexit.Core.Interfaces;
using Nexit.Infrastructure.Services;

namespace Nexit.Tests;

/// <summary>
/// Importar/exportar clientes desde Excel (docs/31) -- cada fila reutiliza exactamente el mismo
/// camino de validación/creación que el formulario (docs/31, ver el comentario de
/// IClientesImportExporter), así que estas pruebas se concentran en lo propio de leer el archivo:
/// filas válidas, filas inválidas que no detienen el resto, y filas vacías que se ignoran.
/// </summary>
public class ClientesImportExporterTests
{
    private static Mock<IClienteRepository> RepositorioSinDuplicados()
    {
        var repo = new Mock<IClienteRepository>();
        repo.Setup(x => x.ExistsByEmailAsync(It.IsAny<string>(), null, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        return repo;
    }

    private static Stream LibroConFila(params string?[] valores)
    {
        using var workbook = new XLWorkbook();
        var hoja = workbook.Worksheets.Add("Clientes");
        string[] columnas = ["Nombre", "Sector", "Ciudad", "Dirección", "Web", "Contacto", "Cargo del contacto", "Email", "Valor de referencia", "Teléfono", "Notas"];
        for (var i = 0; i < columnas.Length; i++) hoja.Cell(1, i + 1).Value = columnas[i];
        for (var i = 0; i < valores.Length; i++) if (valores[i] is not null) hoja.Cell(2, i + 1).Value = valores[i];
        var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;
        return stream;
    }

    [Fact]
    public async Task Importar_crea_una_fila_valida()
    {
        var crear = new Mock<Nexit.Application.UseCases.Clientes.ICrearClienteUseCase>();
        crear.Setup(x => x.ExecuteAsync(It.IsAny<CreateClienteDto>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CreateClienteDto dto, Guid _, CancellationToken _) => new ClienteResponseDto { Nombre = dto.Nombre });
        var validator = new CreateClienteValidator(RepositorioSinDuplicados().Object);
        var importer = new ClientesImportExporter(crear.Object, validator);

        using var archivo = LibroConFila("Acme S.A.", "Retail", "Bogotá", null, null, null, null, null, null, "3000000000", null);
        var resultado = await importer.ImportarAsync(archivo, Guid.NewGuid());

        Assert.Equal(1, resultado.Creados);
        Assert.Empty(resultado.Errores);
        crear.Verify(x => x.ExecuteAsync(It.Is<CreateClienteDto>(d => d.Nombre == "Acme S.A." && d.Telefonos.Count == 1 && d.Telefonos[0].Telefono == "3000000000"), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Importar_reporta_fila_invalida_sin_detener_el_archivo_ni_crear_nada()
    {
        var crear = new Mock<Nexit.Application.UseCases.Clientes.ICrearClienteUseCase>();
        var validator = new CreateClienteValidator(RepositorioSinDuplicados().Object);
        var importer = new ClientesImportExporter(crear.Object, validator);

        // Sin Nombre y sin Teléfono -- ambos requeridos por CreateClienteValidator.
        using var archivo = LibroConFila(null, "Retail", null, null, null, null, null, null, null, null, null);
        var resultado = await importer.ImportarAsync(archivo, Guid.NewGuid());

        Assert.Equal(0, resultado.Creados);
        Assert.Single(resultado.Errores);
        Assert.Equal(2, resultado.Errores[0].Fila);
        crear.Verify(x => x.ExecuteAsync(It.IsAny<CreateClienteDto>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Importar_reporta_como_error_una_excepcion_de_negocio_del_caso_de_uso()
    {
        var crear = new Mock<Nexit.Application.UseCases.Clientes.ICrearClienteUseCase>();
        crear.Setup(x => x.ExecuteAsync(It.IsAny<CreateClienteDto>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new BusinessRuleException("El email ya está registrado"));
        var validator = new CreateClienteValidator(RepositorioSinDuplicados().Object);
        var importer = new ClientesImportExporter(crear.Object, validator);

        using var archivo = LibroConFila("Acme S.A.", null, null, null, null, null, null, "dup@acme.com", null, "3000000000", null);
        var resultado = await importer.ImportarAsync(archivo, Guid.NewGuid());

        Assert.Equal(0, resultado.Creados);
        Assert.Single(resultado.Errores);
        Assert.Contains("email ya está registrado", resultado.Errores[0].Mensaje);
    }

    [Fact]
    public async Task Importar_ignora_una_fila_completamente_vacia()
    {
        var crear = new Mock<Nexit.Application.UseCases.Clientes.ICrearClienteUseCase>();
        var validator = new CreateClienteValidator(RepositorioSinDuplicados().Object);
        var importer = new ClientesImportExporter(crear.Object, validator);

        using var workbook = new XLWorkbook();
        var hoja = workbook.Worksheets.Add("Clientes");
        hoja.Cell(1, 1).Value = "Nombre";
        // Fila 2 se deja completamente vacía a propósito.
        hoja.Cell(3, 1).Value = "Acme S.A.";
        hoja.Cell(3, 10).Value = "3000000000";
        var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;

        crear.Setup(x => x.ExecuteAsync(It.IsAny<CreateClienteDto>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CreateClienteDto dto, Guid _, CancellationToken _) => new ClienteResponseDto { Nombre = dto.Nombre });

        var resultado = await importer.ImportarAsync(stream, Guid.NewGuid());

        Assert.Equal(1, resultado.Creados);
        Assert.Empty(resultado.Errores);
    }

    [Fact]
    public void Exportar_escribe_el_encabezado_y_una_fila_por_cliente()
    {
        var importer = new ClientesImportExporter(Mock.Of<Nexit.Application.UseCases.Clientes.ICrearClienteUseCase>(), Mock.Of<FluentValidation.IValidator<CreateClienteDto>>());
        var clientes = new List<ClienteResponseDto>
        {
            new() { Nombre = "Acme S.A.", Email = "hola@acme.com", Telefonos = [new ClienteTelefonoDto { Telefono = "3000000000" }] },
        };

        var bytes = importer.Exportar(clientes);

        using var workbook = new XLWorkbook(new MemoryStream(bytes));
        var hoja = workbook.Worksheet(1);
        Assert.Equal("Nombre", hoja.Cell(1, 1).GetString());
        Assert.Equal("Acme S.A.", hoja.Cell(2, 1).GetString());
        Assert.Equal("hola@acme.com", hoja.Cell(2, 8).GetString());
        Assert.Equal("3000000000", hoja.Cell(2, 10).GetString());
    }
}
