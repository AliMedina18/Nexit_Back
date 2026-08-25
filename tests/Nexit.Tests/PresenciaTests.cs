using Microsoft.Extensions.Configuration;
using Moq;
using Nexit.Application.UseCases.Presencia;
using Nexit.Core.Constants;
using Nexit.Core.Entities;
using Nexit.Core.Interfaces;

namespace Nexit.Tests;

/// <summary>
/// Presencia en vivo (HU-12, docs/29): registrar el "ping" de una sesión activa, y calcular quién
/// está "en línea ahora mismo" según el umbral configurado.
/// </summary>
public class PresenciaTests
{
    [Fact]
    public async Task RegistrarPresencia_stamps_UltimaActividad_on_the_calling_user()
    {
        var repository = new Mock<IUsuarioRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var id = Guid.NewGuid();
        var usuario = new Usuario { Id = id, Nombre = "Ana", Rol = Roles.Miembro, UltimaActividad = null };
        repository.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(usuario);

        await new RegistrarPresenciaUseCase(repository.Object, unitOfWork.Object).ExecuteAsync(id);

        Assert.NotNull(usuario.UltimaActividad);
        Assert.True((DateTime.UtcNow - usuario.UltimaActividad!.Value) < TimeSpan.FromSeconds(5));
        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RegistrarPresencia_does_nothing_when_the_user_does_not_exist()
    {
        var repository = new Mock<IUsuarioRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        repository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Usuario?)null);

        await new RegistrarPresenciaUseCase(repository.Object, unitOfWork.Object).ExecuteAsync(Guid.NewGuid());

        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ConsultarPresencia_marks_a_user_online_when_the_last_ping_is_inside_the_threshold()
    {
        var repository = new Mock<IUsuarioRepository>();
        repository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(
            [new Usuario { Id = Guid.NewGuid(), Nombre = "Ana", Apellido = "Ruiz", Rol = Roles.Admin, Activo = true, UltimaActividad = DateTime.UtcNow.AddSeconds(-30) }]);
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["Presencia:UmbralMinutos"] = "2" }).Build();

        var result = await new ConsultarPresenciaUseCase(repository.Object, configuration).ExecuteAsync();

        Assert.Single(result);
        Assert.True(result[0].EnLinea);
    }

    [Fact]
    public async Task ConsultarPresencia_marks_a_user_offline_when_the_last_ping_is_past_the_threshold()
    {
        var repository = new Mock<IUsuarioRepository>();
        repository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(
            [new Usuario { Id = Guid.NewGuid(), Nombre = "Ana", Apellido = "Ruiz", Rol = Roles.Admin, Activo = true, UltimaActividad = DateTime.UtcNow.AddMinutes(-5) }]);
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["Presencia:UmbralMinutos"] = "2" }).Build();

        var result = await new ConsultarPresenciaUseCase(repository.Object, configuration).ExecuteAsync();

        Assert.Single(result);
        Assert.False(result[0].EnLinea);
    }

    [Fact]
    public async Task ConsultarPresencia_marks_a_user_offline_when_they_have_never_pinged()
    {
        var repository = new Mock<IUsuarioRepository>();
        repository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(
            [new Usuario { Id = Guid.NewGuid(), Nombre = "Ana", Apellido = "Ruiz", Rol = Roles.Miembro, Activo = true, UltimaActividad = null }]);

        var result = await new ConsultarPresenciaUseCase(repository.Object, new ConfigurationBuilder().Build()).ExecuteAsync();

        Assert.Single(result);
        Assert.False(result[0].EnLinea);
        Assert.Null(result[0].UltimaActividad);
    }

    [Fact]
    public async Task ConsultarPresencia_excludes_deactivated_accounts()
    {
        var repository = new Mock<IUsuarioRepository>();
        repository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(
        [
            new Usuario { Id = Guid.NewGuid(), Nombre = "Activa", Rol = Roles.Miembro, Activo = true, UltimaActividad = DateTime.UtcNow },
            new Usuario { Id = Guid.NewGuid(), Nombre = "Desactivada", Rol = Roles.Miembro, Activo = false, UltimaActividad = DateTime.UtcNow }
        ]);

        var result = await new ConsultarPresenciaUseCase(repository.Object, new ConfigurationBuilder().Build()).ExecuteAsync();

        Assert.Single(result);
        Assert.Equal("Activa", result[0].Nombre);
    }

    [Fact]
    public async Task RegistrarPresencia_updates_only_the_calling_user_not_anyone_else()
    {
        var repository = new Mock<IUsuarioRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var idQueHacePing = Guid.NewGuid();
        var otroUsuario = new Usuario { Id = Guid.NewGuid(), Nombre = "Otro", Rol = Roles.Miembro, UltimaActividad = null };
        var usuario = new Usuario { Id = idQueHacePing, Nombre = "Ana", Rol = Roles.Miembro, UltimaActividad = null };
        repository.Setup(x => x.GetByIdAsync(idQueHacePing, It.IsAny<CancellationToken>())).ReturnsAsync(usuario);
        Usuario? actualizado = null;
        repository.Setup(x => x.Update(It.IsAny<Usuario>())).Callback<Usuario>(u => actualizado = u);

        await new RegistrarPresenciaUseCase(repository.Object, unitOfWork.Object).ExecuteAsync(idQueHacePing);

        Assert.Equal(idQueHacePing, actualizado!.Id);
        Assert.Null(otroUsuario.UltimaActividad);
    }

    [Fact]
    public async Task ConsultarPresencia_treats_a_ping_just_inside_the_threshold_as_online()
    {
        var repository = new Mock<IUsuarioRepository>();
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["Presencia:UmbralMinutos"] = "2" }).Build();
        // Cerca del borde pero con un pequeño margen (500 ms) a favor de "en línea" -- NO usamos
        // exactamente "-2 minutos" porque esta prueba y el caso de uso llaman a DateTime.UtcNow por
        // separado, en momentos distintos: para cuando ConsultarPresenciaUseCase calcula "ahora", ya
        // pasaron unos milisegundos desde que esta prueba calculó "-2 minutos", así que el tiempo
        // transcurrido real siempre termina siendo un poquito MÁS de 2 minutos, y la prueba fallaba
        // de forma intermitente (flaky) incluso con la comparación <= correcta en el código.
        // Con este margen confirmamos el mismo comportamiento (justo antes del límite = en línea)
        // sin depender de que dos llamadas a UtcNow caigan exactamente en el mismo instante.
        repository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(
            [new Usuario { Id = Guid.NewGuid(), Nombre = "Ana", Rol = Roles.Miembro, Activo = true, UltimaActividad = DateTime.UtcNow.AddMinutes(-2).AddMilliseconds(500) }]);

        var result = await new ConsultarPresenciaUseCase(repository.Object, configuration).ExecuteAsync();

        Assert.True(result[0].EnLinea);
    }

    [Fact]
    public async Task ConsultarPresencia_preserves_each_users_role_in_the_response()
    {
        var repository = new Mock<IUsuarioRepository>();
        repository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(
        [
            new Usuario { Id = Guid.NewGuid(), Nombre = "Alicia", Rol = Roles.SuperAdmin, Activo = true, UltimaActividad = DateTime.UtcNow },
            new Usuario { Id = Guid.NewGuid(), Nombre = "Yuliana", Rol = Roles.Admin, Activo = true, UltimaActividad = DateTime.UtcNow },
            new Usuario { Id = Guid.NewGuid(), Nombre = "Beto", Rol = Roles.Manager, Activo = true, UltimaActividad = DateTime.UtcNow }
        ]);

        var result = await new ConsultarPresenciaUseCase(repository.Object, new ConfigurationBuilder().Build()).ExecuteAsync();

        Assert.Contains(result, x => x.Nombre == "Alicia" && x.Rol == Roles.SuperAdmin);
        Assert.Contains(result, x => x.Nombre == "Yuliana" && x.Rol == Roles.Admin);
        Assert.Contains(result, x => x.Nombre == "Beto" && x.Rol == Roles.Manager);
    }

    [Fact]
    public async Task ConsultarPresencia_orders_online_users_first()
    {
        var repository = new Mock<IUsuarioRepository>();
        repository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(
        [
            new Usuario { Id = Guid.NewGuid(), Nombre = "Zoe (desconectada)", Rol = Roles.Miembro, Activo = true, UltimaActividad = DateTime.UtcNow.AddHours(-1) },
            new Usuario { Id = Guid.NewGuid(), Nombre = "Beto (en línea)", Rol = Roles.Miembro, Activo = true, UltimaActividad = DateTime.UtcNow }
        ]);

        var result = await new ConsultarPresenciaUseCase(repository.Object, new ConfigurationBuilder().Build()).ExecuteAsync();

        Assert.Equal("Beto (en línea)", result[0].Nombre);
        Assert.True(result[0].EnLinea);
        Assert.False(result[1].EnLinea);
    }
}
