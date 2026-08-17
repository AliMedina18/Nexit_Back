using Nexit.Application.DTOs.Proyectos;
using Nexit.Core.Entities;
using Nexit.Core.Exceptions;
using Nexit.Core.Interfaces;

namespace Nexit.Application.UseCases.Proyectos;

public class CrearProyectoUseCase(IProyectoRepository repository, IClienteRepository clientes, IProveedorRepository proveedores, ICatalogosRepository catalogos, IUnitOfWork unitOfWork) : ICrearProyectoUseCase
{
    public async Task<ProyectoResponseDto> ExecuteAsync(CrearProyectoDto input, Guid usuarioId, CancellationToken ct = default)
    {
        await ProyectoRules.ValidarReferencias(input, clientes, proveedores, catalogos, ct);
        var proyecto = ProyectoMapper.ToEntity(input); proyecto.CreatedBy = usuarioId;
        await repository.AddAsync(proyecto, ct); await unitOfWork.SaveChangesAsync(ct);
        return ProyectoMapper.ToResponse(proyecto);
    }
}

public class ActualizarProyectoUseCase(IProyectoRepository repository, IClienteRepository clientes, IProveedorRepository proveedores, ICatalogosRepository catalogos, IUnitOfWork unitOfWork) : IActualizarProyectoUseCase
{
    public async Task<ProyectoResponseDto> ExecuteAsync(ActualizarProyectoDto input, CancellationToken ct = default)
    {
        var proyecto = await repository.GetByIdAsync(input.Id, ct) ?? throw new EntityNotFoundException("Proyecto", input.Id);
        await ProyectoRules.ValidarReferencias(input, clientes, proveedores, catalogos, ct);
        ProyectoMapper.Apply(input, proyecto); proyecto.UpdatedAt = DateTime.UtcNow;
        repository.Update(proyecto); await unitOfWork.SaveChangesAsync(ct);
        return ProyectoMapper.ToResponse(proyecto);
    }
}

public class ConsultarProyectosUseCase(IProyectoRepository repository) : IConsultarProyectosUseCase
{
    public async Task<IReadOnlyList<ProyectoResponseDto>> ListAsync(CancellationToken ct = default) => (await repository.GetAllAsync(ct)).Select(ProyectoMapper.ToResponse).ToList();
    public async Task<ProyectoResponseDto> GetByIdAsync(Guid id, CancellationToken ct = default) => ProyectoMapper.ToResponse(await repository.GetByIdAsync(id, ct) ?? throw new EntityNotFoundException("Proyecto", id));
}

public class EliminarProyectoUseCase(IProyectoRepository repository, IUnitOfWork unitOfWork) : IEliminarProyectoUseCase
{
    public async Task ExecuteAsync(Guid id, CancellationToken ct = default)
    {
        if (await repository.GetByIdAsync(id, ct) is null) throw new EntityNotFoundException("Proyecto", id);
        await repository.DeleteAsync(id, ct); await unitOfWork.SaveChangesAsync(ct);
    }
}

public class AgregarSeguimientoProyectoUseCase(IProyectoRepository repository, IUnitOfWork unitOfWork) : IAgregarSeguimientoProyectoUseCase
{
    public async Task<SeguimientoProyectoDto> ExecuteAsync(Guid proyectoId, CrearSeguimientoProyectoDto input, Guid usuarioId, CancellationToken ct = default)
    {
        var proyecto = await repository.GetByIdAsync(proyectoId, ct) ?? throw new EntityNotFoundException("Proyecto", proyectoId);
        var seguimiento = new ProyectoSeguimiento { ProyectoId = proyecto.Id, AutorId = usuarioId, Area = input.Area, Fecha = input.Fecha ?? DateTime.UtcNow, Nota = input.Nota };
        proyecto.Seguimiento.Add(seguimiento); await unitOfWork.SaveChangesAsync(ct);
        return ProyectoMapper.ToResponse(seguimiento);
    }
}

internal static class ProyectoMapper
{
    public static Proyecto ToEntity(CrearProyectoDto dto) { var entity = new Proyecto(); Apply(dto, entity); return entity; }
    public static void Apply(CrearProyectoDto dto, Proyecto entity)
    {
        entity.Nombre = dto.Nombre; entity.ClienteId = dto.ClienteId; entity.ContactoProyecto = dto.ContactoProyecto; entity.TipoProyecto = dto.TipoProyecto; entity.Prioridad = dto.Prioridad; entity.Ciudad = dto.Ciudad; entity.SedeNext = dto.SedeNext; entity.FechaSolicitud = dto.FechaSolicitud; entity.FechaEvento = dto.FechaEvento; entity.EstadoId = dto.EstadoId; entity.PorcentajeAvance = dto.PorcentajeAvance; entity.EstadoBrief = dto.EstadoBrief; entity.PropuestaEstado = dto.PropuestaEstado; entity.NumeroFactura = dto.NumeroFactura; entity.Pagado = dto.Pagado; entity.FechaPago = dto.FechaPago; entity.Notas = dto.Notas;
        entity.Equipo.Clear(); foreach (var miembro in dto.Equipo) entity.Equipo.Add(new ProyectoEquipo { Id = miembro.Id ?? Guid.NewGuid(), ProyectoId = entity.Id, Rol = miembro.Rol, Nombre = miembro.Nombre });
        entity.Proveedores.Clear(); foreach (var proveedorId in dto.ProveedorIds.Distinct()) entity.Proveedores.Add(new ProyectoProveedor { ProyectoId = entity.Id, ProveedorId = proveedorId });
    }
    public static ProyectoResponseDto ToResponse(Proyecto entity) => new()
    {
        Id = entity.Id, Nombre = entity.Nombre, ClienteId = entity.ClienteId, ContactoProyecto = entity.ContactoProyecto, TipoProyecto = entity.TipoProyecto, Prioridad = entity.Prioridad, Ciudad = entity.Ciudad, SedeNext = entity.SedeNext, FechaSolicitud = entity.FechaSolicitud, FechaEvento = entity.FechaEvento, EstadoId = entity.EstadoId, PorcentajeAvance = entity.PorcentajeAvance, EstadoBrief = entity.EstadoBrief, PropuestaEstado = entity.PropuestaEstado, NumeroFactura = entity.NumeroFactura, Pagado = entity.Pagado, FechaPago = entity.FechaPago, Notas = entity.Notas, CreatedAt = entity.CreatedAt, UpdatedAt = entity.UpdatedAt,
        Equipo = entity.Equipo.Select(x => new ProyectoEquipoDto { Id = x.Id, Rol = x.Rol, Nombre = x.Nombre }).ToList(), ProveedorIds = entity.Proveedores.Select(x => x.ProveedorId).ToList()
    };
    public static SeguimientoProyectoDto ToResponse(ProyectoSeguimiento entity) => new() { Id = entity.Id, AutorId = entity.AutorId, Area = entity.Area, Fecha = entity.Fecha, Nota = entity.Nota, CreatedAt = entity.CreatedAt };
}

internal static class ProyectoRules
{
    public static async Task ValidarReferencias(CrearProyectoDto dto, IClienteRepository clientes, IProveedorRepository proveedores, ICatalogosRepository catalogos, CancellationToken ct)
    {
        if (dto.ClienteId.HasValue && await clientes.GetByIdAsync(dto.ClienteId.Value, ct) is null) throw new BusinessRuleException("El cliente indicado no existe.");
        if (await catalogos.GetEstadoAsync(dto.EstadoId, ct) is null) throw new BusinessRuleException("El estado indicado no existe.");
        foreach (var proveedorId in dto.ProveedorIds.Distinct())
            if (await proveedores.GetByIdAsync(proveedorId, ct) is null) throw new BusinessRuleException("Uno de los proveedores indicados no existe.");
    }
}
