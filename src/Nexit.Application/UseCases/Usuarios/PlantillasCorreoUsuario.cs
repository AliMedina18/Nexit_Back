using System.Net;

namespace Nexit.Application.UseCases.Usuarios;

/// <summary>
/// Los correos que manda Nexit por su cuenta (hoy solo los de estado de la cuenta). Vive en
/// Application y no en Infrastructure a propósito: el TEXTO de un aviso es una decisión de negocio
/// ("qué se le dice a alguien a quien le quitaron el acceso"), no un detalle de infraestructura --
/// Infrastructure solo sabe empujar bytes por SMTP, sin enterarse de qué dicen. Además, las pruebas
/// de arquitectura impiden que Application dependa de Infrastructure, así que no podría estar allá.
///
/// Mismo diseño que
/// las plantillas que ya viven en Supabase (docs/14, 15, 16): fondo gris claro, tarjeta blanca con
/// franja de acento arriba, wordmark, cuerpo y pie. Se arman acá y no en un archivo suelto para que
/// no haya un HTML que se pueda quedar sin desplegar junto al código que lo usa.
///
/// Todo lo que venga de la base de datos se escapa antes de entrar al HTML -- un nombre con un
/// carácter raro no debe poder romper la maqueta ni inyectar nada en el correo de nadie.
/// </summary>
internal static class PlantillasCorreoUsuario
{
    private const string Acento = "#6fceca";
    private const string CorreoAdministracion = "analistacompras@agencianextmkt.com";

    public static string CuentaDesactivada(string nombre, DateTime seEliminaEl, int diasDeGracia) => Envolver(
        titulo: "Tu acceso a Nexit quedó suspendido",
        preheader: "Tu cuenta de Nexit fue desactivada.",
        saludo: $"Hola {WebUtility.HtmlEncode(nombre)}",
        cuerpo: "Tu cuenta en <strong>Nexit</strong> fue desactivada, así que por ahora no vas a poder entrar al sistema.",
        recuadro: $"""
            <strong>Qué pasa a partir de ahora</strong><br />
            Si nadie la reactiva, la cuenta se elimina automáticamente el
            <strong>{seEliminaEl:dd 'de' MMMM 'de' yyyy}</strong> (a los {diasDeGracia} días). Si esto no era lo
            esperado, escríbele cuanto antes a la administración del sistema:
            <a href="mailto:{CorreoAdministracion}" style="color:#1B7F79; font-weight:bold; text-decoration:underline;">{CorreoAdministracion}</a>.
            """);

    public static string CuentaReactivada(string nombre) => Envolver(
        titulo: "Tu acceso a Nexit volvió",
        preheader: "Tu cuenta de Nexit fue reactivada.",
        saludo: $"Hola {WebUtility.HtmlEncode(nombre)}",
        cuerpo: "Tu cuenta en <strong>Nexit</strong> volvió a estar activa: ya puedes entrar al sistema con normalidad.",
        recuadro: """
            <strong>Nada que hacer de tu parte</strong><br />
            Entra como siempre, con el mismo correo y la misma contraseña de antes.
            """);

    private static string Envolver(string titulo, string preheader, string saludo, string cuerpo, string recuadro) => $"""
        <!DOCTYPE html>
        <html lang="es">
        <head>
        <meta charset="UTF-8" />
        <meta name="viewport" content="width=device-width, initial-scale=1.0" />
        <title>{titulo}</title>
        </head>
        <body style="margin:0; padding:0; background-color:#EEF0F4; font-family:Arial, Helvetica, sans-serif;">
          <div style="display:none; max-height:0; overflow:hidden; opacity:0;">{preheader}</div>
          <table role="presentation" width="100%" cellpadding="0" cellspacing="0" style="background-color:#EEF0F4; padding:40px 16px;">
            <tr>
              <td align="center">
                <table role="presentation" width="560" cellpadding="0" cellspacing="0" style="max-width:560px; width:100%; background-color:#ffffff; border-radius:14px; overflow:hidden; box-shadow:0 2px 10px rgba(17,24,39,0.08);">
                  <tr><td style="background-color:{Acento}; height:6px; line-height:6px; font-size:0;">&nbsp;</td></tr>
                  <tr>
                    <td style="padding:36px 40px 8px 40px;">
                      <span style="font-family:Arial, Helvetica, sans-serif; font-size:21px; font-weight:bold; color:#111827; letter-spacing:-0.3px;">Nexit</span>
                    </td>
                  </tr>
                  <tr>
                    <td style="padding:20px 40px 8px 40px;">
                      <p style="margin:0 0 6px 0; font-family:Arial, Helvetica, sans-serif; font-size:22px; font-weight:bold; color:#111827;">{saludo}</p>
                      <p style="margin:0 0 28px 0; font-family:Arial, Helvetica, sans-serif; font-size:15px; line-height:1.65; color:#4B5563;">{cuerpo}</p>
                      <table role="presentation" width="100%" cellpadding="0" cellspacing="0">
                        <tr>
                          <td style="background-color:#F0FBFA; border:1px solid #C9EEEC; border-radius:12px; padding:22px 24px;">
                            <p style="margin:0; font-family:Arial, Helvetica, sans-serif; font-size:14.5px; line-height:1.65; color:#111827;">{recuadro}</p>
                          </td>
                        </tr>
                      </table>
                    </td>
                  </tr>
                  <tr><td style="padding:28px 40px 0 40px;"><hr style="border:none; border-top:1px solid #EEF0F4; margin:0;" /></td></tr>
                  <tr>
                    <td style="padding:20px 40px 32px 40px;">
                      <p style="margin:0; font-family:Arial, Helvetica, sans-serif; font-size:12px; line-height:1.6; color:#B0B4BF;">Nexit es el sistema interno de Next.</p>
                    </td>
                  </tr>
                </table>
              </td>
            </tr>
          </table>
        </body>
        </html>
        """;
}
