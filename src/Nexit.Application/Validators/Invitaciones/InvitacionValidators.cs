using FluentValidation;
using Nexit.Application.DTOs.Invitaciones;
using Nexit.Core.Constants;
using Nexit.Core.Interfaces;

namespace Nexit.Application.Validators.Invitaciones;

public class CrearInvitacionValidator : AbstractValidator<CrearInvitacionDto>
{
    public CrearInvitacionValidator(IUsuarioRepository usuarios, IInvitacionEquipoRepository invitaciones, IDominioCorreoPermitidoRepository dominios)
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(255);
        // Mismo respaldo de aplicación que CreateUsuarioValidator (ver ahí) -- el correo debe ser
        // de un dominio laboral permitido.
        RuleFor(x => x.Email).MustAsync(async (email, ct) => await dominios.EsDominioPermitidoAsync(email, ct))
            .WithMessage("El correo no pertenece a un dominio laboral permitido.").When(x => !string.IsNullOrWhiteSpace(x.Email));
        RuleFor(x => x.Email).MustAsync(async (email, ct) => !await usuarios.ExistsByEmailAsync(email, null, ct))
            .WithMessage("Ya existe un usuario con ese correo -- no hace falta invitarlo.").When(x => !string.IsNullOrWhiteSpace(x.Email));
        RuleFor(x => x.Email).MustAsync(async (email, ct) => !await invitaciones.ExistePendientePorEmailAsync(email, ct))
            .WithMessage("Ya hay una invitación pendiente para ese correo.").When(x => !string.IsNullOrWhiteSpace(x.Email));
        RuleFor(x => x.Rol).Must(rol => Roles.Asignables.Contains(rol))
            .WithMessage($"El rol debe ser uno de: {string.Join(", ", Roles.Asignables)}. No se puede invitar a nadie como super administrador.");
        RuleFor(x => x.Mensaje).MaximumLength(500);
    }
}

public class AceptarInvitacionValidator : AbstractValidator<AceptarInvitacionDto>
{
    public AceptarInvitacionValidator()
    {
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(255);
        RuleFor(x => x.Apellido).NotEmpty().MaximumLength(255);
    }
}

/// <summary>
/// Solo valida la FORMA del lote (que venga al menos un correo y no una lista desmedida). Cada
/// correo se valida uno por uno con <see cref="CrearInvitacionValidator"/> dentro del caso de uso,
/// para poder devolver el motivo exacto por correo en vez de un único error para todo el envío.
/// </summary>
public class CrearInvitacionesLoteValidator : AbstractValidator<CrearInvitacionesLoteDto>
{
    /// <summary>Tope por envío. No es un límite técnico sino de sensatez -- el equipo de Next son decenas de personas, y un lote enorme sería casi siempre un pegado accidental.</summary>
    public const int MaximoPorLote = 25;

    public CrearInvitacionesLoteValidator()
    {
        RuleFor(x => x.Emails).NotEmpty().WithMessage("Escribe al menos un correo para invitar.");
        RuleFor(x => x.Emails).Must(x => x.Count <= MaximoPorLote)
            .WithMessage($"Puedes invitar hasta {MaximoPorLote} correos por envío.");
        RuleFor(x => x.Rol).Must(rol => Roles.Asignables.Contains(rol))
            .WithMessage($"El rol debe ser uno de: {string.Join(", ", Roles.Asignables)}. No se puede invitar a nadie como super administrador.");
        RuleFor(x => x.Mensaje).MaximumLength(500);
    }
}
