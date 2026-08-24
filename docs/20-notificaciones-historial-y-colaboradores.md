# Notificaciones, historial de cambios y "trabajando con este proveedor"

Este documento describe lo que ya quedó **construido y probado** de las tres piezas que faltaban del flujo de solicitudes de eliminación y de la gestión de proveedores, según lo que se fue aclarando en conversación (ver `docs/19-diseno-notificaciones-historial-y-mis-proveedores.md` para la propuesta original — este documento la reemplaza como la referencia vigente, en particular la sección de "mis proveedores", que terminó siendo distinta de las dos opciones que se plantearon ahí).

## 1. Notificaciones — bandeja del flujo de solicitudes de eliminación

Ya existía la posibilidad de que varias personas pidieran, cada una por su cuenta, eliminar el mismo cliente/proveedor/proyecto (no hacía falta ningún cambio de esquema para eso — nunca hubo una restricción que lo impidiera). Lo que faltaba era que alguien se enterara sin tener que entrar a revisar activamente. Eso es lo que agrega esta pieza.

### Cuándo se genera una notificación

| Evento | Quién la recibe | Contenido |
|---|---|---|
| Alguien solicita eliminar un **proyecto** con gerente responsable distinto de quien solicita | Ese gerente | Que le pidieron eliminar algo que lidera, con el motivo. |
| Alguien solicita eliminar un **cliente/proveedor**, o un proyecto sin gerente responsable (o el solicitante ES el gerente) | Todos los `admin`/`super_admin` **activos** | El motivo, y si ya había otras solicitudes pendientes para esa misma entidad, cuántas van en total (ej. *"ya van 3 solicitudes pendientes para este mismo proveedor"*) — así el administrador ve de una vez cuánta gente lo está pidiendo, tal como se pidió. |
| El gerente responsable **endosa** una solicitud de proyecto | Todos los `admin`/`super_admin` activos | Que ya falta solo su decisión final. |
| El administrador **aprueba o rechaza** cualquier solicitud | Quien la solicitó originalmente | Aprobada/rechazada, con el comentario del administrador si lo dejó (o un texto genérico si no). |

### La decisión del administrador resuelve TODAS las solicitudes pendientes de esa entidad de una vez

Esto es lo que se aclaró en conversación: si fulanito y María piden, cada uno por separado, eliminar el mismo proveedor, el administrador **no revisa solicitud por solicitud** — decide una vez sobre el proveedor, y esa decisión resuelve automáticamente cualquier otra solicitud que siguiera pendiente para esa misma entidad (esté en la etapa que esté: esperando gerente o esperando admin), con el mismo resultado, el mismo revisor y el mismo comentario que escribió. Cada solicitante recibe su propia notificación individual con esa decisión. Así nadie se queda con una solicitud pendiente apuntando a un proveedor que ya se eliminó (o que el administrador ya decidió que no se elimina).

### Nunca se borran — es historial permanente

Una notificación nunca se elimina. "Leída" es un estado (`leida: true/false`, con marca de tiempo de cuándo se marcó), no una eliminación — así la bandeja funciona también como historial permanente de "qué me han notificado alguna vez", tal como se pidió.

### Endpoints

| Método | Ruta | Qué hace |
|---|---|---|
| `GET` | `/api/notificaciones` | La bandeja del usuario autenticado (propias, más recientes primero — leídas y no leídas). |
| `PUT` | `/api/notificaciones/{id}/marcar-leida` | Marca una notificación propia como leída. 403 si intenta marcar una que no es suya. |

## 2. Historial de cambios — "quién editó qué, tipo Google Docs/Excel"

Un mini-historial por cada proyecto, proveedor y cliente: cada vez que alguien crea, edita o elimina uno de esos tres, queda una fila registrando quién lo hizo y cuándo — y en el caso de una edición, **una fila por cada campo que cambió**, con el valor de antes y el de después. Es justo el comportamiento que se describió como referencia (Excel/Google Docs: "que se sepa quién hizo el cambio").

### Cómo se detecta qué cambió

En vez de escribir a mano, campo por campo, la comparación para Proyecto/Proveedor/Cliente (y tener que acordarse de actualizarla cada vez que se agregue un campo nuevo a cualquiera de las tres), se construyó un detector genérico por reflexión (`CambioDetector`): toma una foto de los campos simples de la entidad *antes* de aplicar la edición, y la compara contra la entidad ya editada. Cualquier campo nuevo que se agregue a Proyecto/Proveedor/Cliente en el futuro queda cubierto automáticamente, sin tocar este código.

Qué SÍ se compara: campos simples (texto, números, fechas, `Guid`, etc.) que la propia entidad expone para editar. Qué NO se compara (a propósito, para no generar ruido): metadata de auditoría (`Id`, fechas de creación/edición, quién creó/editó por última vez) y colecciones/relaciones (equipo del proyecto, teléfonos, adjuntos...) — comparar eso por reflexión habría sido, en el mejor de los casos, ruidoso, y en el peor, incorrecto.

### Endpoint

| Método | Ruta | Qué hace |
|---|---|---|
| `GET` | `/api/historial/{tipoEntidad}/{entidadId}` | El historial completo de un registro puntual (`tipoEntidad` es `proyecto`, `proveedor` o `cliente`), más reciente primero. Cualquier persona autenticada puede consultarlo — no expone nada que no se pudiera inferir ya viendo el registro actual. |

## 3. "Trabajando con este proveedor" — marcado propio, no asignación de un administrador

Esta es la pieza que más cambió respecto a la propuesta original de `docs/19`. Ahí se habían planteado dos opciones: (A) que un administrador asignara un "responsable" único por proveedor (patrón CRM de "account owner"), o (B) una lista de favoritos privada de cada quien. **Se descartó la opción A explícitamente**: no es que un administrador reparta los proveedores, es cada persona marcándose a sí misma.

Lo que se construyó, siguiendo la aclaración exacta:

- **Cada quien se marca a sí misma** — "este proveedor lo manejo yo" — nadie asigna a nadie más.
- **Muchos a muchos** — varias personas pueden estar marcadas en el mismo proveedor al mismo tiempo (no es un campo de "un solo dueño").
- **Público, no privado** — a diferencia de la opción B original (favoritos privados), esto se ve en el listado de proveedores: quién está "trabajando con" cada proveedor (los "circulitos" que se describieron, tipo avatares de las personas marcadas — el backend ya devuelve, por cada proveedor, la lista de colaboradores con nombre e iniciales para que el frontend dibuje eso).
- **Alimenta una vista personal "mis proveedores"** — filtra el listado completo a solo los proveedores donde la persona autenticada se marcó.

Marcarse dos veces en el mismo proveedor no hace nada la segunda vez (no duplica, no falla). Cualquiera puede quitarse a sí misma en cualquier momento.

### Endpoints

| Método | Ruta | Qué hace |
|---|---|---|
| `GET` | `/api/proveedores/mios` | Los proveedores donde la persona autenticada está marcada. |
| `POST` | `/api/proveedores/{id}/colaboradores` | "Estoy trabajando con este proveedor" — se marca a sí misma. |
| `DELETE` | `/api/proveedores/{id}/colaboradores` | Se quita a sí misma. |
| `GET` | `/api/proveedores` y `/api/proveedores/{id}` | Ahora cada proveedor en la respuesta incluye `colaboradores: [{ usuarioId, nombre, iniciales }]` — la lista completa de quién está marcado, para los "circulitos". |

## Lo que quedó deliberadamente fuera de este batch

Dos cosas que se mencionaron en la misma conversación pero que necesitan más definición antes de construirse (no se avanzó nada de código para ninguna de las dos):

- **Sistema de prioridad/sugerencias** ("cuál proveedor/cliente atender primero, por qué razones") — falta definir qué señales de datos deberían alimentar esa prioridad.
- **Mejoras al informe semanal** — el informe semanal ya existe (`docs/07`), falta que se precise específicamente qué se quiere mejorar de él.

## Esquema de base de datos

Tres tablas nuevas: `notificaciones`, `historial_cambios`, `proveedor_colaboradores`. El diseño exacto de columnas, constraints e índices está en la migración de EF Core `AddNotificacionesHistorialColaboradores` y, para aplicarlo contra Supabase, en `docs/schema/07_notificaciones_historial_colaboradores.sql` — ver la nota dentro de ese archivo sobre por qué se escribió a mano en vez de generarse con `dotnet ef migrations script` (mismo motivo que ya aplicaba a `docs/schema/06`: hay un tramo de historial de EF que en Supabase se aplicó a mano, no con `dotnet ef database update`).

Para tu base local `nexit_dev`, en cambio, sí aplica el camino normal:
```
dotnet ef database update --project src/Nexit.Infrastructure --startup-project src/Nexit.API
```
Esto deja tu base local al día no solo con estas tres tablas nuevas, sino también con `usuarios_eliminados`/`fecha_desactivacion` de HU-06 (que hasta ahora solo existían en Supabase, aplicadas a mano) — cierra el pendiente que habías preguntado sobre si hacía falta aplicar los cambios también en local.

## Verificación

170 pruebas en total (163 pasan, 7 fallan por depender de Docker/Testcontainers en este entorno — el mismo grupo de siempre, ninguna nueva), 25 de ellas nuevas para este batch. Cobertura nueva: construcción de mensajes de notificación (destinatarios correctos según el evento, mención de "cuántas solicitudes van" cuando hay más de una, solo administradores **activos**), la cascada de resolución al decidir como administrador (aprobar/rechazar resuelve todas las solicitudes pendientes de la misma entidad, no solo la revisada), el detector de cambios (detecta campos editados, ignora los que no cambiaron, ignora metadata de auditoría y colecciones, reporta un campo que pasa a `null`), y el marcado de colaboradores (falla si el proveedor no existe, no hace nada si ya estaba marcado, filtra correctamente "mis proveedores"). Cero regresiones sobre las pruebas ya existentes.
