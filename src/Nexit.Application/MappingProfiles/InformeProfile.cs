using AutoMapper;
using Nexit.Application.DTOs.Informes;
using Nexit.Core.Entities;

namespace Nexit.Application.MappingProfiles;

public class InformeProfile : Profile
{
    public InformeProfile()
    {
        CreateMap<InformeSnapshot, InformeSnapshotDto>();
        CreateMap<CrearInformeSnapshotDto, InformeSnapshot>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.CreatedAt, o => o.Ignore())
            .ForMember(d => d.UpdatedAt, o => o.Ignore())
            .ForMember(d => d.CreatedBy, o => o.Ignore())
            .ForMember(d => d.TotalProveedores, o => o.Ignore())
            .ForMember(d => d.TotalClientes, o => o.Ignore())
            .ForMember(d => d.TotalProyectos, o => o.Ignore())
            .ForMember(d => d.ProyectosSinProveedor, o => o.Ignore())
            .ForMember(d => d.PorEstado, o => o.Ignore())
            .ForMember(d => d.PorBrief, o => o.Ignore());
    }
}
