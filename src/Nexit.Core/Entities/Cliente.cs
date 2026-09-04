namespace Nexit.Core.Entities;

public class Cliente : BaseEntity
{
    public string Nombre { get; set; } = string.Empty;
    public string? Sector { get; set; }
    /// <summary>
    /// País/departamento/ciudad "de catálogo" (mismas tablas que usa Proveedor) -- agregados
    /// 2026-09-03 para el formulario en cascada del mockup aprobado. Todos opcionales (a
    /// diferencia de Proveedor, donde PaisId es obligatorio) porque los clientes existentes,
    /// creados antes de este cambio, solo tienen la <see cref="Ciudad"/> de texto libre de abajo
    /// y no se puede inferir su país de forma confiable en una migración.
    /// </summary>
    public Guid? PaisId { get; set; }
    public Guid? RegionId { get; set; }
    public Guid? CiudadId { get; set; }
    /// <summary>"Activo" / "Prospecto" / "Inactivo" -- ver constraint ck_clientes_estado en NexitDbContext.</summary>
    public string Estado { get; set; } = "Activo";
    /// <summary>Ciudad como texto libre -- se conserva para los clientes creados antes de <see cref="CiudadId"/>;
    /// el frontend prioriza el nombre resuelto de CiudadId cuando está presente.</summary>
    public string? Ciudad { get; set; }
    public string? Direccion { get; set; }
    public string? Web { get; set; }
    public string? Contacto { get; set; }
    public string? CargoContacto { get; set; }
    public string? Email { get; set; }
    public string? ValorReferencia { get; set; }
    public string? Notas { get; set; }
    public ICollection<ClienteTelefono> Telefonos { get; set; } = new List<ClienteTelefono>();
    public ICollection<Proyecto> Proyectos { get; set; } = new List<Proyecto>();
}
