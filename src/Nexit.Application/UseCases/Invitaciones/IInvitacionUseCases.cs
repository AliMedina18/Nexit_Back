using Nexit.Application.DTOs.Invitaciones;
using Nexit.Application.DTOs.Usuarios;

namespace Nexit.Application.UseCases.Invitaciones;

/// <summary>Invita a alguien nuevo desde dentro de Nexit (docs/25) -- valida, dispara la invitación real por Supabase, y solo entonces la registra.</summary>
public interface ICrearInvitacionUseCase { Task<InvitacionResponseDto> ExecuteAsync(CrearInvitacionDto input, Guid usuarioId, CancellationToken cancellationToken = default); }

public interface IConsultarInvitacionesUseCase { Task<IReadOnlyList<InvitacionResponseDto>> ListAsync(CancellationToken cancellationToken = default); }

/// <summary>La invitación Pendiente que le corresponde a quien está autenticado, según su correo -- null si no hay ninguna.</summary>
public interface IConsultarMiInvitacionUseCase { Task<InvitacionResponseDto?> ExecuteAsync(string email, CancellationToken cancellationToken = default); }

/// <summary>La propia persona invitada acepta: crea su perfil de negocio automáticamente (con el rol que se le propuso) usando su propio UUID de Supabase Auth -- sin que nadie tenga que copiarlo a mano.</summary>
public interface IAceptarInvitacionUseCase { Task<UsuarioResponseDto> ExecuteAsync(Guid invitacionId, AceptarInvitacionDto input, Guid usuarioId, string email, CancellationToken cancellationToken = default); }

public interface IRechazarInvitacionUseCase { Task ExecuteAsync(Guid invitacionId, string email, CancellationToken cancellationToken = default); }

/// <summary>
/// Quien invita se arrepiente o se equivocó de correo (2026-09-08). Distinto de RECHAZAR, que lo
/// hace la persona invitada sobre su propia invitación: esto lo hace la super administradora sobre
/// una invitación que todavía está Pendiente. Borra la fila en vez de marcarla "Cancelada" -- ese
/// estado no existe en el CHECK constraint de la tabla y agregarlo obligaría a correr SQL nuevo en
/// producción, para guardar algo que no le sirve a nadie: una invitación que nunca se respondió no
/// es historia, es ruido. Además así el correo queda libre para volver a invitarlo, que es
/// justamente lo que se quiere después de cancelar.
///
/// Lo que NO hace: borrar la cuenta que Supabase Auth ya creó al mandar el correo. Si esa persona
/// hace clic en el enlace después de cancelada, entra a Nexit sin perfil y sin invitación -- la
/// pantalla de registro le dice que no tiene acceso y le cierra la sesión (ver PerfilRequeridoFilter
/// y docs/36), así que no puede hacer nada, pero la cuenta de Auth queda ahí.
/// </summary>
public interface ICancelarInvitacionUseCase { Task ExecuteAsync(Guid invitacionId, CancellationToken cancellationToken = default); }

/// <summary>
/// Invita a varios correos en una sola operación (ver <see cref="CrearInvitacionesLoteDto"/>).
/// Valida e invita uno por uno reutilizando exactamente el mismo camino que la invitación
/// individual, y nunca aborta el lote por un correo malo -- devuelve enviadas y fallidas.
/// </summary>
public interface ICrearInvitacionesLoteUseCase
{
    Task<InvitacionesLoteResponseDto> ExecuteAsync(CrearInvitacionesLoteDto input, Guid usuarioId, CancellationToken cancellationToken = default);
}
