using ClosedXML.Excel;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using Nexit.Application.DTOs.Invitaciones;
using Nexit.Application.DTOs.Usuarios;
using Nexit.Application.UseCases.Invitaciones;
using Nexit.Core.Constants;
using Nexit.Core.Exceptions;
using Nexit.Infrastructure.Services;

namespace Nexit.Tests;

/// <summary>
/// Exportar el equipo e "importar" usuarios (docs/36). Lo particular de este importador, y lo que
/// más importa que quede fijado en una prueba: importar usuarios NO crea usuarios, los INVITA -- un
/// usuario de Nexit no puede existir sin su cuenta de Supabase Auth, así que no hay forma de
/// fabricar uno desde una fila de Excel como sí se hace con un cliente.
/// </summary>
public class UsuariosImportExporterTests
{
    private static readonly Guid QuienInvita = Guid.NewGuid();

    private static Mock<IValidator<CrearInvitacionDto>> ValidadorQueRechaza(params string[] emailsInvalidos)
    {
        var validador = new Mock<IValidator<CrearInvitacionDto>>();
        validador.Setup(x => x.ValidateAsync(It.IsAny<CrearInvitacionDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CrearInvitacionDto dto, CancellationToken _) => emailsInvalidos.Contains(dto.Email)
                ? new ValidationResult([new ValidationFailure(nameof(dto.Email), "El correo no pertenece a un dominio laboral permitido.")])
                : new ValidationResult());
        return validador;
    }

    private static Mock<ICrearInvitacionUseCase> InvitacionQueFunciona()
    {
        var crear = new Mock<ICrearInvitacionUseCase>();
        crear.Setup(x => x.ExecuteAsync(It.IsAny<CrearInvitacionDto>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CrearInvitacionDto dto, Guid _, CancellationToken _) =>
                new InvitacionResponseDto { Email = dto.Email, Rol = dto.Rol, Estado = EstadosInvitacion.Pendiente });
        return crear;
    }

    /// <summary>Columnas 1 y 2 del archivo -- Correo y Rol; el resto solo se exporta.</summary>
    private static Stream LibroCon(params (string Correo, string? Rol)[] filas)
    {
        using var workbook = new XLWorkbook();
        var hoja = workbook.Worksheets.Add("Usuarios");
        hoja.Cell(1, 1).Value = "Correo";
        hoja.Cell(1, 2).Value = "Rol";
        for (var i = 0; i < filas.Length; i++)
        {
            hoja.Cell(i + 2, 1).Value = filas[i].Correo;
            if (filas[i].Rol is not null) hoja.Cell(i + 2, 2).Value = filas[i].Rol;
        }
        var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;
        return stream;
    }

    [Fact]
    public async Task Importar_invita_a_cada_correo_del_archivo_con_su_rol()
    {
        var crear = InvitacionQueFunciona();
        var importer = new UsuariosImportExporter(crear.Object, ValidadorQueRechaza().Object);

        using var archivo = LibroCon(("una@agencianextmkt.com", "admin"), ("otra@agencianextmkt.com", "manager"));
        var resultado = await importer.ImportarInvitacionesAsync(archivo, QuienInvita);

        Assert.Equal(2, resultado.Creados);
        Assert.Equal(0, resultado.Actualizados);
        Assert.Empty(resultado.Errores);
        crear.Verify(x => x.ExecuteAsync(It.Is<CrearInvitacionDto>(d => d.Email == "una@agencianextmkt.com" && d.Rol == "admin"), QuienInvita, It.IsAny<CancellationToken>()), Times.Once);
        crear.Verify(x => x.ExecuteAsync(It.Is<CrearInvitacionDto>(d => d.Email == "otra@agencianextmkt.com" && d.Rol == "manager"), QuienInvita, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Importar_sin_rol_en_la_fila_invita_como_miembro()
    {
        var crear = InvitacionQueFunciona();
        var importer = new UsuariosImportExporter(crear.Object, ValidadorQueRechaza().Object);

        using var archivo = LibroCon(("una@agencianextmkt.com", null));
        var resultado = await importer.ImportarInvitacionesAsync(archivo, QuienInvita);

        Assert.Equal(1, resultado.Creados);
        crear.Verify(x => x.ExecuteAsync(It.Is<CrearInvitacionDto>(d => d.Rol == Roles.Miembro), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Importar_una_fila_invalida_no_detiene_el_archivo()
    {
        var crear = InvitacionQueFunciona();
        var importer = new UsuariosImportExporter(crear.Object, ValidadorQueRechaza("ajeno@otro-dominio.com").Object);

        using var archivo = LibroCon(("una@agencianextmkt.com", null), ("ajeno@otro-dominio.com", null), ("otra@agencianextmkt.com", null));
        var resultado = await importer.ImportarInvitacionesAsync(archivo, QuienInvita);

        Assert.Equal(2, resultado.Creados);
        var error = Assert.Single(resultado.Errores);
        Assert.Equal(3, error.Fila);
        Assert.Contains("dominio laboral", error.Mensaje);
    }

    [Fact]
    public async Task Importar_el_mismo_correo_dos_veces_en_el_archivo_lo_invita_una_sola_vez()
    {
        var crear = InvitacionQueFunciona();
        var importer = new UsuariosImportExporter(crear.Object, ValidadorQueRechaza().Object);

        using var archivo = LibroCon(("una@agencianextmkt.com", null), ("UNA@agencianextmkt.com", null));
        var resultado = await importer.ImportarInvitacionesAsync(archivo, QuienInvita);

        Assert.Equal(1, resultado.Creados);
        Assert.Single(resultado.Errores);
        crear.Verify(x => x.ExecuteAsync(It.IsAny<CrearInvitacionDto>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Importar_reporta_la_fila_sin_correo_sin_llamar_a_Supabase()
    {
        var crear = InvitacionQueFunciona();
        var importer = new UsuariosImportExporter(crear.Object, ValidadorQueRechaza().Object);

        using var archivo = LibroCon(("", "admin"));
        var resultado = await importer.ImportarInvitacionesAsync(archivo, QuienInvita);

        Assert.Equal(0, resultado.Creados);
        Assert.Single(resultado.Errores);
        Assert.Contains("Falta el correo", resultado.Errores[0].Mensaje);
        crear.Verify(x => x.ExecuteAsync(It.IsAny<CrearInvitacionDto>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Importar_reporta_como_error_la_falla_de_Supabase_de_una_fila()
    {
        var crear = InvitacionQueFunciona();
        crear.Setup(x => x.ExecuteAsync(It.Is<CrearInvitacionDto>(d => d.Email == "yatiene@agencianextmkt.com"), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new BusinessRuleException("Ese correo ya tiene una cuenta en Supabase Auth."));
        var importer = new UsuariosImportExporter(crear.Object, ValidadorQueRechaza().Object);

        using var archivo = LibroCon(("una@agencianextmkt.com", null), ("yatiene@agencianextmkt.com", null));
        var resultado = await importer.ImportarInvitacionesAsync(archivo, QuienInvita);

        Assert.Equal(1, resultado.Creados);
        Assert.Contains("ya tiene una cuenta", Assert.Single(resultado.Errores).Mensaje);
    }

    [Fact]
    public void Exportar_escribe_el_equipo_con_el_estado_de_cada_cuenta()
    {
        var importer = new UsuariosImportExporter(Mock.Of<ICrearInvitacionUseCase>(), Mock.Of<IValidator<CrearInvitacionDto>>());
        var desactivadaEl = new DateTime(2026, 8, 1, 0, 0, 0, DateTimeKind.Utc);
        var usuarios = new List<UsuarioResponseDto>
        {
            new() { Nombre = "Alicia", Apellido = "Medina", Email = "alicia@agencianextmkt.com", Rol = "super_admin", Activo = true, CreatedAt = new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc) },
            new() { Nombre = "Ex", Apellido = "Compañero", Email = "ex@agencianextmkt.com", Rol = "miembro", Activo = false, FechaDesactivacion = desactivadaEl, CreatedAt = new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc) },
        };

        var bytes = importer.Exportar(usuarios);

        using var workbook = new XLWorkbook(new MemoryStream(bytes));
        var hoja = workbook.Worksheet(1);
        // El correo va primero a propósito: el archivo exportado sirve de plantilla para invitar,
        // y las dos columnas que lee la importación son justamente las dos primeras.
        Assert.Equal("Correo", hoja.Cell(1, 1).GetString());
        Assert.Equal("Rol", hoja.Cell(1, 2).GetString());
        Assert.Equal("alicia@agencianextmkt.com", hoja.Cell(2, 1).GetString());
        Assert.Equal("super_admin", hoja.Cell(2, 2).GetString());
        Assert.Equal("Activa", hoja.Cell(2, 6).GetString());
        Assert.Equal("", hoja.Cell(2, 7).GetString());
        Assert.Equal("Desactivada", hoja.Cell(3, 6).GetString());
        Assert.Equal("2026-08-01", hoja.Cell(3, 7).GetString());
    }
}
