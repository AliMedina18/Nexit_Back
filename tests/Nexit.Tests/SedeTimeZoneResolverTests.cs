using Nexit.Core.Utils;

namespace Nexit.Tests;

/// <summary>
/// docs/18-calendario-zona-horaria-por-sede.md: a qué mes/año pertenece un proyecto en el calendario
/// se decide por la hora local de su sede, no por UTC. Estas pruebas cubren el caso que motivó el
/// cambio -- un evento tarde en la noche cerca de fin de mes no debe "saltar" al mes siguiente -- y
/// la resolución de sede por texto libre.
/// </summary>
public class SedeTimeZoneResolverTests
{
    [Theory]
    [InlineData("Bogotá", SedeTimeZoneResolver.ZonaBogota)]
    [InlineData("bogota", SedeTimeZoneResolver.ZonaBogota)]
    [InlineData("México", SedeTimeZoneResolver.ZonaMexico)]
    [InlineData("mexico", SedeTimeZoneResolver.ZonaMexico)]
    [InlineData("CDMX", SedeTimeZoneResolver.ZonaMexico)]
    [InlineData("Ciudad de México", SedeTimeZoneResolver.ZonaMexico)]
    [InlineData(null, SedeTimeZoneResolver.ZonaBogota)]
    [InlineData("", SedeTimeZoneResolver.ZonaBogota)]
    [InlineData("  ", SedeTimeZoneResolver.ZonaBogota)]
    [InlineData("Medellín", SedeTimeZoneResolver.ZonaBogota)]
    [InlineData("Una sede que no existe todavía", SedeTimeZoneResolver.ZonaBogota)]
    public void ResolverZonaHoraria_matches_known_sedes_and_defaults_to_Bogota(string? sedeNext, string zonaEsperada)
    {
        Assert.Equal(zonaEsperada, SedeTimeZoneResolver.ResolverZonaHoraria(sedeNext));
    }

    [Fact]
    public void ConvertirUtcALocal_keeps_a_late_night_Bogota_event_in_the_previous_calendar_day()
    {
        // 31 de enero, 11:30pm hora Bogotá (UTC-5) = 1 de febrero, 04:30 UTC. Si el calendario
        // calculara el mes directo sobre el valor UTC, este proyecto aparecería en febrero.
        var fechaUtc = new DateTime(2026, 2, 1, 4, 30, 0, DateTimeKind.Utc);

        var local = SedeTimeZoneResolver.ConvertirUtcALocal(fechaUtc, "Bogotá");

        Assert.Equal(new DateTime(2026, 1, 31, 23, 30, 0), local);
        Assert.Equal(1, local.Month);
        Assert.Equal(2026, local.Year);
    }

    [Fact]
    public void ConvertirUtcALocal_uses_a_different_offset_for_a_Mexico_sede_on_the_same_instant()
    {
        // El mismo instante UTC cae en un mes distinto según la sede: Bogotá (UTC-5) ya está en
        // enero, México (UTC-6) todavía está en el día anterior de enero también en este caso, pero
        // con una hora distinta -- confirma que cada sede se resuelve con su propio offset.
        var fechaUtc = new DateTime(2026, 1, 1, 3, 30, 0, DateTimeKind.Utc);

        var localBogota = SedeTimeZoneResolver.ConvertirUtcALocal(fechaUtc, "Bogotá");
        var localMexico = SedeTimeZoneResolver.ConvertirUtcALocal(fechaUtc, "México");

        Assert.Equal(new DateTime(2025, 12, 31, 22, 30, 0), localBogota);
        Assert.Equal(new DateTime(2025, 12, 31, 21, 30, 0), localMexico);
    }

    [Fact]
    public void ConvertirUtcALocal_treats_an_Unspecified_kind_input_as_UTC_instead_of_throwing()
    {
        var fechaSinKind = new DateTime(2026, 6, 15, 12, 0, 0, DateTimeKind.Unspecified);

        var local = SedeTimeZoneResolver.ConvertirUtcALocal(fechaSinKind, "Bogotá");

        Assert.Equal(new DateTime(2026, 6, 15, 7, 0, 0), local);
    }
}
