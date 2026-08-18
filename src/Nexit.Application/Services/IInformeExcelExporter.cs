using Nexit.Application.DTOs.Informes;

namespace Nexit.Application.Services;

/// <summary>
/// Genera el archivo .xlsx de un informe (resumen en vivo o snapshot semanal/mensual) -- ver
/// docs/07-calendario-e-informes-excel.md. La interfaz vive en Application (para que el controlador
/// dependa de una abstracción, no de ClosedXML directamente); la implementación concreta con
/// ClosedXML vive en Infrastructure, igual que los repositorios.
/// </summary>
public interface IInformeExcelExporter
{
    /// <param name="titulo">Encabezado del reporte (ej. "Informe general — 2026-08-18" o "Informe mensual — 2026-08").</param>
    /// <param name="datos">El resumen a exportar -- acepta tanto InformeResumenDto (en vivo) como InformeSnapshotDto (snapshot guardado), ya que este último hereda del primero.</param>
    byte[] Exportar(string titulo, InformeResumenDto datos);
}
