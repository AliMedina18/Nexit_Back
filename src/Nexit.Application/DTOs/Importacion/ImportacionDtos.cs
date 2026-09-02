namespace Nexit.Application.DTOs.Importacion;

/// <summary>Un error de una fila puntual del Excel importado -- nunca detiene el resto del archivo (docs/31).</summary>
public class ImportarErrorDto
{
    /// <summary>Número de fila del archivo Excel (1-based, contando el encabezado -- así coincide exactamente con lo que la usuaria ve al abrir el archivo).</summary>
    public int Fila { get; set; }
    public string Mensaje { get; set; } = string.Empty;
}

/// <summary>Resultado de importar un archivo .xlsx completo: cuántas filas sí se crearon y el detalle de las que no.</summary>
public class ImportarResultadoDto
{
    public int Creados { get; set; }
    public List<ImportarErrorDto> Errores { get; set; } = [];
}
