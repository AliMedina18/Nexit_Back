using AutoMapper;
using Nexit.Application.DTOs.Proyectos;
using Nexit.Core.Entities;

namespace Nexit.Application.MappingProfiles;

public class ProyectoProfile : Profile
{
    public ProyectoProfile()
    {
        CreateMap<Proyecto, ProyectoResponseDto>()
            .ForMember(d => d.Equipo, o => o.MapFrom(s => s.Equipo));

        CreateMap<CrearProyectoDto, Proyecto>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.CreatedAt, o => o.Ignore())
            .ForMember(d => d.UpdatedAt, o => o.Ignore())
            .ForMember(d => d.CreatedBy, o => o.Ignore())
            .ForMember(d => d.Cliente, o => o.Ignore())
            .ForMember(d => d.Equipo, o => o.Ignore())
            .ForMember(d => d.Proveedores, o => o.Ignore())
            .ForMember(d => d.Seguimiento, o => o.Ignore());

        CreateMap<ActualizarProyectoDto, Proyecto>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.CreatedAt, o => o.Ignore())
            .ForMember(d => d.CreatedBy, o => o.Ignore())
            .ForMember(d => d.Cliente, o => o.Ignore())
            .ForMember(d => d.Equipo, o => o.Ignore())
            .ForMember(d => d.Proveedores, o => o.Ignore())
            .ForMember(d => d.Seguimiento, o => o.Ignore());

        CreateMap<ProyectoEquipo, ProyectoEquipoDto>().ReverseMap();

        CreateMap<CrearSeguimientoProyectoDto, ProyectoSeguimiento>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.CreatedAt, o => o.Ignore())
            .ForMember(d => d.UpdatedAt, o => o.Ignore())
            .ForMember(d => d.CreatedBy, o => o.Ignore())
            .ForMember(d => d.ProyectoId, o => o.Ignore())
            .ForMember(d => d.AutorId, o => o.Ignore())
            .ForMember(d => d.Proyecto, o => o.Ignore())
            .ForMember(d => d.Autor, o => o.Ignore());
    }
}
