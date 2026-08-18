# Nexit · Calendario de proyectos, restricción de informes y exportación a Excel

**Proyecto:** Sistema de gestión de la información para la organización de proyectos de trabajo
**Nombre del sistema:** Nexus (nombre de trabajo, sujeto a cambio)
**Cliente/organización:** Next — agencia de marketing experiencial
**Fecha:** 2026-08-18

## 1. Por qué existe este documento

Después de cerrar el modelo de permisos de 4 niveles (`docs/06-modelo-permisos-roles.md`), la usuaria pidió tres cosas más para la misma sesión, todas del lado del **backend únicamente** — su pedido explícito fue *"hay que dejar la lógica en el backend"*, dejando claro que las vistas (pantallas) son un trabajo aparte de frontend que no se construye aquí:

1. Una **vista de calendario** (enero a diciembre, cualquier año) que muestre cuántos proyectos hay por mes, con la preocupación explícita de que *"no consuma tanto la base de datos"*.
2. Que el **informe** (semanal/mensual) quede restringido a que **solo lo vean super_admin y admin** — hoy cualquier autenticado podía verlo.
3. Que el informe se pueda **exportar a Excel**.

Sobre el alcance de la exportación a Excel, la usuaria dijo inicialmente *"todo se porta en Excel"*, pero al pedirle que confirmara si eso incluía también clientes/proveedores/proyectos/calendario, o solo los dos casos concretos que había mencionado, **eligió expresamente "solo informes"**. Este documento y el código reflejan esa respuesta: la exportación a Excel implementada hoy cubre únicamente los informes semanal/mensual, no clientes, proveedores, proyectos ni el calendario. Si más adelante se necesita exportar alguno de esos otros listados, el patrón (`IInformeExcelExporter` + ClosedXML) es directamente reutilizable — ver sección 4.

## 2. Calendario de proyectos (backend)

### 2.1 Qué construye la vista (cuando se implemente en el frontend)

Un calendario tipo el de Microsoft Teams: se elige un año, se ve una grilla de enero a diciembre con cuántos proyectos hay en cada mes, y al entrar a un mes se ve el detalle de esos proyectos. El campo que ubica un proyecto en un mes es **`fecha_evento`** (confirmado con la usuaria) — no `fecha_solicitud`, porque lo que se quiere ver es cuándo se *ejecuta* el proyecto, no cuándo llegó la solicitud.

### 2.2 Por qué 3 endpoints separados, no uno solo

La usuaria pidió explícitamente cuidar la carga a la base de datos. La forma más costosa de construir esto sería traer todos los proyectos (con su equipo, proveedores y seguimiento, como ya hace `GET /api/proyectos`) y agrupar por mes en memoria, en el backend o peor, en el frontend. En su lugar, cada necesidad de la vista tiene su propia consulta, resuelta con agregación SQL (`GROUP BY`, `DISTINCT`) en vez de cargar entidades completas:

| Endpoint | Cuándo se usa | Qué hace en la base de datos |
|---|---|---|
| `GET /api/calendario/anios` | Al abrir la vista, para poblar el selector de año | `SELECT DISTINCT EXTRACT(YEAR FROM fecha_evento) ...` — un valor por año que tiene al menos un proyecto, no una fila por proyecto |
| `GET /api/calendario/{anio}` | Al elegir un año, para pintar la grilla de 12 meses | `... GROUP BY EXTRACT(MONTH FROM fecha_evento)` — un conteo por mes, no los proyectos mismos |
| `GET /api/calendario/{anio}/{mes}` | Al entrar a un mes específico | Trae los proyectos de ese mes, pero con una proyección liviana (`ProyectoCalendarioItem`: id, nombre, fecha, cliente, estado, prioridad, ciudad, sede) — sin equipo/proveedores/seguimiento, que esta vista no necesita |

El endpoint de un año siempre devuelve los 12 meses (enero a diciembre), incluidos los que tienen 0 proyectos — el relleno lo hace el caso de uso en memoria (`Enumerable.Range(1, 12)` combinado con el diccionario que sí devolvió la base de datos), no la base de datos, para no forzar 12 consultas o una consulta con `UNION` artificial solo para no dejar huecos.

### 2.3 Acceso

Sin restricción adicional — cualquier usuario autenticado puede ver el calendario, igual que puede ver la lista normal de proyectos. La usuaria no mencionó ninguna restricción de rol para esta vista (a diferencia de informes, ver sección 3).

### 2.4 Dónde vive cada pieza

| Pieza | Archivo |
|---|---|
| `ConteoMesProyectos`, `ProyectoCalendarioItem` (proyecciones) e `IProyectoRepository` (métodos nuevos) | `src/Nexit.Core/Interfaces/IProyectoRepository.cs` |
| Implementación (3 consultas nuevas) | `src/Nexit.Infrastructure/Repositories/ProyectoRepository.cs` |
| DTOs (`CalendarioAnioDto`, `CalendarioMesDto`, `ProyectoCalendarioItemDto`) | `src/Nexit.Application/DTOs/Proyectos/CalendarioDtos.cs` |
| Caso de uso (relleno de meses en cero, validación de año/mes) | `src/Nexit.Application/UseCases/Proyectos/CalendarioProyectosUseCase.cs` |
| Controlador | `src/Nexit.API/Controllers/CalendarioController.cs` |
| Pruebas | `tests/Nexit.Tests/CalendarioProyectosTests.cs` |

## 3. Informes restringidos a super_admin/admin

`InformesController` no tenía ninguna política de autorización propia — solo heredaba el `[Authorize]` base de `BaseController` (cualquier autenticado). Ahora lleva `[Authorize(Policy = "AdminOrAbove")]` a nivel de clase, la misma política que ya usaban otras operaciones administrativas (ver `docs/06-modelo-permisos-roles.md`). Esto cubre los 4 endpoints existentes (`GET resumen`, `GET snapshots/{tipo}/{periodoKey}`, `POST snapshots`) más los 2 nuevos de exportación (sección 4) — nadie con rol `manager`/`miembro` puede ver ni exportar informes.

## 4. Exportación a Excel

### 4.1 Elección de librería: ClosedXML sobre EPPlus

Se investigaron las opciones habituales para generar `.xlsx` en .NET. **EPPlus** dejó de ser gratis para uso comercial desde su versión 5 (la última versión libre para eso es la 4.5.3.3) — usar una versión reciente en un proyecto comercial como este requeriría pagar una licencia. **ClosedXML** (licencia MIT, activamente mantenida, construida sobre `DocumentFormat.OpenXml` de Microsoft) es gratis para cualquier uso, incluido comercial, y tiene una API igual de directa. Se instaló ClosedXML 0.105.1 en `Nexit.Infrastructure`.

Fuente consultada: [EPPlus: No longer free to use in a commercial setting — Itenium](https://itenium.be/blog/dotnet/epplus-pay-to-play/).

### 4.2 Diseño: abstracción en Application, implementación en Infrastructure

Siguiendo el mismo patrón que los repositorios (interfaz en una capa interna, implementación concreta con una librería externa en Infrastructure), se definió `IInformeExcelExporter` en `Nexit.Application` y se implementó con ClosedXML en `Nexit.Infrastructure` — así el controlador depende de una abstracción, no directamente de ClosedXML, y el motor de exportación se podría cambiar sin tocar el controlador ni los casos de uso.

### 4.3 Endpoints nuevos

| Endpoint | Qué exporta |
|---|---|
| `GET /api/informes/resumen/exportar` | El resumen en vivo (los mismos datos de `GET /api/informes/resumen` en este momento) |
| `GET /api/informes/snapshots/{tipo}/{periodoKey}/exportar` | Un snapshot ya guardado (semanal o mensual) |

Ambos devuelven un archivo `.xlsx` (`application/vnd.openxmlformats-officedocument.spreadsheetml.sheet`) con 3 hojas:

- **Resumen**: título del informe y los 4 totales (proveedores, clientes, proyectos, proyectos sin proveedor).
- **Por estado**: una fila por cada estado de proyecto con su conteo, ordenado de mayor a menor.
- **Por brief**: igual que la anterior, pero por estado del brief.

### 4.4 Dónde vive cada pieza

| Pieza | Archivo |
|---|---|
| Abstracción | `src/Nexit.Application/Services/IInformeExcelExporter.cs` |
| Implementación con ClosedXML | `src/Nexit.Infrastructure/Services/InformeExcelExporter.cs` |
| Registro en DI (`AddSingleton` — no tiene estado) | `src/Nexit.Infrastructure/DependencyInjection.cs` |
| Endpoints | `src/Nexit.API/Controllers/InformesController.cs` |
| Pruebas (abren el `.xlsx` generado con ClosedXML y verifican su contenido, no solo que el arreglo de bytes no esté vacío) | `tests/Nexit.Tests/InformesTests.cs` (clase `InformeExcelExporterTests`) |

## 5. Qué queda pendiente (fuera de alcance de esta sesión, a propósito)

- **La vista de calendario en sí (frontend)**: hoy solo existe la lógica de backend que la va a alimentar, tal como pidió la usuaria ("hay que dejar la lógica en el backend"). El frontend (Vercel, framework por definir) es quien construye la grilla visual de enero a diciembre a partir de estos 3 endpoints.
- **Exportar a Excel clientes, proveedores, proyectos o el calendario**: la usuaria confirmó explícitamente que por ahora la exportación a Excel es solo para informes (ver sección 1). El patrón `IInformeExcelExporter` es reutilizable si más adelante se pide exportar alguno de esos otros listados — sería una interfaz y una implementación nuevas siguiendo el mismo molde, no un rediseño.
- **Snapshots automáticos**: `POST /api/informes/snapshots` sigue siendo manual (alguien con rol admin/super_admin lo dispara). No se agregó una tarea programada que genere el snapshot semanal/mensual solo — no fue parte de este pedido.
