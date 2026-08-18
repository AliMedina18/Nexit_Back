using FluentValidation;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Nexit.Application.UseCases.Clientes;
using Nexit.Application.UseCases.Catalogos;
using Nexit.Application.UseCases.Proveedores;
using Nexit.Application.UseCases.Proyectos;
using Nexit.Application.UseCases.Informes;
using Nexit.Application.UseCases.Usuarios;
using Nexit.Application.UseCases.SolicitudesEliminacion;

namespace Nexit.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining(typeof(DependencyInjection));
        services.AddAutoMapper(typeof(DependencyInjection).Assembly);
        services.AddScoped<ICrearClienteUseCase, CrearClienteUseCase>();
        services.AddScoped<IActualizarClienteUseCase, ActualizarClienteUseCase>();
        services.AddScoped<IConsultarClientesUseCase, ConsultarClientesUseCase>();
        services.AddScoped<IEliminarClienteUseCase, EliminarClienteUseCase>();
        services.AddScoped<ICatalogosService, CatalogosService>();
        services.AddScoped<ICrearProveedorUseCase, CrearProveedorUseCase>();
        services.AddScoped<IActualizarProveedorUseCase, ActualizarProveedorUseCase>();
        services.AddScoped<IConsultarProveedoresUseCase, ConsultarProveedoresUseCase>();
        services.AddScoped<IEliminarProveedorUseCase, EliminarProveedorUseCase>();
        services.AddScoped<IProveedorAdjuntosUseCase, ProveedorAdjuntosUseCase>();
        services.AddScoped<ICrearProyectoUseCase, CrearProyectoUseCase>();
        services.AddScoped<IActualizarProyectoUseCase, ActualizarProyectoUseCase>();
        services.AddScoped<IConsultarProyectosUseCase, ConsultarProyectosUseCase>();
        services.AddScoped<IEliminarProyectoUseCase, EliminarProyectoUseCase>();
        services.AddScoped<IAgregarSeguimientoProyectoUseCase, AgregarSeguimientoProyectoUseCase>();
        services.AddScoped<IConsultarInformesUseCase, ConsultarInformesUseCase>();
        services.AddScoped<IGenerarInformeSnapshotUseCase, GenerarInformeSnapshotUseCase>();
        services.AddScoped<ICrearUsuarioUseCase, CrearUsuarioUseCase>();
        services.AddScoped<IActualizarUsuarioUseCase, ActualizarUsuarioUseCase>();
        services.AddScoped<IConsultarUsuariosUseCase, ConsultarUsuariosUseCase>();
        services.AddScoped<IEliminarUsuarioUseCase, EliminarUsuarioUseCase>();
        services.AddScoped<ISolicitarEliminacionUseCase, SolicitarEliminacionUseCase>();
        services.AddScoped<IAprobarComoGerenteUseCase, AprobarComoGerenteUseCase>();
        services.AddScoped<IRechazarComoGerenteUseCase, RechazarComoGerenteUseCase>();
        services.AddScoped<IAprobarComoAdminUseCase, AprobarComoAdminUseCase>();
        services.AddScoped<IRechazarComoAdminUseCase, RechazarComoAdminUseCase>();
        services.AddScoped<IConsultarSolicitudesEliminacionUseCase, ConsultarSolicitudesEliminacionUseCase>();
        return services;
    }
}
