using Nexit.Core.Entities;

namespace Nexit.Core.Utils;

/// <summary>
/// Rúbrica de puntos para proveedores (docs/21, docs/24) -- mismo patrón que
/// <see cref="PrioridadProyectoCalculador"/>: reglas simples y auditables sobre datos que ya
/// existen, sin IA, siempre acompañadas de la lista de razones que las explica.
///
/// A diferencia de proyectos, acá "prioridad" no significa "urgencia" sino "vale la pena que
/// alguien le preste atención": puede ser un buen proveedor que se está dejando de usar, uno mal
/// calificado al que se le sigue asignando trabajo, o uno que nunca se calificó. La idea de mirar
/// el campo <c>Score</c> junto con "hace cuánto no se le asigna un proyecto" y "cuánta gente lo
/// tiene marcado como colaborador" viene de docs/21; el hallazgo de que la confiabilidad de un
/// proveedor importa más que su velocidad (docs/23, del proyecto de referencia sobre cadena de
/// suministro) es lo que justifica seguir dándole peso real al Score en vez de ignorarlo.
///
/// Los pesos son un punto de partida, ajustable con casos reales -- igual que en
/// <see cref="PrioridadProyectoCalculador"/>.
/// </summary>
public static class PrioridadProveedorCalculador
{
    public const int ScoreMaximo = 5; // ver el check constraint ck_proveedores_score (1 a 5)
    public const int ScoreBajoUmbral = 2; // Score <= este valor cuenta como "bajo"
    public const int ScoreAltoUmbral = 4; // Score >= este valor cuenta como "bueno"

    public const int PuntosScoreBajo = 40;
    public const int PuntosSinCalificarYaUsado = 20;
    public const int PuntosBuenProveedorSinUsoReciente = 40;
    public const int DiasSinUsoParaAlertar = 90;
    public const int PuntosColaboradoresSinProyectoFormal = 20;
    public const int ColaboradoresMinimoParaAlertar = 2;

    public record Resultado(int Puntaje, IReadOnlyList<string> Razones);

    /// <param name="proveedor">El proveedor a puntuar.</param>
    /// <param name="ultimoProyectoAsignado">
    /// Fecha de creación (<c>CreatedAt</c>) del proyecto más reciente que lo tiene asociado
    /// (<see cref="ProyectoProveedor"/>), o <c>null</c> si nunca se le ha asignado ninguno -- ver
    /// cómo se calcula en <c>ConsultarPrioridadProveedoresUseCase</c>. Se usa <c>CreatedAt</c> del
    /// proyecto (cuándo se armó) y no <c>FechaEvento</c> (que puede estar en el futuro) porque lo
    /// que importa acá es cuándo fue la última vez que alguien realmente contó con este proveedor.
    /// </param>
    /// <param name="ahora">Momento actual (UTC), recibido como parámetro para que esto sea puro y fácil de probar con fechas fijas.</param>
    public static Resultado Calcular(Proveedor proveedor, DateTime? ultimoProyectoAsignado, DateTime ahora)
    {
        var puntaje = 0;
        var razones = new List<string>();
        var tieneAlgunProyecto = ultimoProyectoAsignado.HasValue;

        // 1) Score bajo -- vale la pena reconsiderar seguir usándolo.
        if (proveedor.Score is int scoreBajo && scoreBajo <= ScoreBajoUmbral)
        {
            puntaje += PuntosScoreBajo;
            razones.Add($"Calificado con Score bajo ({scoreBajo}/{ScoreMaximo}).");
        }
        // 2) Nunca se le puso Score, pero ya se ha usado -- falta calificarlo.
        else if (proveedor.Score is null && tieneAlgunProyecto)
        {
            puntaje += PuntosSinCalificarYaUsado;
            razones.Add("Se ha usado en proyectos pero nunca se le asignó un Score.");
        }

        // 3) Buen proveedor que se está dejando de usar (independiente de 1/2 -- requiere Score alto).
        if (proveedor.Score is int scoreAlto && scoreAlto >= ScoreAltoUmbral)
        {
            var diasSinUso = tieneAlgunProyecto ? (ahora.Date - ultimoProyectoAsignado!.Value.Date).Days : (int?)null;
            if (!tieneAlgunProyecto || diasSinUso >= DiasSinUsoParaAlertar)
            {
                puntaje += PuntosBuenProveedorSinUsoReciente;
                razones.Add(tieneAlgunProyecto
                    ? $"Bien calificado (Score {scoreAlto}/{ScoreMaximo}) pero sin proyectos en los últimos {diasSinUso} días."
                    : $"Bien calificado (Score {scoreAlto}/{ScoreMaximo}) pero nunca se le ha asignado un proyecto.");
            }
        }

        // 4) Varias personas del equipo lo tienen marcado como colaborador sin que tenga ningún proyecto formal todavía.
        if (!tieneAlgunProyecto && proveedor.Colaboradores.Count >= ColaboradoresMinimoParaAlertar)
        {
            puntaje += PuntosColaboradoresSinProyectoFormal;
            razones.Add($"{proveedor.Colaboradores.Count} personas del equipo lo tienen marcado como colaborador, pero no tiene ningún proyecto formal asociado todavía.");
        }

        return new Resultado(puntaje, razones);
    }
}
