# Diseño: notificaciones, historial de cambios y "mis proveedores"

Esto responde a lo que pediste en la última ronda: notificaciones para solicitudes de eliminación (con el número de solicitudes que tiene cada elemento, y que el administrador pueda responder con una razón), un mini-historial tipo Google Docs/Excel para proyectos/proveedores/clientes (quién cambió qué), y un panel personal de "mis proveedores" además de la lista general. Investigué cómo resuelven esto otros sistemas antes de diseñarlo. Es una propuesta para que la confirmes antes de que empiece a construir — son tres piezas grandes, con una decisión tuya pendiente en la tercera.

## 1. Notificaciones (bandeja + historial permanente)

**Tabla nueva `notificaciones`:** una fila por notificación, por destinatario — `id`, `usuario_destinatario_id`, `tipo` (`solicitud_eliminacion_creada`, `solicitud_eliminacion_endosada`, `solicitud_eliminacion_decidida`), `titulo`, `mensaje`, `tipo_entidad`/`entidad_id` (para poder llevar a la persona directo a lo que originó la notificación), `solicitud_id`, `leida` (booleano), `fecha_creacion`, `fecha_leida`. No se borra nunca — "leída" no es lo mismo que "eliminada", así queda el historial permanente que pediste.

**Cuándo se dispara cada una:**
- Alguien crea una solicitud de eliminación de un **proyecto con gerente asignado** (y no es el propio gerente quien la pide) → notificación al gerente responsable.
- El gerente la endosa → notificación al administrador (o a todos los `admin`/`super_admin`, ver nota abajo).
- Alguien crea una solicitud de **cliente o proveedor**, o de un **proyecto sin gerente** → notificación directa al administrador.
- Un administrador aprueba o rechaza → notificación a quien la solicitó, con el comentario de la decisión (`ComentarioRevision`, que ya existe en el modelo).

**La parte de "cuántas solicitudes tiene" — buena noticia, no hace falta cambiar el modelo de datos.** Ya se pueden crear varias solicitudes independientes para el mismo proveedor/cliente (una por persona, cada una con su propio motivo) — lo confirmé revisando el código, no hay ninguna validación que lo impida hoy. Lo único que falta es que la pantalla de un administrador, al ver una solicitud, muestre también cuántas otras solicitudes pendientes existen para ese mismo proveedor/cliente — un conteo simple agrupado por `tipo_entidad` + `entidad_id`.

**Lo que sí hay que agregar (esto sí es nuevo):** hoy, si hay 3 solicitudes distintas para el mismo proveedor y el administrador aprueba (o rechaza) una, las otras dos quedan huérfanas — pendientes para siempre, apuntando a un proveedor que ya no existe (si aprobó) o sin resolver (si rechazó). Como describiste, el administrador decide **una vez** por el proveedor/cliente, no solicitud por solicitud — así que al aprobar o rechazar, el sistema resuelve automáticamente todas las demás solicitudes pendientes de ese mismo proveedor/cliente con la misma decisión y el mismo comentario, y notifica a cada persona que la pidió.

## 2. Historial de cambios (tipo Google Docs / Excel) para proyectos, proveedores y clientes

Hoy cada registro solo guarda quién lo creó y quién hizo la **última** edición (`CreatedBy`/`UpdatedBy`, un solo valor que se sobreescribe) — no un historial completo. Investigué el patrón estándar para esto (tabla de auditoría separada, es el enfoque que usan Google Docs/Office 365 y la mayoría de sistemas de gestión: cada cambio genera una fila nueva, nunca se sobreescribe nada).

**Tabla nueva `historial_cambios`:** `id`, `tipo_entidad` (`proyecto`/`proveedor`/`cliente`), `entidad_id`, `usuario_id`, `accion` (`creacion`/`edicion`/`eliminacion`), `campo` (por ejemplo `"estado"`, `"email"`, `null` si es la creación completa), `valor_anterior`, `valor_nuevo`, `fecha`. Una fila por cada campo que cambió en cada edición — así la pantalla puede mostrar exactamente "María cambió el email de tal a cual, el 20 de agosto", no solo "María editó esto".

**Cómo se llena, sin tener que tocar cada caso de uso uno por uno:** en vez de agregar código repetido en cada `ActualizarXUseCase`, se captura centralizado — un interceptor de EF Core que revisa, en cada `SaveChanges`, qué campos cambiaron en las entidades `Proyecto`/`Proveedor`/`Cliente` que se estén guardando, y escribe las filas de `historial_cambios` automáticamente. Ventaja: ningún caso de uso nuevo puede "olvidarse" de registrar el cambio, porque no depende de que alguien se acuerde de llamarlo.

**Dónde se ve:** un endpoint nuevo por entidad, por ejemplo `GET /api/proyectos/{id}/historial`, que devuelve la lista ordenada por fecha — la pantalla de detalle de un proyecto/proveedor/cliente puede mostrar esto como una pestaña o un panel lateral, igual que el historial de versiones de Google Docs.

## 3. Panel personal de "mis proveedores" — necesito que elijas entre dos opciones

Esto es lo único donde de verdad necesito tu decisión antes de construir, porque cambia qué significa "mis proveedores" y quién lo controla. Investigué cómo lo resuelven los CRM (Salesforce, Zoho): casi todos usan el mismo concepto de fondo, un campo de **"encargado"/"dueño"** en el registro — no una lista personal que cada quien arma por su cuenta.

**Opción A — Encargado asignado (recomendada, es el patrón estándar de CRM):** se agrega un campo `ResponsableId` a `Proveedor` (y opcionalmente a `Cliente`), como ya existe `GerenteId` en `Proyecto`. Un administrador asigna quién es el encargado habitual de cada proveedor (o queda vacío, sin encargado, si nadie lo ha manejado antes). "Mis proveedores" = los que tienen a esa persona como encargado; "todos" sigue siendo la lista completa, visible para cualquiera. Ventaja: refleja de verdad quién es responsable de la relación con ese proveedor — sirve también para saber a quién preguntarle si hay dudas, no es solo una vista personal cosmética.

**Opción B — Favoritos personales:** cada persona marca manualmente qué proveedores usa seguido (una estrellita, como favoritos de Gmail), sin que eso signifique que es "responsable" de nada — es una tabla aparte `proveedores_favoritos` (usuario_id, proveedor_id), cada quien arma la suya, no requiere que un administrador asigne nada.

La diferencia práctica: con la Opción A, si tú (o un administrador) no asigna encargados, la sección "mis proveedores" de todo el mundo aparece vacía hasta que se haga esa asignación — pero sirve para saber de verdad quién atiende a quién. Con la Opción B, cada persona arma su lista sola desde el primer día, pero es solo una conveniencia personal, no dice nada sobre responsabilidad real. ¿Cuál de las dos quieres, o quieres las dos a la vez (encargado asignado por un admin, y favoritos personales aparte)?

## 4. La parte de prioridades/sugerencias — sigue pendiente de definir

La dejo fuera de esta ronda a propósito: para sugerir "a quién atender primero" hace falta decidir qué datos indican urgencia (¿cuánto tiempo sin contacto? ¿una fecha límite próxima? ¿el estado del brief o la propuesta?) — eso no lo puedo inventar, y depende de cómo trabajan ustedes hoy. Lo retomamos aparte cuando definamos qué señales usar.

## 5. Alcance de esta ronda (una vez confirmes la Opción A/B de la sección 3)

Construyo, en este orden: notificaciones (con el conteo y la resolución en cascada de solicitudes duplicadas), historial de cambios, y "mis proveedores" según lo que elijas — con pruebas para las tres, igual que el resto del sistema.
