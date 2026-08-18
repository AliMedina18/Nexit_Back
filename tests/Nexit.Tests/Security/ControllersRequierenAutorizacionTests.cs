using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nexit.API.Controllers;

namespace Nexit.Tests.Security;

/// <summary>
/// "Seguro por defecto" (ver docs/08-tipos-de-pruebas.md): a diferencia de las pruebas de
/// autorización de <see cref="Nexit.Tests.Integration.AuthorizationIntegrationTests"/> (que verifican
/// endpoints ESPECÍFICOS que alguien ya pensó en probar), estas recorren por reflexión TODOS los
/// controladores del ensamblado de la API -- así que si mañana alguien agrega un controlador nuevo y
/// se olvida de protegerlo, esta prueba falla automáticamente sin que nadie tenga que acordarse de
/// escribir un caso nuevo. Esto es lo más parecido a un análisis estático de seguridad (SAST) que se
/// puede hacer con xUnit sin herramientas externas -- comprueba la configuración de autorización en
/// tiempo de compilación/reflexión, no haciendo peticiones HTTP.
/// </summary>
public class ControllersRequierenAutorizacionTests
{
    private static IEnumerable<Type> TodosLosControladores() =>
        typeof(BaseController).Assembly.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } && typeof(ControllerBase).IsAssignableFrom(t));

    public static IEnumerable<object[]> Controladores() => TodosLosControladores().Select(t => new object[] { t });

    [Theory]
    [MemberData(nameof(Controladores))]
    public void Ningun_controlador_puede_quedar_sin_Authorize_ni_en_la_clase_ni_heredado(Type controlador)
    {
        // GetCustomAttributes(inherit: true) SÍ recoge el [Authorize] de BaseController para las
        // clases que heredan de ahí (la inmensa mayoría) -- y para las que no heredan de BaseController
        // (como ProveedorAdjuntosController, que extiende ControllerBase directo), exige que lo hayan
        // puesto ellas mismas. O sea: no importa CÓMO llegó la protección, solo que esté.
        var tieneAuthorize = controlador.GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true).Length > 0;
        Assert.True(tieneAuthorize, $"{controlador.Name} no tiene [Authorize] (ni propio ni heredado) -- quedaría abierto a cualquiera sin autenticar.");
    }

    [Theory]
    [MemberData(nameof(Controladores))]
    public void Ningun_controlador_ni_accion_tiene_AllowAnonymous(Type controlador)
    {
        // En este sistema NO hay ningún endpoint pensado para ser público (ni siquiera un healthcheck
        // vive aquí) -- así que [AllowAnonymous] en cualquier controlador o acción es, por diseño,
        // siempre una señal de alarma. Si alguna vez se necesita un endpoint público de verdad, hay que
        // actualizar esta prueba a propósito (no que se cuele sin darse cuenta).
        var enLaClase = controlador.GetCustomAttributes(typeof(AllowAnonymousAttribute), inherit: true).Length > 0;
        Assert.False(enLaClase, $"{controlador.Name} tiene [AllowAnonymous] a nivel de clase -- ¿es intencional?");

        foreach (var accion in controlador.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
        {
            var enElMetodo = accion.GetCustomAttributes(typeof(AllowAnonymousAttribute), inherit: true).Length > 0;
            Assert.False(enElMetodo, $"{controlador.Name}.{accion.Name} tiene [AllowAnonymous] -- ¿es intencional?");
        }
    }

    [Fact]
    public void El_ensamblado_de_la_API_realmente_tiene_controladores_para_recorrer()
    {
        // Salvaguarda contra un falso positivo silencioso: si por algún cambio futuro
        // TodosLosControladores() dejara de encontrar tipos (ej. un rename, un namespace distinto),
        // las dos pruebas de arriba "pasarían" con [Theory] vacío sin decir nada -- esto se asegura de
        // que eso mismo también falle de forma ruidosa.
        Assert.True(TodosLosControladores().Count() >= 8, "Se esperaban al menos los 8 controladores conocidos del API -- ¿se rompió la reflexión?");
    }
}
