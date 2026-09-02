using ClosedXML.Excel;
using FluentValidation;
using Nexit.Application.DTOs.Importacion;
using Nexit.Application.DTOs.Proveedores;
using Nexit.Application.Services;
using Nexit.Application.UseCases.Proveedores;
using Nexit.Core.Exceptions;
using Nexit.Core.Interfaces;

namespace Nexit.Infrastructure.Services;

/// <summary>
/// Implementación con ClosedXML de <see cref="IProveedoresImportExporter"/> (docs/31) -- ver el
/// comentario de <see cref="IClientesImportExporter"/> para el diseño general. La diferencia con
/// clientes: País y Categoría son obligatorios y se guardan como referencia (Guid) a un catálogo, no
/// como texto libre -- el Excel trae el NOMBRE (lo que la usuaria realmente tiene a mano), y esta
/// clase lo resuelve al Id correspondiente antes de crear el proveedor. Si el nombre no coincide con
/// ningún país/categoría/ciudad existente (por ejemplo, un error de tipeo), la fila queda marcada
/// como error con el nombre exacto que no se encontró -- no se crea el catálogo solo, ni se adivina
/// el más parecido, para no terminar con "Colombia" y "colombiaa" como dos países distintos.
/// </summary>
public class ProveedoresImportExporter(ICrearProveedorUseCase crear, IValidator<CreateProveedorDto> validator, ICatalogosRepository catalogos) : IProveedoresImportExporter
{
    private static readonly string[] Columnas =
    [
        "Nombre", "País", "Ciudad", "Categoría", "Estado", "Contacto", "Cargo del contacto", "Email",
        "Web", "Dirección", "Aforo", "Costo de referencia", "Score (1-5)", "Presupuesto", "Cobertura",
        "Teléfono", "Notas",
    ];

    public byte[] Exportar(IReadOnlyList<ProveedorResponseDto> proveedores)
    {
        using var workbook = new XLWorkbook();
        var hoja = workbook.Worksheets.Add("Proveedores");
        for (var i = 0; i < Columnas.Length; i++) hoja.Cell(1, i + 1).Value = Columnas[i];
        hoja.Range(1, 1, 1, Columnas.Length).Style.Font.Bold = true;

        for (var i = 0; i < proveedores.Count; i++)
        {
            var p = proveedores[i];
            var fila = i + 2;
            hoja.Cell(fila, 1).Value = p.Nombre;
            // País/Ciudad/Categoría se exportan como texto (no Id) a propósito -- es lo que se puede
            // volver a importar; el Id no le sirve de nada a quien abre el Excel.
            hoja.Cell(fila, 5).Value = p.Estado;
            hoja.Cell(fila, 6).Value = p.Contacto ?? "";
            hoja.Cell(fila, 7).Value = p.CargoContacto ?? "";
            hoja.Cell(fila, 8).Value = p.Email ?? "";
            hoja.Cell(fila, 9).Value = p.Web ?? "";
            hoja.Cell(fila, 10).Value = p.Direccion ?? "";
            if (p.Aforo.HasValue) hoja.Cell(fila, 11).Value = p.Aforo.Value;
            hoja.Cell(fila, 12).Value = p.CostoReferencia ?? "";
            if (p.Score.HasValue) hoja.Cell(fila, 13).Value = p.Score.Value;
            hoja.Cell(fila, 14).Value = p.Presupuesto ?? "";
            hoja.Cell(fila, 15).Value = p.Cobertura ?? "";
            hoja.Cell(fila, 16).Value = p.Telefonos.Count > 0 ? p.Telefonos[0].Telefono : "";
            hoja.Cell(fila, 17).Value = p.Notas ?? "";
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

            var nombrePais = Texto(celdas.Cell(2));
            var nombreCiudad = TextoOpcional(celdas.Cell(3));
            var nombreCategoria = Texto(celdas.Cell(4));

            var paisId = string.IsNullOrWhiteSpace(nombrePais) ? null : await catalogos.FindPaisIdPorNombreAsync(nombrePais, cancellationToken);
            var categoriaId = string.IsNullOrWhiteSpace(nombreCategoria) ? null : await catalogos.FindCategoriaIdPorNombreAsync(nombreCategoria, cancellationToken);
            if (!string.IsNullOrWhiteSpace(nombrePais) && paisId is null)
            {
                resultado.Errores.Add(new ImportarErrorDto { Fila = fila, Mensaje = $"El país \"{nombrePais}\" no existe en Catálogos -- créalo ahí primero, o corrige el nombre." });
                continue;
            }
            if (!string.IsNullOrWhiteSpace(nombreCategoria) && categoriaId is null)
            {
                resultado.Errores.Add(new ImportarErrorDto { Fila = fila, Mensaje = $"La categoría \"{nombreCategoria}\" no existe en Catálogos -- créala ahí primero, o corrige el nombre." });
                continue;
            }

            Guid? ciudadId = null, regionId = null;
            if (!string.IsNullOrWhiteSpace(nombreCiudad))
            {
                if (string.IsNullOrWhiteSpace(nombrePais))
                {
                    resultado.Errores.Add(new ImportarErrorDto { Fila = fila, Mensaje = "La ciudad requiere también el país en esa misma fila." });
                    continue;
                }
                var ciudad = await catalogos.FindCiudadPorNombreAsync(nombrePais, nombreCiudad, cancellationToken);
                if (ciudad is null)
                {
                    resultado.Errores.Add(new ImportarErrorDto { Fila = fila, Mensaje = $"La ciudad \"{nombreCiudad}\" no existe en Catálogos dentro de \"{nombrePais}\" -- créala ahí primero, o corrige el nombre." });
                    continue;
                }
                ciudadId = ciudad.Value.CiudadId;
                regionId = ciudad.Value.RegionId;
            }

            var telefono = Texto(celdas.Cell(16));
            var dto = new CreateProveedorDto
            {
                Nombre = Texto(celdas.Cell(1)),
                PaisId = paisId ?? Guid.Empty,
                RegionId = regionId,
                CiudadId = ciudadId,
                CategoriaId = categoriaId ?? Guid.Empty,
                Estado = TextoOpcional(celdas.Cell(5)) ?? "Activo",
                Contacto = TextoOpcional(celdas.Cell(6)),
                CargoContacto = TextoOpcional(celdas.Cell(7)),
                Email = TextoOpcional(celdas.Cell(8)),
                Web = TextoOpcional(celdas.Cell(9)),
                Direccion = TextoOpcional(celdas.Cell(10)),
                Aforo = NumeroEnteroOpcional(celdas.Cell(11)),
                CostoReferencia = TextoOpcional(celdas.Cell(12)),
                Score = NumeroEnteroOpcional(celdas.Cell(13)),
                Presupuesto = TextoOpcional(celdas.Cell(14)),
                Cobertura = TextoOpcional(celdas.Cell(15)),
                Notas = TextoOpcional(celdas.Cell(17)),
                Telefonos = string.IsNullOrWhiteSpace(telefono) ? [] : [new ProveedorTelefonoDto { Telefono = telefono }],
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
    private static int? NumeroEnteroOpcional(IXLCell celda) => celda.TryGetValue(out int numero) ? numero : null;
}
