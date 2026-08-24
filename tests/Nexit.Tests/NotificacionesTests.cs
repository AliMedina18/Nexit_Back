using Moq;
using Nexit.Application.UseCases.Notificaciones;
using Nexit.Core.Entities;
using Nexit.Core.Exceptions;
using Nexit.Core.Interfaces;

namespace Nexit.Tests;

/// <summary>
/// Bandeja de notificaciones (docs/19): hoy solo la dispara el flujo de solicitudes de eliminación,
/// pero se prueba acá también la mecánica genérica de leer/marcar-leída que es independiente de
/// quién las generó.
/// </summary>
public class NotificacionesTests
{
    [Fact]
    public async Task ListarMisNotificaciones_returns_the_recipients_inbox()
    {
        var repository = new Mock<INotificacionRepository>();
        var usuarioId = Guid.NewGuid();
        repository.Setup(x => x.GetPorUsuarioAsync(usuarioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([new Notificacion { UsuarioDestinatarioId = usuarioId, Tipo = "solicitud_eliminacion_creada", Titulo = "t", Mensaje = "m" }]);

        var result = await new ListarMisNotificacionesUseCase(repository.Object).ExecuteAsync(usuarioId);

        Assert.Single(result);
        Assert.Equal("solicitud_eliminacion_creada", result[0].Tipo);
    }

    [Fact]
    public async Task MarcarLeida_rejects_someone_who_is_not_the_recipient()
    {
        var repository = new Mock<INotificacionRepository>();
        var notificacionId = Guid.NewGuid();
        repository.Setup(x => x.GetByIdAsync(notificacionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Notificacion { Id = notificacionId, UsuarioDestinatarioId = Guid.NewGuid() });

        await Assert.ThrowsAsync<ForbiddenOperationException>(() => new MarcarNotificacionLeidaUseCase(repository.Object, Mock.Of<IUnitOfWork>())
            .ExecuteAsync(notificacionId, Guid.NewGuid(), CancellationToken.None));
    }

    [Fact]
    public async Task MarcarLeida_throws_when_the_notification_does_not_exist()
    {
        var repository = new Mock<INotificacionRepository>();
        repository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Notificacion?)null);

        await Assert.ThrowsAsync<EntityNotFoundException>(() => new MarcarNotificacionLeidaUseCase(repository.Object, Mock.Of<IUnitOfWork>())
            .ExecuteAsync(Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None));
    }

    [Fact]
    public async Task MarcarLeida_sets_leida_and_the_timestamp_for_the_recipient()
    {
        var repository = new Mock<INotificacionRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var usuarioId = Guid.NewGuid();
        var notificacionId = Guid.NewGuid();
        var notificacion = new Notificacion { Id = notificacionId, UsuarioDestinatarioId = usuarioId, Leida = false };
        repository.Setup(x => x.GetByIdAsync(notificacionId, It.IsAny<CancellationToken>())).ReturnsAsync(notificacion);

        await new MarcarNotificacionLeidaUseCase(repository.Object, unitOfWork.Object).ExecuteAsync(notificacionId, usuarioId, CancellationToken.None);

        Assert.True(notificacion.Leida);
        Assert.NotNull(notificacion.FechaLeida);
        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task MarcarLeida_is_a_no_op_when_it_was_already_leida()
    {
        // Una notificación nunca se borra (es historial permanente, docs/19); marcarla leída dos
        // veces no debe pisar la FechaLeida original ni volver a guardar de más.
        var repository = new Mock<INotificacionRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var usuarioId = Guid.NewGuid();
        var notificacionId = Guid.NewGuid();
        var fechaOriginal = DateTime.UtcNow.AddDays(-1);
        var notificacion = new Notificacion { Id = notificacionId, UsuarioDestinatarioId = usuarioId, Leida = true, FechaLeida = fechaOriginal };
        repository.Setup(x => x.GetByIdAsync(notificacionId, It.IsAny<CancellationToken>())).ReturnsAsync(notificacion);

        await new MarcarNotificacionLeidaUseCase(repository.Object, unitOfWork.Object).ExecuteAsync(notificacionId, usuarioId, CancellationToken.None);

        Assert.Equal(fechaOriginal, notificacion.FechaLeida);
        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
