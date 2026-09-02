using ClosedXML.Excel;
using FluentValidation;
using Nexit.Application.DTOs.Importacion;
using Nexit.Application.DTOs.Proyectos;
using Nexit.Application.Services;
using Nexit.Application.UseCases.Proyectos;
using Nexit.Core.Exceptions;
using Nexit.Core.Interfaces;

namespace Nexit.Infrastructure.Services;

/// <summary>
/// Implementación con ClosedXML de <see cref="IProyectosImportExporter"/> (docs/31) -- ver el
/// comentario de <see cref="IClientesImportExporter"/> para el diseño general. A propósito, la
/// importación de proyectos NO incluye equipo, proveedores asociados ni gerente explícito -- son
/// relaciones, no datos planos de una fila, y forzarlas a columnas de Excel (nombres separados por
/// comas, con el riesgo de no encontrar a la persona exacta) complicaría el archivo sin necesidad;
/// esas tres cosas se completan después, proyecto por proyecto, desde la pantalla de edición --
/// exactamente igual que si el proyecto se hubiera creado a mano sin llenarlas todavía.
/// </summary>
public class ProyectosImportExporter(ICrearProyectoUseCase crear, IValidator<CrearProyectoDto> validator, IClienteRepository clientes, ICatalogosRepository catalogos) : IProyectosImportExporter
{
    private static readonly string[] Columnas =
    [
        "Nombre", "Cliente", "Contacto del proyecto", "Tipo de proyecto", "Prioridad", "Ciudad",
        "Sede Next", "Fecha de solicitud", "Fecha del evento", "Estado", "% de avance",
        "Estado del brief", "Estado de la propuesta", "N.º de factura", "Pagado (Sí/No)",
        "Fecha de pago", "Notas",
    ];

    /// <summary>
    /// El Excel exportado trae el NOMBRE del cliente y del estado (no su Id) -- es lo único que le
    /// sirve a quien abre el archivo, y es exactamente lo que <see cref="ImportarAsync"/> vuelve a
    /// resolver al importar, así que un archivo exportado siempre se puede reimportar tal cual.
    /// </summary>
    public async Task<byte[]> ExportarAsync(IReadOnlyList<ProyectoResponseDto> proyectos, CancellationToken cancellationToken = default)
    {
        var nombresClientes = new Dictionary<Guid, string>();
        foreach (var clienteId in proyectos.Where(p => p.ClienteId.HasValue).Select(p => p.ClienteId!.Value).Distinct())
        {
            var cliente = await clientes.GetByIdAsync(clienteId, cancellationToken);
            if (cliente is not null) nombresClientes[clienteId] = cliente.Nombre;
        }
        var estados = await catalogos.GetEstadosAsync(null, cancellationToken);
        var nombresEstados = estados.ToDictionary(e => e.Id, e => e.Nombre);

        using var workbook = new XLWorkbook();
        var hoja = workbook.Worksheets.Add("Proyectos");
        for (var i = 0; i < Columnas.Length; i++) hoja.Cell(1, i + 1).Value = Columnas[i];
        hoja.Range(1, 1, 1, Columnas.Length).Style.Font.Bold = true;

        for (var i = 0; i < proyectos.Count; i++)
        {
            var p = proyectos[i];
            var fila = i + 2;
            hoja.Cell(fila, 1).Value = p.Nombre;
            hoja.Cell(fila, 2).Value = p.ClienteId.HasValue && nombresClientes.TryGetValue(p.ClienteId.Value, out var nombreCliente) ? nombreCliente : "";
            hoja.Cell(fila, 3).Value = p.ContactoProyecto ?? "";
            hoja.Cell(fila, 4).Value = p.TipoProyecto ?? "";
            hoja.Cell(fila, 5).Value = p.Prioridad ?? "";
            hoja.Cell(fila, 6).Value = p.Ciudad ?? "";
            hoja.Cell(fila, 7).Value = p.SedeNext ?? "";
            if (p.FechaSolicitud.HasValue) hoja.Cell(fila, 8).Value = p.FechaSolicitud.Value;
            if (p.FechaEvento.HasValue) hoja.Cell(fila, 9).Value = p.FechaEvento.Value;
            hoja.Cell(fila, 10).Value = nombresEstados.TryGetValue(p.EstadoId, out var nombreEstado) ? nombreEstado : "";
            hoja.Cell(fila, 11).Value = p.PorcentajeAvance;
            hoja.Cell(fila, 12).Value = p.EstadoBrief;
            hoja.Cell(fila, 13).Value = p.PropuestaEstado;
            hoja.Cell(fila, 14).Value = p.NumeroFactura ?? "";
            hoja.Cell(fila, 15).Value = p.Pagado ? "Sí" : "No";
            if (p.FechaPago.HasValue) hoja.Cell(fila, 16).Value = p.FechaPago.Value;
            hoja.Cell(fila, 17).Value = p.Notas ?? "";
        }
        for (var i = 1; i <= Columnas.Length; i++) hoja.Column(i).AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public async Task<ImportarResultadoDto> ImportarAsync(Stream archivo, Guid usuarioId, string? usuarioRol, CancellationToken cancellationToken = default)
    {
        var resultado = new ImportarResultadoDto();
        using var workbook = new XLWorkbook(archivo);
        var hoja = workbook.Worksheet(1);
        var ultimaFila = hoja.LastRowUsed()?.RowNumber() ?? 1;

        for (var fila = 2; fila <= ultimaFila; fila++)
        {
            var celdas = hoja.Row(fila);
            if (celdas.IsEmpty()) continue;

            var nombreCliente = TextoOpcional(celdas.Cell(2));
            Guid? clienteId = null;
            if (nombreCliente is not null)
            {
                clienteId = await clientes.FindIdPorNombreAsync(nombreCliente, cancellationToken);
                if (clienteId is null)
                {
                    resultado.Errores.Add(new ImportarErrorDto { Fila = fila, Mensaje = $"El cliente \"{nombreCliente}\" no existe -- créalo primero (en Clientes, o en una fila anterior de este mismo archivo si también lo estás importando)." });
                    continue;
                }
            }

            var nombreEstado = Texto(celdas.Cell(10));
            var estadoId = string.IsNullOrWhiteSpace(nombreEstado) ? null : await catalogos.FindEstadoIdPorNombreAsync(nombreEstado, cancellationToken);
            if (estadoId is null)
            {
                resultado.Errores.Add(new ImportarErrorDto { Fila = fila, Mensaje = string.IsNullOrWhiteSpace(nombreEstado) ? "El estado del proyecto es requerido." : $"El estado \"{nombreEstado}\" no existe en Catálogos -- corrige el nombre." });
                continue;
            }

            var pagado = EsSiONo(celdas.Cell(15));
            var dto = new CrearProyectoDto
            {
                Nombre = Texto(celdas.Cell(1)),
                ClienteId = clienteId,
                ContactoProyecto = TextoOpcional(celdas.Cell(3)),
                TipoProyecto = TextoOpcional(celdas.Cell(4)),
                Prioridad = TextoOpcional(celdas.Cell(5)),
                Ciudad = TextoOpcional(celdas.Cell(6)),
                SedeNext = TextoOpcional(celdas.Cell(7)),
                FechaSolicitud = FechaOpcional(celdas.Cell(8)),
                FechaEvento = FechaOpcional(celdas.Cell(9)),
                EstadoId = estadoId.Value,
                PorcentajeAvance = NumeroEnteroOpcional(celdas.Cell(11)) ?? 0,
                EstadoBrief = TextoOpcional(celdas.Cell(12)) ?? "Pendiente por enviar",
                PropuestaEstado = TextoOpcional(celdas.Cell(13)) ?? "No enviada",
                NumeroFactura = TextoOpcional(celdas.Cell(14)),
                Pagado = pagado,
                FechaPago = pagado ? (FechaOpcional(celdas.Cell(16)) ?? DateTime.UtcNow) : null,
                Notas = TextoOpcional(celdas.Cell(17)),
            };

            var validacion = await validator.ValidateAsync(dto, cancellationToken);
            if (!validacion.IsValid)
            {
                resultado.Errores.Add(new ImportarErrorDto { Fila = fila, Mensaje = string.Join("; ", validacion.Errors.Select(e => e.ErrorMessage)) });
                continue;
            }

            try
            {
                await crear.ExecuteAsync(dto, usuarioId, usuarioRol, cancellationToken);
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
    private static DateTime? FechaOpcional(IXLCell celda) => celda.TryGetValue(out DateTime fecha) ? fecha : null;
    private static bool EsSiONo(IXLCell celda)
    {
        var texto = Texto(celda).ToLowerInvariant();
        return texto is "si" or "sí" or "yes" or "true" or "1";
    }
}
