namespace Nexit.Core.Utils;

/// <summary>
/// Resuelve en qué zona horaria cae un proyecto según su sede (<c>Proyecto.SedeNext</c>), para que
/// el calendario (docs/07, docs/18) ubique cada proyecto en el mes correcto según la hora LOCAL de
/// esa sede, no según UTC ni la zona horaria por defecto del servidor.
///
/// Por qué existe: <c>fecha_evento</c> se guarda como <c>timestamptz</c> (siempre convertido a UTC
/// por Postgres al guardar). Un proyecto registrado, por ejemplo, el 31 de enero a las 11:30pm hora
/// de Bogotá queda guardado como 1 de febrero ~04:30 UTC -- si el calendario calculara el mes
/// directamente sobre ese valor UTC (o sobre la zona horaria de sesión del servidor, que no es un
/// dato de negocio), ese proyecto aparecería en febrero en vez de enero. Ver docs/18 para el
/// hallazgo completo y la investigación que lo respalda.
///
/// <c>SedeNext</c> es texto libre (sin catálogo, ver <c>docs/11</c> sección 3) -- por eso esta
/// resolución es por coincidencia de texto, no por un ID de catálogo. Si el texto no coincide con
/// ninguna sede conocida (vacío, null, o cualquier otra ciudad), se asume Bogotá -- es la sede
/// principal de Next y el comportamiento que ya tenía el sistema antes de que existiera más de una
/// sede.
/// </summary>
public static class SedeTimeZoneResolver
{
    public const string ZonaBogota = "America/Bogota";
    public const string ZonaMexico = "America/Mexico_City";

    /// <summary>
    /// Zonas horarias reconocidas hoy. Si Next abre una sede en un país nuevo, agregar aquí el
    /// patrón de texto que identifica esa sede y su zona horaria IANA -- es el único lugar que hay
    /// que tocar.
    /// </summary>
    public static string ResolverZonaHoraria(string? sedeNext)
    {
        if (!string.IsNullOrWhiteSpace(sedeNext))
        {
            var texto = sedeNext.Trim();
            if (Contiene(texto, "méxico") || Contiene(texto, "mexico") || Contiene(texto, "cdmx"))
                return ZonaMexico;
        }

        return ZonaBogota;
    }

    /// <summary>Devuelve el <see cref="TimeZoneInfo"/> ya resuelto para una sede -- evita repetir el lookup en cada llamado.</summary>
    public static TimeZoneInfo ResolverTimeZoneInfo(string? sedeNext) =>
        TimeZoneInfo.FindSystemTimeZoneById(ResolverZonaHoraria(sedeNext));

    /// <summary>
    /// Convierte un instante UTC (como llega <c>fecha_evento</c> desde Postgres) a la hora local de
    /// la sede del proyecto -- de aquí se lee el año/mes real que debe usar el calendario (docs/18).
    /// </summary>
    public static DateTime ConvertirUtcALocal(DateTime fechaUtc, string? sedeNext)
    {
        var utc = fechaUtc.Kind == DateTimeKind.Utc ? fechaUtc : DateTime.SpecifyKind(fechaUtc, DateTimeKind.Utc);
        return TimeZoneInfo.ConvertTimeFromUtc(utc, ResolverTimeZoneInfo(sedeNext));
    }

    private static bool Contiene(string texto, string buscado) =>
        texto.Contains(buscado, StringComparison.OrdinalIgnoreCase);
}
