using ClosedXML.Excel;
using FluentValidation;
using Moq;
using Nexit.Application.DTOs.Proveedores;
using Nexit.Application.UseCases.Proveedores;
using Nexit.Application.Validators.Proveedores;
using Nexit.Core.Entities;
using Nexit.Core.Interfaces;
using Nexit.Infrastructure.Services;

namespace Nexit.Tests;

/// <summary>
/// Importar/exportar proveedores desde Excel (docs/31) -- la diferencia con clientes es la
/// resolución de País/Ciudad/Categoría por NOMBRE contra Catálogos, así que estas pruebas se
/// concentran en eso: nombre que sí existe, que no existe, y ciudad sin país.
///
/// Desde docs/35 el importador también ACTUALIZA en vez de duplicar cuando ya existe un proveedor
/// con el mismo nombre -- esa rama tiene su prueba al final.
/// </summary>
public class ProveedoresImportExporterTests
{
    private static readonly Guid PaisId = Guid.NewGuid();
    private static readonly Guid CategoriaId = Guid.NewGuid();
    private static readonly Guid CiudadId = Guid.NewGuid();
    private static readonly Guid RegionId = Guid.NewGuid();

    private static Mock<ICatalogosRepository> CatalogosConColombiaYBogota()
    {
        var catalogos = new Mock<ICatalogosRepository>();
        catalogos.Setup(x => x.FindPaisIdPorNombreAsync("Colombia", It.IsAny<CancellationToken>())).ReturnsAsync(PaisId);
        catalogos.Setup(x => x.FindPaisIdPorNombreAsync(It.Is<string>(n => n != "Colombia"), It.IsAny<CancellationToken>())).ReturnsAsync((Guid?)null);
        catalogos.Setup(x => x.FindCategoriaIdPorNombreAsync("Producción audiovisual", It.IsAny<CancellationToken>())).ReturnsAsync(CategoriaId);
        catalogos.Setup(x => x.FindCategoriaIdPorNombreAsync(It.Is<string>(n => n != "Producción audiovisual"), It.IsAny<CancellationToken>())).ReturnsAsync((Guid?)null);
        catalogos.Setup(x => x.FindCiudadPorNombreAsync("Colombia", "Bogotá", It.IsAny<CancellationToken>())).ReturnsAsync((CiudadId, RegionId, PaisId));
        catalogos.Setup(x => x.FindCiudadPorNombreAsync(It.IsAny<string>(), It.Is<string>(c => c != "Bogotá"), It.IsAny<CancellationToken>())).ReturnsAsync(((Guid, Guid, Guid)?)null);
        return catalogos;
    }

    private static Mock<IProveedorRepository> RepositorioSinDuplicados()
    {
        var repo = new Mock<IProveedorRepository>();
        repo.Setup(x => x.ExistsByEmailAsync(It.IsAny<string>(), null, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        return repo;
    }

    /// <summary>
    /// Arma el importador con las seis piezas que pide desde docs/35. Por defecto el repositorio no
    /// encuentra ningún proveedor con el mismo nombre (`FindIdPorNombreAsync` devuelve null, el
    /// valor por defecto de Moq), así que se toma la rama de CREAR.
    /// </summary>
    private static ProveedoresImportExporter Importer(
        Mock<ICrearProveedorUseCase> crear,
        ICatalogosRepository catalogos,
        Mock<IProveedorRepository>? repositorio = null,
        Mock<IActualizarProveedorUseCase>? actualizar = null)
    {
        var repo = repositorio ?? RepositorioSinDuplicados();
        return new ProveedoresImportExporter(
            crear.Object,
            (actualizar ?? new Mock<IActualizarProveedorUseCase>()).Object,
            new CreateProveedorValidator(repo.Object),
            new UpdateProveedorValidator(repo.Object),
            repo.Object,
            catalogos);
    }

    private static Stream LibroConFila(params string?[] valores)
    {
        using var workbook = new XLWorkbook();
        var hoja = workbook.Worksheets.Add("Proveedores");
        string[] columnas = ["Nombre", "País", "Ciudad", "Categoría", "Estado", "Contacto", "Cargo del contacto", "Email", "Web", "Dirección", "Aforo", "Costo de referencia", "Score (1-5)", "Presupuesto", "Cobertura", "Teléfono", "Notas"];
        for (var i = 0; i < columnas.Length; i++) hoja.Cell(1, i + 1).Value = columnas[i];
        for (var i = 0; i < valores.Length; i++) if (valores[i] is not null) hoja.Cell(2, i + 1).Value = valores[i];
        var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;
        return stream;
    }

    [Fact]
    public async Task Importar_crea_una_fila_valida_resolviendo_pais_ciudad_y_categoria_por_nombre()
    {
        var crear = new Mock<ICrearProveedorUseCase>();
        crear.Setup(x => x.ExecuteAsync(It.IsAny<CreateProveedorDto>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CreateProveedorDto dto, Guid _, CancellationToken _) => new ProveedorResponseDto { Nombre = dto.Nombre });
        var importer = Importer(crear, CatalogosConColombiaYBogota().Object);

        using var archivo = LibroConFila("Estudio X", "Colombia", "Bogotá", "Producción audiovisual", "Activo", null, null, null, null, null, null, null, null, null, null, null, null);
        var resultado = await importer.ImportarAsync(archivo, Guid.NewGuid());

        Assert.Equal(1, resultado.Creados);
        Assert.Empty(resultado.Errores);
        crear.Verify(x => x.ExecuteAsync(It.Is<CreateProveedorDto>(d => d.Nombre == "Estudio X" && d.PaisId == PaisId && d.CiudadId == CiudadId && d.RegionId == RegionId && d.CategoriaId == CategoriaId), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Importar_reporta_error_cuando_el_pais_no_existe_en_catalogos()
    {
        var crear = new Mock<ICrearProveedorUseCase>();
        var importer = Importer(crear, CatalogosConColombiaYBogota().Object);

        using var archivo = LibroConFila("Estudio X", "Narnia", null, "Producción audiovisual", null, null, null, null, null, null, null, null, null, null, null, null, null);
        var resultado = await importer.ImportarAsync(archivo, Guid.NewGuid());

        Assert.Equal(0, resultado.Creados);
        Assert.Single(resultado.Errores);
        Assert.Contains("Narnia", resultado.Errores[0].Mensaje);
        crear.Verify(x => x.ExecuteAsync(It.IsAny<CreateProveedorDto>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Importar_reporta_error_cuando_la_ciudad_no_existe_dentro_del_pais_dado()
    {
        var crear = new Mock<ICrearProveedorUseCase>();
        var importer = Importer(crear, CatalogosConColombiaYBogota().Object);

        using var archivo = LibroConFila("Estudio X", "Colombia", "Atlantis", "Producción audiovisual", null, null, null, null, null, null, null, null, null, null, null, null, null);
        var resultado = await importer.ImportarAsync(archivo, Guid.NewGuid());

        Assert.Equal(0, resultado.Creados);
        Assert.Single(resultado.Errores);
        Assert.Contains("Atlantis", resultado.Errores[0].Mensaje);
    }

    [Fact]
    public async Task Importar_reporta_error_cuando_hay_ciudad_pero_falta_el_pais_en_esa_fila()
    {
        var crear = new Mock<ICrearProveedorUseCase>();
        var importer = Importer(crear, CatalogosConColombiaYBogota().Object);

        using var archivo = LibroConFila("Estudio X", null, "Bogotá", "Producción audiovisual", null, null, null, null, null, null, null, null, null, null, null, null, null);
        var resultado = await importer.ImportarAsync(archivo, Guid.NewGuid());

        Assert.Equal(0, resultado.Creados);
        Assert.Single(resultado.Errores);
        crear.Verify(x => x.ExecuteAsync(It.IsAny<CreateProveedorDto>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public void Exportar_escribe_el_encabezado_y_una_fila_por_proveedor()
    {
        var importer = new ProveedoresImportExporter(
            Mock.Of<ICrearProveedorUseCase>(), Mock.Of<IActualizarProveedorUseCase>(),
            Mock.Of<IValidator<CreateProveedorDto>>(), Mock.Of<IValidator<UpdateProveedorDto>>(),
            Mock.Of<IProveedorRepository>(), Mock.Of<ICatalogosRepository>());
        var proveedores = new List<ProveedorResponseDto> { new() { Nombre = "Estudio X", Estado = "Activo", Score = 4 } };

        var bytes = importer.Exportar(proveedores);

        using var workbook = new XLWorkbook(new MemoryStream(bytes));
        var hoja = workbook.Worksheet(1);
        Assert.Equal("Nombre", hoja.Cell(1, 1).GetString());
        Assert.Equal("Estudio X", hoja.Cell(2, 1).GetString());
        Assert.Equal("Activo", hoja.Cell(2, 5).GetString());
        Assert.Equal(4, hoja.Cell(2, 13).GetValue<int>());
    }

    // --- Reimportar (docs/35): si el proveedor ya existe, la fila lo actualiza en vez de duplicarlo.

    private static readonly Guid ProveedorExistenteId = Guid.NewGuid();

    [Fact]
    public async Task Reimportar_actualiza_al_proveedor_existente_y_conserva_lo_que_el_Excel_no_trae()
    {
        // Lo que más importa de docs/35 para proveedores: los servicios asociados no vienen en el
        // Excel y no se pueden vaciar por reimportar, y una celda en blanco conserva el valor previo.
        var servicioId = Guid.NewGuid();
        var existente = new Proveedor
        {
            Id = ProveedorExistenteId,
            Nombre = "Estudio X",
            Estado = "Activo",
            Notas = "Proveedor de confianza",
            Score = 5,
            Servicios = [new ProveedorServicio { ServicioId = servicioId }],
        };
        var repo = RepositorioSinDuplicados();
        repo.Setup(x => x.FindIdPorNombreAsync("Estudio X", It.IsAny<CancellationToken>())).ReturnsAsync(ProveedorExistenteId);
        repo.Setup(x => x.GetByIdAsync(ProveedorExistenteId, It.IsAny<CancellationToken>())).ReturnsAsync(existente);

        var crear = new Mock<ICrearProveedorUseCase>();
        var actualizar = new Mock<IActualizarProveedorUseCase>();
        actualizar.Setup(x => x.ExecuteAsync(It.IsAny<UpdateProveedorDto>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((UpdateProveedorDto dto, Guid _, CancellationToken _) => new ProveedorResponseDto { Nombre = dto.Nombre });
        var importer = Importer(crear, CatalogosConColombiaYBogota().Object, repo, actualizar);

        // Solo cambia el contacto; Notas y Score van en blanco, y los servicios no tienen columna.
        using var archivo = LibroConFila("Estudio X", "Colombia", null, "Producción audiovisual", null, "Ana Ruiz");
        var resultado = await importer.ImportarAsync(archivo, Guid.NewGuid());

        Assert.Equal(0, resultado.Creados);
        Assert.Equal(1, resultado.Actualizados);
        Assert.Empty(resultado.Errores);
        crear.Verify(x => x.ExecuteAsync(It.IsAny<CreateProveedorDto>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        actualizar.Verify(x => x.ExecuteAsync(
            It.Is<UpdateProveedorDto>(d => d.Id == ProveedorExistenteId
                && d.Contacto == "Ana Ruiz"
                && d.Notas == "Proveedor de confianza"
                && d.Score == 5
                && d.ServicioIds.Count == 1 && d.ServicioIds[0] == servicioId),
            It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
