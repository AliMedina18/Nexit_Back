# Nexit · Modelo de permisos de 4 niveles

**Proyecto:** Sistema de gestión de la información para la organización de proyectos de trabajo
**Nombre del sistema:** Nexus (nombre de trabajo, sujeto a cambio)
**Cliente/organización:** Next — agencia de marketing experiencial
**Fecha:** 2026-08-18

## 1. Por qué existe este documento

El backend arrancó con un modelo de 3 roles (`admin`/`manager`/`miembro`), implementado en la sesión de auditoría/remediación de seguridad del 2026-08-17 (ver documentos 02 y 05). Al revisarlo con la usuaria, describió un modelo más específico de **4 niveles**, con dos ideas nuevas que el modelo de 3 roles no cubría:

1. Un nivel por encima de `admin` — **super_admin** — exclusivo de quien desarrolla/administra el sistema, y el único que puede *gestionar* (crear/editar/eliminar) cuentas de usuario. **Actualizado 2026-08-26:** *ver* esas cuentas es más amplio -- `admin` ve el directorio completo, y cualquier autenticado puede ver el perfil individual de cualquier compañero (solo lectura, como el directorio de Microsoft Teams) -- ver sección 6.
2. El concepto de que un **gerente es "dueño" de un proyecto** cuando lo lidera, con un **flujo de solicitud de eliminación** de dos pasos (gerente dueño → administrador) para proyectos, clientes y proveedores.

Este documento describe el modelo resultante, las decisiones de diseño tomadas para llenar los huecos que la descripción original no cubría explícitamente, y dónde vive cada pieza en el código.

La fuente de este modelo es la descripción directa de la usuaria (dueña del producto), no un flujo que le haya confirmado su compañera de equipo — inicialmente se preparó un cuestionario para esa compañera (`docs/preguntas-permisos-roles-para-companera.md`), pero la usuaria terminó respondiendo ella misma, de forma explícita y cerrando con "esos serían todos los permisos de usuario". Se trata como la respuesta final y autoritativa.

## 2. Los 4 roles

| Rol | Quién es | Qué puede hacer |
|---|---|---|
| `super_admin` | La desarrolladora/administradora del sistema. Un único rol, pensado para muy pocas personas (idealmente una). | Todo. Es el único rol que puede *gestionar* la tabla de usuarios (crear, editar rol/estado, eliminar) -- *ver* el directorio ya no es exclusivo suyo, ver sección 6. También hereda todo lo que puede hacer `admin`. |
| `admin` | Administración operativa del negocio, sin poder *gestionar* (crear/editar/eliminar) cuentas de usuario -- desde 2026-08-26 sí puede *verlas*. | Todo lo de catálogos, clientes, proveedores, proyectos, informes, más ver (no gestionar) el directorio completo de usuarios (`GET /api/usuarios`, sección 6). Decide (aprueba/rechaza) las solicitudes de eliminación de clientes/proveedores/proyectos. |
| `manager` (gerente) | Un miembro del equipo con un rol adicional: puede ser el "dueño" de uno o más proyectos. | Todo lo de clientes/proveedores/proyectos (crear, ver, editar) igual que un miembro, más: es dueño de los proyectos donde quedó asignado como `gerente_id`, y decide (endosa/rechaza) las solicitudes de eliminación de esos proyectos antes de que lleguen a un administrador. Puede ver el perfil individual de cualquier compañero, solo lectura (sección 6). |
| `miembro` | El equipo de marketing / usuarios normales de la aplicación. | Ver y trabajar con clientes, proveedores y proyectos (crear, editar). No puede eliminar directamente ninguno de los tres — debe pasar por una solicitud de eliminación. Puede ver el perfil individual de cualquier compañero, solo lectura (sección 6). |

Nota textual de la usuaria que resume la relación manager/miembro: *"el gerente es un miembro, solo que con un rol más arriba, porque es el dueño del proyecto"* — por eso `manager` no es un rol aparte con permisos distintos de `miembro` sobre clientes/proveedores/proyectos (ambos pueden crear/ver/editar los tres por igual); lo único que distingue a un `manager` es que puede quedar como `gerente_id` de un proyecto y, cuando lo es, participa en el flujo de aprobación de su eliminación.

`usuarios.rol` sigue gobernado por un CHECK constraint (`ck_usuarios_rol`), ahora `rol IN ('super_admin', 'admin', 'manager', 'miembro')` — ver `src/Nexit.Infrastructure/Data/NexitDbContext.cs` y la migración `AddRbacFourTierRoles`. El script SQL de referencia para una instalación nueva de Supabase (`docs/schema/nexus_schema_v2.sql`) usa el mismo conjunto de valores en el ENUM `rol_usuario`.

## 3. Políticas de autorización

Dos políticas estáticas de ASP.NET Core (`src/Nexit.API/Program.cs`), evaluadas contra el claim `app_role`/`user_role` del JWT (agregado por el Auth Hook de Supabase, `docs/schema/03_auth_hook_custom_claims.sql`):

- **`SuperAdminOnly`** — únicamente `super_admin`. Protege crear/editar/eliminar en `UsuariosController` (`POST`/`PUT`/`DELETE`) -- desde 2026-08-26 ya no protege el controlador completo, ver sección 6.
- **`AdminOrAbove`** — `admin` o `super_admin`. Protege la administración directa de catálogos, la eliminación directa (sin pasar por una solicitud) de catálogos y adjuntos, las decisiones finales (`aprobar`/`rechazar`) sobre solicitudes de eliminación, y (desde 2026-08-26) listar el directorio completo de usuarios (`GET /api/usuarios`).

Todo lo demás (crear/ver/editar clientes, proveedores, proyectos; crear una solicitud de eliminación; endosar/rechazar una solicitud como gerente) solo exige estar autenticado — no hay una política estática más restrictiva porque la restricción real depende de datos en tiempo de ejecución (¿soy el gerente dueño de *este* proyecto en particular?), algo que una política estática de ASP.NET Core no puede expresar. Esas verificaciones viven como reglas de negocio dentro de los casos de uso, y lanzan `ForbiddenOperationException` (nueva, mapea a HTTP 403) cuando no se cumplen — a diferencia de `BusinessRuleException` (409, conflictos de datos/negocio) y `EntityNotFoundException` (404).

## 4. El gerente "dueño" de un proyecto

`Proyecto.GerenteId` (nullable, FK a `usuarios.id`, `ON DELETE SET NULL`) representa quién es el gerente dueño/líder de un proyecto.

Reglas de asignación (`CrearProyectoUseCase`/`ActualizarProyectoUseCase`, `src/Nexit.Application/UseCases/Proyectos/ProyectoUseCases.cs`):

- Al **crear** un proyecto: si quien lo crea ya tiene rol `manager`, queda asignado automáticamente como su propio `gerente_id` (así se cumple "cuando ya eres gerente y lideras el proyecto, ese proyecto es tuyo", sin depender de un paso manual adicional). Si quien lo crea es `admin`/`super_admin`, puede indicar explícitamente cualquier `gerente_id` (incluido ninguno). Si quien lo crea es `miembro`, el proyecto queda sin gerente (`null`).
- **Reasignar** el `gerente_id` de un proyecto ya existente (cambiarlo a una persona distinta) es exclusivo de `admin`/`super_admin` — ni el propio gerente dueño ni cualquier otro `manager` pueden reasignarlo. Intentarlo lanza `ForbiddenOperationException`.
- Un proyecto sin `gerente_id` (`null`) no tiene "dueño": su eliminación, si alguien la solicita, va directo a un administrador (ver sección 5).

### Decisión de diseño: un solo gerente dueño por proyecto

La descripción de la usuaria habla de "el gerente" en singular ("tú directamente tienes tu proyecto"), así que el modelo asume **un único dueño por proyecto**, no una lista. `proyecto_equipo` (el equipo del proyecto) sigue siendo texto libre sin vínculo a cuentas reales de `usuarios` — eso no cambió; `gerente_id` es un campo aparte, pensado específicamente para la mecánica de aprobación de eliminación, no para reemplazar el equipo del proyecto. Si más adelante Next necesita más de un gerente responsable por proyecto (co-liderazgo), el cambio natural sería una tabla intermedia `proyecto_gerentes` en vez de una columna — no está implementado porque la usuaria no lo describió así.

## 5. Flujo de solicitudes de eliminación

Ni `manager` ni `miembro` pueden eliminar directamente un cliente, proveedor o proyecto — en su lugar, crean una **solicitud de eliminación** (tabla `solicitudes_eliminacion`, `SolicitudesEliminacionController`). El flujo tiene hasta dos pasos, según la descripción textual de la usuaria: *"si otra persona trata de eliminar ese proyecto, no puede, te tiene que pedir permiso a ti [el gerente dueño]. Y tú para eliminar ese proyecto le tienes que pedir permiso a la administradora [...] Es así, con los clientes, proveedores y proyectos también."*

Estados posibles (`solicitudes_eliminacion.estado`): `pendiente_gerente` → `pendiente_admin` → `aprobada` / `rechazada`.

```
Alguien pide eliminar un cliente/proveedor/proyecto
        │
        ▼
¿Es un proyecto con gerente_id asignado, y quien pide NO es ese gerente?
        │
   sí ──┴── no (cliente, proveedor, proyecto sin gerente, o lo pide el propio gerente dueño)
   │                              │
   ▼                              ▼
pendiente_gerente          pendiente_admin
   │  (el gerente dueño          │
   │   aprueba o rechaza)        │
   ▼                              │
pendiente_admin ───────────────►│
   │                              │
   ▼                              ▼
   (un admin/super_admin aprueba o rechaza — al aprobar, se ejecuta el DELETE real)
```

Endpoints (`SolicitudesEliminacionController`):

| Endpoint | Quién | Qué hace |
|---|---|---|
| `POST /api/solicitudeseliminacion` | Cualquier autenticado | Crea la solicitud. El backend decide sola el estado inicial (`pendiente_gerente` o `pendiente_admin`) según la regla de arriba — quien la crea no lo elige. |
| `GET /api/solicitudeseliminacion` | `AdminOrAbove` | Lista todas las solicitudes. |
| `GET /api/solicitudeseliminacion/pendientes-para-mi` | Cualquier autenticado | Lista las solicitudes `pendiente_gerente` donde el usuario autenticado es el `gerente_responsable_id`. |
| `GET /api/solicitudeseliminacion/{id}` | Cualquier autenticado | Detalle de una solicitud. |
| `PUT /api/solicitudeseliminacion/{id}/aprobar-gerente` | Cualquier autenticado (la política estática no filtra; la verificación de que sea el gerente dueño ocurre en el caso de uso) | Solo tiene efecto si el usuario es el `gerente_responsable_id` de esa solicitud y su estado es `pendiente_gerente`; la mueve a `pendiente_admin`. Si no, lanza `ForbiddenOperationException` (403) o `BusinessRuleException` (409). |
| `PUT /api/solicitudeseliminacion/{id}/rechazar-gerente` | Igual que arriba | La marca `rechazada` (fin del flujo, no llega a un admin). |
| `PUT /api/solicitudeseliminacion/{id}/aprobar` | `AdminOrAbove` | Solo tiene efecto sobre solicitudes en `pendiente_admin`. Ejecuta el `DELETE` real de la entidad (cliente/proveedor/proyecto) y marca `aprobada`. Si la entidad ya no existe (alguien más ya la borró), no vuelve a intentar el `DELETE` — solo marca la solicitud como `aprobada`. |
| `PUT /api/solicitudeseliminacion/{id}/rechazar` | `AdminOrAbove` | La marca `rechazada` sin borrar nada. |

### Por qué la verificación de "eres el gerente dueño" no es una política estática

Las políticas de ASP.NET Core (`SuperAdminOnly`, `AdminOrAbove`) solo pueden mirar claims del JWT — no pueden consultar "¿esta solicitud en particular tiene a este usuario como `gerente_responsable_id`?". Por eso `aprobar-gerente`/`rechazar-gerente` quedan abiertos a cualquier autenticado a nivel de política, y la comprobación real vive dentro de `AprobarComoGerenteUseCase`/`RechazarComoGerenteUseCase` (`src/Nexit.Application/UseCases/SolicitudesEliminacion/SolicitudEliminacionUseCases.cs`), devolviendo 403 si no corresponde.

### Decisiones de diseño en el flujo

- **Catálogos y adjuntos no pasan por este flujo.** La usuaria describió el flujo de aprobación específicamente para *"clientes, proveedores y proyectos"*; catálogos (países, categorías, estados de proyecto, etc.) y adjuntos de proveedor no tienen el concepto de "dueño" y siguen su eliminación directa existente, restringida a `AdminOrAbove`.
- **Un proyecto sin gerente asignado, o cuyo propio gerente dueño pide la eliminación, va directo a `pendiente_admin`** (salta el paso intermedio) — no tendría sentido pedirle permiso a un gerente que no existe, ni pedirle a alguien que se apruebe a sí mismo.
- **`AprobarComoAdminUseCase` es case-insensitive respecto a si la entidad ya fue borrada por otra vía** (por ejemplo, un admin la eliminó directo mientras la solicitud seguía pendiente): revisa que exista antes de intentar el `DELETE`, así una aprobación tardía no lanza un error por "entidad no encontrada".
- **La tabla `solicitudes_eliminacion` es un histórico de decisiones, no una papelera de reciclaje.** No hay una operación de "deshacer" ni de restaurar — al aprobar, el `DELETE` es real y definitivo (hard delete), igual que la eliminación directa que ya usaban `admin`/`super_admin`.

## 6. Gestión de usuarios

`UsuariosController` expone CRUD completo sobre `usuarios`, pero ya no con una sola política de clase -- desde 2026-08-26 hay tres niveles de acceso dentro del mismo controlador:

- **Ver el directorio completo** (`GET /api/usuarios`): `AdminOrAbove` -- `admin` y `super_admin`.
- **Ver un perfil individual, el propio o el de otra persona** (`GET /api/usuarios/me`, `GET /api/usuarios/{id}`): cualquier autenticado, sin importar el rol -- solo lectura, como mirar el perfil de un compañero en el directorio de Microsoft Teams. No expone forma de editar ni eliminar.
- **Crear/editar/eliminar** (`POST`/`PUT`/`DELETE`): `SuperAdminOnly`, sin cambios.

- **Crear** (`POST /api/usuarios`): recibe el `Id` (el UUID que Supabase Auth ya le asignó a la cuenta) además de nombre/apellido/email/rol — este backend nunca crea contraseñas ni envía invitaciones; ese flujo vive en Supabase Auth (Authentication → Users → Invite). La súper administradora primero invita a la persona desde el dashboard de Supabase, y luego registra aquí su perfil de negocio con ese mismo UUID.
- **Editar** (`PUT /api/usuarios/{id}`): nombre, apellido, rol, iniciales, activo.
- **Eliminar** (`DELETE /api/usuarios/{id}`): hard delete real (no lógico). Es seguro a nivel de base de datos porque las columnas de auditoría que referencian a `usuarios` (`created_by`, `autor_id`, etc.) están configuradas con `ON DELETE SET NULL` — al borrar un usuario, su historial (proyectos creados, notas de seguimiento) queda intacto, solo pierde el vínculo a "quién lo hizo".
- **Desactivar sin eliminar**: `PUT` con `activo = false` — para dar de baja a alguien temporalmente sin perder su historial ni su fila.

### Protecciones de auto-bloqueo

Para que nadie deje el sistema sin ningún `super_admin` que pueda administrar usuarios, `ActualizarUsuarioUseCase`/`EliminarUsuarioUseCase` bloquean, cuando quien hace la operación es el mismo usuario objetivo (`id == callerId`):

- Desactivar su propia cuenta (`activo = false`).
- Quitarse a sí mismo el rol `super_admin`.
- Eliminar su propia cuenta.

Cualquiera de estas lanza `ForbiddenOperationException` (403). Un `super_admin` sí puede editar sus propios datos no sensibles (nombre, apellido, iniciales) mientras se mantenga activo y con rol `super_admin`.

## 7. Dónde vive cada pieza (referencia rápida)

| Pieza | Archivo(s) |
|---|---|
| Constantes de rol | `src/Nexit.Core/Constants/Roles.cs` |
| CHECK constraint de `usuarios.rol` | `src/Nexit.Infrastructure/Data/NexitDbContext.cs` |
| `Proyecto.GerenteId` | `src/Nexit.Core/Entities/Proyecto.cs`, `NexitDbContext.cs`, `ProyectoDtos.cs`, `ProyectoUseCases.cs` |
| Entidad `SolicitudEliminacion` | `src/Nexit.Core/Entities/SolicitudEliminacion.cs` |
| Repositorios | `src/Nexit.Core/Interfaces/IUsuarioRepository.cs`, `ISolicitudEliminacionRepository.cs`, y sus implementaciones en `src/Nexit.Infrastructure/Repositories/` |
| `ForbiddenOperationException` → 403 | `src/Nexit.Core/Exceptions/ForbiddenOperationException.cs`, `src/Nexit.API/Middleware/GlobalExceptionHandlerMiddleware.cs` |
| Políticas `SuperAdminOnly`/`AdminOrAbove` | `src/Nexit.API/Program.cs` |
| Casos de uso de usuarios | `src/Nexit.Application/UseCases/Usuarios/` |
| Casos de uso de solicitudes de eliminación | `src/Nexit.Application/UseCases/SolicitudesEliminacion/` |
| Controladores | `src/Nexit.API/Controllers/UsuariosController.cs`, `SolicitudesEliminacionController.cs` |
| Migración EF Core | `src/Nexit.Infrastructure/Migrations/20260818015920_AddRbacFourTierRoles.cs` |
| Esquema SQL de referencia (instalación nueva de Supabase) | `docs/schema/nexus_schema_v2.sql` (nota de diseño 17), `docs/schema/03_auth_hook_custom_claims.sql` |
| Pruebas | `tests/Nexit.Tests/UsuariosTests.cs`, `SolicitudesEliminacionTests.cs`, casos nuevos en `ProyectosTests.cs`, e integración en `tests/Nexit.Tests/Integration/AuthorizationIntegrationTests.cs` |

## 8. Supuestos y alcance (para revisar si el negocio cambia)

- Un proyecto tiene **como máximo un** gerente dueño (`gerente_id` es una sola columna, no una relación muchos-a-muchos). Ver sección 4.
- **Catálogos y adjuntos de proveedor** siguen su eliminación directa `AdminOrAbove`, fuera del flujo de solicitudes — la usuaria solo describió el flujo de aprobación para clientes/proveedores/proyectos.
- **`manager` y `miembro` tienen los mismos permisos** sobre clientes/proveedores/proyectos (crear/ver/editar) — lo único que distingue a un `manager` es la posibilidad de ser `gerente_id` de un proyecto y participar en la aprobación de su eliminación. No hay restricciones adicionales de edición basadas en pertenencia al equipo de un proyecto (`proyecto_equipo` sigue sin vínculo a cuentas de `usuarios`).
- **Reasignar el gerente dueño de un proyecto** (cambiarlo después de creado) es exclusivo de `admin`/`super_admin` — ni el propio gerente ni otro `manager` pueden hacerlo.
- El origen de estas reglas es la descripción directa de la usuaria en esta sesión, no una confirmación explícita de su compañera de equipo (se preparó un cuestionario para ella, `docs/preguntas-permisos-roles-para-companera.md`, pero la usuaria contestó ella misma antes de reenviarlo). Si al usarlo en la práctica el equipo encuentra un caso que este documento no cubre, hay que volver a ajustar el modelo.
