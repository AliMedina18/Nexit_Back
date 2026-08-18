# Nexit · Auditoría del backend (funcionalidad y seguridad)

**Proyecto:** Sistema de gestión de la información para la organización de proyectos de trabajo
**Nombre del sistema:** Nexus (nombre de trabajo, sujeto a cambio)
**Cliente/organización:** Next — agencia de marketing experiencial
**Fecha:** 17 de agosto de 2026
**Repositorio auditado:** Nexit_Back (commit de trabajo del 17 de agosto de 2026, sin publicar aún)
**Rol de esta sesión:** auditoría de software (compilación, pruebas, revisión de código) + investigación de buenas prácticas de seguridad vigentes

---

## 1. Objetivo y alcance

Next pidió una auditoría completa del backend de Nexus antes de avanzar: revisar que controladores, servicios (casos de uso) y repositorios funcionen correctamente, correr las pruebas automatizadas, y evaluar específicamente la seguridad del sistema — en particular el middleware que filtra qué peticiones del cliente pueden entrar al servidor y quién puede hacer qué.

Esta sesión fue solo de **auditoría y documentación**: no se modificó código del proyecto. Todos los hallazgos de abajo son para que Next decida qué aplicar y cuándo.

Se revisó el 100% del código fuente versionable (95 archivos `.cs`, sin contar `bin/`/`obj/`/`logs/`): las cuatro capas de Clean Architecture (`Nexit.Core`, `Nexit.Application`, `Nexit.Infrastructure`, `Nexit.API`), el proyecto de pruebas (`Nexit.Tests`), la configuración (`Program.cs`, `appsettings*.json`, `.env.example`), las migraciones de Entity Framework Core y el esquema SQL de referencia para Supabase (`docs/schema/nexus_schema_v2.sql`).

## 2. Resumen ejecutivo

El backend está en mejor estado del que suele estar un proyecto en esta etapa. **Compila sin advertencias y las 18 pruebas automatizadas existentes pasan.** Ya existen, y funcionan, varias piezas de seguridad que normalmente hay que pedir explícitamente: autenticación JWT contra Supabase Auth, un middleware de cabeceras de seguridad, un manejador global de excepciones que oculta detalles internos, límite de tamaño de petición, *rate limiting* por IP, CORS restringido en producción, y una política `AdminOnly` para las operaciones de catálogo. El equipo que escribió este código ya venía pensando en seguridad, no partimos de cero.

Dicho esto, la auditoría encontró **12 hallazgos** que conviene revisar antes de exponer el backend a internet, agrupados así:

| Severidad | Cantidad | Naturaleza |
|---|---|---|
| Alta | 3 | Verificación del modo de firma JWT de Supabase, privilegios de la conexión a base de datos, y ausencia de autorización por rol de negocio más allá de "autenticado" |
| Media | 5 | Incoherencia entre RLS y el patrón de acceso real, paquetes NuGet desactualizados (con un CVE conocido), fin de soporte de .NET 8 en 3 meses, *rate limiting* no confiable detrás de un proxy, falta de pruebas de integración de autenticación/autorización |
| Baja | 4 | Validación de URL en adjuntos, cabecera CSP ausente, trazabilidad de quién edita (no solo quién crea), e inconsistencia de nombres entre la documentación y el código |

Ninguno de los hallazgos es una vulnerabilidad explotable *hoy* de forma trivial (no hay inyección SQL, no hay secretos filtrados en el repositorio, no hay endpoints completamente abiertos por descuido). Son, en su mayoría, decisiones de arquitectura que hay que tomar conscientemente antes de producción, y una actualización de dependencias pendiente.

## 3. Compilación y pruebas automatizadas

```
dotnet build Nexit.sln  →  Build succeeded. 0 Warning(s). 0 Error(s).
dotnet test Nexit.sln   →  Total tests: 18. Passed: 18. Failed: 0.
```

Las 18 pruebas (en `tests/Nexit.Tests/ClientesTests.cs`, que en realidad contiene 6 clases de prueba: `ClientesTests`, `ProveedoresTests`, `CatalogosTests`, `ProyectosTests`, `InformesTests` y `SecurityMiddlewareTests`) cubren el camino feliz y algunas reglas de negocio de cada módulo, más dos pruebas unitarias de los middlewares de seguridad. No hay pruebas de integración de extremo a extremo (ver hallazgo H8). Como nota menor de organización: tener 6 clases de prueba distintas en un solo archivo (`ClientesTests.cs`) dificulta encontrarlas; conviene separarlas en archivos con el nombre de su clase.

## 4. Lo que ya está bien (para no perderlo de vista)

- **Autenticación por defecto:** `BaseController` lleva `[Authorize]` a nivel de clase, así que *todo* controlador que hereda de él exige un JWT válido salvo que se diga lo contrario. Los controladores no heredan de `ControllerBase` directamente, así que es difícil "olvidar" proteger un endpoint nuevo.
- **Autorización por política para catálogos:** crear/editar/eliminar países, regiones, ciudades, categorías, servicios, fases y estados exige la política `AdminOnly`, que revisa rol o claims (`admin`, `app_role=admin`, `user_role=admin`).
- **CORS con salvavidas de producción:** `Program.cs` lanza una excepción en el arranque si `Cors:AllowedOrigins` está vacío fuera de `Development` — no es posible desplegar a producción con CORS abierto a cualquier origen por accidente.
- **Cabeceras de seguridad y manejo de errores:** `SecurityHeadersMiddleware` agrega `X-Content-Type-Options`, `X-Frame-Options`, `Referrer-Policy`, `Permissions-Policy` y `Cache-Control: no-store` a toda respuesta; `GlobalExceptionHandlerMiddleware` atrapa cualquier excepción no manejada y responde con un mensaje genérico (nunca el stack trace ni el mensaje interno), registrando el detalle real solo en los logs del servidor. Esto está probado explícitamente en `SecurityMiddlewareTests`.
- **Sin inyección SQL:** toda consulta a la base de datos pasa por LINQ/Entity Framework Core con parámetros; no se encontró ni un solo `FromSqlRaw`/`ExecuteSqlRaw` con texto concatenado en las 7 clases de repositorio revisadas.
- **Sin *mass assignment*:** los DTOs de entrada (`CreateClienteDto`, `CreateProveedorDto`, `CrearProyectoDto`, etc.) son explícitos sobre qué campos acepta cada operación; el mapeo a la entidad es manual (no hay un `AutoMapper` genérico "mapea todo lo que llegue"), así que un cliente no puede, por ejemplo, mandar `CreatedBy` o campos internos en el cuerpo de la petición y que el servidor los use.
- **Defensa en profundidad en la base de datos:** además de la validación en `FluentValidation`, el esquema tiene `CHECK` constraints para los mismos valores permitidos (estado, prioridad, tipo de proyecto, rol de equipo, etc.), así que un bug en la validación de la aplicación no basta para meter un dato inválido en la tabla.
- **Manejo de secretos razonable:** `.gitignore` excluye `appsettings.Production.json`, `appsettings.Development.json`, `.env` y `.env.local`; lo que sí está en el repositorio son plantillas `*.example.json` con placeholders, y la cadena de conexión de producción se resuelve también desde la variable de entorno `DATABASE_URL` como alternativa a un archivo de configuración.
- **Límite de tamaño de petición y *rate limiting* configurados:** 10 MB máximo por petición, y un límite de 100 peticiones/minuto por IP configurable vía `RateLimiting:PermitLimit`.

## 5. Hallazgos

Los hallazgos están numerados por severidad. Donde aplica, se referencia la categoría correspondiente del [OWASP API Security Top 10 (2023)](https://owasp.org/API-Security/editions/2023/en/0x11-t10/), el estándar de referencia actual para seguridad de APIs.

### Severidad alta

**H1 — Confirmar el modo de firma del JWT de Supabase antes de desplegar.**
`Program.cs` valida el token con `options.Authority = "https://TU_PROYECTO.supabase.co/auth/v1"`, lo que hace que ASP.NET Core busque automáticamente las claves públicas de verificación en el documento de descubrimiento OIDC / JWKS de ese dominio. Esto es correcto **solo si el proyecto de Supabase usa las claves de firma asimétricas modernas** (RS256/ES256, activables en *Settings → API → JWT Keys*). Si el proyecto todavía usa el secreto compartido heredado (HS256, el modo por defecto en proyectos Supabase más antiguos), la validación por `Authority` puede no encontrar una clave utilizable y, según la configuración, rechazar silenciosamente todos los tokens o (peor) fallar en detectar mal configuración hasta que se prueba en producción. Supabase mismo desaconseja el secreto compartido porque es más difícil de rotar y más fácil de filtrar accidentalmente.
*Cómo verificarlo:* revisar en el panel de Supabase si el proyecto tiene "JWT Signing Keys" (asimétricas) habilitadas, o si sigue en "Legacy JWT Secret". Si sigue en modo legado, migrar a claves asimétricas (Supabase lo permite sin invalidar sesiones activas) o, si no es posible migrar todavía, cambiar la configuración de `Program.cs` para validar con la clave compartida (`TokenValidationParameters.IssuerSigningKey`) en vez de `Authority`.
*Relacionado con:* API2:2023 — Broken Authentication.

**H2 — La conexión a la base de datos usa el superusuario `postgres`, no un rol de aplicación con privilegios mínimos.**
Tanto `appsettings.json` como `appsettings.Production.example.json` y `.env.example` configuran `Username=postgres`. En Supabase, `postgres` es el superusuario del proyecto: tiene permiso para crear/borrar tablas, cambiar cualquier dato sin restricción, y **le son invisibles todas las políticas de Row Level Security** (un superusuario de Postgres las ignora por diseño). Si en el futuro apareciera un bug de lógica en el backend, o una dependencia con una vulnerabilidad de inyección, el radio de impacto es "toda la base de datos", no "las tablas que la aplicación necesita tocar".
*Recomendación:* crear un rol de Postgres dedicado para el backend (p. ej. `nexit_app`) con `GRANT SELECT, INSERT, UPDATE, DELETE` únicamente sobre las tablas del esquema de la aplicación (sin `DROP`, sin `ALTER`, sin acceso a otros esquemas), y usar ese rol en la cadena de conexión de la API. Las migraciones/el `nexus_schema_v2.sql` sí pueden seguir aplicándose con el superusuario, porque eso es un paso administrativo puntual, no el tráfico normal de la aplicación.
*Relacionado con:* API8:2023 — Security Misconfiguration.

**H3 — No hay autorización por rol de negocio más allá de "autenticado" vs. `AdminOnly` en catálogos.**
`ClientesController`, `ProveedoresController` y `ProyectosController` solo exigen `[Authorize]` (heredado de `BaseController`): cualquier usuario autenticado, sin importar su rol (`admin`, `manager` o `miembro` en el modelo actual), puede crear, editar y **eliminar** cualquier cliente, proveedor o proyecto, y puede cambiar campos sensibles del negocio como `Proveedor.Estado` (p. ej. pasarlo a `Bloqueado`) directamente en el mismo endpoint de edición. Esto puede ser exactamente lo que Next quiere para el MVP (un equipo pequeño y de confianza), pero tal como está, **es una decisión implícita, no una decisión tomada**. Vale la pena decidirla explícitamente: por ejemplo, ¿debería un `miembro` poder eliminar un proyecto? ¿Debería requerir `manager` o `admin`? El propio `docs/schema/nexus_schema_v2.sql` (comentario en la línea 647) ya anticipaba este caso — "que 'compras' no pueda eliminar proveedores" — pero esa regla nunca se implementó en el backend.
*Recomendación:* definir una tabla corta de qué rol puede hacer qué (lectura / creación / edición / eliminación) por entidad, y reflejarla con políticas de autorización adicionales (`[Authorize(Policy = "...")]`) en los controladores, igual que ya se hizo para `AdminOnly` en catálogos.
*Relacionado con:* API5:2023 — Broken Function Level Authorization.

### Severidad media

**H4 — Las políticas de Row Level Security del esquema SQL no coinciden con el patrón de acceso real del backend.**
`docs/schema/nexus_schema_v2.sql` habilita RLS en las 19 tablas de negocio con políticas como `FOR ALL USING (auth.role() = 'authenticated')`. La función `auth.role()` es específica de Supabase: lee una variable de sesión (`request.jwt.claims`) que **PostgREST/GoTrue** rellena cuando el propio cliente (frontend) llama directamente a la API de Supabase con su JWT. Pero en esta arquitectura el frontend no habla directo con Supabase para los datos de negocio: habla con el backend de ASP.NET Core, que a su vez abre una conexión directa a Postgres vía Npgsql. En una conexión directa así, `auth.role()` no tiene ese contexto y no se comporta como en PostgREST. En la práctica hoy esto no causa un error visible porque el backend se conecta como `postgres` (ver H2), que **ignora RLS por completo** — pero es la razón exacta por la que H2 importa: si mañana alguien "arregla" H2 conectando con un rol normal sin ajustar también las políticas, el backend completo dejaría de poder leer o escribir cualquier tabla, no solo los usuarios no autorizados.
*Recomendación:* decidir conscientemente una de dos rutas: (a) RLS es vestigial en este patrón de arquitectura porque la autorización real vive en el backend — en ese caso, documentarlo así en el propio `nexus_schema_v2.sql` para que nadie asuma que protege algo, y opcionalmente simplificar las políticas; o (b) se quiere RLS como capa adicional de defensa en profundidad — en ese caso hay que reescribir las políticas para que funcionen con una conexión directa (por ejemplo, usando el rol de Postgres dedicado de H2 en vez de `auth.role()`, con `SET ROLE`/`current_setting` puestos por el propio backend en cada conexión).

**H5 — Paquetes NuGet sin actualizar desde la versión inicial de .NET 8 (noviembre 2023).**
`Microsoft.AspNetCore.Authentication.JwtBearer`, `Microsoft.EntityFrameworkCore(.Design)`, `Npgsql.EntityFrameworkCore.PostgreSQL` y `Microsoft.Extensions.Configuration.Json` están fijados en `8.0.0`, la versión de lanzamiento inicial de .NET 8, sin ninguno de los parches acumulados desde entonces. Esto incluye [CVE-2024-21319](https://github.com/dotnet/aspnetcore/security/advisories/GHSA-59j7-ghrg-fj52): una vulnerabilidad de denegación de servicio por consumo excesivo de recursos en `Microsoft.IdentityModel.JsonWebTokens`, que es justo el paquete que valida los tokens JWT en este proyecto (dependencia transitiva de `JwtBearer`).
*Recomendación:* actualizar los paquetes de Microsoft a la última versión parcheada de la serie 8.0.x (`dotnet list package --outdated` para ver el detalle exacto en el momento de aplicar el cambio).

**H6 — .NET 8 llega a fin de soporte el 10 de noviembre de 2026.**
Quedan menos de 3 meses desde hoy. Después de esa fecha, .NET 8 deja de recibir parches de seguridad — incluyendo cualquier vulnerabilidad que se descubra en el runtime, ASP.NET Core o `System.Text.Json` después de esa fecha. La serie LTS actual es .NET 10. No es urgente para el MVP si el objetivo es lanzar antes de esa fecha, pero conviene que quede en el radar del roadmap de Fase 2/3 en vez de descubrirlo tarde.

**H7 — El *rate limiting* por IP no es confiable detrás de un proxy inverso, y todos los candidatos de despliegue anotados en la memoria del proyecto (Railway, Render, Azure App Service, Fly.io) usan uno.**
`Program.cs` particiona el límite de peticiones con `context.Connection.RemoteIpAddress`. Sin `app.UseForwardedHeaders()` configurado con los proxies/redes de confianza del proveedor elegido, esa propiedad va a devolver la IP interna del proxy (la misma para todas las peticiones de todos los usuarios) en vez de la IP real del cliente. El efecto práctico: o el límite se aplica a todos los usuarios juntos como si fueran uno solo (un usuario activo puede bloquear a los demás), o dependiendo de la configuración del proveedor, deja de limitar nada.
*Recomendación:* una vez elegido el proveedor de despliegue (pendiente según la memoria del proyecto), agregar `ForwardedHeadersMiddleware` con `KnownProxies`/`KnownNetworks` explícitos según la documentación de ese proveedor, antes de `UseRateLimiter()`.

**H8 — Faltan pruebas de integración que verifiquen autenticación y autorización de extremo a extremo.**
Las 18 pruebas actuales validan casos de uso de forma aislada (con repositorios en memoria) y, por separado, que los dos middlewares de seguridad hacen lo que deben cuando se los invoca directamente. Ninguna prueba levanta la aplicación completa (`WebApplicationFactory`) para verificar, por ejemplo: "una petición sin token a `POST /api/clientes` responde 401", "un token válido sin rol admin a `POST /api/catalogos/paises` responde 403", o "el pipeline completo de middleware se ejecuta en el orden correcto". Es exactamente el tipo de prueba que detecta una regresión si alguien, sin querer, quita un `[Authorize]` en un refactor futuro.
*Relacionado con:* API2:2023 — Broken Authentication.

### Severidad baja

**H9 — Sin validación de esquema en URLs de adjuntos de proveedor.**
`CrearProveedorAdjuntoDto.Url` (usado cuando `Tipo == "link"`) solo se valida como no vacío, sin exigir que empiece por `http://` o `https://`. Si el frontend llegara a renderizar ese valor como un enlace clicable sin sanear el esquema, alguien podría guardar una URL `javascript:` y lograr un XSS almacenado activado por otro usuario del equipo que haga clic. El riesgo depende del frontend (que no fue parte de esta auditoría), pero es barato de cerrar en el backend.
*Recomendación:* en `CreateProveedorAdjuntoValidator` (o donde se agregue), exigir que `Url` empiece por `http://` o `https://` cuando `Tipo == "link"`.

**H10 — Falta cabecera `Content-Security-Policy`.**
`SecurityHeadersMiddleware` no agrega CSP. El riesgo es bajo porque la API es JSON puro (no sirve HTML de negocio), pero Swagger UI sí se sirve en `Development` y es HTML/JS; y una CSP básica es una recomendación estándar de defensa en profundidad incluso para APIs, por si en el futuro se sirve cualquier contenido HTML (páginas de error personalizadas, documentación, etc.).

**H11 — Solo se registra quién *crea* un registro, no quién lo *edita*.**
`BaseEntity.CreatedBy` se rellena en los tres casos de uso de creación (`CrearClienteUseCase`, `CrearProveedorUseCase`, `CrearProyectoUseCase`) vía `GetUserId()`, pero las operaciones de actualización (`ActualizarClienteUseCase`, `ActualizarProveedorUseCase`, `ActualizarProyectoUseCase`) no registran quién hizo el cambio más reciente. No es un problema de control de acceso, pero si Next quiere poder responder "¿quién cambió esto y cuándo?" — algo que suele importar en un sistema con varios roles tocando los mismos registros — hace falta un campo `UpdatedBy` además del `UpdatedAt` que ya existe.

**H12 — Inconsistencia de nombres entre la documentación conversacional del proyecto y el código implementado.**
La memoria de este proyecto (y las notas de fases anteriores) mencionan el rol `compras` como parte del ENUM de roles de usuario. El código real —tanto `docs/schema/nexus_schema_v2.sql` como la migración `AddLocalDatabaseRules` y la entidad `Usuario.cs`— implementa el ENUM como `admin`, `manager`, `miembro`. No es un riesgo de seguridad, pero si en algún momento se documenta o comunica el modelo de permisos hacia el equipo de Next, conviene que todos los lugares digan lo mismo.

## 6. Cómo priorizar (si Next decide actuar sobre esto)

No se aplicó ningún cambio de código en esta sesión — esto es solo una guía de orden sugerido para cuando se decida abordarlos:

1. **Antes de conectar a un Supabase real de producción:** H1 (confirmar modo de firma JWT) y H2 (rol de base de datos de mínimo privilegio) — son la base de todo lo demás.
2. **Antes de invitar a más de un rol al sistema:** H3 (decidir y aplicar permisos por rol) y H4 (resolver la incoherencia de RLS, aunque sea documentándola).
3. **Mantenimiento de rutina, no bloqueante:** H5 (actualizar paquetes) y H6 (anotar el fin de soporte de .NET 8 en el roadmap).
4. **Al elegir proveedor de despliegue:** H7 (`ForwardedHeadersMiddleware`).
5. **Cuando se retome trabajo de pruebas:** H8 (pruebas de integración de auth).
6. **Bajo costo, se pueden hacer en cualquier momento:** H9, H10, H11, H12.

## 7. Fuentes consultadas

- [OWASP API Security Top 10 (2023)](https://owasp.org/API-Security/editions/2023/en/0x11-t10/) — catálogo de referencia usado para clasificar los hallazgos.
- [Supabase Docs — JSON Web Token (JWT)](https://supabase.com/docs/guides/auth/jwts) — validación de JWT de Supabase desde backends externos, diferencia entre secreto compartido (HS256) y claves de firma asimétricas.
- [GitHub Security Advisory GHSA-59j7-ghrg-fj52 / CVE-2024-21319](https://github.com/dotnet/aspnetcore/security/advisories/GHSA-59j7-ghrg-fj52) — vulnerabilidad de denegación de servicio en `Microsoft.IdentityModel.JsonWebTokens` y `System.IdentityModel.Tokens.Jwt`.
- [.NET 8 and .NET 9 will reach End of Support on November 10, 2026 — .NET Blog](https://devblogs.microsoft.com/dotnet/dotnet-8-9-end-of-support/) — fecha de fin de soporte de .NET 8.
- [Configure ASP.NET Core to work with proxy servers and load balancers — Microsoft Learn](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/proxy-load-balancer) — configuración de `ForwardedHeadersMiddleware` para *rate limiting*/IP real detrás de un proxy inverso.

## 8. Notas

- Este documento es el compañero de [`01-analisis-fase1.md`](01-analisis-fase1.md); mientras aquel cubre el modelo de datos, este cubre el estado del código del backend y su seguridad.
- Ver [[nexit_proyecto]] (memoria del proyecto) para retomar contexto en próximas sesiones.
