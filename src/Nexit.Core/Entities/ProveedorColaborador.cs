namespace Nexit.Core.Entities;

/// <summary>
/// "Estoy trabajando con este proveedor" (docs/19, sección 3) -- cada persona se marca a sí misma,
/// nadie más lo hace por ella (a diferencia de <see cref="Proyecto.GerenteId"/>, que sí asigna un
/// administrador). Varias personas pueden estar marcadas en el mismo proveedor a la vez -- así se
/// muestran como los "circulitos" en la lista general, y alimentan la sección personal "mis
/// proveedores" de cada quien.
/// </summary>
public class ProveedorColaborador
{
    public Guid ProveedorId { get; set; }
    public Guid UsuarioId { get; set; }
    public DateTime FechaAgregado { get; set; } = DateTime.UtcNow;

    public Proveedor Proveedor { get; set; } = null!;
    public Usuario Usuario { get; set; } = null!;
}
