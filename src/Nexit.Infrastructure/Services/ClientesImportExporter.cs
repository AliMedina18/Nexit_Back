using ClosedXML.Excel;
using FluentValidation;
using Nexit.Application.DTOs.Clientes;
using Nexit.Application.DTOs.Importacion;
using Nexit.Application.Services;
using Nexit.Application.UseCases.Clientes;
using Nexit.Core.Exceptions;
using Nexit.Core.Interfaces;

namespace Nexit.Infrastructure.Services;

/// <summary>
/// Implementación con ClosedXML de <see cref="IClientesImportExporter"/> (docs/31) -- ver el
/// comentario de la interfaz para el diseño general (una fila por registro, siempre crea, nunca
/// actualiza, una fila inválida no detiene el archivo).
/// "País" y "Estado" se agregaron 2026-09-03 al FINAL de las columnas (no intercaladas) para no
/// romper una plantilla de Excel que alguien ya tenga con el layout anterior. "Ciudad" se mantiene
/// como texto libre (no se resuelve contra el catálogo, a diferencia de Proveedores) -- Cliente
/// guarda ambas cosas (ver comentario de CiudadId en la entidad); "País" si se resuelve, porque es
/// la referencia que de verdad necesita el formulario en cascada del mockup.
/// </summary>
public class ClientesImportExporter(ICrearClienteUseCase crear, IValidator<CreateClienteDto> validator, ICatalogosRepository catalogos) : IClientesImportExporter
{
    private static readonly string[] Columnas =
    [
        "Nombre", "Sector", "Ciudad", "Dirección", "Web", "Contacto", "Cargo del contacto",
        "Email", "Valor de referencia", "Teléfono", "Notas", "País", "Estado",
    ];

    public byte[] Exportar(IReadOnlyList<ClienteResponseDto> clientes)
    {
        using var workbook = new XLWorkbook();
        var hoja = workbook.Worksheets.Add("Clientes");
        for (var i = 0; i < Columnas.Length; i++) hoja.Cell(1, i + 1).Value = Columnas[i];
        hoja.Range(1, 1, 1, Columnas.Length).Style.Font.Bold = true;

        for (var i = 0; i < clientes.Count; i++)
        {
            var c = clientes[i];
            var fila = i + 2;
            hoja.Cell(fila, 1).Value = c.Nombre;
            hoja.Cell(fila, 2).Value = c.Sector ?? "";
            hoja.Cell(fila, 3).Value = c.Ciudad ?? "";
            hoja.Cell(fila, 4).Value = c.Direccion ?? "";
            hoja.Cell(fila, 5).Value = c.Web ?? "";
            hoja.Cell(fila, 6).Value = c.Contacto ?? "";
            hoja.Cell(fila, 7).Value = c.CargoContacto ?? "";
            hoja.Cell(fila, 8).Value = c.Email ?? "";
            hoja.Cell(fila, 9).Value = c.ValorReferencia ?? "";
            hoja.Cell(fila, 10).Value = c.Telefonos.Count > 0 ? c.Telefonos[0].Telefono : "";
            hoja.Cell(fila, 11).Value = c.Notas ?? "";
            // País se exporta como texto (no Id) -- ver comentario de ProveedoresImportExporter.
            hoja.Cell(fila, 13).Value = c.Estado;
        }
        for (var i = 1; i <= Columnas.Length; i++) hoja.Column(i).AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public async Task<ImportarResultadoDto> ImportarAsync(Stream archivo, Guid usuarioId, CancellationToken cancellationToken = default)
    {
        var resultado = new ImportarResultadoDto();
        using var workbook = new XLWorkbook(archivo);
        var hoja = workbook.Worksheet(1);
        var ultimaFila = hoja.LastRowUsed()?.RowNumber() ?? 1;

        for (var fila = 2; fila <= ultimaFila; fila++)
        {
            var celdas = hoja.Row(fila);
            if (celdas.IsEmpty()) continue;

            var nombrePais = TextoOpcional(celdas.Cell(12));
            Guid? paisId = null;
            if (nombrePais is not null)
            {
                paisId = await catalogos.FindPaisIdPorNombreAsync(nombrePais, cancellationToken);
                if (paisId is null)
                {
                    resultado.Errores.Add(new ImportarErrorDto { Fila = fila, Mensaje = $"El país \"{nombrePais}\" no existe en Catálogos -- créalo ahí primero, o corrige el nombre." });
                    continue;
                }
            }

            var telefono = Texto(celdas.Cell(10));
            var dto = new CreateClienteDto
            {
                Nombre = Texto(celdas.Cell(1)),
                Sector = TextoOpcional(celdas.Cell(2)),
                PaisId = paisId,
                Ciudad = TextoOpcional(celdas.Cell(3)),
                Direccion = TextoOpcional(celdas.Cell(4)),
                Web = TextoOpcional(celdas.Cell(5)),
                Contacto = TextoOpcional(celdas.Cell(6)),
                CargoContacto = TextoOpcional(celdas.Cell(7)),
                Email = TextoOpcional(celdas.Cell(8)),
                ValorReferencia = TextoOpcional(celdas.Cell(9)),
                Notas = TextoOpcional(celdas.Cell(11)),
                Estado = TextoOpcional(celdas.Cell(13)) ?? "Activo",
                Telefonos = string.IsNullOrWhiteSpace(telefono) ? [] : [new ClienteTelefonoDto { Telefono = telefono }],
            };

            var validacion = await validator.ValidateAsync(dto, cancellationToken);
            if (!validacion.IsValid)
            {
                resultado.Errores.Add(new ImportarErrorDto { Fila = fila, Mensaje = string.Join("; ", validacion.Errors.Select(e => e.ErrorMessage)) });
                continue;
            }

            try
            {
                await crear.ExecuteAsync(dto, usuarioId, cancellationToken);
                resultado.Creados++;
            }
            catch (BusinessRuleException ex)
            {
                resultado.Errores.Add(new ImportarErrorDto { Fila = fila, Mensaje = ex.Message });
            }
        }
        return resultado;
    }

    private static string Texto(IXLCell celda) => celda.GetString().Trim();
    private static string? TextoOpcional(IXLCell celda) { var texto = Texto(celda); return string.IsNullOrWhiteSpace(texto) ? null : texto; }
}
