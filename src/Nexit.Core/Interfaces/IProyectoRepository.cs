using Nexit.Core.Entities;

namespace Nexit.Core.Interfaces;

// El calendario de proyectos se eliminó del sistema el 2026-09-09 (ver docs/41): con él se fueron
// los records ConteoMesProyectos/ProyectoCalendarioItem y los tres métodos de agregación por
// año/mes que solo esa vista usaba.

public interface IProyectoRepository : IRepository<Proyecto>
{
    /// <summary>
    /// Busca un proyecto por su cliente (puede ser null -- "sin cliente" es válido) y su nombre exacto
    /// (sin distinguir mayúsculas) -- para la importación masiva (docs/31, docs/35). El nombre solo NO
    /// alcanza como llave: el mismo nombre de proyecto puede repetirse legítimamente para clientes
    /// distintos (o para varios proyectos "sin cliente"), así que la pareja (Cliente, Nombre) es la
    /// llave que evita fusionar por error dos proyectos que no tienen nada que ver.
    /// </summary>
    Task<Guid?> FindIdPorClienteYNombreAsync(Guid? clienteId, string nombre, CancellationToken cancellationToken = default);
}
