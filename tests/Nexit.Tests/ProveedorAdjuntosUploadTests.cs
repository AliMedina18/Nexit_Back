using System.Text;
using Moq;
using Nexit.Application.UseCases.Proveedores;
using Nexit.Core.Entities;
using Nexit.Core.Exceptions;
using Nexit.Core.Interfaces;

namespace Nexit.Tests;

/// <summary>
/// Subir un archivo real de adjunto a Supabase Storage (docs/28) -- solo PDF/Excel, máximo 20 MB,
/// y que eliminar un adjunto de tipo "file" también borre el archivo real, no solo la fila.
/// </summary>
public class ProveedorAdjuntosUploadTests
{
    private static ProveedorAdjuntosUseCase CrearUseCase(Mock<IProveedorRepository> proveedores, Mock<IProveedorAdjuntoRepository> adjuntos, Mock<ISupabaseStorageService> storage, Mock<IUnitOfWork>? unitOfWork = null) =>
        new(proveedores.Object, adjuntos.Object, storage.Object, (unitOfWork ?? new Mock<IUnitOfWork>()).Object);

    private static Mock<IProveedorRepository> ProveedorExistente(Guid proveedorId)
    {
        var proveedores = new Mock<IProveedorRepository>();
        proveedores.Setup(x => x.GetByIdAsync(proveedorId, It.IsAny<CancellationToken>())).ReturnsAsync(new Proveedor { Id = proveedorId });
        return proveedores;
    }

    [Fact]
    public async Task Subir_rejects_a_disallowed_file_type()
    {
        var proveedorId = Guid.NewGuid();
        var proveedores = ProveedorExistente(proveedorId);
        var adjuntos = new Mock<IProveedorAdjuntoRepository>();
        var storage = new Mock<ISupabaseStorageService>();
        using var contenido = new MemoryStream(Encoding.UTF8.GetBytes("no importa"));

        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            CrearUseCase(proveedores, adjuntos, storage).SubirAsync(proveedorId, "virus.exe", "application/octet-stream", 10, contenido, CancellationToken.None));

        Assert.Contains("PDF", ex.Message);
        storage.Verify(x => x.SubirAsync(It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Subir_rejects_a_file_over_the_size_limit()
    {
        var proveedorId = Guid.NewGuid();
        var proveedores = ProveedorExistente(proveedorId);
        var adjuntos = new Mock<IProveedorAdjuntoRepository>();
        var storage = new Mock<ISupabaseStorageService>();
        using var contenido = new MemoryStream(new byte[1]);

        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            CrearUseCase(proveedores, adjuntos, storage).SubirAsync(proveedorId, "contrato.pdf", "application/pdf", 21 * 1024 * 1024, contenido, CancellationToken.None));

        storage.Verify(x => x.SubirAsync(It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Subir_uploads_a_valid_pdf_and_saves_the_row_with_tipo_file()
    {
        var proveedorId = Guid.NewGuid();
        var proveedores = ProveedorExistente(proveedorId);
        var adjuntos = new Mock<IProveedorAdjuntoRepository>();
        var storage = new Mock<ISupabaseStorageService>();
        storage.Setup(x => x.SubirAsync(It.IsAny<string>(), It.IsAny<Stream>(), "application/pdf", It.IsAny<CancellationToken>()))
            .ReturnsAsync((string ruta, Stream _, string _, CancellationToken _) => ruta);
        using var contenido = new MemoryStream(Encoding.UTF8.GetBytes("contenido del pdf"));

        var resultado = await CrearUseCase(proveedores, adjuntos, storage).SubirAsync(proveedorId, "Contrato Final.pdf", "application/pdf", contenido.Length, contenido, CancellationToken.None);

        Assert.Equal("file", resultado.Tipo);
        Assert.Equal("Contrato Final.pdf", resultado.Nombre);
        Assert.Equal("application/pdf", resultado.ContentType);
        Assert.NotNull(resultado.StoragePath);
        Assert.StartsWith($"proveedores/{proveedorId}/", resultado.StoragePath);
        Assert.EndsWith(".pdf", resultado.StoragePath);
        adjuntos.Verify(x => x.AddAsync(It.Is<ProveedorAdjunto>(a => a.Tipo == "file" && a.ProveedorId == proveedorId), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Subir_accepts_xlsx_and_normalizes_the_content_type_from_the_extension()
    {
        // El content-type que manda el cliente se ignora a propósito -- se usa el que corresponde a
        // la extensión, para no confiar en lo que el navegador haya mandado.
        var proveedorId = Guid.NewGuid();
        var proveedores = ProveedorExistente(proveedorId);
        var adjuntos = new Mock<IProveedorAdjuntoRepository>();
        var storage = new Mock<ISupabaseStorageService>();
        storage.Setup(x => x.SubirAsync(It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string ruta, Stream _, string _, CancellationToken _) => ruta);
        using var contenido = new MemoryStream(new byte[] { 1, 2, 3 });

        var resultado = await CrearUseCase(proveedores, adjuntos, storage).SubirAsync(proveedorId, "presupuesto.xlsx", "application/octet-stream", 3, contenido, CancellationToken.None);

        Assert.Equal("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", resultado.ContentType);
        storage.Verify(x => x.SubirAsync(It.IsAny<string>(), It.IsAny<Stream>(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Eliminar_also_deletes_the_real_file_from_storage_when_tipo_is_file()
    {
        var proveedorId = Guid.NewGuid();
        var adjuntoId = Guid.NewGuid();
        var proveedores = ProveedorExistente(proveedorId);
        var adjuntos = new Mock<IProveedorAdjuntoRepository>();
        var storage = new Mock<ISupabaseStorageService>();
        adjuntos.Setup(x => x.GetByIdAsync(adjuntoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProveedorAdjunto { Id = adjuntoId, ProveedorId = proveedorId, Tipo = "file", Nombre = "x.pdf", StoragePath = "proveedores/x/y.pdf" });

        await CrearUseCase(proveedores, adjuntos, storage).EliminarAsync(proveedorId, adjuntoId, CancellationToken.None);

        adjuntos.Verify(x => x.DeleteAsync(adjuntoId, It.IsAny<CancellationToken>()), Times.Once);
        storage.Verify(x => x.EliminarAsync("proveedores/x/y.pdf", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Eliminar_does_not_touch_storage_when_tipo_is_link()
    {
        var proveedorId = Guid.NewGuid();
        var adjuntoId = Guid.NewGuid();
        var proveedores = ProveedorExistente(proveedorId);
        var adjuntos = new Mock<IProveedorAdjuntoRepository>();
        var storage = new Mock<ISupabaseStorageService>();
        adjuntos.Setup(x => x.GetByIdAsync(adjuntoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProveedorAdjunto { Id = adjuntoId, ProveedorId = proveedorId, Tipo = "link", Nombre = "x", Url = "https://example.com" });

        await CrearUseCase(proveedores, adjuntos, storage).EliminarAsync(proveedorId, adjuntoId, CancellationToken.None);

        storage.Verify(x => x.EliminarAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ObtenerUrlDescarga_returns_the_url_as_is_for_a_link()
    {
        var proveedorId = Guid.NewGuid();
        var adjuntoId = Guid.NewGuid();
        var proveedores = ProveedorExistente(proveedorId);
        var adjuntos = new Mock<IProveedorAdjuntoRepository>();
        var storage = new Mock<ISupabaseStorageService>();
        adjuntos.Setup(x => x.GetByIdAsync(adjuntoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProveedorAdjunto { Id = adjuntoId, ProveedorId = proveedorId, Tipo = "link", Nombre = "x", Url = "https://example.com/doc" });

        var url = await CrearUseCase(proveedores, adjuntos, storage).ObtenerUrlDescargaAsync(proveedorId, adjuntoId, CancellationToken.None);

        Assert.Equal("https://example.com/doc", url);
        storage.Verify(x => x.ObtenerUrlFirmadaAsync(It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ObtenerUrlDescarga_asks_storage_for_a_signed_url_for_a_file()
    {
        var proveedorId = Guid.NewGuid();
        var adjuntoId = Guid.NewGuid();
        var proveedores = ProveedorExistente(proveedorId);
        var adjuntos = new Mock<IProveedorAdjuntoRepository>();
        var storage = new Mock<ISupabaseStorageService>();
        adjuntos.Setup(x => x.GetByIdAsync(adjuntoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProveedorAdjunto { Id = adjuntoId, ProveedorId = proveedorId, Tipo = "file", Nombre = "x.pdf", StoragePath = "proveedores/x/y.pdf" });
        storage.Setup(x => x.ObtenerUrlFirmadaAsync("proveedores/x/y.pdf", It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>())).ReturnsAsync("https://firmada.example.com/y.pdf");

        var url = await CrearUseCase(proveedores, adjuntos, storage).ObtenerUrlDescargaAsync(proveedorId, adjuntoId, CancellationToken.None);

        Assert.Equal("https://firmada.example.com/y.pdf", url);
    }
}
