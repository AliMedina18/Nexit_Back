# Tipos de pruebas del backend

Este documento explica qué tipos de prueba tiene el backend, qué cubre cada uno (y qué NO cubre), y
por qué hacían falta más allá de "pruebas unitarias" y "pruebas de integración". Se hizo porque, aunque
ya existían esas dos capas, un sistema como este — que maneja información real de clientes, proveedores,
proyectos y usuarios de la agencia — necesita comprobarse desde varios ángulos distintos para poder
confiar en que "funciona correctamente y de forma segura", no solo en que "compila y los casos felices
pasan".

## La pirámide de pruebas: por qué tres capas y no solo dos

Microsoft (en su propia guía de pruebas para ASP.NET Core) describe tres niveles, no dos, y cada uno
detecta una clase de error distinta que los otros dos no ven:

| Nivel | Qué prueba | Qué NO puede detectar | Cuántas |
|---|---|---|---|
| **Unitarias** | Un caso de uso o una regla de negocio aislada, con los repositorios simulados (mocks) | Que la consulta real a Postgres funcione, que las migraciones apliquen limpio, que dos capas se conecten bien | Muchas, rápidas |
| **Integración** | Que el pipeline completo de middleware (autenticación → limitador de peticiones → autorización) esté bien conectado, sin base de datos real | Que una operación termine escribiendo/leyendo lo correcto en la base de datos | Algunas |
| **Funcionales** | Un flujo de extremo a extremo real: petición HTTP → controlador → caso de uso → Entity Framework Core → **Postgres real** → respuesta | — (es el nivel más completo, pero el más lento) | Pocas, las justas |

Antes de este trabajo, el proyecto tenía las dos primeras capas, pero lo que aquí se llamaba
"pruebas de integración" (`tests/Nexit.Tests/Integration/`) en realidad solo verificaba el pipeline de
autenticación/autorización — nunca tocaba una base de datos de verdad. Le faltaba exactamente la capa
que Microsoft llama **funcional**: la única capaz de detectar errores que solo aparecen contra Postgres
de verdad (un tipo de columna que no cuadra, una migración que en teoría se ve bien pero no aplica
limpio, o — como pasó acá, ver más abajo — un detalle de Entity Framework Core que hace que una
actualización se guarde mal).

## 1. Pruebas unitarias (`tests/Nexit.Tests/*.cs`)

Prueban un caso de uso a la vez, con los repositorios mockeados (Moq). Son las más rápidas y las más
numerosas — cubren reglas de negocio como "un gerente no puede eliminar un cliente directamente", "el
código de catálogo no puede repetirse", etc. No necesitan Docker ni base de datos.

## 2. Pruebas de integración (`tests/Nexit.Tests/Integration/`)

Levantan la aplicación completa con `WebApplicationFactory<Program>`, reemplazando solo la
autenticación JWT de Supabase por un esquema de prueba (`TestAuthHandler`, controlado con las cabeceras
`X-Test-Role` / `X-Test-UserId`). Todo lo demás — el orden real del pipeline de middleware, las
políticas de autorización, el limitador de peticiones — es el mismo código que corre en producción. No
usan base de datos real, así que solo pueden probar los casos donde la petición se resuelve ANTES de
llegar a la base de datos (típicamente, un 401/403 de autenticación/autorización, o —como se explica en
la sección de seguridad— un 429 del limitador de peticiones, que corre antes que la autorización).

## 3. Pruebas funcionales (`tests/Nexit.Tests/Functional/`) — la capa nueva

Usan [Testcontainers](https://dotnet.testcontainers.org/) para levantar un contenedor real de
`postgres:16-alpine`, le aplican las migraciones reales de Entity Framework Core (las mismas que
correrían en producción), y dejan la aplicación completa lista para peticiones HTTP de extremo a
extremo: la petición pasa por el controlador, el caso de uso, Entity Framework Core, y termina
escribiendo/leyendo filas de verdad en Postgres. Cada prueba relee lo que creó con OTRA petición HTTP
independiente — nunca confía en el objeto que devolvió el POST — para confirmar que de verdad quedó
guardado, no solo que quedó en memoria.

**Requieren Docker.** Si Docker no está disponible en la máquina (o en el runner de CI), estas pruebas
fallan al arrancar el contenedor — no se cuelgan silenciosamente. Para correrlas en tu máquina hace
falta [Docker Desktop](https://www.docker.com/products/docker-desktop/) corriendo; si tu equipo no
tiene Docker instalado, hay dos caminos: instalarlo, o dejar que las corra únicamente el pipeline de CI
(donde si se agrega un pipeline más adelante, GitHub Actions y la mayoría de proveedores ya traen Docker
disponible por defecto).

### El bug real que esta capa encontró (y que ni las pruebas unitarias ni las de integración podían ver)

Al escribir la primera prueba funcional de "crear, actualizar y eliminar un cliente", la actualización
fallaba contra Postgres real con `DbUpdateConcurrencyException` (0 filas afectadas) — algo que nunca
había fallado en ninguna prueba unitaria porque los repositorios mockeados no ejecutan la lógica interna
de seguimiento de cambios de Entity Framework Core.

La causa: `ActualizarClienteUseCase` (y el mismo patrón en `ActualizarProveedorUseCase` y
`ActualizarProyectoUseCase`) reemplaza la lista de teléfonos de un cliente ya cargado (`Clear()` +
`Add()` de teléfonos nuevos). Como esos teléfonos nuevos ya traían un `Id` de tipo `Guid` asignado
explícitamente (`Guid.NewGuid()`), y la tabla tiene ese `Id` configurado con un valor por defecto de la
base de datos (`gen_random_uuid()`), Entity Framework Core — al descubrir esas filas nuevas solo por
navegación, no por un `Add()` explícito al contexto — no podía distinguir "esta es una fila nueva" de
"esta ya existe en la base", y por defecto asume que ya existe: genera un `UPDATE` en vez de un
`INSERT`. Como esa fila todavía no existía, el `UPDATE` no afectaba ninguna fila, y Entity Framework
Core lo reporta como un conflicto de concurrencia.

La corrección: dejar el `Id` en su valor por defecto (`Guid.Empty`) para las filas genuinamente nuevas,
en vez de asignarles un `Guid` a mano — así Entity Framework Core las reconoce correctamente como
nuevas y deja que la base de datos les genere el identificador real. Se corrigió en los tres casos de
uso que tenían el mismo patrón (clientes, proveedores, proyectos), y las pruebas funcionales quedan
como la red de seguridad que detecta si este problema reaparece.

## 4. Pruebas de seguridad (`tests/Nexit.Tests/Security/`) — la capa nueva más reciente

Un sistema que maneja datos reales de la operación necesita, además de "funciona", una verificación
explícita de "es seguro para quien lo usa". Esta capa no es un nivel más de la pirámide (unitaria /
integración / funcional) sino un **corte transversal**: cada prueba de seguridad vive en el nivel que le
corresponde según qué necesite (reflexión pura, HTTP sin base de datos, o HTTP contra Postgres real),
pero todas están agrupadas en `Security/` para que se vea de un vistazo qué categorías de riesgo están
cubiertas. Se basan en el [OWASP API Security Top 10](https://owasp.org/www-project-api-security/), el
catálogo de referencia de la industria para APIs.

| Prueba | Categoría OWASP | Nivel | Qué comprueba |
|---|---|---|---|
| `ControllersRequierenAutorizacionTests` | Security Misconfiguration | Reflexión (sin HTTP) | Recorre TODOS los controladores del ensamblado por reflexión y falla si alguno se queda sin `[Authorize]` (propio o heredado) o si aparece un `[AllowAnonymous]` no intencional — a diferencia de las pruebas de integración normales, no hay que acordarse de escribir un caso por cada controlador nuevo: si mañana alguien agrega uno y lo olvida proteger, esto se rompe solo. |
| `RateLimitingIntegrationTests` | API4 — Unrestricted Resource Consumption | Integración (sin base de datos) | Confirma que después de 100 peticiones/minuto el servidor responde `429`, y que el cupo se cuenta por usuario (no por IP), para que varias personas de la misma oficina no se bloqueen entre sí. |
| `SeguridadFunctionalTests` — inyección SQL | Injection | Funcional (Postgres real) | Guarda un texto con un payload clásico de inyección SQL como nombre de un cliente y confirma que se guarda y se relee tal cual, como texto — nunca se ejecuta, porque todo el acceso a datos usa LINQ de Entity Framework Core (parámetros), nunca SQL concatenado a mano. |
| `SeguridadFunctionalTests` — mass assignment | API3 — Broken Object Property Level Authorization | Funcional (Postgres real) | Un usuario con rol "miembro" intenta auto-asignarse como gerente responsable de un proyecto mandando `gerenteId` directamente en el cuerpo de la petición — confirma que la regla de negocio lo ignora sin importar qué mande el cliente. |
| `SeguridadFunctionalTests` — autorización a nivel de objeto (BOLA) | API1 — Broken Object Level Authorization | Funcional (Postgres real) | Un gerente que NO es el responsable de un proyecto en particular no puede aprobar su solicitud de eliminación, aunque su rol sí le permita llegar al endpoint — la política estática por rol no alcanza aquí, la restricción depende de datos concretos (quién es el dueño de ESE proyecto). |

### Otras dos prácticas de seguridad aplicadas en esta misma sesión (no son pruebas de xUnit, pero son parte de "comprobar que el sistema sea seguro")

- **Análisis de dependencias vulnerables (SCA):** se corrió `dotnet list package --vulnerable
  --include-transitive` sobre toda la solución. Encontró una vulnerabilidad de severidad alta en
  `AutoMapper` 12.0.1 (denegación de servicio por recursión sin límite,
  [GHSA-rvv3-g6hj-g44x](https://github.com/advisories/GHSA-rvv3-g6hj-g44x)), traída de forma transitiva
  por el paquete `AutoMapper.Extensions.Microsoft.DependencyInjection`. Al revisar el código se confirmó
  que AutoMapper estaba registrado (`AddAutoMapper(...)`) y tenía 5 perfiles de mapeo escritos, pero
  **nunca se usaba**: todo el mapeo real del proyecto se hace a mano con clases estáticas internas
  (`ClienteMapper`, `ProveedorMapper`, etc.), y ningún caso de uso ni controlador inyectaba `IMapper`.
  En vez de solo subir de versión, se eliminó la dependencia completa (el paquete, el registro en DI y
  los 5 perfiles sin usar) — la corrección más segura posible es quitar código vulnerable que ni
  siquiera se estaba usando, no solo parchearlo. `dotnet list package --vulnerable` quedó en cero para
  toda la solución. Se recomienda correr este comando periódicamente (o agregarlo a CI) según vayan
  saliendo nuevos paquetes.
- **Revisión manual de las reglas de acceso por objeto:** además de la prueba automatizada de BOLA de
  la tabla de arriba, se revisó todo el flujo de `SolicitudEliminacion` (el único lugar del sistema
  donde el "dueño" de un recurso concreto, no solo su rol, determina permisos) para confirmar que la
  verificación de propiedad (`GerenteResponsableId != gerenteId`) ocurre siempre en el caso de uso, no
  solo en el frontend.

## 5. Pruebas de arquitectura (`tests/Nexit.Tests/ArchitectureTests.cs`) — la capa nueva (2026-08-25)

A diferencia de las cuatro capas anteriores, estas pruebas no verifican comportamiento de negocio —
verifican que la **separación en capas de Clean Architecture se mantenga con el tiempo**, recorriendo
por reflexión los ensamblados ya compilados (no el código fuente). Usan
[NetArchTest.Rules](https://github.com/BenMorris/NetArchTest), una librería hecha exactamente para esto.

**Por qué se agregaron — el bug real que las motivó:** al construir HU-12 (presencia en vivo), un caso
de uso de `Nexit.Application` terminó con un `using Microsoft.EntityFrameworkCore;` para atrapar
`DbUpdateConcurrencyException` — rompiendo la regla de que `Nexit.Application` no debe saber nada de la
tecnología de persistencia concreta (eso es trabajo exclusivo de `Nexit.Infrastructure`, la única capa
que debe importar Entity Framework Core). El error se descubrió recién al compilar, a mano, en la
máquina de la usuaria. Estas pruebas convierten esa regla — que hasta ahora solo vivía "en la cabeza de
quien programa" — en algo que `dotnet test` verifica solo, cada vez, sin depender de que alguien lo note
en una revisión de código.

Reglas verificadas hoy:
- `Nexit.Core` no depende de `Nexit.Application`, `Nexit.Infrastructure` ni `Nexit.API`.
- `Nexit.Application` no depende de `Nexit.Infrastructure` ni de `Nexit.API`.
- `Nexit.Application` no depende directamente de `Microsoft.EntityFrameworkCore` (la regla puntual del
  bug de arriba).
- `Nexit.Infrastructure` no depende de `Nexit.API`.
- Todo controlador (hereda de `ControllerBase`) vive en el namespace `Nexit.API.Controllers`.

Son pruebas muy rápidas (no arrancan la aplicación ni tocan una base de datos) — se corren siempre,
junto con las unitarias, en cada `dotnet test`.

## Cobertura de código: cómo verla

El proyecto ya recolecta datos de cobertura en cada corrida (paquete `coverlet.collector`, ya incluido
en `Nexit.Tests.csproj`), pero hasta ahora nadie los convertía en un reporte legible. Para generar un
reporte HTML navegable (qué líneas y qué ramas de código sí se ejecutaron durante las pruebas y cuáles
no):

```powershell
# Una sola vez, para instalar la herramienta (queda guardada en el repo vía dotnet-tools.json)
dotnet new tool-manifest
dotnet tool install dotnet-reportgenerator-globaltool

# Cada vez que quieras un reporte nuevo
dotnet test tests/Nexit.Tests/Nexit.Tests.csproj --collect:"XPlat Code Coverage"
dotnet reportgenerator -reports:"tests/Nexit.Tests/TestResults/*/coverage.cobertura.xml" -targetdir:"CoverageReport" -reporttypes:Html
```

Esto deja un `CoverageReport/index.html` que puedes abrir en el navegador. También se dejó
`scripts/generar-reporte-cobertura.ps1`, que hace exactamente estos tres últimos pasos en un solo
comando (`.\scripts\generar-reporte-cobertura.ps1` desde la raíz del repo, en PowerShell).

No hay un porcentaje mínimo de cobertura exigido todavía (no tiene sentido imponer un número sin antes
ver el reporte real una vez) — la recomendación es generarlo, revisarlo, y decidir con datos reales si
hace falta reforzar alguna zona concreta.

## Pruebas de mutación (opcional, no integradas al `dotnet test` normal)

Las pruebas de mutación miden qué tan buenas son las pruebas que ya existen: introducen bugs pequeños a
propósito en el código compilado (cambiar un `>` por un `>=`, invertir un `if`, etc. — cada uno se llama
una "mutación") y revisan si alguna prueba se rompe. Si ninguna se rompe, esa prueba en teoría "cubre"
esa línea pero en realidad no está verificando nada de verdad ahí.

Se dejan documentadas como un chequeo **manual, periódico** (por ejemplo, antes de una release grande),
no como parte de cada `dotnet test` — son mucho más lentas que el resto de la pirámide (corren la
suite de pruebas una vez por cada mutación introducida) y no aportan tanto valor corriéndolas en cada
cambio chico. Usa [Stryker.NET](https://stryker-mutator.io/docs/stryker-net/introduction/):

```powershell
dotnet tool install -g dotnet-stryker
cd tests/Nexit.Tests
dotnet-stryker
```

Genera un reporte HTML con el "mutation score" (% de mutaciones que sí detectó alguna prueba) por
archivo. No hay que correrlo todavía —queda documentado para cuando la usuaria quiera un chequeo más a
fondo antes de una release importante.

## Cómo correr cada capa

```bash
# Todo (unitarias + integración + funcionales + arquitectura) — requiere Docker para las funcionales
dotnet test tests/Nexit.Tests/Nexit.Tests.csproj

# Solo una capa
dotnet test tests/Nexit.Tests/Nexit.Tests.csproj --filter "FullyQualifiedName~Functional"
dotnet test tests/Nexit.Tests/Nexit.Tests.csproj --filter "FullyQualifiedName~Integration"
dotnet test tests/Nexit.Tests/Nexit.Tests.csproj --filter "FullyQualifiedName~Security"
dotnet test tests/Nexit.Tests/Nexit.Tests.csproj --filter "FullyQualifiedName~ArchitectureTests"

# Análisis de dependencias vulnerables (recomendado antes de cada release)
dotnet list package --vulnerable --include-transitive

# Reporte de cobertura (ver la sección de arriba)
.\scripts\generar-reporte-cobertura.ps1
```

Estado al cerrar esta sesión (2026-08-18): **118/118 pruebas pasando** (unitarias + integración +
funcionales + seguridad), 0 advertencias de compilación, 0 paquetes vulnerables. **Actualizado
2026-08-25:** el número real de pruebas creció bastante desde entonces con cada historia nueva (adjuntos,
presencia, arquitectura) — ver `docs/README.md` para el conteo más reciente reportado por la usuaria en
cada `dotnet test`, este documento no se actualiza número por número para no quedar desactualizado.

## Qué queda fuera del alcance de este backend (y por qué)

Para ser transparente sobre los límites de esta capa de pruebas: no incluye pruebas de carga/rendimiento
(cuántas peticiones simultáneas aguanta el servidor bajo estrés sostenido — distinto del límite de
peticiones por usuario, que sí se prueba), ni un escaneo dinámico de seguridad (DAST, tipo OWASP ZAP,
que ataca la aplicación ya desplegada desde afuera). Ninguna de las dos es indispensable para el tamaño y
el uso actual de este sistema (una agencia, no un producto masivo, 20-25 usuarios), pero quedan anotadas
aquí como próximos pasos razonables si el sistema crece o se expone directamente a internet sin capas
adicionales (WAF, etc.) por delante. Las pruebas de mutación, que también estaban en esta lista, ya se
documentaron arriba como chequeo manual opcional — dejaron de estar "fuera de alcance" para pasar a
"disponibles, pero no obligatorias en cada corrida".
