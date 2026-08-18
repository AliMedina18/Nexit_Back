using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nexit.Core.Interfaces;
using Nexit.Infrastructure.Data;
using Nexit.Infrastructure.Repositories;
using Nexit.Infrastructure.UnitOfWork;

namespace Nexit.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection") ?? Environment.GetEnvironmentVariable("DATABASE_URL");
        if (string.IsNullOrWhiteSpace(connectionString)) throw new InvalidOperationException("Configure ConnectionStrings:DefaultConnection or DATABASE_URL.");
        services.AddDbContext<NexitDbContext>(options => options.UseNpgsql(connectionString, npgsql => npgsql.MigrationsAssembly(typeof(NexitDbContext).Assembly.FullName)).UseSnakeCaseNamingConvention());
        services.AddScoped<IUnitOfWork, UnitOfWork.UnitOfWork>();
        services.AddScoped<IClienteRepository, ClienteRepository>();
        services.AddScoped<IProveedorRepository, ProveedorRepository>();
        services.AddScoped<IProveedorAdjuntoRepository, ProveedorAdjuntoRepository>();
        services.AddScoped<IProyectoRepository, ProyectoRepository>();
        services.AddScoped<IInformesRepository, InformesRepository>();
        services.AddScoped<ICatalogosRepository, CatalogosRepository>();
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<ISolicitudEliminacionRepository, SolicitudEliminacionRepository>();
        return services;
    }
}
