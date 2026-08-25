using Microsoft.Extensions.Configuration;
using Nexit.Application.DTOs.Presencia;
using Nexit.Core.Interfaces;

namespace Nexit.Application.UseCases.Presencia;

public class RegistrarPresenciaUseCase(IUsuarioRepository repository, IUnitOfWork unitOfWork) : IRegistrarPresenciaUseCase
{
    public async Task ExecuteAsync(Guid usuarioId, CancellationToken cancellationToken = default)
    {
        var usuario = await repository.GetByIdAsync(usuarioId, cancellationToken);
        // Silencioso a propósito: si la cuenta no existe (o ya se eliminó justo entre que el frontend
        // cargó y este ping salió), no tiene sentido reventar la sesión del frontend por esto -- HU-12
        // es una vista informativa, no una acción crítica.
        if (usuario is null) return;

        usuario.UltimaActividad = DateTime.UtcNow;
        repository.Update(usuario);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

public class ConsultarPresenciaUseCase(IUsuarioRepository repository, IConfiguration configuration) : IConsultarPresenciaUseCase
{
    public async Task<IReadOnlyList<PresenciaResponseDto>> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var umbral = TimeSpan.FromMinutes(configuration.GetValue("Presencia:UmbralMinutos", 2));
        var ahora = DateTime.UtcNow;
        var usuarios = await repository.GetAllAsync(cancellationToken);

        return usuarios
            .Where(x => x.Activo)
            .Select(x => new PresenciaResponseDto
            {
                Id = x.Id,
                Nombre = x.Nombre,
                Apellido = x.Apellido,
                Rol = x.Rol,
                UltimaActividad = x.UltimaActividad,
                EnLinea = x.UltimaActividad.HasValue && (ahora - x.UltimaActividad.Value) <= umbral
            })
            .OrderByDescending(x => x.EnLinea)
            .ThenBy(x => x.Nombre)
            .ToList();
    }
}
