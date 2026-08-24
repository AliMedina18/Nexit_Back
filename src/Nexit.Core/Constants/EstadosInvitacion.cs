namespace Nexit.Core.Constants;

/// <summary>Estados posibles de <see cref="Entities.InvitacionEquipo"/> -- deben coincidir con el CHECK constraint ck_invitaciones_equipo_estado.</summary>
public static class EstadosInvitacion
{
    public const string Pendiente = "Pendiente";
    public const string Aceptada = "Aceptada";
    public const string Rechazada = "Rechazada";

    public static readonly string[] Todos = [Pendiente, Aceptada, Rechazada];
}
