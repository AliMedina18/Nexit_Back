using Nexit.Application.DTOs.Auth;
using Nexit.Core.Interfaces;

namespace Nexit.Application.UseCases.Auth;

/// <summary>
/// Detección de "primera vez" vs. "recurrente" para el login (docs/30) -- endpoint público
/// (GET /api/auth/estado-cuenta, sin autenticación), así que la respuesta NUNCA revela si el
/// correo existe o no: "no existe" y "existe pero aún no configuró contraseña (o está inactivo)"
/// dan exactamente la misma respuesta (TieneContrasena = false), siguiendo el mismo principio ya
/// establecido en docs/12 (HU-04) para no permitir enumeración de cuentas por correo.
///
/// Esto NO consulta Supabase Auth -- no hay forma de saber desde la Admin API si una cuenta tiene
/// contraseña configurada (esa información no la expone). En vez de eso, se apoya en la columna
/// propia `usuarios.contrasena_configurada`, que Nexit_Front marca en true la primera vez que la
/// persona termina de crear/restablecer su contraseña DENTRO de Nexit (ver
/// ConfirmarContrasenaConfiguradaUseCase). Si por algún motivo esa marca nunca se puso (por
/// ejemplo, alguien que estableció su contraseña fuera de Nexit y nunca volvió a pasar por ese
/// paso), el login simplemente la trata como "primera vez" -- no es un error, solo hace que vea
/// una vez más la pantalla de código en vez de la de contraseña directamente; el enlace manual
/// "¿ya tienes contraseña?" se deja en el login como respaldo para ese caso.
/// </summary>
public class ConsultarEstadoCuentaUseCase(IUsuarioRepository repository) : IConsultarEstadoCuentaUseCase
{
    public async Task<EstadoCuentaResponseDto> ExecuteAsync(string email, CancellationToken cancellationToken = default)
    {
        var correo = email.Trim();
        if (correo.Length == 0) return new EstadoCuentaResponseDto { TieneContrasena = false };

        var usuario = await repository.GetByEmailAsync(correo, cancellationToken);
        // Inactiva == se trata igual que "no existe": no puede entrar de todos modos, y no hay
        // razón para distinguir los dos casos en una respuesta pública.
        var tieneContrasena = usuario is { Activo: true, ContrasenaConfigurada: true };
        return new EstadoCuentaResponseDto { TieneContrasena = tieneContrasena };
    }
}

/// <summary>
/// Marca `usuarios.contrasena_configurada = true` para quien llama -- Nexit_Front la invoca justo
/// después de que `supabase.auth.updateUser({ password })` responde sin error, tanto al crear la
/// contraseña por primera vez como al restablecerla (docs/30). Requiere sesión (JWT de Supabase ya
/// válido en ese punto -- verifyOtp acaba de dar una).
///
/// Best-effort a propósito: si todavía no existe fila en `usuarios` para este id (por ejemplo,
/// alguien que crea su contraseña antes de aceptar su invitación -- ver docs/25), esto no lanza
/// error, simplemente no hace nada. No vale la pena arriesgar la experiencia de "ya creaste tu
/// contraseña" por una marca puramente cosmética que de todos modos tiene el respaldo manual del
/// login si no queda puesta.
/// </summary>
public class ConfirmarContrasenaConfiguradaUseCase(IUsuarioRepository repository, IUnitOfWork unitOfWork) : IConfirmarContrasenaConfiguradaUseCase
{
    public async Task ExecuteAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var usuario = await repository.GetByIdAsync(userId, cancellationToken);
        if (usuario is null || usuario.ContrasenaConfigurada) return;

        usuario.ContrasenaConfigurada = true;
        usuario.UpdatedAt = DateTime.UtcNow;
        repository.Update(usuario);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
