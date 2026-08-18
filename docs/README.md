# Documentación de Nexit — índice por fase

Este índice existe para que cualquiera (incluida una futura sesión) pueda retomar el proyecto sin tener que adivinar en qué orden leer los documentos. Cada archivo lleva un número que refleja el orden en que se hicieron las cosas, no necesariamente el orden en que conviene leerlas si solo quieres entender "dónde estamos hoy" — para eso, empieza por el resumen de abajo y entra al documento que te interesa.

## Resumen de dónde estamos

1. Se analizó a fondo la operación real de Next (Excel históricos + prototipo HTML) y se diseñó el modelo de datos → **Fase 1, completa.**
2. Se diseñó y construyó el backend (Clean Architecture, .NET 8): las 4 capas, los controladores, autenticación JWT, autorización → **Fase 2 (construcción del backend), en marcha y bastante avanzada.**
3. Se auditó ese backend (funcionalidad + seguridad) y se aplicaron las correcciones que no dependen de que exista todavía un proyecto de Supabase real → **hecho el 2026-08-17, ver el documento 02 y el 05.**
4. Se diseñó e implementó el modelo de permisos de 4 niveles (super_admin/admin/manager/miembro), con el flujo de solicitudes de eliminación para clientes/proveedores/proyectos → **hecho el 2026-08-18, ver el documento 06.**
5. Se construyó la lógica de backend del calendario de proyectos (por mes/año, eficiente), se restringieron los informes a solo super_admin/admin, y se agregó exportación a Excel de informes → **hecho el 2026-08-18, ver el documento 07.**
6. Se agregó la capa de pruebas funcionales (Postgres real con Testcontainers, encontró y corrigió un bug real de guardado) y una capa de pruebas de seguridad (autorización por defecto, límite de peticiones, inyección SQL, mass assignment, autorización a nivel de objeto), y se eliminó una dependencia vulnerable sin usar (AutoMapper) → **hecho el 2026-08-18, ver el documento 08.**
7. Pendiente: crear el proyecto de Supabase, terminar de conectar los puntos que quedaron condicionados a que exista (firma de JWT, rol de base de datos), y avanzar con el frontend (incluida la vista visual del calendario) y el despliegue.

## Documentos

| # | Documento | Qué contiene |
|---|---|---|
| 01 | [`01-analisis-fase1.md`](01-analisis-fase1.md) | Análisis de la operación real de Next, comparación contra el prototipo de Luisa, y el modelo de datos completo (20 tablas, normalizado 1FN-4FN, con el modelo relacional formal). |
| 02 | [`02-auditoria-seguridad-backend.md`](02-auditoria-seguridad-backend.md) | Auditoría de funcionalidad y seguridad del backend ya construido: qué compila, qué pasa en pruebas, y 12 hallazgos priorizados (H1-H12). |
| 05 | [`05-plan-remediacion-seguridad.md`](05-plan-remediacion-seguridad.md) | Qué se corrigió de esos 12 hallazgos, cómo, qué queda pendiente y por qué (casi todo lo pendiente depende de que exista un proyecto de Supabase real), más la investigación de APIs públicas para almacenamiento de adjuntos y geolocalización. |
| 06 | [`06-modelo-permisos-roles.md`](06-modelo-permisos-roles.md) | Modelo de permisos de 4 niveles (super_admin/admin/manager/miembro): matriz de permisos, el concepto de gerente "dueño" de un proyecto, y el flujo de solicitudes de eliminación para clientes/proveedores/proyectos. |
| 07 | [`07-calendario-e-informes-excel.md`](07-calendario-e-informes-excel.md) | Backend del calendario de proyectos (por mes/año, agregación eficiente sin cargar entidades completas), restricción de Informes a solo super_admin/admin, y exportación a Excel de informes con ClosedXML. |
| 08 | [`08-tipos-de-pruebas.md`](08-tipos-de-pruebas.md) | La pirámide de pruebas completa: unitarias, integración, funcionales (Postgres real con Testcontainers, incluye el bug real que encontró) y seguridad (autorización por defecto, límite de peticiones, inyección, mass assignment, BOLA), más el hallazgo y limpieza de una dependencia vulnerable sin usar. |
| — | [`preguntas-permisos-roles-para-companera.md`](preguntas-permisos-roles-para-companera.md) | Preguntas preparadas para la compañera de equipo sobre el flujo de permisos — la usuaria terminó respondiéndolas ella misma directamente (ver documento 06), quedan aquí como referencia. |
| — | [`schema/nexus_schema_v2.sql`](schema/nexus_schema_v2.sql) | El esquema SQL de referencia para crear la base de datos en Supabase (tablas, triggers, Row Level Security), incluido el modelo de permisos de 4 niveles y solicitudes_eliminacion (novena revisión). Este es el que se ejecuta una sola vez al crear el proyecto Supabase — no se aplica con las migraciones de Entity Framework Core. |
| — | [`schema/02_rol_aplicacion_minimo_privilegio.sql`](schema/02_rol_aplicacion_minimo_privilegio.sql) | Crea el rol `nexit_app` de mínimo privilegio que usa el backend para conectarse (hallazgo H2). |
| — | [`schema/03_auth_hook_custom_claims.sql`](schema/03_auth_hook_custom_claims.sql) | Auth Hook de Supabase que agrega el rol del usuario (`user_role`) al JWT (hallazgo H3) — ahora con los 4 valores posibles. |
| — | [`schema/seed_geografia_categorias_estados.sql`](schema/seed_geografia_categorias_estados.sql) | Datos iniciales de catálogos (países, regiones, ciudades, categorías de proveedor, estados de proyecto). |
| — | [`erd.png`](erd.png) | Diagrama entidad-relación del modelo de datos. |

## Convención para documentos futuros

Cuando se agregue un documento nuevo de una fase o revisión importante, se numera siguiendo el consecutivo (`06-...`, `07-...`) y se agrega una fila a esta tabla. Evita crear carpetas nuevas para "una sesión" o "una herramienta" (como pasó con `superpowers/`, que ya se reorganizó dentro de esta numeración) — todo documento de proyecto vive directamente en `docs/`, plano, numerado.
