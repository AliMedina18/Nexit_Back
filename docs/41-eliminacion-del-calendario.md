# 41 · Se elimina el calendario de proyectos

**Fecha:** 2026-09-09
**Alicia:** *"elimina toda la parte de calendario, que eso ya no lo vamos a hacer. Eso es un extra, pero no, ya no se va a llevar a cabo. Solo vamos a concentrarnos en lo que de verdad se va a hacer dentro del sistema."*

El calendario se construyó en agosto (`docs/07`, con la corrección de zona horaria de `docs/18`) como
un extra. Nunca entró al alcance real del sistema, así que se saca entero en vez de dejarlo ahí
ocupando menú, endpoints y pruebas.

---

## Qué se fue

**Backend**

| Archivo | Qué era |
|---|---|
| `src/Nexit.API/Controllers/CalendarioController.cs` | `GET /api/calendario/{anio}` y `/{anio}/{mes}` |
| `src/Nexit.Application/DTOs/Proyectos/CalendarioDtos.cs` | los tres DTO de la vista |
| `src/Nexit.Application/UseCases/Proyectos/CalendarioProyectosUseCase.cs` + su interfaz | el caso de uso |
| `src/Nexit.Core/Utils/SedeTimeZoneResolver.cs` | conversión UTC → hora local de la sede |
| `tests/Nexit.Tests/CalendarioProyectosTests.cs` | 5 pruebas |
| `tests/Nexit.Tests/SedeTimeZoneResolverTests.cs` | 22 pruebas |

Y de paso, dentro de archivos que se quedan:

- `IProyectoRepository`: los records `ConteoMesProyectos` y `ProyectoCalendarioItem`, y los métodos
  `ObtenerAniosConProyectosAsync`, `ObtenerConteoPorMesAsync`, `ObtenerPorMesAsync`. Se verificó uno
  por uno que **nadie más los llamaba** antes de quitarlos.
- `ProyectoRepository`: las tres consultas correspondientes y sus dos helpers privados.
- `DependencyInjection`: el registro del caso de uso.
- `AuthorizationIntegrationTests`: las dos pruebas de autorización del calendario.

**Frontend**

| Archivo | Qué era |
|---|---|
| `src/app/(dashboard)/calendario/page.tsx` | la pantalla |
| `src/app/(dashboard)/calendario/CalendarGrid.tsx` | la grilla del mes |
| `src/services/api/calendario-service.ts` | el cliente HTTP |

Más el ítem del menú (`nav-items.ts`), los tipos `CalendarioMes`/`CalendarioAnio`/
`ProyectoCalendarioItem` (`types/api.ts`), la exportación en `services/api/index.ts`, y las menciones
en comentarios de `primitives.tsx`, `dashboard.module.css` y `styles/README.md`.

## Qué NO se tocó

- **La fecha del evento sigue en su sitio.** `proyectos.fecha_evento` es un campo del proyecto, no del
  calendario: se sigue capturando, mostrando y exportando igual.
- **Informes.** Comparten el documento `docs/07` pero nada más: la exportación a Excel con ClosedXML y
  la restricción a `super_admin`/`admin` quedan intactas.
- **La base de datos.** El calendario nunca tuvo tablas ni columnas propias — solo leía
  `fecha_evento`. **No hay que correr ningún SQL por esto.**

## Sobre `SedeTimeZoneResolver`

Se va porque su única razón de existir era el calendario (`docs/18`), y dejarlo sería código muerto
con 22 pruebas que lo hacen parecer vivo. Pero el bug que resolvía es real y **vuelve a aparecer en
cuanto algo agrupe proyectos por mes** — un informe mensual, por ejemplo: si se calcula el mes con
`fecha_evento.Month` sobre una columna `timestamptz`, un evento de las 11 de la noche del 31 en Bogotá
cae en el mes siguiente. Por eso `docs/18` se conserva con un aviso de obsoleto en vez de borrarse, y
el código está a un `git checkout` de distancia.

## Estado después del cambio

- Backend: **279 pruebas pasan** (bajaron de 306: se fueron 27 del calendario y su resolver). Cero
  regresiones en el resto.
- Frontend: 58 pruebas, `tsc`, ESLint y `next build` limpios. El menú pasó de 6 a 5 secciones.

## Pendiente manual

Las carpetas `_to_delete/calendario/` de los dos repos, y la carpeta vacía
`Nexit_Front/src/app/(dashboard)/calendario/`, hay que borrarlas a mano — el puente al computador no
puede borrar archivos ni carpetas. La carpeta vacía no rompe nada mientras tanto (sin `page.tsx` no
genera ruta; se confirmó con `next build`).
