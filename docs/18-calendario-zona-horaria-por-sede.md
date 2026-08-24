# Calendario: el mes de un proyecto se decide por la hora local de su sede, no por UTC

Revisando el backend del calendario (`docs/07`) contra cómo lo resuelven otros sistemas de calendario, encontré un riesgo real (no una rareza teórica) en cómo se calculaba a qué mes pertenece un proyecto, y lo corregí. Este documento explica el problema, la investigación que lo confirma, y cómo quedó.

## 1. El problema

`fecha_evento` se guarda en Postgres como `timestamptz` — Postgres siempre la convierte a UTC internamente. Las tres consultas del calendario (`ObtenerAniosConProyectosAsync`, `ObtenerConteoPorMesAsync`, `ObtenerPorMesAsync`) calculaban el mes/año directamente sobre esa columna (`.Year`, `.Month`, que EF Core traduce a `EXTRACT(YEAR/MONTH FROM ...)`). Ese cálculo depende de la zona horaria de la *sesión* de la base de datos — un dato de configuración del servidor, no una decisión de negocio.

El caso concreto que esto rompe: un proyecto con evento el 31 de enero a las 11:30pm hora de Bogotá queda guardado como 1 de febrero, 04:30 UTC. Si el mes se calcula sobre ese valor sin fijar la zona horaria, ese proyecto aparece en **febrero** en el calendario, cuando para cualquiera en Next ese evento fue en **enero**.

## 2. Investigación

Antes de tocarlo, busqué si este es un problema conocido o una preocupación mía sin fundamento — es lo primero, está bien documentado:

- **[Grouping Multi-Timezone Data in PostgreSQL Without Getting It Wrong](https://tunasakara.com/en/blog/multi-timezone-grouping-postgresql)** — confirma exactamente este patrón ("eventos justo después de medianoche quedan mal atribuidos al día anterior") y recomienda resolver la fecha local de forma explícita, con una zona horaria conocida, en vez de depender de la zona ambiente de la sesión.
- **[How to Work with Timezones in PostgreSQL](https://oneuptime.com/blog/post/2026-01-25-postgresql-timezone-handling/view)** — recomienda evitar comparar `timestamptz` directo contra una fecha, y en su lugar usar comparaciones de rango explícitas (`>= inicio AND < fin`), que además son más eficientes (usan índice) que una función como `EXTRACT` sobre cada fila.
- **[Google Calendar System Design](https://www.systemdesignhandbook.com/guides/google-calendar-system-design/)** — confirma que el patrón de "grilla de 12 meses con conteo agregado, detalle solo al entrar a un mes" (que ya teníamos) es el estándar; el problema no era el diseño general, era el cálculo del mes.

## 3. Otra decisión que hizo falta: no es una sola zona horaria, es por sede

Next tiene proyectos en más de una sede (`Proyecto.SedeNext`, hoy visto en los datos: Bogotá y México — texto libre, sin catálogo fijo, ver `docs/11`). Confirmaste que cada proyecto debe ubicarse en el calendario según la hora local de **su propia sede**, no todos según una sola zona horaria fija. Como `SedeNext` es texto libre, la resolución es por coincidencia de texto (ver `SedeTimeZoneResolver`, sección 4) — no hay un ID de catálogo detrás.

## 4. Cómo quedó

**`src/Nexit.Core/Utils/SedeTimeZoneResolver.cs`** (nuevo): reconoce el texto de `SedeNext` y devuelve la zona horaria IANA correspondiente:

| Texto de la sede (sin importar mayúsculas/acentos) | Zona horaria |
|---|---|
| Contiene "méxico"/"mexico"/"cdmx" | `America/Mexico_City` |
| Cualquier otro caso — incluye "Bogotá", vacío, `null`, o cualquier sede no reconocida | `America/Bogota` (sede principal, y el valor por defecto) |

Si Next abre una sede en un país nuevo, ese es el único archivo que hay que tocar — se agrega un patrón de texto más a la tabla de arriba.

**Las tres consultas del calendario** (`ProyectoRepository`) cambiaron así: en vez de calcular el mes/año directo en SQL sobre la columna UTC (el cálculo frágil), ahora traen un rango un poco más ancho (año pedido, más/menos 1 — ese margen es más que suficiente, ninguna diferencia de zona horaria real mueve una fecha más de un día) usando un filtro que sigue siendo eficiente en la base de datos (usa el índice de `fecha_evento`, no carga las entidades completas — el diseño original de `docs/07` no cambió en ese sentido), y ya en memoria, con la zona horaria exacta de cada proyecto, se calcula el mes/año real y se arma el resultado. No cambia cuántos datos se traen de la base para una vista de calendario normal (un año tiene, como mucho, los proyectos de ese año más un margen chico a cada lado), solo *cuándo* se decide a qué mes pertenece cada uno.

## 5. Verificado

- `SedeTimeZoneResolverTests.cs` (nuevo, 14 pruebas): confirma la resolución de zona por texto (Bogotá, México, CDMX, vacío, sede desconocida) y, el caso que motivó todo esto, que un evento a las 11:30pm del 31 de enero hora Bogotá se queda en enero, no salta a febrero — además del caso de dos sedes distintas sobre el mismo instante UTC.
- Compilación limpia, 134/141 pruebas totales pasan (las 7 restantes son las funcionales de siempre, que necesitan Docker/Postgres real, no relacionadas con este cambio) — cero pruebas rotas por este cambio.

## 6. Nota honesta sobre el alcance

Esto corrige el cálculo del mes/año en la vista de calendario. No cambia cómo se guarda `fecha_evento` (sigue siendo `timestamptz`, sigue llegando del frontend como venía llegando) ni agrega ninguna pantalla nueva — es una corrección puntual de lógica de backend, sin ningún script SQL que correr ni configuración nueva de Supabase. No requiere ninguna acción tuya aparte de que el código ya quedó actualizado en el repositorio.
