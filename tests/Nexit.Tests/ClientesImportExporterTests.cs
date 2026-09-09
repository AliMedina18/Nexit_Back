using ClosedXML.Excel;
using FluentValidation;
using Moq;
using Nexit.Application.DTOs.Clientes;
using Nexit.Application.UseCases.Clientes;
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
///
/// Desde docs/35 el importador también ACTUALIZA en vez de duplicar cuando ya existe un cliente con
/// el mismo nombre. Esa rama tiene sus propias pruebas al final -- hasta ahora solo se había
/// verificado con una simulación en Python, nunca con el código real.
/// </summary>
public class ClientesImportExporterTests
{
    private static readonly Guid PaisId = Guid.NewGuid();

    private static Mock<IClienteRepository> RepositorioSinDuplicados()
    {
        var repo = new Mock<IClienteRepository>();
        repo.Setup(x => x.ExistsByEmailAsync(It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        return repo;
    }

    /// <summary>
    /// Arma el importador con las seis piezas que pide desde docs/35. Por defecto el repositorio no
    /// encuentra ningún cliente con el mismo nombre (`FindIdPorNombreAsync` devuelve null, el valor
    /// por defecto de Moq), así que se toma la rama de CREAR -- que es la que prueban todos los
    /// casos salvo los de reimportar del final.
    /// </summary>
    private static ClientesImportExporter Importer(
        Mock<ICrearClienteUseCase> crear,
        ICatalogosRepository catalogos,
        Mock<IClienteRepository>? repositorio = null,
        Mock<IActualizarClienteUseCase>? actualizar = null)
    {
        var repo = repositorio ?? RepositorioSinDuplicados();
        return new ClientesImportExporter(
            crear.Object,
            (actualizar ?? new Mock<IActualizarClienteUseCase>()).Object,
            new CreateClienteValidator(repo.Object),
            new UpdateClienteValidator(repo.Object),
            repo.Object,
            catalogos);
    }

    private static Mock<ICrearClienteUseCase> CrearQueDevuelveElCliente()
    {
        var crear = new Mock<ICrearClienteUseCase>();
        crear.Setup(x => x.ExecuteAsync(It.IsAny<CreateClienteDto>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CreateClienteDto dto, Guid _, CancellationToken _) => new ClienteResponseDto { Nombre = dto.Nombre });
        return crear;
    }

    private static Mock<ICatalogosRepository> CatalogosConColombia()
    {
        var catalogos = new Mock<ICatalogosRepository>();
        catalogos.Setup(x => x.FindPaisIdPorNombreAsync("Colombia", It.IsAny<CancellationToken>())).ReturnsAsync(PaisId);
        catalogos.Setup(x => x.FindPaisIdPorNombreAsync(It.Is<string>(n => n != "Colombia"), It.IsAny<CancellationToken>())).ReturnsAsync((Guid?)null);
        return catalogos;
    }

    private static Stream LibroConFila(params string?[] valores)
    {
        using var workbook = new XLWorkbook();
        var hoja = workbook.Worksheets.Add("Clientes");
        string[] columnas = ["Nombre", "Sector", "Ciudad", "Dirección", "Web", "Contacto", "Cargo del contacto", "Email", "Valor de referencia", "Teléfono", "Notas", "País", "Estado"];
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
        var crear = CrearQueDevuelveElCliente();
        var importer = Importer(crear, Mock.Of<ICatalogosRepository>());

        using var archivo = LibroConFila("Acme S.A.", "Retail", "Bogotá", null, null, null, null, null, null, "3000000000", null);
        var resultado = await importer.ImportarAsync(archivo, Guid.NewGuid());

        Assert.Equal(1, resultado.Creados);
        Assert.Equal(0, resultado.Actualizados);
        Assert.Empty(resultado.Errores);
        crear.Verify(x => x.ExecuteAsync(It.Is<CreateClienteDto>(d => d.Nombre == "Acme S.A." && d.Telefonos.Count == 1 && d.Telefonos[0].Telefono == "3000000000"), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Importar_resuelve_el_pais_por_nombre_y_usa_el_estado_de_la_columna()
    {
        var crear = CrearQueDevuelveElCliente();
        var importer = Importer(crear, CatalogosConColombia().Object);

        using var archivo = LibroConFila("Acme S.A.", "Retail", "Bogotá", null, null, null, null, null, null, "3000000000", null, "Colombia", "Prospecto");
        var resultado = await importer.ImportarAsync(archivo, Guid.NewGuid());

        Assert.Equal(1, resultado.Creados);
        Assert.Empty(resultado.Errores);
        crear.Verify(x => x.ExecuteAsync(It.Is<CreateClienteDto>(d => d.PaisId == PaisId && d.Estado == "Prospecto"), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Importar_reporta_error_si_el_pais_no_existe_en_catalogos()
    {
        var crear = new Mock<ICrearClienteUseCase>();
        var importer = Importer(crear, CatalogosConColombia().Object);

        using var archivo = LibroConFila("Acme S.A.", null, null, null, null, null, null, null, null, "3000000000", null, "Narnia", null);
        var resultado = await importer.ImportarAsync(archivo, Guid.NewGuid());

        Assert.Equal(0, resultado.Creados);
        Assert.Single(resultado.Errores);
        Assert.Contains("Narnia", resultado.Errores[0].Mensaje);
        crear.Verify(x => x.ExecuteAsync(It.IsAny<CreateClienteDto>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Importar_reporta_fila_invalida_sin_detener_el_archivo_ni_crear_nada()
    {
        var crear = new Mock<ICrearClienteUseCase>();
        var importer = Importer(crear, Mock.Of<ICatalogosRepository>());

        // Sin Nombre -- requerido por CreateClienteValidator (el teléfono dejó de serlo, docs/32).
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
        var crear = new Mock<ICrearClienteUseCase>();
        crear.Setup(x => x.ExecuteAsync(It.IsAny<CreateClienteDto>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new BusinessRuleException("El email ya está registrado"));
        var importer = Importer(crear, Mock.Of<ICatalogosRepository>());

        using var archivo = LibroConFila("Acme S.A.", null, null, null, null, null, null, "dup@acme.com", null, "3000000000", null);
        var resultado = await importer.ImportarAsync(archivo, Guid.NewGuid());

        Assert.Equal(0, resultado.Creados);
        Assert.Single(resultado.Errores);
        Assert.Contains("email ya está registrado", resultado.Errores[0].Mensaje);
    }

    [Fact]
    public async Task Importar_ignora_una_fila_completamente_vacia()
    {
        var crear = CrearQueDevuelveElCliente();
        var importer = Importer(crear, Mock.Of<ICatalogosRepository>());

        using var workbook = new XLWorkbook();
        var hoja = workbook.Worksheets.Add("Clientes");
        hoja.Cell(1, 1).Value = "Nombre";
        // Fila 2 se deja completamente vacía a propósito.
        hoja.Cell(3, 1).Value = "Acme S.A.";
        hoja.Cell(3, 10).Value = "3000000000";
        var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;

        var resultado = await importer.ImportarAsync(stream, Guid.NewGuid());

        Assert.Equal(1, resultado.Creados);
        Assert.Empty(resultado.Errores);
    }

    [Fact]
    public void Exportar_escribe_el_encabezado_y_una_fila_por_cliente()
    {
        var importer = new ClientesImportExporter(
            Mock.Of<ICrearClienteUseCase>(), Mock.Of<IActualizarClienteUseCase>(),
            Mock.Of<IValidator<CreateClienteDto>>(), Mock.Of<IValidator<UpdateClienteDto>>(),
            Mock.Of<IClienteRepository>(), Mock.Of<ICatalogosRepository>());
        var clientes = new List<ClienteResponseDto>
        {
            new() { Nombre = "Acme S.A.", Emails = [new ClienteEmailDto { Email = "hola@acme.com" }], Telefonos = [new ClienteTelefonoDto { Telefono = "3000000000" }] },
        };

        var bytes = importer.Exportar(clientes);

        using var workbook = new XLWorkbook(new MemoryStream(bytes));
        var hoja = workbook.Worksheet(1);
        Assert.Equal("Nombre", hoja.Cell(1, 1).GetString());
        Assert.Equal("Acme S.A.", hoja.Cell(2, 1).GetString());
        Assert.Equal("hola@acme.com", hoja.Cell(2, 8).GetString());
        Assert.Equal("3000000000", hoja.Cell(2, 10).GetString());
    }

    // --- Reimportar (docs/35): si el cliente ya existe, la fila lo actualiza en vez de duplicarlo.

    private static readonly Guid ClienteExistenteId = Guid.NewGuid();

    /// <summary>Repositorio que ya tiene a "Acme S.A." guardado, con los datos que se le pasen.</summary>
    private static Mock<IClienteRepository> RepositorioCon(Cliente existente)
    {
        var repo = RepositorioSinDuplicados();
        repo.Setup(x => x.FindIdPorNombreAsync("Acme S.A.", It.IsAny<CancellationToken>())).ReturnsAsync(ClienteExistenteId);
        repo.Setup(x => x.GetByIdAsync(ClienteExistenteId, It.IsAny<CancellationToken>())).ReturnsAsync(existente);
        return repo;
    }

    private static Mock<IActualizarClienteUseCase> ActualizarQueDevuelveElCliente()
    {
        var actualizar = new Mock<IActualizarClienteUseCase>();
        actualizar.Setup(x => x.ExecuteAsync(It.IsAny<UpdateClienteDto>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((UpdateClienteDto dto, Guid _, CancellationToken _) => new ClienteResponseDto { Nombre = dto.Nombre });
        return actualizar;
    }

    [Fact]
    public async Task Reimportar_actualiza_el_cliente_existente_en_vez_de_duplicarlo()
    {
        var existente = new Cliente { Id = ClienteExistenteId, Nombre = "Acme S.A.", Sector = "Retail", Estado = "Activo", Notas = "Cliente de años" };
        var crear = new Mock<ICrearClienteUseCase>();
        var actualizar = ActualizarQueDevuelveElCliente();
        var importer = Importer(crear, Mock.Of<ICatalogosRepository>(), RepositorioCon(existente), actualizar);

        using var archivo = LibroConFila("Acme S.A.", "Servicios", null, null, null, null, null, null, null, null, null);
        var resultado = await importer.ImportarAsync(archivo, Guid.NewGuid());

        Assert.Equal(0, resultado.Creados);
        Assert.Equal(1, resultado.Actualizados);
        Assert.Empty(resultado.Errores);
        crear.Verify(x => x.ExecuteAsync(It.IsAny<CreateClienteDto>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        actualizar.Verify(x => x.ExecuteAsync(It.Is<UpdateClienteDto>(d => d.Id == ClienteExistenteId && d.Sector == "Servicios"), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Reimportar_con_una_celda_vacia_conserva_el_valor_que_ya_tenia()
    {
        // La regla que más importa de docs/35: dejar una celda en blanco NUNCA borra un dato.
        var existente = new Cliente { Id = ClienteExistenteId, Nombre = "Acme S.A.", Sector = "Retail", Estado = "Prospecto", Notas = "Cliente de años", Web = "acme.com" };
        var actualizar = ActualizarQueDevuelveElCliente();
        var importer = Importer(new Mock<ICrearClienteUseCase>(), Mock.Of<ICatalogosRepository>(), RepositorioCon(existente), actualizar);

        // Solo se llena el nombre: todo lo demás va vacío.
        using var archivo = LibroConFila("Acme S.A.");
        await importer.ImportarAsync(archivo, Guid.NewGuid());

        actualizar.Verify(x => x.ExecuteAsync(
            It.Is<UpdateClienteDto>(d => d.Sector == "Retail" && d.Notas == "Cliente de años" && d.Web == "acme.com" && d.Estado == "Prospecto"),
            It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Reimportar_no_duplica_el_telefono_ni_el_correo_que_ya_estaban()
    {
        var existente = new Cliente
        {
            Id = ClienteExistenteId,
            Nombre = "Acme S.A.",
            Telefonos = [new ClienteTelefono { Id = Guid.NewGuid(), Telefono = "3000000000" }],
            Emails = [new ClienteEmail { Id = Guid.NewGuid(), Email = "hola@acme.com" }],
        };
        var actualizar = ActualizarQueDevuelveElCliente();
        var importer = Importer(new Mock<ICrearClienteUseCase>(), Mock.Of<ICatalogosRepository>(), RepositorioCon(existente), actualizar);

        // El mismo correo y el mismo teléfono, escritos distinto (mayúsculas/espacios).
        using var archivo = LibroConFila("Acme S.A.", null, null, null, null, null, null, "HOLA@ACME.COM", null, " 3000000000 ", null);
        await importer.ImportarAsync(archivo, Guid.NewGuid());

        actualizar.Verify(x => x.ExecuteAsync(
            It.Is<UpdateClienteDto>(d => d.Telefonos.Count == 1 && d.Emails.Count == 1),
            It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Reimportar_agrega_un_telefono_nuevo_sin_borrar_el_anterior()
    {
        var existente = new Cliente
        {
            Id = ClienteExistenteId,
            Nombre = "Acme S.A.",
            Telefonos = [new ClienteTelefono { Id = Guid.NewGuid(), Telefono = "3000000000" }],
        };
        var actualizar = ActualizarQueDevuelveElCliente();
        var importer = Importer(new Mock<ICrearClienteUseCase>(), Mock.Of<ICatalogosRepository>(), RepositorioCon(existente), actualizar);

        using var archivo = LibroConFila("Acme S.A.", null, null, null, null, null, null, null, null, "3111111111", null);
        await importer.ImportarAsync(archivo, Guid.NewGuid());

        actualizar.Verify(x => x.ExecuteAsync(
            It.Is<UpdateClienteDto>(d => d.Telefonos.Count == 2
                && d.Telefonos.Any(t => t.Telefono == "3000000000")
                && d.Telefonos.Any(t => t.Telefono == "3111111111")),
            It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
