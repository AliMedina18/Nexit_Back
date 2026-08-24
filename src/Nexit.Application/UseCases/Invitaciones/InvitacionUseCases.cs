using Nexit.Application.DTOs.Invitaciones;
using Nexit.Application.DTOs.Usuarios;
using Nexit.Application.UseCases.Usuarios;
using Nexit.Core.Constants;
using Nexit.Core.Entities;
using Nexit.Core.Exceptions;
using Nexit.Core.Interfaces;

namespace Nexit.Application.UseCases.Invitaciones;

public class CrearInvitacionUseCase(IInvitacionEquipoRepository repository, ISupabaseAuthAdminService authAdmin, IUnitOfWork unitOfWork) : ICrearInvitacionUseCase
{
    public async Task<InvitacionResponseDto> ExecuteAsync(CrearInvitacionDto input, Guid usuarioId, CancellationToken ct = default)
    {
        // Primero se dispara la invitación real por Supabase -- si eso falla (Service Role Key sin
        // configurar, Supabase caído, correo ya registrado), no queda ninguna invitación "pendiente"
        // a medias en nuestra base sin que se haya enviado nada de verdad.
        await authAdmin.InvitarUsuarioAsync(input.Email, ct);

        var invitacion = new InvitacionEquipo
        {
            Email = input.Email, Rol = input.Rol, Mensaje = input.Mensaje,
            Estado = EstadosInvitacion.Pendiente, InvitadoPorId = usuarioId, CreatedBy = usuarioId
        };
        await repository.AddAsync(invitacion, ct);
        await unitOfWork.SaveChangesAsync(ct);
        return InvitacionMapper.ToResponse(invitacion, invitadoPorNombre: null);
    }
}

public class ConsultarInvitacionesUseCase(IInvitacionEquipoRepository repository) : IConsultarInvitacionesUseCase
{
    public async Task<IReadOnlyList<InvitacionResponseDto>> ListAsync(CancellationToken ct = default) =>
        (await repository.GetAllAsync(ct)).Select(x => InvitacionMapper.ToResponse(x, NombreCompleto(x.InvitadoPor))).ToList();

    internal static string? NombreCompleto(Usuario? usuario) => usuario is null ? null : $"{usuario.Nombre} {usuario.Apellido}".Trim();
}

public class ConsultarMiInvitacionUseCase(IInvitacionEquipoRepository repository) : IConsultarMiInvitacionUseCase
{
    public async Task<InvitacionResponseDto?> ExecuteAsync(string email, CancellationToken ct = default)
    {
        var invitacion = await repository.GetPendientePorEmailAsync(email, ct);
        return invitacion is null ? null : InvitacionMapper.ToResponse(invitacion, ConsultarInvitacionesUseCase.NombreCompleto(invitacion.InvitadoPor));
    }
}

public class AceptarInvitacionUseCase(IInvitacionEquipoRepository invitaciones, IUsuarioRepository usuarios, IUnitOfWork unitOfWork) : IAceptarInvitacionUseCase
{
    public async Task<UsuarioResponseDto> ExecuteAsync(Guid invitacionId, AceptarInvitacionDto input, Guid usuarioId, string email, CancellationToken ct = default)
    {
        var invitacion = await invitaciones.GetByIdAsync(invitacionId, ct) ?? throw new EntityNotFoundException("InvitacionEquipo", invitacionId);
        // Ver docs/25: esto es lo que reemplaza el UUID que antes había que copiar a mano en
        // POST /api/usuarios -- acá se usa el propio GetUserId() de quien acepta, tomado de su JWT.
        if (!string.Equals(invitacion.Email, email, StringComparison.OrdinalIgnoreCase))
            throw new ForbiddenOperationException("Esta invitación no es para tu correo.");
        if (invitacion.Estado != EstadosInvitacion.Pendiente)
            throw new BusinessRuleException($"Esta invitación ya se {(invitacion.Estado == EstadosInvitacion.Aceptada ? "aceptó" : "rechazó")} antes.");
        if (await usuarios.GetByIdAsync(usuarioId, ct) is not null)
            throw new BusinessRuleException("Ya tienes un perfil creado en el sistema.");

        var usuario = new Usuario { Id = usuarioId, Nombre = input.Nombre, Apellido = input.Apellido, Email = invitacion.Email, Rol = invitacion.Rol, Iniciales = input.Iniciales, Activo = true };
        await usuarios.AddAsync(usuario, ct);
        invitacion.Estado = EstadosInvitacion.Aceptada;
        invitacion.FechaRespuesta = DateTime.UtcNow;
        await unitOfWork.SaveChangesAsync(ct);
        return UsuarioMapper.ToResponse(usuario);
    }
}

public class RechazarInvitacionUseCase(IInvitacionEquipoRepository invitaciones, IUnitOfWork unitOfWork) : IRechazarInvitacionUseCase
{
    public async Task ExecuteAsync(Guid invitacionId, string email, CancellationToken ct = default)
    {
        var invitacion = await invitaciones.GetByIdAsync(invitacionId, ct) ?? throw new EntityNotFoundException("InvitacionEquipo", invitacionId);
        if (!string.Equals(invitacion.Email, email, StringComparison.OrdinalIgnoreCase))
            throw new ForbiddenOperationException("Esta invitación no es para tu correo.");
        if (invitacion.Estado != EstadosInvitacion.Pendiente)
            throw new BusinessRuleException($"Esta invitación ya se {(invitacion.Estado == EstadosInvitacion.Aceptada ? "aceptó" : "rechazó")} antes.");

        invitacion.Estado = EstadosInvitacion.Rechazada;
        invitacion.FechaRespuesta = DateTime.UtcNow;
        await unitOfWork.SaveChangesAsync(ct);
    }
}

internal static class InvitacionMapper
{
    public static InvitacionResponseDto ToResponse(InvitacionEquipo invitacion, string? invitadoPorNombre) => new()
    {
        Id = invitacion.Id, Email = invitacion.Email, Rol = invitacion.Rol, Mensaje = invitacion.Mensaje, Estado = invitacion.Estado,
        InvitadoPorNombre = invitadoPorNombre, CreatedAt = invitacion.CreatedAt, FechaRespuesta = invitacion.FechaRespuesta
    };
}
