using FluentValidation;
using Nexit.Application.DTOs.Clientes;
using Nexit.Core.Interfaces;

namespace Nexit.Application.Validators.Clientes;

public class UpdateClienteValidator : AbstractValidator<UpdateClienteDto>
{
    public UpdateClienteValidator(IClienteRepository repository)
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(255);
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email));
        RuleFor(x => x.Email).MustAsync(async (dto, email, token) => !await repository.ExistsByEmailAsync(email!, dto.Id, token))
            .WithMessage("El email ya está registrado").When(x => !string.IsNullOrWhiteSpace(x.Email));
        RuleFor(x => x.Telefonos).NotEmpty().WithMessage("Al menos un teléfono es requerido");
        RuleForEach(x => x.Telefonos).ChildRules(phone => phone.RuleFor(x => x.Telefono).NotEmpty().MaximumLength(50));
    }
}
