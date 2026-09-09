namespace Nexit.Core.Constants;

/// <summary>
/// Qué se puede pedir eliminar a través de una <c>SolicitudEliminacion</c>. Está espejado en el
/// CHECK <c>ck_solicitudes_eliminacion_tipo</c> de la base (ver NexitDbContext y
/// docs/schema/25_gestion_usuarios_al_dia.sql): agregar un valor acá sin ampliar ese CHECK hace que
/// la inserción falle en tiempo de ejecución, no de compilación.
///
/// <c>usuario</c> se agregó el 2026-09-08 (docs/40): eliminar a una persona dejó de ser una acción
/// directa del super_admin y pasa por el mismo circuito de solicitud + decisión que el resto.
/// </summary>
public static class TiposEntidadEliminable
{
    public const string Cliente = "cliente";
    public const string Proveedor = "proveedor";
    public const string Proyecto = "proyecto";
    public const string Usuario = "usuario";

    public static readonly string[] Todos = [Cliente, Proveedor, Proyecto, Usuario];
}
