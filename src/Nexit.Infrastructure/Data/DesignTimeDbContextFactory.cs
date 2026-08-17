using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Nexit.Infrastructure.Data;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<NexitDbContext>
{
    public NexitDbContext CreateDbContext(string[] args)
    {
        var currentDirectory = Directory.GetCurrentDirectory();
        var apiDirectory = Directory.Exists(Path.Combine(currentDirectory, "src", "Nexit.API"))
            ? Path.Combine(currentDirectory, "src", "Nexit.API")
            : Path.GetFullPath(Path.Combine(currentDirectory, "..", "Nexit.API"));
        var configuration = new ConfigurationBuilder().SetBasePath(apiDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();
        var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL")
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Configure ConnectionStrings:DefaultConnection or DATABASE_URL.");
        return new NexitDbContext(new DbContextOptionsBuilder<NexitDbContext>().UseNpgsql(connectionString).UseSnakeCaseNamingConvention().Options);
    }
}
