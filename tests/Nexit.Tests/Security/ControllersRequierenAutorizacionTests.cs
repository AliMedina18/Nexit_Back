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

    /// <summary>
    /// Única excepción deliberada a "nada es público" (ver docs/30): el login necesita saber, ANTES
    /// de que la persona escriba su contraseña, si su cuenta ya tiene una configurada -- y eso, por
    /// definición, no puede exigir sesión. La respuesta en sí no revela si el correo existe (ver
    /// ConsultarEstadoCuentaUseCase), y el endpoint tiene su propio límite de tasa mucho más estricto
    /// que el resto de la API ("auth-anon" en Program.cs) precisamente por ser el único público.
    /// Cualquier otro [AllowAnonymous] que aparezca en el ensamblado, en cualquier controlador o
    /// acción que no sea exactamente esta, sigue siendo motivo de alarma.
    /// </summary>
    private static readonly (Type Controlador, string Metodo) UnicaAccionPublicaPermitida =
        (typeof(AuthController), nameof(AuthController.EstadoCuenta));

    [Theory]
    [MemberData(nameof(Controladores))]
    public void Ningun_controlador_ni_accion_tiene_AllowAnonymous(Type controlador)
    {
        // En este sistema casi ningún endpoint está pensado para ser público -- así que
        // [AllowAnonymous] en cualquier controlador o acción es, por diseño, siempre una señal de
        // alarma, salvo la única excepción documentada arriba (UnicaAccionPublicaPermitida). Si
        // alguna vez se necesita OTRO endpoint público de verdad, hay que actualizar esta prueba a
        // propósito (no que se cuele sin darse cuenta).
        var enLaClase = controlador.GetCustomAttributes(typeof(AllowAnonymousAttribute), inherit: true).Length > 0;
        Assert.False(enLaClase, $"{controlador.Name} tiene [AllowAnonymous] a nivel de clase -- ¿es intencional?");

        foreach (var accion in controlador.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
        {
            var enElMetodo = accion.GetCustomAttributes(typeof(AllowAnonymousAttribute), inherit: true).Length > 0;
            if (!enElMetodo) continue;
            var esLaExcepcionPermitida = controlador == UnicaAccionPublicaPermitida.Controlador && accion.Name == UnicaAccionPublicaPermitida.Metodo;
            Assert.True(esLaExcepcionPermitida, $"{controlador.Name}.{accion.Name} tiene [AllowAnonymous] -- ¿es intencional? (la única excepción permitida hoy es {UnicaAccionPublicaPermitida.Controlador.Name}.{UnicaAccionPublicaPermitida.Metodo})");
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
