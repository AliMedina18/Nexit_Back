namespace Nexit.Core.Utils;

/// <summary>
/// Nombres de <see cref="Entities.EstadoProyecto"/> que se consideran "terminales" -- el proyecto ya
/// se cerró, así que no tiene sentido incluirlo al decidir qué atender primero. Se usa tanto en el
/// endpoint de prioridad de proyectos (docs/22) como en el de clientes (docs/24, para contar
/// "proyectos activos" de cada cliente) -- centralizado acá para no repetir la lista en dos sitios.
/// </summary>
public static class EstadosProyectoTerminales
{
    public static readonly HashSet<string> Nombres = new(StringComparer.OrdinalIgnoreCase)
    {
        "Finalizado", "Cancelado", "No ejecutado", "Facturado"
    };
}
