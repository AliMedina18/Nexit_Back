namespace Nexit.Core.Entities;

/// <summary>
/// Registro de auditoría/historial de cambios (docs/19) -- una fila por cada campo que cambió en
/// cada edición de un proyecto, proveedor o cliente, tipo Google Docs/Excel ("quién cambió qué,
/// cuándo"). Nunca se sobreescribe ni se borra -- a diferencia de <c>Usuario.UpdatedBy</c>, que solo
/// guarda la ÚLTIMA edición, esto guarda todas.
/// </summary>
public class HistorialCambio
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string TipoEntidad { get; set; } = string.Empty; // "proyecto" | "proveedor" | "cliente"
    public Guid EntidadId { get; set; }
    public Guid UsuarioId { get; set; }
    public string Accion { get; set; } = string.Empty; // "creacion" | "edicion" | "eliminacion"
    public string? Campo { get; set; } // null cuando Accion es "creacion" o "eliminacion" (aplica a todo el registro, no a un campo)
    public string? ValorAnterior { get; set; }
    public string? ValorNuevo { get; set; }
    public DateTime Fecha { get; set; } = DateTime.UtcNow;

    public Usuario Usuario { get; set; } = null!;
}
