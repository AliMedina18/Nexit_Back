namespace Nexit.Core.Entities;

/// <summary>
/// Catálogo de las 6 etapas del proceso comercial de Next (E1 a E6 -- ver docs/33 y la hoja "Etapas
/// clientes" del Excel de seguimiento de proyectos que dio la usuaria): Contacto inicial,
/// Reconocimiento, Oportunidad de negocio, Cierre y pre-producción, Finalización, Facturación.
/// Es una etapa PREVIA/paralela al ciclo de vida de un Proyecto (EstadoProyecto/FaseProyecto) --
/// describe en qué punto del proceso comercial está la relación con un Cliente, incluyendo las
/// etapas E1/E2 que ocurren ANTES de que exista un brief o un Proyecto.
/// </summary>
public class EtapaCliente
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Nombre { get; set; } = string.Empty;
    public short Orden { get; set; }
    /// <summary>0-100 -- "% del proceso" de la hoja "Etapas clientes" (10, 30, 60, 80, 90, 100).</summary>
    public short PorcentajeProceso { get; set; }
}
