using AutoMapper;
using Nexit.Application.DTOs.Proveedores;
using Nexit.Core.Entities;

namespace Nexit.Application.MappingProfiles;

public class ProveedorProfile : Profile
{
    public ProveedorProfile()
    {
        CreateMap<Proveedor, ProveedorResponseDto>()
            .ForMember(d => d.Telefonos, o => o.MapFrom(s => s.Telefonos));

        CreateMap<CreateProveedorDto, Proveedor>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.CreatedAt, o => o.Ignore())
            .ForMember(d => d.UpdatedAt, o => o.Ignore())
            .ForMember(d => d.CreatedBy, o => o.Ignore())
            .ForMember(d => d.Telefonos, o => o.Ignore())
            .ForMember(d => d.Servicios, o => o.Ignore())
            .ForMember(d => d.Proyectos, o => o.Ignore());

        CreateMap<UpdateProveedorDto, Proveedor>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.CreatedAt, o => o.Ignore())
            .ForMember(d => d.CreatedBy, o => o.Ignore())
            .ForMember(d => d.Telefonos, o => o.Ignore())
            .ForMember(d => d.Servicios, o => o.Ignore())
            .ForMember(d => d.Proyectos, o => o.Ignore());

        CreateMap<ProveedorTelefono, ProveedorTelefonoDto>().ReverseMap();
    }
}
