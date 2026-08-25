using Nexit.Application.DTOs.Presencia;

namespace Nexit.Application.UseCases.Presencia;

/// <summary>
/// El frontend llama esto periódicamente (cada 45-60 segundos, mientras la pestaña siga abierta y con
/// sesión activa -- ver docs/29) para dejar constancia de que alguien sigue usando el sistema. Nunca
/// lanza si el usuario no existe o ya no está activo -- un ping perdido no es un error, solo significa
/// que esa cuenta no se va a ver "en línea" en el próximo GET /api/presencia.
/// </summary>
public interface IRegistrarPresenciaUseCase
{
    Task ExecuteAsync(Guid usuarioId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Directorio de presencia para admin/super_admin (HU-12): todos los usuarios activos, cada uno con
/// si está "en línea ahora mismo" según el umbral configurado.
/// </summary>
public interface IConsultarPresenciaUseCase
{
    Task<IReadOnlyList<PresenciaResponseDto>> ExecuteAsync(CancellationToken cancellationToken = default);
}
