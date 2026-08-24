using Nexit.Application.DTOs.Historial;
using Nexit.Core.Entities;
using Nexit.Core.Interfaces;
using Nexit.Core.Utils;

namespace Nexit.Application.UseCases.Historial;

public class ConsultarHistorialCambiosUseCase(IHistorialCambioRepository repository) : IConsultarHistorialCambiosUseCase
{
    public async Task<IReadOnlyList<HistorialCambioResponseDto>> ExecuteAsync(string tipoEntidad, Guid entidadId, CancellationToken cancellationToken = default) =>
        (await repository.GetPorEntidadAsync(tipoEntidad, entidadId, cancellationToken)).Select(x => new HistorialCambioResponseDto
        {
            Id = x.Id, UsuarioId = x.UsuarioId, UsuarioNombre = x.Usuario is null ? null : $"{x.Usuario.Nombre} {x.Usuario.Apellido}".Trim(),
            Accion = x.Accion, Campo = x.Campo, ValorAnterior = x.ValorAnterior, ValorNuevo = x.ValorNuevo, Fecha = x.Fecha
        }).ToList();
}

/// <summary>
/// Helper compartido por los casos de uso de Proyecto/Proveedor/Cliente para registrar el historial
/// de cambios (docs/19) sin repetir la construcción de <see cref="HistorialCambio"/> en cada uno.
/// No llama a SaveChanges -- el caso de uso que la usa ya hace su propio SaveChanges al final, así
/// el registro de historial queda en la MISMA transacción que el cambio que describe.
/// </summary>
public static class HistorialRegistrador
{
    public static async Task RegistrarCreacionAsync(IHistorialCambioRepository historial, string tipoEntidad, Guid entidadId, Guid usuarioId, CancellationToken ct) =>
        await historial.AddAsync(new HistorialCambio { TipoEntidad = tipoEntidad, EntidadId = entidadId, UsuarioId = usuarioId, Accion = "creacion" }, ct);

    public static async Task RegistrarEdicionAsync<T>(IHistorialCambioRepository historial, string tipoEntidad, Guid entidadId, Guid usuarioId, Dictionary<string, object?> antes, T despues, CancellationToken ct)
    {
        var cambios = CambioDetector.Comparar(antes, despues);
        if (cambios.Count == 0) return;
        await historial.AddRangeAsync(cambios.Select(c => new HistorialCambio
        {
            TipoEntidad = tipoEntidad, EntidadId = entidadId, UsuarioId = usuarioId, Accion = "edicion",
            Campo = c.Campo, ValorAnterior = c.ValorAnterior, ValorNuevo = c.ValorNuevo
        }), ct);
    }

    public static async Task RegistrarEliminacionAsync(IHistorialCambioRepository historial, string tipoEntidad, Guid entidadId, Guid usuarioId, CancellationToken ct) =>
        await historial.AddAsync(new HistorialCambio { TipoEntidad = tipoEntidad, EntidadId = entidadId, UsuarioId = usuarioId, Accion = "eliminacion" }, ct);
}
