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
