using FluentValidation;
using Nexit.Application.DTOs.Usuarios;
using Nexit.Core.Constants;
using Nexit.Core.Interfaces;

namespace Nexit.Application.Validators.Usuarios;

public class CreateUsuarioValidator : AbstractValidator<CreateUsuarioDto>
{
    public CreateUsuarioValidator(IUsuarioRepository repository)
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Falta el UUID del usuario en Supabase Auth (invítalo primero desde el dashboard de Supabase).");
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(255);
        RuleFor(x => x.Apellido).NotEmpty().MaximumLength(255);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(255);
        RuleFor(x => x.Email).MustAsync(async (email, token) => !await repository.ExistsByEmailAsync(email, null, token))
            .WithMessage("Ya existe un usuario con ese email.").When(x => !string.IsNullOrWhiteSpace(x.Email));
        RuleFor(x => x.Rol).Must(rol => Roles.Todos.Contains(rol)).WithMessage($"El rol debe ser uno de: {string.Join(", ", Roles.Todos)}.");
    }
}

public class UpdateUsuarioValidator : AbstractValidator<UpdateUsuarioDto>
{
    public UpdateUsuarioValidator()
    {
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(255);
        RuleFor(x => x.Apellido).NotEmpty().MaximumLength(255);
        RuleFor(x => x.Rol).Must(rol => Roles.Todos.Contains(rol)).WithMessage($"El rol debe ser uno de: {string.Join(", ", Roles.Todos)}.");
    }
}
