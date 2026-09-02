using Nexit.Application.DTOs.Auth;

namespace Nexit.Application.UseCases.Auth;

/// <summary>Ver ConsultarEstadoCuentaUseCase (implementación) y docs/30 para el diseño completo.</summary>
public interface IConsultarEstadoCuentaUseCase
{
    Task<EstadoCuentaResponseDto> ExecuteAsync(string email, CancellationToken cancellationToken = default);
}

/// <summary>Ver ConfirmarContrasenaConfiguradaUseCase (implementación) y docs/30.</summary>
public interface IConfirmarContrasenaConfiguradaUseCase
{
    Task ExecuteAsync(Guid userId, CancellationToken cancellationToken = default);
}
