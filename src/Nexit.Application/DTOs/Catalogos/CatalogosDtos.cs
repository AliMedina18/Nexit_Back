namespace Nexit.Application.DTOs.Catalogos;

public record NombreDto(string Nombre);
public record PaisDto(Guid Id, string Nombre, string EtiquetaRegion);
public record RegionDto(Guid Id, Guid PaisId, string Nombre);
public record CiudadDto(Guid Id, Guid RegionId, string Nombre);
public record ItemCatalogoDto(Guid Id, string Nombre);
public record FaseProyectoDto(short Fase, string Nombre);
public record EstadoProyectoDto(Guid Id, string Nombre, short Fase, short Orden);
public record CrearPaisDto(string Nombre, string EtiquetaRegion);
public record CrearRegionDto(Guid PaisId, string Nombre);
public record CrearCiudadDto(Guid RegionId, string Nombre);
public record CrearEstadoProyectoDto(string Nombre, short Fase, short Orden);
