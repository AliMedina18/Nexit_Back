using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Nexit.Application.UseCases.Usuarios;

namespace Nexit.Infrastructure.BackgroundServices;

/// <summary>
/// Corre una vez al día (configurable) y ejecuta IEliminarUsuariosInactivosUseCase: elimina, con
/// respaldo previo, a quien lleva 30+ días desactivado. Ver docs/17-eliminacion-automatica-usuarios.md.
///
/// Limitación conocida y aceptada por ahora: esto vive dentro del proceso de la API, así que solo
/// corre mientras la API está corriendo -- si el servidor está caído justo el día que a alguien le
/// tocaba, se ejecuta en el siguiente arranque/tick, no exactamente ese día. Para un panel de
/// administración interno esto es aceptable; si más adelante hace falta que corra aunque la API esté
/// caída, la alternativa es moverlo a un cron de Postgres (pg_cron, ya disponible en el proyecto de
/// Supabase) -- documentado como alternativa en docs/17, no implementado.
/// </summary>
public class EliminacionAutomaticaUsuariosInactivosService(
    IServiceScopeFactory scopeFactory,
    IConfiguration configuration,
    ILogger<EliminacionAutomaticaUsuariosInactivosService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var intervalo = TimeSpan.FromHours(configuration.GetValue("EliminacionAutomatica:IntervaloHoras", 24));

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var useCase = scope.ServiceProvider.GetRequiredService<IEliminarUsuariosInactivosUseCase>();
                var eliminados = await useCase.ExecuteAsync(stoppingToken);
                if (eliminados > 0) logger.LogInformation("Eliminación automática de usuarios inactivos: {Eliminados} cuenta(s) eliminada(s) (respaldadas en usuarios_eliminados).", eliminados);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // No debe tumbar la API si esta corrida falla (ej. la base de datos no responde un
                // momento) -- se reintenta en el siguiente tick.
                logger.LogError(ex, "Falló la corrida de eliminación automática de usuarios inactivos.");
            }

            try { await Task.Delay(intervalo, stoppingToken); } catch (OperationCanceledException) { }
        }
    }
}
