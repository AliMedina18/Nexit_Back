using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Nexit.Core.Interfaces;

namespace Nexit.Infrastructure.Services;

/// <summary>
/// Ver <see cref="IEmailService"/>. Manda por SMTP con la misma cuenta de Gmail que ya usa Supabase
/// para sus propios correos (docs/13) -- decisión de Alicia el 2026-09-08: cero costo y funciona hoy,
/// a cambio de los límites de envío de Gmail y de que el remitente se vea como una cuenta de Gmail.
/// Si algún día se pasa a un proveedor transaccional, se cambia esta clase y nada más.
///
/// <b>Contraseña de aplicación, no la del correo.</b> Gmail no acepta la contraseña normal de la
/// cuenta para SMTP: hay que generar una "contraseña de aplicación" con la verificación en dos pasos
/// activada, y esa es la que va en <c>Email:Contrasena</c>.
///
/// Nunca lanza, por contrato de la interfaz: sin configuración deja un aviso en el log y sigue; si
/// el envío falla, lo registra con el destinatario y sigue. La operación que disparó el correo
/// (desactivar a alguien) ya se guardó y no puede deshacerse por esto.
/// </summary>
public class SmtpEmailService(IConfiguration configuration, ILogger<SmtpEmailService> logger) : IEmailService
{
    public async Task EnviarAsync(string destinatario, string asunto, string cuerpoHtml, CancellationToken cancellationToken = default)
    {
        var host = configuration["Email:SmtpHost"];
        var usuario = configuration["Email:Usuario"];
        var contrasena = configuration["Email:Contrasena"];
        if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(usuario) || string.IsNullOrWhiteSpace(contrasena))
        {
            logger.LogWarning(
                "No se envió el correo \"{Asunto}\" a {Destinatario}: falta configurar Email:SmtpHost / Email:Usuario / Email:Contrasena.",
                asunto, destinatario);
            return;
        }

        var puerto = configuration.GetValue("Email:SmtpPuerto", 587);
        var remitente = configuration["Email:Remitente"] ?? "Nexit";

        try
        {
            using var cliente = new SmtpClient(host, puerto)
            {
                EnableSsl = true, // STARTTLS en el 587, que es como Gmail espera la conexión
                Credentials = new NetworkCredential(usuario, contrasena),
            };
            using var mensaje = new MailMessage
            {
                From = new MailAddress(usuario, remitente),
                Subject = asunto,
                Body = cuerpoHtml,
                IsBodyHtml = true,
            };
            mensaje.To.Add(destinatario);

            await cliente.SendMailAsync(mensaje, cancellationToken);
            logger.LogInformation("Correo \"{Asunto}\" enviado a {Destinatario}.", asunto, destinatario);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Falló el envío del correo \"{Asunto}\" a {Destinatario}.", asunto, destinatario);
        }
    }
}
