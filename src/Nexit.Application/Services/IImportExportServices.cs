using Nexit.Application.DTOs.Clientes;
using Nexit.Application.DTOs.Importacion;
using Nexit.Application.DTOs.Proveedores;
using Nexit.Application.DTOs.Proyectos;
using Nexit.Application.DTOs.Usuarios;

namespace Nexit.Application.Services;

/// <summary>
/// Exportar/importar clientes, proveedores y proyectos como Excel (docs/31, docs/35) -- mismo patrón
/// de capas que <see cref="IInformeExcelExporter"/>: la interfaz vive en Application (para que el
/// controlador dependa de una abstracción, no de ClosedXML directamente), la implementación concreta
/// vive en Infrastructure. A diferencia del exportador de informes, estas SÍ pueden depender de los
/// demás casos de uso de Application (crear/actualizar cada fila reutiliza exactamente el mismo
/// camino -- validación incluida -- que crear/editar un registro uno por uno desde el formulario, así
/// nunca hay dos formas distintas de decidir si un dato es válido).
///
/// Diseño de la importación (deliberado, ver docs/31): cada fila se procesa de forma independiente --
/// una fila inválida (falta un dato requerido, o un nombre de catálogo que no existe, p. ej. un país
/// mal escrito) no detiene ni revierte las demás, solo queda reportada con su número de fila y el
/// motivo exacto.
///
/// Reimportar (docs/35, antes esto NO existía -- toda fila creaba un registro nuevo a propósito):
/// Clientes y Proveedores hacen "upsert" por Nombre (sin distinguir mayúsculas/espacios) -- si ya
/// existe uno con ese nombre, la fila lo ACTUALIZA en vez de duplicarlo; si no existe, lo crea.
/// Proyectos hace upsert por la pareja (Cliente, Nombre) -- el nombre solo no alcanza como llave,
/// porque el mismo nombre de proyecto puede repetirse legítimamente para clientes distintos (o
/// incluso sin cliente). En todos los casos, un campo en blanco en el Excel NUNCA borra un dato que
/// el registro ya tenía (se conserva el valor existente); un campo con un valor SÍ lo reemplaza si es
/// distinto. Los campos que la importación nunca ha tocado (Teléfono/Email son colecciones, y
/// Equipo/Proveedores asociados/Gerente en Proyectos) se tratan igual en la actualización: nunca se
/// vacían por reimportar, solo se completan si el Excel trae algo nuevo que agregar.
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

/// <summary>
/// El equipo como Excel (2026-09-08, pedido de Alicia: "al lado de la campanita te falta importar y
/// exportar datos"). Es el único de los cuatro que NO es simétrico, y a propósito:
///
///  - <b>Exportar</b> da la foto del equipo -- quién tiene acceso, con qué rol y en qué estado.
///  - <b>Importar</b> NO crea usuarios: los <i>invita</i>. Un usuario de Nexit no puede existir sin
///    su cuenta en Supabase Auth, y esa cuenta la crea Supabase cuando la persona acepta el correo
///    de invitación (docs/25) -- no hay forma de fabricar una desde una fila de Excel como sí se
///    hace con un cliente. Así que el archivo que se sube es una lista de correos con su rol, y cada
///    fila dispara exactamente la misma invitación que el modal de "Invitar": misma validación de
///    dominio, mismos duplicados detectados, mismo correo real enviado por Supabase.
///
/// Por eso <c>ImportarResultadoDto.Creados</c> acá significa "invitaciones enviadas" y
/// <c>Actualizados</c> siempre queda en cero -- reinvitar a alguien no actualiza nada, o ya existe
/// (y queda reportado como error de esa fila) o no existía y se invita.
/// </summary>
public interface IUsuariosImportExporter
{
    byte[] Exportar(IReadOnlyList<UsuarioResponseDto> usuarios);

    /// <summary><paramref name="usuarioId"/> es quien invita -- queda registrado como <c>InvitadoPorId</c> de cada invitación creada.</summary>
    Task<ImportarResultadoDto> ImportarInvitacionesAsync(Stream archivo, Guid usuarioId, CancellationToken cancellationToken = default);
}
