using Nexit.Core.Entities;

namespace Nexit.Core.Utils;

/// <summary>
/// Rúbrica de puntos para clientes (docs/21, docs/24) -- mismo patrón que
/// <see cref="PrioridadProyectoCalculador"/> y <see cref="PrioridadProveedorCalculador"/>: reglas
/// simples y auditables, sin IA, con razones explícitas. Se inspira en la parte de "recencia" y
/// "frecuencia" del modelo RFM que ya se investigó en docs/21 -- se dejó fuera el eje "valor
/// monetario" porque Cliente no tiene todavía un campo numérico confiable para eso
/// (<c>ValorReferencia</c> es texto libre); queda anotado como posible extensión futura en docs/24.
///
/// Los pesos son un punto de partida, ajustable con casos reales.
/// </summary>
public static class PrioridadClienteCalculador
{
    public const int PuntosSinActividadReciente = 35;
    public const int DiasSinActividadParaAlertar = 90;
    public const int PuntosClienteFrecuente = 25;
    public const int ProyectosActivosMinimoParaFrecuente = 3;
    public const int PuntosClienteSinProyectosAun = 20;

    public record Resultado(int Puntaje, IReadOnlyList<string> Razones);

    /// <param name="cliente">El cliente a puntuar.</param>
    /// <param name="ultimoProyecto">
    /// Fecha de creación (<c>CreatedAt</c>) del proyecto más reciente de este cliente, sin importar
    /// su estado (incluso uno ya cerrado cuenta como "la última vez que trabajamos con él"), o
    /// <c>null</c> si nunca ha tenido ninguno.
    /// </param>
    /// <param name="proyectosActivos">Cuántos de sus proyectos están en un estado no terminal ahora mismo.</param>
    /// <param name="ahora">Momento actual (UTC), recibido como parámetro para que esto sea puro y fácil de probar con fechas fijas.</param>
    public static Resultado Calcular(Cliente cliente, DateTime? ultimoProyecto, int proyectosActivos, DateTime ahora)
    {
        var puntaje = 0;
        var razones = new List<string>();

        // 1) Recencia: sin proyectos nuevos hace rato, o nunca ha tenido ninguno (mutuamente excluyentes).
        if (ultimoProyecto is DateTime ultima)
        {
            var dias = (ahora.Date - ultima.Date).Days;
            if (dias >= DiasSinActividadParaAlertar)
            {
                puntaje += PuntosSinActividadReciente;
                razones.Add($"Sin proyectos nuevos hace {dias} días.");
            }
        }
        else
        {
            puntaje += PuntosClienteSinProyectosAun;
            razones.Add("Cliente registrado sin ningún proyecto todavía.");
        }

        // 2) Frecuencia: cliente con varios proyectos activos al mismo tiempo -- mantenerlo atendido es prioritario.
        if (proyectosActivos >= ProyectosActivosMinimoParaFrecuente)
        {
            puntaje += PuntosClienteFrecuente;
            razones.Add($"Cliente frecuente: {proyectosActivos} proyectos activos en este momento.");
        }

        return new Resultado(puntaje, razones);
    }
}
