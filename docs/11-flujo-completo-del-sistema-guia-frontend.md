# El flujo completo del sistema, módulo por módulo — guía para el frontend

Este documento retoma la pregunta original de la usuaria completa: no solo correos/login, sino **todo el recorrido de alguien usando el sistema** — desde que entra, hasta cada CRUD (clientes, proveedores, proyectos, calendario, catálogos, informes, usuarios, solicitudes de eliminación) — con el detalle exacto de campos, reglas y permisos que ya están construidos en el backend, para que el frontend (otro repositorio) se pueda construir sobre esto sin adivinar.

## 0. Punto de entrada: iniciar sesión

Ya está documentado a fondo en `docs/10-correos-autenticacion-y-guia-frontend.md` — el resumen que hace falta para seguir leyendo: la persona entra con correo+contraseña (solo la super administradora) o correo+código de 6 dígitos (todos los demás), **directo contra Supabase Auth desde el frontend**, sin pasar por este backend. Una vez tiene el JWT de Supabase, lo manda como `Authorization: Bearer <token>` en cada llamada a `Nexit_Back`. El backend lee de ese token el rol de negocio (`super_admin`/`admin`/`manager`/`miembro`) y con eso decide qué puede hacer la persona en cada uno de los módulos de abajo.

**Todo endpoint de la API exige sesión iniciada** (no hay ninguna ruta pública) — si el token falta o es inválido, cualquier llamada responde `401`.

## 1. Los 4 roles, en una tabla

| Rol | Puede | No puede |
|---|---|---|
| `super_admin` | Todo, incluida la tabla `usuarios` (crear/editar/desactivar/eliminar cuentas) | — |
| `admin` | Todo excepto `usuarios`; administra catálogos; decide (aprueba/rechaza) solicitudes de eliminación de clientes/proveedores/proyectos; elimina directamente catálogos y adjuntos | Gestionar usuarios |
| `manager` (gerente) | Crear/ver/editar clientes, proveedores, proyectos; puede ser el "dueño" (`GerenteId`) de uno o más proyectos, y si lo es, endosa o rechaza solicitudes de eliminación de esos proyectos | Eliminar directamente clientes/proveedores/proyectos (pasa por solicitud); gestionar usuarios ni catálogos |
| `miembro` | Crear/ver/editar clientes, proveedores, proyectos | Eliminar directamente nada de eso (pasa por solicitud); gestionar usuarios ni catálogos |

Dos políticas de autorización cubren todo esto en el código: `SuperAdminOnly` (solo `usuarios`) y `AdminOrAbove` (`admin` o `super_admin`, para catálogos, informes, eliminación directa). Todo lo demás solo exige estar autenticado, sin importar el rol.

## 2. Clientes — `/api/clientes`

| Acción | Método | Quién |
|---|---|---|
| Listar todos | `GET /api/clientes` | Cualquier autenticado |
| Ver uno | `GET /api/clientes/{id}` | Cualquier autenticado |
| Crear | `POST /api/clientes` | Cualquier autenticado |
| Editar | `PUT /api/clientes/{id}` | Cualquier autenticado |
| Eliminar directo | `DELETE /api/clientes/{id}` | Solo `AdminOrAbove` — `manager`/`miembro` usan el flujo de solicitudes (sección 9) |

**Campos:** `nombre` (obligatorio), `sector`, `ciudad` (texto libre, no ligado al catálogo de ciudades), `direccion`, `web`, `contacto`, `cargoContacto`, `email` (si se llena, debe ser válido y único entre clientes), `valorReferencia` (texto), `notas`, y `telefonos` (lista de `{id?, telefono, etiqueta?}` — **al menos uno es obligatorio**, es la única entidad donde el teléfono es requerido).

`GET /api/clientes` no acepta filtros, búsqueda ni paginación hoy — devuelve la lista completa siempre; cualquier búsqueda/orden es responsabilidad del frontend sobre esa lista completa (ver sección 10).

**Detalle importante para el formulario de edición:** al editar, hay que mandar la lista `telefonos` completa (los que ya existían más los nuevos) — el backend **reemplaza** toda la colección, no aplica un diff. Un teléfono que no viene en el `PUT` desaparece.

## 3. Proveedores — `/api/proveedores`

| Acción | Método | Quién |
|---|---|---|
| Listar / ver uno | `GET /api/proveedores`, `GET /api/proveedores/{id}` | Cualquier autenticado |
| Crear / editar | `POST` / `PUT /api/proveedores/{id}` | Cualquier autenticado |
| Eliminar directo | `DELETE /api/proveedores/{id}` | Solo `AdminOrAbove` — resto usa solicitud |

**Campos:** `nombre` (obligatorio), `paisId` (obligatorio, catálogo), `regionId`/`ciudadId` (opcionales, catálogo — pero si mandas región debe pertenecer al país, y si mandas ciudad debe pertenecer a esa región, si no el backend responde `409`), `categoriaId` (obligatorio, catálogo), `estado` (uno de `Activo` / `En evaluación` / `Pausado` / `Bloqueado`, default `Activo`), `contacto`, `cargoContacto`, `email` (único entre proveedores si se llena), `web`, `direccion`, `aforo` (número), `costoReferencia` (texto), `score` (1 a 5, opcional), `presupuesto` (opcional: `$ Bajo (<20k)` / `$$ Medio (20k–100k)` / `$$$ Alto (100k–500k)` / `$$$$ Premium (>500k)`), `cobertura` (opcional: `Solo ciudad` / `Regional` / `Nacional` / `Internacional`), `notas`, `telefonos` (igual patrón que clientes, pero aquí **no es obligatorio** tener al menos uno), y `servicioIds` (lista de IDs del catálogo de servicios, muchos-a-muchos).

Igual que clientes: al editar, `telefonos` y `servicioIds` se **reemplazan por completo** con lo que mandes en el `PUT`.

### 3.1. Adjuntos de un proveedor — `/api/proveedores/{proveedorId}/adjuntos`

Cada proveedor puede tener adjuntos de dos tipos: `link` (una URL — debe ser `http://` o `https://`, cualquier otro esquema como `javascript:` se rechaza por seguridad) o `file` (requiere `storagePath`, es decir, este backend no sube el archivo — solo guarda la referencia a dónde quedó, hoy pensado para Supabase Storage aunque todavía no está integrado — ver `docs/05-plan-remediacion-seguridad.md`, sección 6). Campos: `tipo`, `nombre` (obligatorio), `url`, `storagePath`, `meta` (texto libre), `fecha`.

`GET`/`POST` los puede usar cualquier autenticado; `DELETE /api/proveedores/{proveedorId}/adjuntos/{id}` es `AdminOrAbove`.

## 4. Proyectos — `/api/proyectos`

| Acción | Método | Quién |
|---|---|---|
| Listar / ver uno | `GET /api/proyectos`, `GET /api/proyectos/{id}` | Cualquier autenticado |
| Crear / editar | `POST` / `PUT /api/proyectos/{id}` | Cualquier autenticado (con la excepción del gerente, ver abajo) |
| Eliminar directo | `DELETE /api/proyectos/{id}` | Solo `AdminOrAbove` — resto usa solicitud |
| Agregar una nota de seguimiento | `POST /api/proyectos/{id}/seguimiento` | Cualquier autenticado |

**Campos:** `nombre` (obligatorio), `clienteId` (opcional, debe existir), `contactoProyecto`, `tipoProyecto` (opcional: `Corporativo` / `Evento social`), `prioridad` (opcional: `Alta` / `Media` / `Baja`), `ciudad` (texto libre, no ligado a catálogo), `sedeNext` (sede de Next que atiende), `fechaSolicitud`, `fechaEvento` (esta es la fecha que usa el calendario, sección 5), `estadoId` (obligatorio, catálogo de estados — ver sección 6), `porcentajeAvance` (0-100), `estadoBrief` (uno de `Pendiente por enviar` / `Entregado, a espera de respuesta` / `Requiere ajustes` / `Aprobado`, default `Pendiente por enviar`), `propuestaEstado` (uno de `No enviada` / `En proceso` / `Enviada`, default `No enviada`), `numeroFactura`, `pagado` (booleano — si es `true`, `fechaPago` pasa a ser obligatoria), `fechaPago`, `notas`, `gerenteId` (ver abajo), `equipo` (lista de `{id?, rol, nombre}` — `rol` debe ser uno de `Ejecutivo` / `Comercial` / `Administrativo` / `Diseñador 3D` / `Diseñador gráfico`; `nombre` aquí es texto libre, **no** un `usuarioId` — a propósito, porque no todo responsable de un proyecto tiene cuenta en el sistema), y `proveedorIds` (lista de IDs de proveedores asociados al proyecto).

**El campo `gerenteId` tiene una regla especial que el frontend necesita conocer:**
- Al **crear** un proyecto: si quien lo crea ya es `manager` y no mandó `gerenteId`, el backend lo asigna automáticamente a sí mismo como dueño. Si quien crea es `admin`/`super_admin`, puede mandar cualquier `gerenteId` (o dejarlo vacío). Si quien crea es `miembro`, el proyecto queda sin gerente sin importar lo que mande.
- Al **editar**: cambiar el `gerenteId` de un proyecto ya existente es exclusivo de `admin`/`super_admin` — si un `manager` o `miembro` intenta mandar un `gerenteId` distinto al que ya tenía el proyecto, el backend responde `403`.

Igual que en clientes/proveedores, `equipo` y `proveedorIds` se **reemplazan por completo** en cada `PUT`.

### 4.1. Bitácora de seguimiento del proyecto

`POST /api/proyectos/{id}/seguimiento` agrega una nota (no reemplaza nada, esta sí es un historial que crece — a diferencia de teléfonos/equipo). Campos: `area` (uno de `General` / `Creativo` / `Comercial` / `Administrativo`), `fecha` (opcional, default ahora), `nota` (obligatoria). Queda registrado quién la escribió (`autorId`, el usuario autenticado) y cuándo. No hay endpoint para editar ni borrar una nota de seguimiento ya creada — es un historial de solo-agregar.

## 5. Calendario de proyectos — `/api/calendario`

Cualquier autenticado, sin restricción de rol. Pensado para ser liviano: 3 endpoints separados en vez de uno que traiga todo.

`GET /api/calendario/anios` — lista los años que tienen al menos un proyecto con `fechaEvento` (para poblar el selector de año, sin inventar un rango fijo).

`GET /api/calendario/{anio}` — resumen de un año: siempre trae los 12 meses (enero a diciembre) con la cantidad de proyectos de cada uno, aunque algunos meses tengan 0 — así el frontend pinta la grilla completa sin huecos que adivinar. Es solo un conteo (`GROUP BY`), no trae los proyectos.

`GET /api/calendario/{anio}/{mes}` — la lista de proyectos de ese mes específico (una versión liviana: id, nombre, fecha, cliente, estado, prioridad, ciudad, sede — sin equipo/proveedores/seguimiento). Se pide solo cuando alguien entra a ver ese mes.

**El campo que ubica un proyecto en el calendario es `fechaEvento`, no `fechaSolicitud`.**

## 6. Catálogos — `/api/catalogos`

Todos los `GET` son para cualquier autenticado (para poblar selects en los formularios de arriba); todo `POST`/`PUT`/`DELETE` es `AdminOrAbove`.

- **Países** (`/paises`): `nombre`, `etiquetaRegion` (cómo se le llama a la subdivisión de ese país — "Departamento" para Colombia, por ejemplo — para que el formulario de proveedor muestre la etiqueta correcta).
- **Regiones** (`/regiones?paisId=`): `nombre`, ligadas a un país.
- **Ciudades** (`/ciudades?regionId=`): `nombre`, ligadas a una región.
- **Categorías de proveedor** (`/categorias-proveedor`): solo `nombre`.
- **Servicios** (`/servicios`): solo `nombre` (el catálogo que usa el multi-select de servicios de un proveedor).
- **Fases de proyecto** (`/fases-proyecto`): son 3 fases fijas (1/2/3) con nombre editable, no se crean nuevas — solo `PUT` para renombrar una fase existente, no hay `POST`.
- **Estados de proyecto** (`/estados-proyecto?fase=`): `nombre`, `fase` (a cuál de las 3 pertenece), `orden` (para ordenarlos dentro de su fase en el selector).

`DELETE /api/catalogos/{tipo}/{id}` es genérico — `tipo` es uno de `paises`/`regiones`/`ciudades`/`categorias`/`servicios`/`estados` (nota: en plural y sin el sufijo "-proveedor"/"-proyecto" que sí llevan las rutas de arriba).

## 7. Informes — `/api/informes` (todo el controlador es `AdminOrAbove` — `manager`/`miembro` reciben `403` en cualquiera de estos)

`GET /api/informes/resumen` — datos en vivo, calculados en el momento: total de proveedores/clientes/proyectos, proyectos sin proveedor asociado, y dos desgloses (`porEstado`, `porBrief`) como diccionario `{"nombre del estado": cantidad}`.

`GET /api/informes/resumen/exportar` — el mismo resumen, como archivo `.xlsx` (3 hojas: Resumen, Por estado, Por brief) generado con ClosedXML — el navegador recibe el archivo directo para descargar.

`POST /api/informes/snapshots` — congela el resumen actual bajo un `tipo` (`semanal` o `mensual`) y un `periodoKey` (texto libre que identifica el período, por ejemplo `"2026-S34"` o `"2026-08"` — la convención exacta la define quien lo genera, el backend no la impone salvo que no puede repetirse un `tipo`+`periodoKey` ya usado). **Esto es manual** — no hay ninguna tarea programada que genere snapshots solos; alguien (`admin`/`super_admin`) tiene que llamar este endpoint cada semana/mes si quiere guardar el histórico.

`GET /api/informes/snapshots/{tipo}/{periodoKey}` y su versión `/exportar` — recupera un snapshot ya guardado (los datos quedan congelados tal como estaban al momento de crearlo, no se recalculan).

## 8. Usuarios — `/api/usuarios` (todo el controlador es `SuperAdminOnly`)

Ver `docs/10-correos-autenticacion-y-guia-frontend.md` para el detalle completo de por qué crear un usuario aquí no envía ningún correo (la invitación ya pasó antes, en Supabase). Campos de `POST`: `id` (el UUID que Supabase le asignó a la cuenta al aceptar la invitación — no lo genera este backend), `nombre`, `apellido`, `email` (único, debe ser de un dominio permitido), `rol` (uno de los 4), `iniciales` (opcional, para mostrar un avatar con iniciales en vez de foto), `activo`.

Protecciones que el frontend debe reflejar en la UI (deshabilitar el botón, no solo esperar el error): nadie puede desactivarse a sí mismo, ni quitarse a sí mismo el rol `super_admin`, ni eliminar su propia cuenta — el backend responde `403` si se intenta.

## 9. Solicitudes de eliminación — `/api/solicitudes-eliminacion`

Este es el mecanismo que reemplaza el `DELETE` directo para `manager`/`miembro` sobre clientes/proveedores/proyectos (catálogos, adjuntos y usuarios NO pasan por aquí, se eliminan directo por quien tenga permiso).

`POST /api/solicitudes-eliminacion` (cualquier autenticado) — `{tipoEntidad: "cliente"|"proveedor"|"proyecto", entidadId, motivo?}`. El backend decide el flujo solo: si es un proyecto con un `gerenteId` asignado distinto de quien solicita, la solicitud queda `pendiente_gerente`; en cualquier otro caso (cliente, proveedor, proyecto sin gerente, o el propio gerente dueño solicitando su eliminación) queda `pendiente_admin` directamente.

`GET /api/solicitudes-eliminacion/pendientes-para-mi` — para que un `manager` vea qué solicitudes le tocan endosar a él específicamente (filtra por `gerenteResponsableId`).

`GET /api/solicitudes-eliminacion` (`AdminOrAbove`) — todas las solicitudes, cualquier estado.

`PUT .../{id}/aprobar-gerente` y `.../rechazar-gerente` — solo el gerente responsable de ESE proyecto (no cualquier `manager`) puede resolver una solicitud en estado `pendiente_gerente`; al aprobar pasa a `pendiente_admin`, al rechazar queda `rechazada` (fin del camino, no vuelve a pedirse).

`PUT .../{id}/aprobar` y `.../rechazar` (`AdminOrAbove`) — la decisión final. Al aprobar, **aquí sí se ejecuta el `DELETE` real** de la entidad (si ya no existe porque alguien más la borró por otro camino, simplemente se marca `aprobada` sin fallar).

**Recordatorio de la sección anterior (doc 10):** nada de este flujo manda correo ni notificación — el gerente/admin tiene que entrar a revisar `pendientes-para-mi` o la lista general para enterarse.

## 10. Cosas transversales que el frontend necesita saber, no específicas de un módulo

**Reemplazo completo de colecciones hijas en cada `PUT`.** Aplica a `telefonos` (clientes y proveedores), `servicioIds` (proveedores), `equipo` y `proveedorIds` (proyectos). El formulario de edición debe mandar siempre la lista completa deseada, no un delta — omitir un elemento existente lo borra.

**No hay búsqueda, filtros ni paginación del lado del servidor todavía**, en ningún `GET` de listado (`clientes`, `proveedores`, `proyectos`, `usuarios`, `solicitudes-eliminacion`). Cada uno de esos endpoints devuelve la tabla completa; hoy es responsabilidad exclusiva del frontend filtrar/ordenar/paginar sobre esa lista en memoria. Con pocos cientos de registros (el volumen actual de Next) no es un problema, pero si la base crece mucho, en algún momento valdría la pena pedir paginación real al backend — no está construida hoy.

**Formato de errores, igual en todos los módulos:**
- `400` — validación de FluentValidation (formato `ValidationProblemDetails`, con el detalle por campo).
- `401` — no autenticado (token ausente/inválido/expirado).
- `403` — autenticado pero sin permiso (rol insuficiente, o una de las protecciones específicas como "no puedes eliminar tu propia cuenta").
- `404` — la entidad no existe.
- `409` — regla de negocio (ej. catálogo referenciado no existe, email duplicado), o choque de concurrencia (dos personas editando el mismo registro casi al mismo tiempo — el segundo guardado recibe este error con el mensaje "Otra persona modificó este registro mientras lo editabas").
- Todas las respuestas de error (excepto `400` de validación, que usa el formato estándar de ASP.NET) tienen la forma `{statusCode, message, traceId, timestamp}`.

**Límite de peticiones:** 100 peticiones por minuto por usuario autenticado (no por IP) — si el frontend hace polling agresivo de algún endpoint, puede toparse con `429`.

## 11. Referencias

- `docs/10-correos-autenticacion-y-guia-frontend.md` — login, correos, qué falta ahí.
- `docs/06-modelo-permisos-roles.md` — la matriz de permisos y el flujo de solicitudes de eliminación con más contexto de diseño.
- `docs/07-calendario-e-informes-excel.md` — el calendario y los informes con más detalle de implementación.
- `docs/schema/01_esquema_completo.sql` — el esquema real (de aquí salen los valores fijos de los `CHECK` como estados de proveedor, presupuesto, cobertura, etc., listados arriba).
- Controllers en `src/Nexit.API/Controllers/` — la fuente de verdad de cada ruta exacta.
