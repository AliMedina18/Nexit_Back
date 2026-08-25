using NetArchTest.Rules;
using Nexit.API.Controllers;
using Nexit.Application.UseCases.Usuarios;
using Nexit.Core.Entities;
using Nexit.Infrastructure.Data;

namespace Nexit.Tests;

/// <summary>
/// Pruebas de arquitectura (ver docs/08, seccion 5): no verifican comportamiento de negocio, verifican
/// que la separacion en capas de Clean Architecture (Core/Application/Infrastructure/API) se mantenga
/// con el tiempo -- recorren los ensamblados YA COMPILADOS por reflexion, no leen el codigo fuente.
///
/// Nacieron de un bug real (2026-08-25): al construir HU-12 (presencia en vivo), un caso de uso de
/// Nexit.Application referencio por accidente Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException
/// directamente -- rompiendo la regla de que esa capa no debe saber nada de la tecnologia de persistencia
/// concreta (eso es trabajo exclusivo de Nexit.Infrastructure). El error solo se descubrio al compilar,
/// a mano. Estas pruebas lo habrian detectado solas, en el mismo dotnet test de siempre, sin depender
/// de que alguien lo note revisando el codigo.
/// </summary>
public class ArchitectureTests
{
    private static readonly System.Reflection.Assembly CoreAssembly = typeof(Usuario).Assembly;
    private static readonly System.Reflection.Assembly ApplicationAssembly = typeof(CrearUsuarioUseCase).Assembly;
    private static readonly System.Reflection.Assembly InfrastructureAssembly = typeof(NexitDbContext).Assembly;
    private static readonly System.Reflection.Assembly ApiAssembly = typeof(UsuariosController).Assembly;

    [Fact]
    public void Core_no_depende_de_ninguna_otra_capa_del_proyecto()
    {
        // Nexit.Core (entidades, interfaces, constantes) es el centro de Clean Architecture: no debe
        // importar nada de Application/Infrastructure/API, para poder reutilizarse o probarse sin
        // arrastrar ninguna de esas otras capas.
        var result = Types.InAssembly(CoreAssembly)
            .ShouldNot().HaveDependencyOn("Nexit.Application")
            .And().HaveDependencyOn("Nexit.Infrastructure")
            .And().HaveDependencyOn("Nexit.API")
            .GetResult();

        Assert.True(result.IsSuccessful, "Nexit.Core no deberia depender de Application/Infrastructure/API -- revisa el using que se agrego.");
    }

    [Fact]
    public void Application_no_depende_de_Infrastructure_ni_de_API()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .ShouldNot().HaveDependencyOn("Nexit.Infrastructure")
            .And().HaveDependencyOn("Nexit.API")
            .GetResult();

        Assert.True(result.IsSuccessful, "Nexit.Application no deberia depender de Infrastructure ni de API -- revisa el using que se agrego.");
    }

    [Fact]
    public void Application_no_depende_directamente_de_Entity_Framework_Core()
    {
        // La regla exacta que este bug necesitaba (ver el resumen de la clase): Application define
        // abstracciones (IUsuarioRepository, IUnitOfWork), pero nunca debe importar
        // Microsoft.EntityFrameworkCore directamente -- solo Nexit.Infrastructure, donde vive la
        // implementacion real con EF Core.
        var result = Types.InAssembly(ApplicationAssembly)
            .ShouldNot().HaveDependencyOn("Microsoft.EntityFrameworkCore")
            .GetResult();

        Assert.True(result.IsSuccessful, "Nexit.Application no deberia depender de Microsoft.EntityFrameworkCore directamente -- ese es el bug real que motivo esta prueba.");
    }

    [Fact]
    public void Infrastructure_no_depende_de_API()
    {
        var result = Types.InAssembly(InfrastructureAssembly)
            .ShouldNot().HaveDependencyOn("Nexit.API")
            .GetResult();

        Assert.True(result.IsSuccessful, "Nexit.Infrastructure no deberia depender de Nexit.API -- revisa el using que se agrego.");
    }

    [Fact]
    public void Los_controladores_viven_solo_en_Nexit_API()
    {
        // Confirma que ninguna otra capa se salto el patron de casos de uso y definio su propio
        // controlador (o algo que herede de ControllerBase) fuera de la capa de presentacion.
        var result = Types.InAssembly(ApiAssembly)
            .That().Inherit(typeof(Microsoft.AspNetCore.Mvc.ControllerBase))
            .Should().ResideInNamespace("Nexit.API.Controllers")
            .GetResult();

        Assert.True(result.IsSuccessful, "Todo controlador (hereda de ControllerBase) deberia vivir en el namespace Nexit.API.Controllers.");
    }
}
