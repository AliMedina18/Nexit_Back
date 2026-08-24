using Microsoft.Extensions.Configuration;
using Moq;
using Nexit.Application.DTOs.Usuarios;
using Nexit.Application.UseCases.Usuarios;
using Nexit.Core.Constants;
using Nexit.Core.Entities;
using Nexit.Core.Exceptions;
using Nexit.Core.Interfaces;

namespace Nexit.Tests;

/// <summary>
/// Gestión de usuarios (exclusiva del super administrador, ver docs/06-modelo-permisos-roles.md).
/// El foco de estas pruebas está en las protecciones de auto-bloqueo: nadie debe poder desactivarse
/// a sí mismo, quitarse el rol de super_admin, ni eliminar su propia cuenta.
/// </summary>
public class UsuariosTests
{
    [Fact]
    public async Task CrearUsuario_persists_the_supabase_auth_id_as_the_profile_id()
    {
        var repository = new Mock<IUsuarioRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        Usuario? saved = null;
        repository.Setup(x => x.AddAsync(It.IsAny<Usuario>(), It.IsAny<CancellationToken>())).Callback<Usuario, CancellationToken>((u, _) => saved = u).Returns(Task.CompletedTask);
        var authId = Guid.NewGuid();

        var result = await new CrearUsuarioUseCase(repository.Object, unitOfWork.Object).ExecuteAsync(
            new CreateUsuarioDto { Id = authId, Nombre = "Ana", Apellido = "Ruiz", Email = "ana@next.com", Rol = Roles.Miembro });

        Assert.Equal(authId, result.Id);
        Assert.NotNull(saved);
        Assert.Equal(authId, saved!.Id);
        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ActualizarUsuario_throws_when_user_does_not_exist()
    {
        var repository = new Mock<IUsuarioRepository>();
        repository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Usuario?)null);
        await Assert.ThrowsAsync<EntityNotFoundException>(() => new ActualizarUsuarioUseCase(repository.Object, Mock.Of<IUnitOfWork>())
            .ExecuteAsync(Guid.NewGuid(), new UpdateUsuarioDto { Rol = Roles.Admin }, Guid.NewGuid()));
    }

    [Fact]
    public async Task ActualizarUsuario_allows_a_super_admin_to_edit_someone_else()
    {
        var repository = new Mock<IUsuarioRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var id = Guid.NewGuid();
        var usuario = new Usuario { Id = id, Nombre = "Ana", Apellido = "Ruiz", Email = "ana@next.com", Rol = Roles.Miembro, Activo = true };
        repository.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(usuario);

        var result = await new ActualizarUsuarioUseCase(repository.Object, unitOfWork.Object)
            .ExecuteAsync(id, new UpdateUsuarioDto { Nombre = "Ana", Apellido = "Ruiz", Rol = Roles.Manager, Activo = true }, Guid.NewGuid());

        Assert.Equal(Roles.Manager, result.Rol);
        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ActualizarUsuario_rejects_deactivating_your_own_account()
    {
        var repository = new Mock<IUsuarioRepository>();
        var id = Guid.NewGuid();
        repository.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(new Usuario { Id = id, Rol = Roles.SuperAdmin, Activo = true });

        await Assert.ThrowsAsync<ForbiddenOperationException>(() => new ActualizarUsuarioUseCase(repository.Object, Mock.Of<IUnitOfWork>())
            .ExecuteAsync(id, new UpdateUsuarioDto { Rol = Roles.SuperAdmin, Activo = false }, id));
    }

    [Fact]
    public async Task ActualizarUsuario_rejects_removing_your_own_super_admin_role()
    {
        var repository = new Mock<IUsuarioRepository>();
        var id = Guid.NewGuid();
        repository.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(new Usuario { Id = id, Rol = Roles.SuperAdmin, Activo = true });

        await Assert.ThrowsAsync<ForbiddenOperationException>(() => new ActualizarUsuarioUseCase(repository.Object, Mock.Of<IUnitOfWork>())
            .ExecuteAsync(id, new UpdateUsuarioDto { Rol = Roles.Admin, Activo = true }, id));
    }

    [Fact]
    public async Task ActualizarUsuario_allows_a_super_admin_to_edit_their_own_non_sensitive_fields()
    {
        var repository = new Mock<IUsuarioRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var id = Guid.NewGuid();
        repository.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(new Usuario { Id = id, Rol = Roles.SuperAdmin, Activo = true });

        var result = await new ActualizarUsuarioUseCase(repository.Object, unitOfWork.Object)
            .ExecuteAsync(id, new UpdateUsuarioDto { Nombre = "Alicia", Apellido = "Medina", Rol = Roles.SuperAdmin, Activo = true }, id);

        Assert.Equal("Alicia", result.Nombre);
    }

    [Fact]
    public async Task EliminarUsuario_rejects_deleting_your_own_account()
    {
        var repository = new Mock<IUsuarioRepository>();
        var id = Guid.NewGuid();
        await Assert.ThrowsAsync<ForbiddenOperationException>(() => new EliminarUsuarioUseCase(repository.Object, Mock.Of<IUsuarioEliminadoRepository>(), Mock.Of<ISupabaseAuthAdminService>(), Mock.Of<IUnitOfWork>()).ExecuteAsync(id, id));
        repository.Verify(x => x.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task EliminarUsuario_archives_then_deletes_someone_else_and_removes_their_auth_account()
    {
        var repository = new Mock<IUsuarioRepository>();
        var archivoRepository = new Mock<IUsuarioEliminadoRepository>();
        var authAdmin = new Mock<ISupabaseAuthAdminService>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var id = Guid.NewGuid();
        var callerId = Guid.NewGuid();
        repository.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(new Usuario { Id = id, Nombre = "Ana", Apellido = "Ruiz", Email = "ana@agencianextmkt.com", Rol = Roles.Miembro });
        UsuarioEliminado? archivado = null;
        archivoRepository.Setup(x => x.AddAsync(It.IsAny<UsuarioEliminado>(), It.IsAny<CancellationToken>())).Callback<UsuarioEliminado, CancellationToken>((u, _) => archivado = u).Returns(Task.CompletedTask);

        await new EliminarUsuarioUseCase(repository.Object, archivoRepository.Object, authAdmin.Object, unitOfWork.Object).ExecuteAsync(id, callerId);

        Assert.NotNull(archivado);
        Assert.Equal(id, archivado!.UsuarioIdOriginal);
        Assert.Equal(callerId, archivado.EliminadoPorId);
        repository.Verify(x => x.DeleteAsync(id, It.IsAny<CancellationToken>()), Times.Once);
        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        authAdmin.Verify(x => x.EliminarCuentaAsync(id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task EliminarUsuario_throws_when_user_does_not_exist()
    {
        var repository = new Mock<IUsuarioRepository>();
        repository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Usuario?)null);
        await Assert.ThrowsAsync<EntityNotFoundException>(() => new EliminarUsuarioUseCase(repository.Object, Mock.Of<IUsuarioEliminadoRepository>(), Mock.Of<ISupabaseAuthAdminService>(), Mock.Of<IUnitOfWork>()).ExecuteAsync(Guid.NewGuid(), Guid.NewGuid()));
    }

    [Fact]
    public async Task ActualizarUsuario_stamps_FechaDesactivacion_when_deactivating()
    {
        var repository = new Mock<IUsuarioRepository>();
        var id = Guid.NewGuid();
        repository.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(new Usuario { Id = id, Rol = Roles.Miembro, Activo = true, FechaDesactivacion = null });

        var result = await new ActualizarUsuarioUseCase(repository.Object, Mock.Of<IUnitOfWork>())
            .ExecuteAsync(id, new UpdateUsuarioDto { Nombre = "Ana", Apellido = "Ruiz", Rol = Roles.Miembro, Activo = false }, Guid.NewGuid());

        Assert.False(result.Activo);
        Assert.NotNull(result.FechaDesactivacion);
    }

    [Fact]
    public async Task ActualizarUsuario_clears_FechaDesactivacion_when_reactivating()
    {
        var repository = new Mock<IUsuarioRepository>();
        var id = Guid.NewGuid();
        repository.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(new Usuario { Id = id, Rol = Roles.Miembro, Activo = false, FechaDesactivacion = DateTime.UtcNow.AddDays(-10) });

        var result = await new ActualizarUsuarioUseCase(repository.Object, Mock.Of<IUnitOfWork>())
            .ExecuteAsync(id, new UpdateUsuarioDto { Nombre = "Ana", Apellido = "Ruiz", Rol = Roles.Miembro, Activo = true }, Guid.NewGuid());

        Assert.True(result.Activo);
        Assert.Null(result.FechaDesactivacion);
    }

    [Fact]
    public async Task EliminarUsuariosInactivos_archives_and_deletes_only_those_past_the_configured_threshold()
    {
        var repository = new Mock<IUsuarioRepository>();
        var archivoRepository = new Mock<IUsuarioEliminadoRepository>();
        var authAdmin = new Mock<ISupabaseAuthAdminService>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["EliminacionAutomatica:DiasInactividad"] = "30" }).Build();
        var vencido1 = new Usuario { Id = Guid.NewGuid(), Nombre = "A", Apellido = "B", Email = "a@agencianextmkt.com", Rol = Roles.Miembro, Activo = false, FechaDesactivacion = DateTime.UtcNow.AddDays(-31) };
        var vencido2 = new Usuario { Id = Guid.NewGuid(), Nombre = "C", Apellido = "D", Email = "c@agencianextmkt.com", Rol = Roles.Miembro, Activo = false, FechaDesactivacion = DateTime.UtcNow.AddDays(-45) };
        // El repositorio real solo devuelve los que ya cumplieron el plazo -- este mock simula ese filtro.
        repository.Setup(x => x.GetInactivosDesdeAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>())).ReturnsAsync([vencido1, vencido2]);

        var eliminados = await new EliminarUsuariosInactivosUseCase(repository.Object, archivoRepository.Object, authAdmin.Object, unitOfWork.Object, configuration).ExecuteAsync();

        Assert.Equal(2, eliminados);
        archivoRepository.Verify(x => x.AddAsync(It.IsAny<UsuarioEliminado>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        repository.Verify(x => x.DeleteAsync(vencido1.Id, It.IsAny<CancellationToken>()), Times.Once);
        repository.Verify(x => x.DeleteAsync(vencido2.Id, It.IsAny<CancellationToken>()), Times.Once);
        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        authAdmin.Verify(x => x.EliminarCuentaAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task EliminarUsuariosInactivos_does_nothing_when_no_one_is_past_the_threshold()
    {
        var repository = new Mock<IUsuarioRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        repository.Setup(x => x.GetInactivosDesdeAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>())).ReturnsAsync([]);

        var eliminados = await new EliminarUsuariosInactivosUseCase(repository.Object, Mock.Of<IUsuarioEliminadoRepository>(), Mock.Of<ISupabaseAuthAdminService>(), unitOfWork.Object, new ConfigurationBuilder().Build()).ExecuteAsync();

        Assert.Equal(0, eliminados);
        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ConsultarUsuarios_returns_mapped_users()
    {
        var repository = new Mock<IUsuarioRepository>();
        repository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync([new Usuario { Nombre = "Ana", Rol = Roles.Miembro }]);
        var result = await new ConsultarUsuariosUseCase(repository.Object).ListAsync();
        Assert.Single(result);
        Assert.Equal("Ana", result[0].Nombre);
    }
}
