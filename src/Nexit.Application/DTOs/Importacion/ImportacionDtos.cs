namespace Nexit.Application.DTOs.Importacion;

/// <summary>Un error de una fila puntual del Excel importado -- nunca detiene el resto del archivo (docs/31).</summary>
public class ImportarErrorDto
{
    /// <summary>Número de fila del archivo Excel (1-based, contando el encabezado -- así coincide exactamente con lo que la usuaria ve al abrir el archivo).</summary>
    public int Fila { get; set; }
    public string Mensaje { get; set; } = string.Empty;
}

/// <summary>
/// Resultado de importar un archivo .xlsx completo (docs/31, docs/35): cuántas filas se crearon,
/// cuántas ya existían y se actualizaron en su lugar (reimportar el mismo archivo no duplica), y el
/// detalle de las que no se pudieron procesar.
/// </summary>
public class ImportarResultadoDto
{
    public int Creados { get; set; }
    /// <summary>Filas que ya existían (mismo nombre -- o mismo cliente+nombre en Proyectos) y se actualizaron con los datos de esta fila en vez de crear un duplicado.</summary>
    public int Actualizados { get; set; }
    public List<ImportarErrorDto> Errores { get; set; } = [];
}
