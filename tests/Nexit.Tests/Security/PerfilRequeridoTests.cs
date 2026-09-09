using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Nexit.API.Controllers;
using Nexit.API.Filters;

namespace Nexit.Tests.Security;

/// <summary>
/// Mismo espíritu que <see cref="ControllersRequierenAutorizacionTests"/>, para la otra mitad del
/// "seguro por defecto": no basta con estar autenticado en Supabase Auth, hace falta además tener
/// perfil de negocio en <c>usuarios</c> (ver <see cref="PerfilRequeridoFilter"/>). El filtro es
/// global, así que lo peligroso no es olvidarse de ponerlo sino EXIMIR de más: cada
/// <c>[PermitirSinPerfil]</c> es una puerta abierta a alguien que todavía no es nadie dentro del
/// sistema. Por eso la lista de eximidos está fijada acá: agregar uno nuevo rompe esta prueba a
/// propósito, para que sea una decisión consciente y no algo que se cuele en un refactor.
/// </summary>
public class PerfilRequeridoTests
{
    /// <summary>
    /// Las únicas acciones que alguien SIN perfil puede llamar -- exactamente las que necesita
    /// para conseguir uno (ver docs/25: ver su invitación, aceptarla o rechazarla), más las de
    /// apoyo al login y el <c>me</c> con el que el frontend averigua si ya está registrado.
    /// </summary>
    private static readonly HashSet<string> EximidosEsperados =
    [
        $"{nameof(AuthController)}.{nameof(AuthController.EstadoCuenta)}",
        $"{nameof(AuthController)}.{nameof(AuthController.ConfirmarContrasena)}",
        $"{nameof(UsuariosController)}.{nameof(UsuariosController.GetMe)}",
        $"{nameof(InvitacionesController)}.{nameof(InvitacionesController.GetMia)}",
        $"{nameof(InvitacionesController)}.{nameof(InvitacionesController.Aceptar)}",
        $"{nameof(InvitacionesController)}.{nameof(InvitacionesController.Rechazar)}",
    ];

    private static IEnumerable<(Type Controlador, MethodInfo Accion)> TodasLasAcciones() =>
        typeof(BaseController).Assembly.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } && typeof(ControllerBase).IsAssignableFrom(t))
            .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(m => !m.IsSpecialName)
                .Select(m => (Controlador: t, Accion: m)));

    private static bool EstaEximida(Type controlador, MethodInfo accion) =>
        accion.GetCustomAttribute<PermitirSinPerfilAttribute>() is not null
        || controlador.GetCustomAttribute<PermitirSinPerfilAttribute>() is not null;

    [Fact]
    public void Solo_las_acciones_del_registro_pueden_llamarse_sin_perfil()
    {
        var eximidas = TodasLasAcciones()
            .Where(x => EstaEximida(x.Controlador, x.Accion))
            .Select(x => $"{x.Controlador.Name}.{x.Accion.Name}")
            .ToHashSet();

        var deMas = eximidas.Except(EximidosEsperados).ToList();
        var faltantes = EximidosEsperados.Except(eximidas).ToList();

        Assert.True(deMas.Count == 0,
            $"Estas acciones se pueden llamar SIN perfil y no deberían: {string.Join(", ", deMas)}. " +
            "Si es intencional, agrégalas a EximidosEsperados explicando por qué.");
        Assert.True(faltantes.Count == 0,
            $"Estas acciones deberían poder llamarse sin perfil y ya no pueden: {string.Join(", ", faltantes)}. " +
            "Sin ellas, alguien recién invitado no puede completar su registro.");
    }

    [Fact]
    public void El_resto_de_los_controladores_exige_perfil()
    {
        // Comprobación redundante a propósito, escrita "al revés": si alguien pusiera
        // [PermitirSinPerfil] a nivel de CLASE en un controlador de datos, la prueba de arriba lo
        // vería como muchas acciones de más, pero este mensaje señala directo al controlador.
        var controladoresEximidosEnteros = typeof(BaseController).Assembly.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } && typeof(ControllerBase).IsAssignableFrom(t))
            .Where(t => t.GetCustomAttribute<PermitirSinPerfilAttribute>() is not null)
            .Select(t => t.Name)
            .ToList();

        Assert.Equal([nameof(AuthController)], controladoresEximidosEnteros);
    }
}
