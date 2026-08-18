using FluentValidation;
using Nexit.Application.DTOs.SolicitudesEliminacion;
using Nexit.Core.Interfaces;

namespace Nexit.Application.Validators.SolicitudesEliminacion;

public class CrearSolicitudEliminacionValidator : AbstractValidator<CrearSolicitudEliminacionDto>
{
    public CrearSolicitudEliminacionValidator(IClienteRepository clientes, IProveedorRepository proveedores, IProyectoRepository proyectos)
    {
        RuleFor(x => x.TipoEntidad).Must(tipo => tipo is "cliente" or "proveedor" or "proyecto")
            .WithMessage("tipoEntidad debe ser 'cliente', 'proveedor' o 'proyecto'.");
        RuleFor(x => x.EntidadId).MustAsync(async (dto, entidadId, token) => dto.TipoEntidad switch
        {
            "cliente" => await clientes.GetByIdAsync(entidadId, token) is not null,
            "proveedor" => await proveedores.GetByIdAsync(entidadId, token) is not null,
            "proyecto" => await proyectos.GetByIdAsync(entidadId, token) is not null,
            _ => true // el tipo inválido ya lo reporta la regla de arriba
        }).WithMessage("La entidad indicada no existe.").When(x => x.TipoEntidad is "cliente" or "proveedor" or "proyecto");
    }
}
