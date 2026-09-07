using Nexit.Core.Entities;

namespace Nexit.Core.Interfaces;

/// <summary>
/// Un mes del calendario de proyectos, con cuántos proyectos tienen fecha_evento en ese mes de un
/// año dado. No trae los proyectos completos a propósito — la vista de calendario (año completo,
/// enero a diciembre) solo necesita el conteo por mes para pintar la grilla; los datos completos de
/// un mes se piden aparte (<see cref="ProyectoCalendarioItem"/>) solo cuando alguien entra a ese mes.
/// </summary>
public record ConteoMesProyectos(int Mes, int Cantidad);

/// <summary>
/// Proyección liviana de un proyecto para la vista de calendario (al entrar a un mes específico) —
/// a propósito NO es la entidad Proyecto completa (sin equipo/proveedores/seguimiento): evita cargar
/// esos datos relacionados cuando solo se necesita la lista para pintar el calendario de ese mes.
/// </summary>
// FechaEventoLocal ("yyyy-MM-dd", vacio hasta que se calcula) se llena DESPUES de traer los
// candidatos a memoria en ProyectoRepository.ObtenerPorMesAsync (con with) -- no se puede pedir
// en el select de la consulta SQL porque SedeTimeZoneResolver no es traducible a SQL (ver el
// comentario en ese metodo). Consumida por CalendarioController/{anio}/{mes} para que el frontend
// agrupe cada proyecto en el DIA local de su sede, no en el dia UTC (docs/18).
public record ProyectoCalendarioItem(Guid Id, string Nombre, DateTime FechaEvento, Guid? ClienteId, string? ClienteNombre, string EstadoNombre, string? Prioridad, string? Ciudad, string? SedeNext, string FechaEventoLocal = "");

public interface IProyectoRepository : IRepository<Proyecto>
{
    /// <summary>Años (de fecha_evento) que tienen al menos un proyecto — para poblar el selector de año del calendario sin adivinar un rango fijo.</summary>
    Task<IReadOnlyList<int>> ObtenerAniosConProyectosAsync(CancellationToken cancellationToken = default);

    /// <summary>Conteo de proyectos por mes (1-12) dentro de un año — un solo GROUP BY en la base de datos, no carga proyectos completos.</summary>
    Task<IReadOnlyList<ConteoMesProyectos>> ObtenerConteoPorMesAsync(int anio, CancellationToken cancellationToken = default);

    /// <summary>Proyectos (proyección liviana) cuya fecha_evento cae en un mes/año específico, para cuando alguien entra a ver ese mes del calendario.</summary>
    Task<IReadOnlyList<ProyectoCalendarioItem>> ObtenerPorMesAsync(int anio, int mes, CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca un proyecto por su cliente (puede ser null -- "sin cliente" es válido) y su nombre exacto
    /// (sin distinguir mayúsculas) -- para la importación masiva (docs/31, docs/35). El nombre solo NO
    /// alcanza como llave: el mismo nombre de proyecto puede repetirse legítimamente para clientes
    /// distintos (o para varios proyectos "sin cliente"), así que la pareja (Cliente, Nombre) es la
    /// llave que evita fusionar por error dos proyectos que no tienen nada que ver.
    /// </summary>
    Task<Guid?> FindIdPorClienteYNombreAsync(Guid? clienteId, string nombre, CancellationToken cancellationToken = default);
}
