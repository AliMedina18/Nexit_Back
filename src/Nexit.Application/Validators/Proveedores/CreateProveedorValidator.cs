using FluentValidation;
using Nexit.Application.DTOs.Proveedores;
using Nexit.Core.Interfaces;

namespace Nexit.Application.Validators.Proveedores;

public class CreateProveedorValidator : AbstractValidator<CreateProveedorDto>
{
    public CreateProveedorValidator(IProveedorRepository repository)
    {
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(255);
        RuleFor(x => x.PaisId).NotEmpty();
        RuleFor(x => x.CategoriaId).NotEmpty();
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email));
        RuleFor(x => x.Email).MustAsync(async (email, ct) => !await repository.ExistsByEmailAsync(email!, null, ct)).WithMessage("El email ya está registrado").When(x => !string.IsNullOrWhiteSpace(x.Email));
        RuleFor(x => x.Score).InclusiveBetween(1, 5).When(x => x.Score.HasValue);
        RuleForEach(x => x.Telefonos).ChildRules(phone => phone.RuleFor(x => x.Telefono).NotEmpty().MaximumLength(50));
    }
}
