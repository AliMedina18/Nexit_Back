using Moq;
using Nexit.Application.UseCases.Proveedores;
using Nexit.Core.Entities;
using Nexit.Core.Exceptions;
using Nexit.Core.Interfaces;

namespace Nexit.Tests;

/// <summary>
/// "Trabajando con este proveedor" (docs/19): marcado propio, muchos-a-muchos y público -- cada
/// usuario se marca a sí mismo (nadie asigna a nadie más), varias personas pueden estar marcadas en
/// el mismo proveedor a la vez, y alimenta la vista personal "mis proveedores".
/// </summary>
public class ProveedorColaboradoresTests
{
    [Fact]
    public async Task Marcar_throws_when_the_proveedor_does_not_exist()
    {
        var proveedores = new Mock<IProveedorRepository>();
        proveedores.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Proveedor?)null);

        await Assert.ThrowsAsync<EntityNotFoundException>(() => new MarcarColaboradorProveedorUseCase(Mock.Of<IProveedorColaboradorRepository>(), proveedores.Object, Mock.Of<IUnitOfWork>())
            .ExecuteAsync(Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None));
    }

    [Fact]
    public async Task Marcar_adds_the_collaborator_when_not_already_marked()
    {
        var colaboradores = new Mock<IProveedorColaboradorRepository>();
        var proveedores = new Mock<IProveedorRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var proveedorId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        proveedores.Setup(x => x.GetByIdAsync(proveedorId, It.IsAny<CancellationToken>())).ReturnsAsync(new Proveedor { Id = proveedorId });
        colaboradores.Setup(x => x.ExisteAsync(proveedorId, usuarioId, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        await new MarcarColaboradorProveedorUseCase(colaboradores.Object, proveedores.Object, unitOfWork.Object).ExecuteAsync(proveedorId, usuarioId, CancellationToken.None);

        colaboradores.Verify(x => x.AgregarAsync(proveedorId, usuarioId, It.IsAny<CancellationToken>()), Times.Once);
        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Marcar_is_a_no_op_when_already_marked()
    {
        // Varias personas pueden estar marcadas en el mismo proveedor, pero la MISMA persona
        // marcándose dos veces no debe duplicar la fila ni disparar un guardado de más.
        var colaboradores = new Mock<IProveedorColaboradorRepository>();
        var proveedores = new Mock<IProveedorRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var proveedorId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        proveedores.Setup(x => x.GetByIdAsync(proveedorId, It.IsAny<CancellationToken>())).ReturnsAsync(new Proveedor { Id = proveedorId });
        colaboradores.Setup(x => x.ExisteAsync(proveedorId, usuarioId, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        await new MarcarColaboradorProveedorUseCase(colaboradores.Object, proveedores.Object, unitOfWork.Object).ExecuteAsync(proveedorId, usuarioId, CancellationToken.None);

        colaboradores.Verify(x => x.AgregarAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Quitar_removes_the_mark_and_saves()
    {
        var colaboradores = new Mock<IProveedorColaboradorRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var proveedorId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();

        await new QuitarColaboradorProveedorUseCase(colaboradores.Object, unitOfWork.Object).ExecuteAsync(proveedorId, usuarioId, CancellationToken.None);

        colaboradores.Verify(x => x.QuitarAsync(proveedorId, usuarioId, It.IsAny<CancellationToken>()), Times.Once);
        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ListarMisProveedores_returns_empty_without_querying_all_providers_when_none_are_marked()
    {
        var colaboradores = new Mock<IProveedorColaboradorRepository>();
        var proveedores = new Mock<IProveedorRepository>();
        colaboradores.Setup(x => x.GetProveedorIdsPorUsuarioAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync([]);

        var result = await new ListarMisProveedoresUseCase(colaboradores.Object, proveedores.Object).ExecuteAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.Empty(result);
        proveedores.Verify(x => x.GetAllAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ListarMisProveedores_returns_only_the_providers_this_user_marked()
    {
        var colaboradores = new Mock<IProveedorColaboradorRepository>();
        var proveedores = new Mock<IProveedorRepository>();
        var usuarioId = Guid.NewGuid();
        var mioId = Guid.NewGuid();
        var otroId = Guid.NewGuid();
        colaboradores.Setup(x => x.GetProveedorIdsPorUsuarioAsync(usuarioId, It.IsAny<CancellationToken>())).ReturnsAsync([mioId]);
        proveedores.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(
            [new Proveedor { Id = mioId, Nombre = "El mío" }, new Proveedor { Id = otroId, Nombre = "El de otro" }]);

        var result = await new ListarMisProveedoresUseCase(colaboradores.Object, proveedores.Object).ExecuteAsync(usuarioId, CancellationToken.None);

        var proveedor = Assert.Single(result);
        Assert.Equal("El mío", proveedor.Nombre);
    }
}
