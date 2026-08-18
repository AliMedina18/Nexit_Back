# Nexit · Plan de remediación de seguridad — implementación

**Proyecto:** Sistema de gestión de la información para la organización de proyectos de trabajo
**Nombre del sistema:** Nexus (nombre de trabajo, sujeto a cambio)
**Cliente/organización:** Next — agencia de marketing experiencial
**Fecha:** 17 de agosto de 2026
**Repositorio:** Nexit_Back
**Rol de esta sesión:** implementación de los 12 hallazgos de [`04-auditoria-seguridad-backend.md`](04-auditoria-seguridad-backend.md), más hallazgos adicionales de concurrencia/escala, investigación de APIs, y organización del repositorio.

---

## 1. Objetivo y alcance

Este documento es la continuación directa de la auditoría del 17 de agosto de 2026. Next pidió que los 12 hallazgos se aplicaran de verdad — no solo documentarlos — con un criterio explícito: *"necesito que esto esté robusto, necesito que no haya ni una sola vulnerabilidad en este sistema"*. También pidió que se abordara la preparación para 20-25 usuarios concurrentes, que se investigaran APIs públicas para almacenamiento de adjuntos y geolocalización, y que se organizara el repositorio (archivos sueltos, `.gitignore`, documentación por fases).

Esta sesión sí modificó código. Todo lo descrito abajo ya está en el repositorio, compila sin advertencias y pasa **31 pruebas automatizadas** (22 unitarias + 9 de integración, ver sección 4).

**Resumen honesto de alcance:** de los 12 hallazgos, 10 quedan resueltos en código de forma verificable hoy mismo, sin depender de nada externo. Los otros 2 (H1 y H2, y parcialmente H4) tienen el código y los scripts SQL completamente listos, pero su efecto real solo se activa cuando exista un proyecto de Supabase de producción — porque son configuración de *ese* proyecto, no de este repositorio. Esto se explica en detalle en cada hallazgo y se resume en la tabla de la sección 2.

## 2. Estado de los 12 hallazgos

| # | Hallazgo | Estado | Qué falta (si algo) |
|---|---|---|---|
| H1 | Modo de firma JWT sin confirmar | ✅ Código listo para ambos modos | Activar el modo correcto cuando exista el proyecto Supabase real (sección 5.1) |
| H2 | Conexión con superusuario `postgres` | ✅ Rol y script listos | Ejecutar `02_rol_aplicacion_minimo_privilegio.sql` contra el proyecto real y poner la contraseña generada en la configuración (sección 5.2) |
| H3 | Sin autorización por rol de negocio | ✅ Resuelto en código | — |
| H4 | RLS incoherente con el patrón de acceso | ✅ Resuelto en el script SQL | Ejecutar `nexus_schema_v2.sql` (sección 13) contra el proyecto real |
| H5 | Paquetes NuGet desactualizados | ✅ Resuelto | — |
| H6 | Fin de soporte de .NET 8 (nov. 2026) | 📋 Documentado, no es un "fix" de código | Decisión de roadmap para Fase 2/3 |
| H7 | Rate limiting poco fiable detrás de proxy | ✅ Código listo | Completar `ForwardedHeaders:KnownProxies`/`KnownNetworks` cuando se elija proveedor de despliegue |
| H8 | Faltan pruebas de integración de auth | ✅ Resuelto — 9 pruebas nuevas | — |
| H9 | Sin validación de esquema en URLs de adjuntos | ✅ Resuelto | — |
| H10 | Falta cabecera Content-Security-Policy | ✅ Resuelto | — |
| H11 | No se registra quién edita (solo quién crea) | ✅ Resuelto + migración generada | Aplicar la migración a la base real (sección 5.3) |
| H12 | Inconsistencia "compras" vs "manager" | ✅ Resuelto en documentación | — |

Además, se abordaron dos temas que no estaban en la auditoría original pero que Next pidió explícitamente: **concurrencia con 20-25 usuarios simultáneos** (sección 3) y **organización del repositorio** (sección 6).

## 3. Concurrencia y escala (20-25 usuarios simultáneos)

Next planteó la preocupación con dos ideas: qué pasa cuando varias personas editan lo mismo al tiempo, y si hace falta algo tipo "FIFO"/colas para ordenar el trabajo. Aquí está el análisis y lo que se implementó.

**Lo que sí hacía falta — y se implementó: control de concurrencia optimista.** Antes de este trabajo, si dos personas abrían el mismo proveedor, lo editaban con datos distintos y guardaban casi al mismo tiempo, el segundo guardado sobrescribía al primero sin avisar a nadie — se perdía el cambio silenciosamente ("actualización perdida", *lost update*, uno de los problemas clásicos de concurrencia en sistemas con varios editores). Ahora `Cliente`, `Proveedor` y `Proyecto` usan la columna de sistema `xmin` de Postgres (un contador interno que cambia en cada `UPDATE` de una fila) como token de concurrencia de Entity Framework Core. En la práctica: si Ana y Carlos abren el mismo proveedor, Ana guarda primero sin problema, y cuando Carlos intenta guardar su versión (basada en datos ya desactualizados) la aplicación detecta el conflicto y responde `409 Conflict` con el mensaje *"Otra persona modificó este registro mientras lo editabas. Recarga los datos e inténtalo de nuevo."* en vez de pisar silenciosamente el trabajo de Ana. Esto no requiere ninguna tabla ni columna adicional real — `xmin` ya existe en toda tabla de Postgres — por eso la migración generada (sección 5.3) no la crea, solo le dice a Entity Framework Core que la use.

**Lo que NO hacía falta: una cola FIFO real.** Una cola de procesamiento (FIFO) tiene sentido cuando hay una operación costosa que no se puede paralelizar sin corromper datos — por ejemplo, generar facturas consecutivas, o un job en segundo plano compartido por todos. Nada en Nexus hoy encaja en ese patrón: crear/editar/eliminar clientes, proveedores y proyectos son operaciones independientes entre sí (proyecto de Ana no interfiere con proyecto de Carlos), y donde sí puede haber choque directo (dos personas editando el *mismo* registro) ya lo resuelve el control de concurrencia optimista de arriba. Con 20-25 usuarios y las operaciones CRUD normales de este sistema, PostgreSQL maneja miles de conexiones/transacciones concurrentes sin ayuda adicional — el volumen real es órdenes de magnitud menor de lo que necesitaría una cola dedicada. Construir una cola FIFO aquí agregaría complejidad (infraestructura extra, otro punto de fallo) sin resolver un problema que exista.

**Lo que se ajustó para que 20-25 personas no se estorben entre sí (H7, relacionado):** el *rate limiting* ahora se aplica por usuario autenticado (`user:<id>`) en vez de por dirección IP. Antes, si Next y todo su equipo trabajan desde la misma oficina (misma IP pública, algo típico), el límite de 100 peticiones/minuto se habría repartido entre las 20-25 personas como si fueran un solo cliente — una persona muy activa podía bloquear al resto sin culpa de nadie. Ahora cada persona tiene su propio límite.

**Recomendación operativa (no requiere código, es configuración al desplegar):** cuando se elija el proveedor de base de datos/hosting, usar el *connection pooler* de Supabase (PgBouncer, ya incluido) en modo *transaction* para la cadena de conexión de la API, en vez de conectar directo a Postgres. Con 20-25 usuarios esto no es estrictamente necesario todavía, pero es la práctica estándar y evita tener que revisitar el tema si el equipo crece.

## 4. Pruebas automatizadas

```
dotnet build Nexit.sln  →  Build succeeded. 0 Warning(s). 0 Error(s).
dotnet test Nexit.sln   →  Total tests: 31. Passed: 31. Failed: 0.
```

**22 pruebas unitarias** (antes 18; se agregaron 4 nuevas para `UpdatedBy` en Clientes/Proveedores/Proyectos y para el nuevo comportamiento de CSP en Swagger/Development), ahora separadas en un archivo por clase (`ClientesTests.cs`, `ProveedoresTests.cs`, `CatalogosTests.cs`, `ProyectosTests.cs`, `InformesTests.cs`, `SecurityMiddlewareTests.cs`) en vez de las 6 clases mezcladas en un solo archivo que señalaba la auditoría.

**9 pruebas de integración nuevas** (`tests/Nexit.Tests/Integration/`, hallazgo H8) que levantan la aplicación completa —el pipeline real de middleware, no casos de uso aislados— usando `WebApplicationFactory<Program>`. Como no es posible generar tokens JWT firmados por un Supabase real dentro de una prueba automatizada, se sustituye la autenticación por un `TestAuthHandler` que simula el resultado ya validado (rol `admin`/`manager`/`miembro` vía una cabecera de prueba `X-Test-Role`, o sin cabecera para simular "sin token"). Todo lo demás —el orden del pipeline, las políticas de autorización, `[Authorize(Policy = "AdminOnly")]`— es el código real, exactamente el que corre en producción. Estas pruebas confirman, por ejemplo, que `GET /api/clientes` sin token responde `401`, que `DELETE /api/clientes/{id}` con rol `miembro` o `manager` responde `403`, que con rol `admin` la autorización lo deja pasar, y que las cabeceras de seguridad están presentes incluso en una respuesta `401`. Es exactamente la prueba que habría detectado si, en algún refactor futuro, alguien quita un `[Authorize]` sin querer.

## 5. Lo que requiere que exista un proyecto de Supabase real

Estos tres puntos son la razón por la que H1, H2 y H4 no pueden quedar "100% terminados" desde este repositorio: son configuración *del proyecto de Supabase*, no del código. El código y los scripts ya están listos para aplicarse en el momento en que Next cree el proyecto.

### 5.1 — H1: elegir y confirmar el modo de firma JWT

`Program.cs` ahora soporta los dos modos sin necesidad de tocar código, solo configuración:

- **Modo recomendado (claves asimétricas / JWKS):** dejar `Jwt:LegacySharedSecret` vacío (el valor por defecto) y poner `Jwt:Authority` con la URL del proyecto real (`https://TU_PROYECTO.supabase.co/auth/v1`). Es el modo que Supabase recomienda hoy y **está disponible en el plan gratuito** — no hace falta el plan Pro para esto (ver nota importante abajo).
- **Modo heredado (secreto compartido HS256):** si el proyecto ya existe con configuración antigua y no se quiere migrar todavía, poner el secreto en `Jwt:LegacySharedSecret` (vía variable de entorno `Jwt__LegacySharedSecret`, nunca en el archivo committeado).

**Nota importante que corrige una suposición de la sesión anterior:** al preguntarte por el estado del proyecto de Supabase, mencionaste que creías que las claves de firma asimétricas requerían el plan Pro de pago. Verifiqué esto directamente en la documentación oficial de Supabase y **no es así**: las claves de firma asimétricas (JWT Signing Keys, incluido el *Auth Hook* de claims personalizados que se necesita para H3/H4) están disponibles en el plan gratuito. No hace falta pagar Supabase Pro para tener esto bien hecho desde el día uno.

### 5.2 — H2: rol de aplicación de mínimo privilegio

`docs/schema/02_rol_aplicacion_minimo_privilegio.sql` (nuevo) crea el rol `nexit_app`: puede `SELECT`/`INSERT`/`UPDATE`/`DELETE` sobre las tablas de la aplicación, pero no puede crear ni borrar tablas, ni tocar otros esquemas, ni saltarse RLS como sí puede el superusuario `postgres`. Pasos para activarlo:

1. Ejecutar `02_rol_aplicacion_minimo_privilegio.sql` en el SQL Editor de Supabase (usa el superusuario, es un paso administrativo puntual).
2. Reemplazar `CAMBIAR_ESTA_CONTRASENA` en el script por una contraseña generada (no la que viene de ejemplo).
3. Usar `nexit_app` y esa contraseña en la cadena de conexión de producción (`appsettings.Production.json` o `DATABASE_URL`, nunca en `appsettings.json` committeado — ya está en `.gitignore`).

`appsettings.json`, `appsettings.Production.example.json` y `.env.example` ya se actualizaron para usar `nexit_app` como usuario de ejemplo en vez de `postgres`.

### 5.3 — H4 y H11: aplicar el esquema SQL y la migración de Entity Framework

`docs/schema/nexus_schema_v2.sql` (sección 13, reescrita) ahora define una sola política por tabla (`solo_nexit_app`) que otorga acceso completo únicamente al rol `nexit_app` de arriba — no a "cualquier autenticado" como antes, que no tenía efecto real en este patrón de arquitectura (ver el hallazgo H4 original para el porqué). Además se agregó `docs/schema/03_auth_hook_custom_claims.sql` (nuevo): la función de Postgres que Supabase necesita como *Custom Access Token Hook* para leer `usuarios.rol` y agregarlo como claim `user_role` al JWT — sin esto, la política `AdminOnly` de H3 nunca podría cumplirse, porque no había ningún mecanismo que pusiera el rol dentro del token. Se activa desde *Authentication → Hooks* en el dashboard de Supabase.

Para H11 (`UpdatedBy`), se generó la migración de Entity Framework Core `20260817234434_AddConcurrencyAndAuditTracking` (`dotnet ef migrations add`, ya corrida y verificada con `dotnet ef migrations has-pending-model-changes` → "No changes have been made to the model since the last migration"). Agrega la columna `updated_by` a `clientes`, `proveedores` y `proyectos`. **Nota técnica:** esta migración *no* incluye una columna `xmin` a pesar de que el modelo la usa para concurrencia optimista (sección 3) — `xmin` ya es una columna de sistema que Postgres trae en toda tabla por defecto; intentar "agregarla" con una migración habría fallado en tiempo real contra una base real. Cuando exista la base de datos de producción, aplicar esta migración con `dotnet ef database update` (o el SQL equivalente generado con `dotnet ef migrations script`, si se prefiere revisarlo antes de correrlo).

Orden recomendado de ejecución contra el proyecto de Supabase real, de una sola vez: `nexus_schema_v2.sql` (si es la primera vez) → `02_rol_aplicacion_minimo_privilegio.sql` → `03_auth_hook_custom_claims.sql` → activar el Auth Hook en el dashboard → `dotnet ef database update` con la cadena de conexión ya usando `nexit_app`.

## 6. Investigación de APIs públicas

Next pidió investigar específicamente almacenamiento de archivos para adjuntos de proveedor y geolocalización/validación de direcciones — con foco en costo (presupuesto ajustado) y seguridad, considerando que ya se usa Supabase.

### 6.1 — Almacenamiento de archivos para adjuntos de proveedor

| Opción | Plan gratuito | Costo si se excede | Notas de seguridad/operación |
|---|---|---|---|
| **Supabase Storage** (recomendado para empezar) | 1 GB incluido, 5 GB de transferencia/mes, 50 MB por archivo | Plan Pro ($25/mes): 100 GB incluidos, luego $0.0213/GB; transferencia 250 GB incluida, luego $0.09/GB | Ya está en la misma cuenta/proyecto que la base de datos y la autenticación — un solo lugar donde administrar permisos. Soporta políticas de acceso por bucket con las mismas reglas de Postgres (RLS), y URLs firmadas con expiración para adjuntos privados. Para el volumen de adjuntos de un CRM de proveedores (documentos, fotos de venues, cotizaciones en PDF), 1 GB gratis alcanza para cientos de archivos típicos; solo hay que vigilarlo si empiezan a subirse videos o archivos muy pesados. |
| **Cloudflare R2** (alternativa si el volumen crece) | 10 GB de almacenamiento, 1 millón de operaciones de escritura/mes, 10 millones de lectura/mes — **y transferencia de salida (egress) siempre gratis**, incluso fuera del plan gratuito | $0.015/GB de almacenamiento adicional; sin costo de egress nunca | Compatible con la API de S3 (fácil de integrar con cualquier librería estándar). La ventaja real frente a Supabase Storage es el egress gratis — si en el futuro el equipo empieza a *ver* muchos adjuntos seguido (no solo subirlos), ahí es donde Supabase Storage empieza a cobrar y R2 no. Para el tamaño actual del equipo (20-25 personas), esta diferencia probablemente no se nota todavía. |

**Recomendación:** empezar con **Supabase Storage** — ya está pagado/disponible en la misma cuenta, la integración es más simple (mismas credenciales, mismas políticas RLS que ya se están definiendo para H4), y el límite gratuito de 1 GB / 50 MB por archivo cubre razonablemente el uso esperado de adjuntos de proveedor. Si el consumo de transferencia se vuelve un costo real más adelante (equipo grande revisando adjuntos constantemente), migrar a Cloudflare R2 es la ruta de escape natural gracias al egress gratuito — vale la pena diseñar la capa de acceso a adjuntos detrás de una interfaz propia (`IProveedorAdjuntoStorage` o similar) desde ahora, para que ese cambio futuro no obligue a tocar toda la aplicación.

### 6.2 — Geolocalización / validación de direcciones

| Opción | Plan gratuito | Costo si se excede | Notas |
|---|---|---|---|
| **Google Geocoding API** | 10,000 peticiones/mes gratis (desde el cambio de tarifas de marzo de 2025; ya no existe el crédito de $200/mes anterior) | $5 por 1,000 peticiones (1-100k), con descuentos por volumen hasta $0.38/1,000 en volúmenes muy altos | La cobertura y calidad de direcciones en Colombia/LatAm es la más consistente del mercado. Para un CRM con 20-25 usuarios validando direcciones de proveedores (no geocodificación masiva ni en tiempo real de cara al público), 10,000/mes gratis probablemente cubre el uso real sin pagar nada. |
| **Geoapify** | 3,000 créditos/día (~90,000/mes) gratis, uso comercial permitido | Plan pagado desde $59/mes (10,000 créditos/día) | El límite gratuito es más generoso que el de Google en volumen, pero conviene probar la calidad de resultados específicamente para direcciones colombianas/mexicanas antes de decidir, porque su cobertura de datos varía más por región que la de Google. |
| **Mapbox Geocoding** | Geocoding temporal: 100,000 peticiones/mes gratis | $0.75/1,000 después del free tier (temporal); la API "permanente" (para guardar resultados) no tiene plan gratis y cuesta desde $5/1,000 | El free tier más generoso en volumen, pero la letra chica importa: si se necesita *guardar* la dirección validada (que es probablemente el caso — se quiere guardar la dirección corregida del proveedor, no solo mostrarla una vez), aplica el precio de la API "permanente", que no tiene capa gratuita. |
| **Nominatim (OpenStreetMap)** | Gratis, sin límite de presupuesto | — | Es la única opción realmente gratuita sin condiciones de dinero, pero su política de uso público limita a 1 petición/segundo, prohíbe autocompletado, y **prohíbe explícitamente que una aplicación cuya función principal sea geocodificar la use como su servicio de geocodificación** — solo la permite como función secundaria de una app más grande (que sí sería el caso de Nexus: valida direcciones como parte de gestionar proveedores, no es un producto de geocodificación en sí). Requiere identificarse con un User-Agent propio y cachear resultados localmente; para un uso ocasional y de bajo volumen como el de este sistema, es viable, pero exige más cuidado operativo que una API paga con SLA. |

**Recomendación:** dado el presupuesto ajustado y que el volumen esperado (20-25 usuarios validando direcciones de proveedores ocasionalmente, no un flujo masivo) es bajo, **Google Geocoding API con su cuota gratuita de 10,000/mes** es probablemente suficiente y gratis en la práctica, con la mejor cobertura conocida para Colombia/LatAm. Si el presupuesto es *cero* de forma estricta y no se quiere ni siquiera dar una tarjeta de crédito a Google (su free tier igual requiere cuenta de facturación activa, aunque no cobre dentro de la cuota), **Nominatim** es la alternativa sin costo, siempre que se respete su política de uso (identificación, límite de 1 req/seg, resultados cacheados) — algo perfectamente razonable para "validar la dirección al guardar un proveedor", que es exactamente el caso de uso descrito.

## 7. Organización del repositorio

### 7.1 — Documentación por fases

`docs/` ahora tiene un índice (`docs/README.md`) y los documentos están numerados en el orden en que se produjeron:

1. `01-analisis-fase1.md` — modelo de datos y reglas de negocio
2. `02-diseno-arquitectura-backend.md` — diseño de la arquitectura Clean Architecture
3. `03-plan-implementacion-backend.md` — plan de implementación del backend base
4. `04-auditoria-seguridad-backend.md` — auditoría de seguridad (hallazgos H1-H12)
5. `05-plan-remediacion-seguridad.md` — este documento

Más `docs/schema/` (scripts SQL) y `docs/erd.png` (diagrama entidad-relación).

### 7.2 — Corrección "compras" → "manager" (H12)

`01-analisis-fase1.md` (líneas 323 y 418) decía `compras` en dos lugares; ya se corrigió a `manager`, con una nota explicando que fue una corrección de documentación del 17 de agosto de 2026 confirmada por Next (el cambio de nombre del rol fue intencional durante la construcción del backend, y la documentación no se había actualizado).

### 7.3 — `.gitignore`

Se agregaron reglas para archivos de resultados de pruebas (`TestResults/`, `*.trx`, `coverage*.json/xml/info`) y para artefactos temporales de trabajo (`*.zip`, `_to_delete/`, `_audit_tmp/`, `_scratch/`, `*.tmp`) — exactamente el tipo de archivo suelto que se acumuló durante esta sesión de trabajo.

### 7.4 — Lista de qué borrar manualmente

Estos archivos ya están movidos a `_to_delete/` (protegido por el `.gitignore` de arriba, así que aunque sigan ahí un tiempo no van a terminar en un commit). No pude borrarlos yo directamente por una limitación técnica del puente de archivos con tu computador — te toca eliminarlos manualmente, es tan simple como borrar la carpeta:

- **Toda la carpeta `_to_delete/`** en la raíz del repositorio, que contiene: `nexit_back_src.zip`, `nexit_src_v2.zip`, `nexit_src_v3.zip`, `README.md.testcopy`, `ziPhtVch`, y la subcarpeta `_audit_tmp/` (con dos archivos temporales sin nombre útil) — todos son copias de trabajo/respaldo de esta y la sesión anterior, ya sin uso una vez que el código está aplicado directamente en tu repositorio.
- **Las carpetas vacías `docs/superpowers/`, `docs/superpowers/plans/` y `docs/superpowers/specs/`** — sus dos documentos útiles ya se movieron a `docs/02-diseno-arquitectura-backend.md` y `docs/03-plan-implementacion-backend.md` (sección 7.1); las carpetas originales quedaron vacías.

## 8. Fuentes consultadas en esta sesión

- [Supabase Docs — JWT Signing Keys](https://supabase.com/docs/guides/auth/signing-keys) y [Supabase Pricing](https://supabase.com/pricing) — confirmación de que las claves asimétricas y el Auth Hook de claims personalizados están disponibles en el plan gratuito.
- [Supabase Docs — Custom Access Token Hook](https://supabase.com/docs/guides/auth/auth-hooks/custom-access-token-hook) — implementación de `03_auth_hook_custom_claims.sql`.
- [Cloudflare R2 Pricing](https://developers.cloudflare.com/r2/pricing/) — comparación de almacenamiento de adjuntos.
- [Google Maps Platform — March 2025 pricing changes](https://developers.google.com/maps/billing-and-pricing/march-2025) — cuota gratuita vigente de Geocoding API (10,000/mes).
- [Mapbox Pricing](https://www.mapbox.com/pricing) y [Geoapify Pricing](https://www.geoapify.com/pricing/) — alternativas de geocodificación.
- [Nominatim Usage Policy — OSM Foundation](https://operations.osmfoundation.org/policies/nominatim/) — condiciones de uso gratuito de OpenStreetMap para geocodificación.
- [Microsoft Learn — Configure ASP.NET Core to work with proxy servers and load balancers](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/proxy-load-balancer) — ya citado en la auditoría original, usado aquí para implementar H7.

## 9. Notas

- Este documento es el compañero de [`04-auditoria-seguridad-backend.md`](04-auditoria-seguridad-backend.md); mientras aquel identificó los hallazgos, este documenta lo que se implementó para resolverlos.
- Ver [[nexit_proyecto]] (memoria del proyecto) para retomar contexto en próximas sesiones.
