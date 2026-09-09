using FluentValidation;
using Nexit.Application.DTOs.Invitaciones;
using Nexit.Application.DTOs.Usuarios;
using Nexit.Application.UseCases.Notificaciones;
using Nexit.Application.UseCases.Usuarios;
using Nexit.Core.Constants;
using Nexit.Core.Entities;
using Nexit.Core.Exceptions;
using Nexit.Core.Interfaces;

namespace Nexit.Application.UseCases.Invitaciones;

public class CrearInvitacionUseCase(IInvitacionEquipoRepository repository, IUsuarioRepository usuarios, ISupabaseAuthAdminService authAdmin, IUnitOfWork unitOfWork) : ICrearInvitacionUseCase
{
    /// <summary>Mismas etiquetas que ROL_LABELS en el frontend (usuarios/page.tsx) -- solo para el
    /// correo de invitación (docs/42); si algún día se desincronizan no rompe nada, en el peor caso
    /// el correo muestra el nombre técnico del rol en vez de la etiqueta bonita.</summary>
    private static readonly Dictionary<string, string> RolLabels = new()
    {
        [Roles.SuperAdmin] = "Super admin", [Roles.Admin] = "Admin", [Roles.Manager] = "Director", [Roles.Miembro] = "Miembro",
    };

    public async Task<InvitacionResponseDto> ExecuteAsync(CrearInvitacionDto input, Guid usuarioId, CancellationToken ct = default)
    {
        // Quien invita, para que el correo (docs/42) pueda decir "Fulana te invitó" en vez de solo
        // "Te invitaron" -- se busca ANTES de invitar porque si por lo que sea no se encuentra (no
        // debería pasar: es quien está autenticado y pasó por la validación del endpoint), la
        // invitación se sigue mandando igual, solo que sin ese dato en el correo.
        var invitador = await usuarios.GetByIdAsync(usuarioId, ct);
        var datosCorreo = new Dictionary<string, string> { ["rol"] = RolLabels.GetValueOrDefault(input.Rol, input.Rol) };
        if (ConsultarInvitacionesUseCase.NombreCompleto(invitador) is { } nombreInvitador) datosCorreo["invitadoPor"] = nombreInvitador;
        if (!string.IsNullOrWhiteSpace(input.Mensaje)) datosCorreo["mensaje"] = input.Mensaje;

        // Primero se dispara la invitación real por Supabase -- si eso falla (Service Role Key sin
        // configurar, Supabase caído, correo ya registrado), no queda ninguna invitación "pendiente"
        // a medias en nuestra base sin que se haya enviado nada de verdad.
        await authAdmin.InvitarUsuarioAsync(input.Email, datosCorreo, ct);

        var invitacion = new InvitacionEquipo
        {
            Email = input.Email, Rol = input.Rol, Mensaje = input.Mensaje,
            Estado = EstadosInvitacion.Pendiente, InvitadoPorId = usuarioId, CreatedBy = usuarioId
        };
        await repository.AddAsync(invitacion, ct);
        await unitOfWork.SaveChangesAsync(ct);
        return InvitacionMapper.ToResponse(invitacion, ConsultarInvitacionesUseCase.NombreCompleto(invitador));
    }
}

/// <summary>
/// Ver ICrearInvitacionesLoteUseCase. Reutiliza el validador y el caso de uso de la invitación
/// individual correo por correo, así que las reglas son exactamente las mismas (dominio permitido,
/// que no exista ya el usuario, que no haya otra invitación pendiente) y no hay una segunda copia
/// de esas reglas que se pueda desincronizar. Cada correo se resuelve por separado a propósito: un
/// correo repetido o de un dominio ajeno no debe impedir que los demás se inviten -- y cada uno
/// lleva su propio rol (Alicia 2026-09-09: un mismo envío puede mezclar miembros, un admin y un
/// directivo).
/// </summary>
public class CrearInvitacionesLoteUseCase(ICrearInvitacionUseCase crear, IValidator<CrearInvitacionDto> validador) : ICrearInvitacionesLoteUseCase
{
    public async Task<InvitacionesLoteResponseDto> ExecuteAsync(CrearInvitacionesLoteDto input, Guid usuarioId, CancellationToken ct = default)
    {
        var resultado = new InvitacionesLoteResponseDto();
        // Normaliza y quita repetidos DENTRO del mismo envío (escribir dos veces el mismo correo en
        // el modal es un error de dedo, no dos invitaciones) -- los repetidos contra la base los
        // sigue detectando el validador de siempre. Si el mismo correo viene dos veces con roles
        // distintos, se queda con el primero -- el modal no debería dejar que pase, pero por si acaso.
        var destinatarios = input.Destinatarios
            .Select(x => new { Email = (x.Email ?? string.Empty).Trim(), x.Rol })
            .Where(x => x.Email.Length > 0)
            .DistinctBy(x => x.Email.ToLowerInvariant())
            .ToList();

        foreach (var destinatario in destinatarios)
        {
            var individual = new CrearInvitacionDto { Email = destinatario.Email, Rol = destinatario.Rol, Mensaje = input.Mensaje };
            var validacion = await validador.ValidateAsync(individual, ct);
            if (!validacion.IsValid)
            {
                resultado.Fallidas.Add(new InvitacionFallidaDto { Email = destinatario.Email, Motivo = string.Join(" ", validacion.Errors.Select(e => e.ErrorMessage)) });
                continue;
            }
            try
            {
                resultado.Enviadas.Add(await crear.ExecuteAsync(individual, usuarioId, ct));
            }
            catch (BusinessRuleException ex)
            {
                // Falla propia de Supabase para ESE correo (ya tiene cuenta, rechazo del proveedor de
                // correo, etc.). Se anota y se sigue con el resto del lote.
                resultado.Fallidas.Add(new InvitacionFallidaDto { Email = destinatario.Email, Motivo = ex.Message });
            }
        }
        return resultado;
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

public class AceptarInvitacionUseCase(IInvitacionEquipoRepository invitaciones, IUsuarioRepository usuarios, INotificacionRepository notificaciones, IUnitOfWork unitOfWork) : IAceptarInvitacionUseCase
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
        // Si quien invitó ya no está en el equipo (InvitadoPorId quedó en null al eliminarlo), no hay
        // a quién avisarle -- la invitación sigue siendo válida igual.
        if (NotificacionFactory.InvitacionRespondida(invitacion.InvitadoPorId, invitacion.Email, aceptada: true) is { } aviso)
            await notificaciones.AddAsync(aviso, ct);
        await unitOfWork.SaveChangesAsync(ct);
        return UsuarioMapper.ToResponse(usuario);
    }
}

public class RechazarInvitacionUseCase(IInvitacionEquipoRepository invitaciones, INotificacionRepository notificaciones, IUnitOfWork unitOfWork) : IRechazarInvitacionUseCase
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
        if (NotificacionFactory.InvitacionRespondida(invitacion.InvitadoPorId, invitacion.Email, aceptada: false) is { } aviso)
            await notificaciones.AddAsync(aviso, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }
}

/// <summary>Ver ICancelarInvitacionUseCase.</summary>
public class CancelarInvitacionUseCase(IInvitacionEquipoRepository invitaciones, IUnitOfWork unitOfWork) : ICancelarInvitacionUseCase
{
    public async Task ExecuteAsync(Guid invitacionId, CancellationToken ct = default)
    {
        var invitacion = await invitaciones.GetByIdAsync(invitacionId, ct) ?? throw new EntityNotFoundException("InvitacionEquipo", invitacionId);
        // Una ya respondida no se cancela: si se aceptó, la persona ya tiene perfil (para darla de
        // baja se desactiva el usuario, no se borra su invitación); si se rechazó, ya no hay nada
        // pendiente que cancelar.
        if (invitacion.Estado != EstadosInvitacion.Pendiente)
            throw new BusinessRuleException($"Esta invitación ya se {(invitacion.Estado == EstadosInvitacion.Aceptada ? "aceptó" : "rechazó")} -- no hay nada que cancelar.");

        await invitaciones.DeleteAsync(invitacionId, ct);
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
