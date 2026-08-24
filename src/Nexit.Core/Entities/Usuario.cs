namespace Nexit.Core.Entities;

public class Usuario : BaseEntity
{
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Rol { get; set; } = "miembro";
    public string? Iniciales { get; set; }
    public bool Activo { get; set; } = true;

    /// <summary>
    /// Momento en que Activo pasó de true a false. Null mientras está activa. Se usa para calcular
    /// cuándo se cumplen los 30 días de inactividad que disparan la eliminación automática (ver
    /// EliminarUsuariosInactivosUseCase y docs/17-eliminacion-automatica-usuarios.md). Se limpia
    /// (vuelve a null) si la cuenta se reactiva antes de cumplir el plazo.
    /// </summary>
    public DateTime? FechaDesactivacion { get; set; }

    public ICollection<Cliente> ClientesCreados { get; set; } = new List<Cliente>();
    public ICollection<Proveedor> ProveedoresCreados { get; set; } = new List<Proveedor>();
    public ICollection<Proyecto> ProyectosCreados { get; set; } = new List<Proyecto>();
    public ICollection<ProyectoSeguimiento> SeguimientosEscritos { get; set; } = new List<ProyectoSeguimiento>();
}
