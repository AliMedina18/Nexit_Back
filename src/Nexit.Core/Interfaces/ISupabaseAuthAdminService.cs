namespace Nexit.Core.Interfaces;

/// <summary>
/// Elimina la cuenta de Supabase Auth de alguien cuyo perfil de negocio (fila en `usuarios`) ya se
/// eliminó -- para que además de perder su perfil, pierda por completo la posibilidad de iniciar
/// sesión (sin esto, el auth hook le seguiría dando el rol por defecto "miembro" al no encontrar su
/// fila -- ver docs/17-eliminacion-automatica-usuarios.md, sección "Por qué también hay que borrar
/// la cuenta de Supabase Auth"). Requiere la Service Role Key de Supabase, que no vive en este
/// repositorio -- si no está configurada, la implementación no hace nada y solo deja un aviso en el
/// log, para no bloquear la eliminación del perfil de negocio por falta de esa clave.
/// </summary>
public interface ISupabaseAuthAdminService
{
    Task EliminarCuentaAsync(Guid usuarioId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Invita a alguien nuevo por correo usando la Admin API de Supabase Auth (docs/25) -- a
    /// diferencia de <see cref="EliminarCuentaAsync"/>, si la Service Role Key no está configurada
    /// esto SÍ lanza una excepción (<c>BusinessRuleException</c>): invitar es la acción principal de
    /// la operación, no un cleanup de mejor esfuerzo, así que no tiene sentido dejar a alguien
    /// pensando que se envió la invitación cuando en realidad no pasó nada.
    /// </summary>
    Task InvitarUsuarioAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Crea la cuenta de acceso de alguien SIN mandarle un correo de invitación (2026-09-08, pedido
    /// de Alicia: "a veces uno también puede crear un usuario manualmente"). Devuelve el UUID que
    /// Supabase le asigna, que es el mismo que se usa como <c>usuarios.id</c> -- eso es lo que
    /// permite dar de alta a alguien de una vez, sin depender de que abra un correo y responda.
    ///
    /// La cuenta se crea SIN contraseña a propósito: nadie más que la propia persona debe conocerla.
    /// La primera vez entra con el código de un solo uso que ya maneja la pantalla de login
    /// (docs/30) y ahí mismo crea su contraseña, exactamente igual que quien llega por invitación.
    ///
    /// Igual que <see cref="InvitarUsuarioAsync"/> y a diferencia de <see cref="EliminarCuentaAsync"/>,
    /// lanza <c>BusinessRuleException</c> si la Service Role Key no está configurada o si Supabase
    /// rechaza la creación: sin cuenta de acceso no hay usuario, así que fallar en silencio dejaría
    /// un perfil que nunca podría iniciar sesión.
    /// </summary>
    Task<Guid> CrearCuentaAsync(string email, CancellationToken cancellationToken = default);
}
