using Nexit.Application.DTOs.Clientes;
using Nexit.Application.DTOs.Importacion;
using Nexit.Application.DTOs.Proveedores;
using Nexit.Application.DTOs.Proyectos;

namespace Nexit.Application.Services;

/// <summary>
/// Exportar/importar clientes, proveedores y proyectos como Excel (docs/31) -- mismo patrón de capas
/// que <see cref="IInformeExcelExporter"/>: la interfaz vive en Application (para que el controlador
/// dependa de una abstracción, no de ClosedXML directamente), la implementación concreta vive en
/// Infrastructure. A diferencia del exportador de informes, estas SÍ pueden depender de los demás
/// casos de uso de Application (crear cada fila reutiliza exactamente el mismo camino -- validación
/// incluida -- que crear un registro uno por uno desde el formulario, así nunca hay dos formas
/// distintas de decidir si un dato es válido).
///
/// Diseño de la importación (deliberado, ver docs/31): cada fila se procesa de forma independiente --
/// una fila inválida (falta un dato requerido, o un nombre de catálogo que no existe, p. ej. un país
/// mal escrito) no detiene ni revierte las demás, solo queda reportada con su número de fila y el
/// motivo exacto. La importación SIEMPRE crea filas nuevas, nunca actualiza una fila existente (no
/// hay una llave natural confiable para decidir "esto ya existe" -- ni el nombre ni el email son
/// obligatorios/únicos en todas las entidades), así que re-importar el mismo archivo dos veces crea
/// duplicados a propósito, no los evita en silencio.
/// </summary>
public interface IClientesImportExporter
{
    byte[] Exportar(IReadOnlyList<ClienteResponseDto> clientes);
    Task<ImportarResultadoDto> ImportarAsync(Stream archivo, Guid usuarioId, CancellationToken cancellationToken = default);
}

public interface IProveedoresImportExporter
{
    byte[] Exportar(IReadOnlyList<ProveedorResponseDto> proveedores);
    Task<ImportarResultadoDto> ImportarAsync(Stream archivo, Guid usuarioId, CancellationToken cancellationToken = default);
}

public interface IProyectosImportExporter
{
    /// <summary>
    /// A diferencia de <see cref="IClientesImportExporter.Exportar"/>/<see cref="IProveedoresImportExporter.Exportar"/>
    /// (síncronos, solo formatean datos ya cargados), esta es async: <c>ProyectoResponseDto</c> solo
    /// trae Id de cliente/estado, así que la implementación necesita resolverlos a nombre contra
    /// Clientes/Catálogos antes de poder escribir las filas.
    /// </summary>
    Task<byte[]> ExportarAsync(IReadOnlyList<ProyectoResponseDto> proyectos, CancellationToken cancellationToken = default);
    /// <summary>
    /// <paramref name="usuarioRol"/> sigue el mismo propósito que en <c>ICrearProyectoUseCase</c>: si
    /// quien importa es gerente, cada proyecto creado sin gerente explícito en el Excel queda asignado
    /// a quien importó (mismo comportamiento que crear un proyecto uno por uno desde el formulario).
    /// </summary>
    Task<ImportarResultadoDto> ImportarAsync(Stream archivo, Guid usuarioId, string? usuarioRol, CancellationToken cancellationToken = default);
}
