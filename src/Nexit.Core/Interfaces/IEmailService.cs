namespace Nexit.Core.Interfaces;

/// <summary>
/// Envío de correo propio de Nexit (2026-09-08). Hasta ahora este backend NUNCA había enviado un
/// correo -- todos los que recibe el equipo (invitación, código de acceso, restablecer contraseña,
/// aviso de contraseña cambiada) los manda Supabase Auth, ver docs/10. Pero Supabase solo sabe
/// mandar los correos de su propio flujo de autenticación: no hay forma de pedirle que envíe un
/// mensaje cualquiera, y avisarle a alguien que su cuenta fue desactivada es exactamente eso.
/// Tampoco sirve una notificación dentro del sistema: esa persona ya no puede entrar.
///
/// Por eso existe esta interfaz, y por eso es deliberadamente mínima -- un destinatario, un asunto
/// y un cuerpo HTML. Si algún día se cambia Gmail por un proveedor transaccional (Resend, evaluado
/// en docs/13), se reemplaza la implementación y nada más.
///
/// <b>Nunca lanza.</b> Un correo que no sale no puede tumbar la operación que lo disparó: desactivar
/// a alguien tiene que quedar guardado aunque el aviso no llegue. Los fallos quedan en el log.
/// </summary>
public interface IEmailService
{
    Task EnviarAsync(string destinatario, string asunto, string cuerpoHtml, CancellationToken cancellationToken = default);
}
