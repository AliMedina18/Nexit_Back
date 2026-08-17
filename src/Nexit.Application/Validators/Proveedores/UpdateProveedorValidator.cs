using FluentValidation;
using Nexit.Application.DTOs.Proveedores;
using Nexit.Core.Interfaces;

namespace Nexit.Application.Validators.Proveedores;

public class UpdateProveedorValidator : AbstractValidator<UpdateProveedorDto>
{
    public UpdateProveedorValidator(IProveedorRepository repository)
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(255);
        RuleFor(x => x.PaisId).NotEmpty();
        RuleFor(x => x.CategoriaId).NotEmpty();
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email));
        RuleFor(x => x.Email).MustAsync(async (dto, email, ct) => !await repository.ExistsByEmailAsync(email!, dto.Id, ct)).WithMessage("El email ya está registrado").When(x => !string.IsNullOrWhiteSpace(x.Email));
        RuleFor(x => x.Score).InclusiveBetween(1, 5).When(x => x.Score.HasValue);
        RuleForEach(x => x.Telefonos).ChildRules(phone => phone.RuleFor(x => x.Telefono).NotEmpty().MaximumLength(50));
    }
}
