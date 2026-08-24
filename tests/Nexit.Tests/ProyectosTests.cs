using Moq;
using Nexit.Application.DTOs.Proyectos;
using Nexit.Application.UseCases.Proyectos;
using Nexit.Core.Constants;
using Nexit.Core.Entities;
using Nexit.Core.Exceptions;
using Nexit.Core.Interfaces;

namespace Nexit.Tests;

public class ProyectosTests
{
    [Fact]
    public async Task CrearProyecto_persists_project_with_team_and_providers()
    {
        var proyectos = new Mock<IProyectoRepository>();
        var clientes = new Mock<IClienteRepository>();
        var proveedores = new Mock<IProveedorRepository>();
        var catalogos = new Mock<ICatalogosRepository>();
        var clienteId = Guid.NewGuid();
        var estadoId = Guid.NewGuid();
        var proveedorId = Guid.NewGuid();
        clientes.Setup(x => x.GetByIdAsync(clienteId, It.IsAny<CancellationToken>())).ReturnsAsync(new Cliente { Id = clienteId });
        catalogos.Setup(x => x.GetEstadoAsync(estadoId, It.IsAny<CancellationToken>())).ReturnsAsync(new EstadoProyecto { Id = estadoId });
        proveedores.Setup(x => x.GetByIdAsync(proveedorId, It.IsAny<CancellationToken>())).ReturnsAsync(new Proveedor { Id = proveedorId });

        var result = await new CrearProyectoUseCase(proyectos.Object, clientes.Object, proveedores.Object, catalogos.Object, Mock.Of<IHistorialCambioRepository>(), Mock.Of<IUnitOfWork>()).ExecuteAsync(
            new CrearProyectoDto { Nombre = "Lanzamiento", ClienteId = clienteId, EstadoId = estadoId, Equipo = [new ProyectoEquipoDto { Nombre = "Ana", Rol = "Ejecutivo" }], ProveedorIds = [proveedorId] }, Guid.NewGuid(), Roles.Admin);

        Assert.Equal("Lanzamiento", result.Nombre);
        proyectos.Verify(x => x.AddAsync(It.Is<Proyecto>(p => p.Equipo.Count == 1 && p.Proveedores.Count == 1), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CrearProyecto_rejects_unknown_state()
    {
        var estadoId = Guid.NewGuid();
        var catalogos = new Mock<ICatalogosRepository>();
        catalogos.Setup(x => x.GetEstadoAsync(estadoId, It.IsAny<CancellationToken>())).ReturnsAsync((EstadoProyecto?)null);

        await Assert.ThrowsAsync<BusinessRuleException>(() => new CrearProyectoUseCase(Mock.Of<IProyectoRepository>(), Mock.Of<IClienteRepository>(), Mock.Of<IProveedorRepository>(), catalogos.Object, Mock.Of<IHistorialCambioRepository>(), Mock.Of<IUnitOfWork>())
            .ExecuteAsync(new CrearProyectoDto { Nombre = "Lanzamiento", EstadoId = estadoId }, Guid.NewGuid(), Roles.Admin));
    }

    [Fact]
    public async Task AgregarSeguimiento_assigns_author_and_saves()
    {
        var proyectoId = Guid.NewGuid();
        var autorId = Guid.NewGuid();
        var proyectos = new Mock<IProyectoRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        proyectos.Setup(x => x.GetByIdAsync(proyectoId, It.IsAny<CancellationToken>())).ReturnsAsync(new Proyecto { Id = proyectoId });

        var result = await new AgregarSeguimientoProyectoUseCase(proyectos.Object, unitOfWork.Object).ExecuteAsync(proyectoId, new CrearSeguimientoProyectoDto { Nota = "Proveedor confirmado" }, autorId);

        Assert.Equal(autorId, result.AutorId);
        Assert.Equal("General", result.Area);
        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ActualizarProyecto_registers_who_made_the_edit()
    {
        var proyectos = new Mock<IProyectoRepository>();
        var catalogos = new Mock<ICatalogosRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var proyectoId = Guid.NewGuid();
        var editorId = Guid.NewGuid();
        var estadoId = Guid.NewGuid();
        var proyecto = new Proyecto { Id = proyectoId, Nombre = "Lanzamiento" };
        proyectos.Setup(x => x.GetByIdAsync(proyectoId, It.IsAny<CancellationToken>())).ReturnsAsync(proyecto);
        catalogos.Setup(x => x.GetEstadoAsync(estadoId, It.IsAny<CancellationToken>())).ReturnsAsync(new EstadoProyecto { Id = estadoId });

        await new ActualizarProyectoUseCase(proyectos.Object, Mock.Of<IClienteRepository>(), Mock.Of<IProveedorRepository>(), catalogos.Object, Mock.Of<IHistorialCambioRepository>(), unitOfWork.Object)
            .ExecuteAsync(new ActualizarProyectoDto { Id = proyectoId, Nombre = "Lanzamiento Renovado", EstadoId = estadoId }, editorId, Roles.Admin);

        Assert.Equal(editorId, proyecto.UpdatedBy);
        Assert.NotNull(proyecto.UpdatedAt);
    }

    [Fact]
    public async Task CrearProyecto_by_a_manager_auto_assigns_them_as_the_owning_gerente()
    {
        var catalogos = new Mock<ICatalogosRepository>();
        var estadoId = Guid.NewGuid();
        catalogos.Setup(x => x.GetEstadoAsync(estadoId, It.IsAny<CancellationToken>())).ReturnsAsync(new EstadoProyecto { Id = estadoId });
        var gerenteId = Guid.NewGuid();

        var result = await new CrearProyectoUseCase(Mock.Of<IProyectoRepository>(), Mock.Of<IClienteRepository>(), Mock.Of<IProveedorRepository>(), catalogos.Object, Mock.Of<IHistorialCambioRepository>(), Mock.Of<IUnitOfWork>())
            .ExecuteAsync(new CrearProyectoDto { Nombre = "Lanzamiento", EstadoId = estadoId }, gerenteId, Roles.Manager);

        Assert.Equal(gerenteId, result.GerenteId);
    }

    [Fact]
    public async Task CrearProyecto_by_a_miembro_leaves_it_without_an_owning_gerente()
    {
        var catalogos = new Mock<ICatalogosRepository>();
        var estadoId = Guid.NewGuid();
        catalogos.Setup(x => x.GetEstadoAsync(estadoId, It.IsAny<CancellationToken>())).ReturnsAsync(new EstadoProyecto { Id = estadoId });

        var result = await new CrearProyectoUseCase(Mock.Of<IProyectoRepository>(), Mock.Of<IClienteRepository>(), Mock.Of<IProveedorRepository>(), catalogos.Object, Mock.Of<IHistorialCambioRepository>(), Mock.Of<IUnitOfWork>())
            .ExecuteAsync(new CrearProyectoDto { Nombre = "Lanzamiento", EstadoId = estadoId }, Guid.NewGuid(), Roles.Miembro);

        Assert.Null(result.GerenteId);
    }

    [Fact]
    public async Task CrearProyecto_by_an_admin_honors_the_explicit_gerente_assignment()
    {
        var catalogos = new Mock<ICatalogosRepository>();
        var estadoId = Guid.NewGuid();
        catalogos.Setup(x => x.GetEstadoAsync(estadoId, It.IsAny<CancellationToken>())).ReturnsAsync(new EstadoProyecto { Id = estadoId });
        var elegidoGerenteId = Guid.NewGuid();

        var result = await new CrearProyectoUseCase(Mock.Of<IProyectoRepository>(), Mock.Of<IClienteRepository>(), Mock.Of<IProveedorRepository>(), catalogos.Object, Mock.Of<IHistorialCambioRepository>(), Mock.Of<IUnitOfWork>())
            .ExecuteAsync(new CrearProyectoDto { Nombre = "Lanzamiento", EstadoId = estadoId, GerenteId = elegidoGerenteId }, Guid.NewGuid(), Roles.Admin);

        Assert.Equal(elegidoGerenteId, result.GerenteId);
    }

    [Fact]
    public async Task ActualizarProyecto_rejects_a_manager_reassigning_the_owning_gerente()
    {
        var proyectos = new Mock<IProyectoRepository>();
        var catalogos = new Mock<ICatalogosRepository>();
        var proyectoId = Guid.NewGuid();
        var estadoId = Guid.NewGuid();
        var proyecto = new Proyecto { Id = proyectoId, Nombre = "Lanzamiento", GerenteId = Guid.NewGuid() };
        proyectos.Setup(x => x.GetByIdAsync(proyectoId, It.IsAny<CancellationToken>())).ReturnsAsync(proyecto);
        catalogos.Setup(x => x.GetEstadoAsync(estadoId, It.IsAny<CancellationToken>())).ReturnsAsync(new EstadoProyecto { Id = estadoId });

        await Assert.ThrowsAsync<ForbiddenOperationException>(() => new ActualizarProyectoUseCase(proyectos.Object, Mock.Of<IClienteRepository>(), Mock.Of<IProveedorRepository>(), catalogos.Object, Mock.Of<IHistorialCambioRepository>(), Mock.Of<IUnitOfWork>())
            .ExecuteAsync(new ActualizarProyectoDto { Id = proyectoId, Nombre = "Lanzamiento", EstadoId = estadoId, GerenteId = Guid.NewGuid() }, Guid.NewGuid(), Roles.Manager));
    }

    [Fact]
    public async Task ActualizarProyecto_allows_an_admin_to_reassign_the_owning_gerente()
    {
        var proyectos = new Mock<IProyectoRepository>();
        var catalogos = new Mock<ICatalogosRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var proyectoId = Guid.NewGuid();
        var estadoId = Guid.NewGuid();
        var nuevoGerenteId = Guid.NewGuid();
        var proyecto = new Proyecto { Id = proyectoId, Nombre = "Lanzamiento", GerenteId = Guid.NewGuid() };
        proyectos.Setup(x => x.GetByIdAsync(proyectoId, It.IsAny<CancellationToken>())).ReturnsAsync(proyecto);
        catalogos.Setup(x => x.GetEstadoAsync(estadoId, It.IsAny<CancellationToken>())).ReturnsAsync(new EstadoProyecto { Id = estadoId });

        var result = await new ActualizarProyectoUseCase(proyectos.Object, Mock.Of<IClienteRepository>(), Mock.Of<IProveedorRepository>(), catalogos.Object, Mock.Of<IHistorialCambioRepository>(), unitOfWork.Object)
            .ExecuteAsync(new ActualizarProyectoDto { Id = proyectoId, Nombre = "Lanzamiento", EstadoId = estadoId, GerenteId = nuevoGerenteId }, Guid.NewGuid(), Roles.Admin);

        Assert.Equal(nuevoGerenteId, result.GerenteId);
    }
}
