namespace Nexit.Core.Entities;

public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public Guid? CreatedBy { get; set; }

    /// <summary>
    /// Quién hizo la última edición (distinto de CreatedBy). Se ignora en las entidades
    /// que no tienen un caso de uso de "actualizar" propio (ver NexitDbContext.OnModelCreating).
    /// </summary>
    public Guid? UpdatedBy { get; set; }
}
