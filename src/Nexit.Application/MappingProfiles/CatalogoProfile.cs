using AutoMapper;
using Nexit.Application.DTOs.Catalogos;
using Nexit.Core.Entities;

namespace Nexit.Application.MappingProfiles;

public class CatalogoProfile : Profile
{
    public CatalogoProfile()
    {
        CreateMap<Pais, PaisDto>();
        CreateMap<Region, RegionDto>();
        CreateMap<Ciudad, CiudadDto>();
        CreateMap<FaseProyecto, FaseProyectoDto>();
        CreateMap<EstadoProyecto, EstadoProyectoDto>();
        CreateMap<Servicio, ItemCatalogoDto>();
    }
}
