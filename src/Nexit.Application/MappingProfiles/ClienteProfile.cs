using AutoMapper;
using Nexit.Application.DTOs.Clientes;
using Nexit.Core.Entities;

namespace Nexit.Application.MappingProfiles;

public class ClienteProfile : Profile
{
    public ClienteProfile()
    {
        CreateMap<Cliente, ClienteResponseDto>()
            .ForMember(d => d.Telefonos, o => o.MapFrom(s => s.Telefonos));

        CreateMap<CreateClienteDto, Cliente>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.CreatedAt, o => o.Ignore())
            .ForMember(d => d.UpdatedAt, o => o.Ignore())
            .ForMember(d => d.CreatedBy, o => o.Ignore());

        CreateMap<UpdateClienteDto, Cliente>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.CreatedAt, o => o.Ignore())
            .ForMember(d => d.CreatedBy, o => o.Ignore());

        CreateMap<ClienteTelefono, ClienteTelefonoDto>().ReverseMap();
    }
}
