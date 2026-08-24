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
}
