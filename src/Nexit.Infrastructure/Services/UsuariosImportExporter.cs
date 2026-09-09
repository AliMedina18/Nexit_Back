using ClosedXML.Excel;
using FluentValidation;
using Nexit.Application.DTOs.Importacion;
using Nexit.Application.DTOs.Invitaciones;
using Nexit.Application.DTOs.Usuarios;
using Nexit.Application.Services;
using Nexit.Application.UseCases.Invitaciones;
using Nexit.Core.Exceptions;

namespace Nexit.Infrastructure.Services;

/// <summary>
/// Ver <see cref="IUsuariosImportExporter"/> para el porqué de la asimetría (exportar da el equipo;
/// importar invita, no crea). Cada fila del archivo se procesa por separado, igual que en los otros
/// tres importadores: un correo de un dominio ajeno o ya invitado no detiene el resto del archivo,
/// queda reportado con su número de fila y el motivo exacto.
/// </summary>
public class UsuariosImportExporter(
    ICrearInvitacionUseCase crearInvitacion,
    IValidator<CrearInvitacionDto> invitacionValidator) : IUsuariosImportExporter
{
    /// <summary>
    /// Las dos primeras columnas son las que lee <see cref="ImportarInvitacionesAsync"/>; el resto
    /// solo se exporta. Así el archivo exportado sirve de plantilla para invitar sin tener que
    /// borrarle columnas -- se llenan dos y las demás se ignoran.
    /// </summary>
    private static readonly string[] Columnas =
    [
        "Correo", "Rol", "Nombre", "Apellido", "Iniciales", "Estado de la cuenta",
        "Desactivada desde", "Fecha de alta",
    ];

    public byte[] Exportar(IReadOnlyList<UsuarioResponseDto> usuarios)
    {
        using var workbook = new XLWorkbook();
        var hoja = workbook.Worksheets.Add("Usuarios");
        for (var i = 0; i < Columnas.Length; i++) hoja.Cell(1, i + 1).Value = Columnas[i];
        hoja.Range(1, 1, 1, Columnas.Length).Style.Font.Bold = true;

        for (var i = 0; i < usuarios.Count; i++)
        {
            var u = usuarios[i];
            var fila = i + 2;
            hoja.Cell(fila, 1).Value = u.Email;
            hoja.Cell(fila, 2).Value = u.Rol;
            hoja.Cell(fila, 3).Value = u.Nombre;
            hoja.Cell(fila, 4).Value = u.Apellido;
            hoja.Cell(fila, 5).Value = u.Iniciales ?? "";
            hoja.Cell(fila, 6).Value = u.Activo ? "Activa" : "Desactivada";
            // Solo tiene sentido en las desactivadas -- es la fecha desde la que corren los 30 días
            // de la eliminación automática (docs/17).
            if (u.FechaDesactivacion is not null) hoja.Cell(fila, 7).Value = u.FechaDesactivacion.Value.ToString("yyyy-MM-dd");
            hoja.Cell(fila, 8).Value = u.CreatedAt.ToString("yyyy-MM-dd");
        }
        for (var i = 1; i <= Columnas.Length; i++) hoja.Column(i).AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public async Task<ImportarResultadoDto> ImportarInvitacionesAsync(Stream archivo, Guid usuarioId, CancellationToken cancellationToken = default)
    {
        var resultado = new ImportarResultadoDto();
        using var workbook = new XLWorkbook(archivo);
        var hoja = workbook.Worksheet(1);
        var ultimaFila = hoja.LastRowUsed()?.RowNumber() ?? 1;

        // Un mismo archivo puede traer el mismo correo dos veces (copiado de otra hoja). La primera
        // se invita; la segunda ya no pasaría el validador de "invitación pendiente" igual, pero se
        // corta antes para no gastar una llamada a Supabase ni dar un mensaje confuso.
        var vistos = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        for (var fila = 2; fila <= ultimaFila; fila++)
        {
            var celdas = hoja.Row(fila);
            if (celdas.IsEmpty()) continue;

            var email = celdas.Cell(1).GetString().Trim();
            if (string.IsNullOrWhiteSpace(email))
            {
                resultado.Errores.Add(new ImportarErrorDto { Fila = fila, Mensaje = "Falta el correo en esta fila." });
                continue;
            }
            if (!vistos.Add(email))
            {
                resultado.Errores.Add(new ImportarErrorDto { Fila = fila, Mensaje = $"\"{email}\" ya venía antes en este mismo archivo -- se invitó una sola vez." });
                continue;
            }

            var rol = celdas.Cell(2).GetString().Trim();
            var dto = new CrearInvitacionDto
            {
                Email = email,
                // Sin rol en la fila, se invita como "miembro" -- el mismo valor por defecto del
                // modal de invitar, y el menos privilegiado de los cuatro.
                Rol = string.IsNullOrWhiteSpace(rol) ? "miembro" : rol,
            };

            var validacion = await invitacionValidator.ValidateAsync(dto, cancellationToken);
            if (!validacion.IsValid)
            {
                resultado.Errores.Add(new ImportarErrorDto { Fila = fila, Mensaje = string.Join("; ", validacion.Errors.Select(e => e.ErrorMessage)) });
                continue;
            }

            try
            {
                await crearInvitacion.ExecuteAsync(dto, usuarioId, cancellationToken);
                resultado.Creados++;
            }
            catch (BusinessRuleException ex)
            {
                resultado.Errores.Add(new ImportarErrorDto { Fila = fila, Mensaje = ex.Message });
            }
        }
        return resultado;
    }
}
