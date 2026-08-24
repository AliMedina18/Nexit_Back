using System.Reflection;

namespace Nexit.Core.Utils;

public record CambioDetectado(string Campo, string? ValorAnterior, string? ValorNuevo);

/// <summary>
/// Compara los valores de las propiedades "simples" (no colecciones ni referencias a otras
/// entidades) de un objeto antes y después de una edición, para alimentar el historial de cambios
/// (docs/19 -- "que se sepa quién cambió qué, tipo Google Docs/Excel"). Funciona por reflexión sobre
/// cualquier tipo, así no hace falta escribir el diff a mano para cada entidad (Proyecto, Proveedor,
/// Cliente...) ni acordarse de actualizarlo cada vez que se agrega un campo nuevo.
///
/// Uso: tomar un <see cref="Snapshot{T}"/> ANTES de aplicar los cambios a la entidad rastreada por EF
/// Core (una vez que se aplican, el valor "de antes" ya se perdió en memoria), y comparar ese
/// snapshot contra la entidad ya modificada con <see cref="Comparar{T}"/>.
/// </summary>
public static class CambioDetector
{
    /// <summary>Nombres de propiedades que nunca se registran como "cambio de negocio" -- son metadata de auditoría, no dato que alguien haya editado a propósito.</summary>
    private static readonly HashSet<string> PropiedadesIgnoradas = new(StringComparer.Ordinal)
    {
        "Id", "CreatedAt", "UpdatedAt", "CreatedBy", "UpdatedBy"
    };

    public static Dictionary<string, object?> Snapshot<T>(T entidad) =>
        PropiedadesDiffables<T>().ToDictionary(p => p.Name, p => p.GetValue(entidad));

    public static List<CambioDetectado> Comparar<T>(Dictionary<string, object?> antes, T despues)
    {
        var cambios = new List<CambioDetectado>();
        foreach (var prop in PropiedadesDiffables<T>())
        {
            var valorNuevo = prop.GetValue(despues);
            var valorAnterior = antes.GetValueOrDefault(prop.Name);
            if (!Equals(valorAnterior, valorNuevo))
                cambios.Add(new CambioDetectado(prop.Name, Formatear(valorAnterior), Formatear(valorNuevo)));
        }
        return cambios;
    }

    private static IEnumerable<PropertyInfo> PropiedadesDiffables<T>() =>
        typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead && p.CanWrite && p.GetIndexParameters().Length == 0)
            .Where(p => !PropiedadesIgnoradas.Contains(p.Name))
            .Where(p => EsTipoSimple(p.PropertyType));

    private static bool EsTipoSimple(Type tipo)
    {
        var t = Nullable.GetUnderlyingType(tipo) ?? tipo;
        return t.IsPrimitive || t.IsEnum || t == typeof(string) || t == typeof(decimal) || t == typeof(DateTime) || t == typeof(Guid);
    }

    private static string? Formatear(object? valor) => valor switch
    {
        null => null,
        DateTime dt => dt.ToString("O"),
        _ => valor.ToString()
    };
}
