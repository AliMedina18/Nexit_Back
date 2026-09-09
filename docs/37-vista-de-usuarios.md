# La pantalla de Usuarios, terminada

*2026-09-08, misma jornada que `docs/36`. Alicia entró a `/usuarios` como super administradora y
repasó en voz alta todo lo que faltaba. Este documento recoge lo que pidió, lo que se investigó
antes de tocar nada, y lo que quedó construido.*

## 1. Lo que estaba mal

- **La barra superior no tenía el botón de Excel.** Clientes, Proveedores y Proyectos sí lo tienen
  (`docs/31`); Usuarios era la única sección sin exportar ni importar.
- **La columna "Estado" mentía.** Mostraba presencia (Disponible / Desconectado), no si la cuenta
  estaba activa. Una cuenta desactivada se veía exactamente igual que alguien que cerró el navegador,
  y el aviso de "se elimina sola a los 30 días" (`docs/17`) no aparecía por ningún lado, aunque el
  campo `fechaDesactivacion` ya venía en la respuesta desde hacía semanas.
- **Nombre y correo iban apretados en una sola celda**, y la columna de acciones no tenía título.
- **Los indicadores de arriba no reflejaban el modelo de 4 roles** (`docs/06`): decían "Miembros" y
  "Administradores", metiendo super_admin y admin en el mismo saco y sin mencionar a manager.
- **No había forma de ver el perfil de una persona.** El endpoint `GET /api/usuarios/{id}` existía y
  está abierto a cualquiera desde 2026-08-26; ninguna pantalla lo usaba.
- **No había lista de invitaciones**, solo el número en un recuadro. No se podía ver a quién se
  invitó, con qué rol, ni cancelar una que se mandó al correo equivocado.
- **Los botones de aprobar/rechazar de las solicitudes de eliminación salían en todas las filas**,
  sin importar su estado: en las ya resueltas y en las que esperan al gerente, el clic solo servía
  para recibir un error del backend. Y aprobar —que ejecuta un borrado real e irreversible— no pedía
  ninguna confirmación.

## 2. Qué se miró antes de diseñar

A pedido de Alicia ("investiguemos otros sistemas que hagan lo mismo"). Lo que coincide entre las
guías de UX de páginas de miembros y las tablas de datos de producto:

- La fila debe traer **identidad (nombre + correo + avatar), rol y estado** — activo, invitado o
  desactivado — sin tener que abrir el registro para lo básico.
- **Pendiente y desactivado tienen que verse distinto**, "para que la lista diga la verdad sobre
  quién puede entrar de verdad ahora mismo".
- Las acciones por fila (cambiar rol, reenviar, quitar) **van en la fila**, no escondidas.
- Conviene mostrar **última actividad** para detectar cuentas dormidas.
- Y **filtrar por rol**, en vez de obligar a leer la lista entera.

Nada de esto contradecía lo que Alicia pedía; lo que hizo fue decidir la forma concreta.

## 3. Lo que quedó

### La barra superior: Excel, con una asimetría a propósito

`GET /api/usuarios/exportar` (admin/super_admin) baja el equipo como .xlsx: correo, rol, nombre,
apellido, iniciales, estado de la cuenta, desde cuándo está desactivada y fecha de alta.

**Importar no crea usuarios: invita.** Un usuario de Nexit no puede existir sin su cuenta en Supabase
Auth, y esa cuenta la crea Supabase cuando la persona acepta el correo de invitación (`docs/25`,
`docs/36`) — no hay forma de fabricar una desde una fila de Excel como sí se hace con un cliente. Así
que `POST /api/invitaciones/importar` (solo super_admin) toma un archivo con columna **Correo** y
**Rol** opcional, y dispara exactamente la misma invitación que el modal: misma validación de
dominio, mismos duplicados detectados, mismo correo real enviado por Supabase. Una fila mala no
detiene el archivo. Por eso el menú dice **"Invitar desde Excel"** y no "Importar", y el resumen
cuenta *invitaciones enviadas* en vez de "usuarios nuevos".

El archivo exportado sirve de plantilla: sus dos primeras columnas son justamente las que lee la
importación, así que se puede bajar, agregar filas al final y volver a subirlo sin borrar nada.

### Indicadores y filtros

**Primer intento, descartado el mismo día:** tres recuadros más una fila con el conteo de los cuatro
roles en pastillas de colores, clicables para filtrar. Alicia lo vio y fue tajante ("horrendo"), con
razón: con un solo usuario registrado la fila se leía *Todos 1 · Super admin 1 · Admin 0 · Manager 0
· Miembro 0*, cinco pastillas de colores distintos llenas de ceros, que además no se parecían a nada
más de la aplicación.

**Lo que quedó:** tres recuadros — **Todos los usuarios**, **Conectados ahora**, **Invitaciones
pendientes** — y debajo el mismo **panel de filtros** que ya usan Clientes y Proveedores
(`styles.filtersPanel` + `Dropdown` + chips de "Aplicados"): filtrar por rol, por estado de la cuenta
y por conexión. La lección: cuando una pantalla necesita filtros, la respuesta es el patrón de
filtros que la aplicación ya tiene, no uno nuevo inventado para esta pantalla.

Queda un cuarto recuadro por definir; Alicia sabe que falta uno pero todavía no cuál.

### El título

"Usuarios" pasó a **"Gestión de usuarios"**, alineado con "Gestión de clientes" y "Gestión de
proveedores". El subtítulo dejó de describir lo obvio ("el directorio del equipo: quién entra a
Nexit") para decir qué se hace aquí: se invita, se ajustan permisos y se retira el acceso.

### La tabla

| Usuario | Correo | Rol | Estado de la cuenta | Acciones |
|---|---|---|---|---|

- **Usuario** trae el avatar con un **punto verde** si la persona está conectada en este momento
  (presencia, HU-12), la etiqueta "Tú" en la propia fila y, debajo del nombre, si está conectada o no.
- **Correo, rol, estado y acciones van centrados**; solo la primera columna queda alineada a la
  izquierda. Es lo que pidió Alicia mirando la tabla real, y con cinco columnas de las cuales tres
  son etiquetas cortas, se lee mejor así.
- **Estado de la cuenta** es Activa o Desactivada — otra cosa que estar conectado — y en las
  desactivadas muestra debajo *"se elimina el 7 oct 2026"*, calculado con los 30 días de `docs/17`.
- **Acciones** ya tiene título. Editar y eliminar solo para super_admin; el resto ve "Solo lectura".
- Hacer clic en cualquier fila abre el perfil.
- El buscador de la barra superior filtra por nombre, correo o rol.

### El formulario de edición

Rehecho como una ficha, no como una lista de campos sueltos: arriba, una tarjeta con el avatar (que
se actualiza con lo que se va escribiendo), el nombre, el correo y el estado de la cuenta; después
los datos en dos columnas; y el rol **como cuatro tarjetas seleccionables con su explicación al
lado**, en vez de un desplegable — son solo cuatro y cada uno necesita decir qué permite, que es
justo lo que un desplegable no puede mostrar. Sobre la propia cuenta, el bloque de "Acceso" sale
bloqueado con una explicación de por qué (si la super administradora pudiera quitarse el rol o
desactivarse, Nexit se quedaría sin nadie capaz de administrar usuarios, sin arreglo desde la
aplicación) — pero nombre, apellido e iniciales sí se editan sobre uno mismo.

### El perfil, en panel lateral

Mismo patrón que el detalle de Cliente/Proveedor/Proyecto. Trae el rol **con la explicación de qué
puede hacer ese rol en una línea**, el estado de la cuenta (y la fecha exacta de eliminación
automática si está desactivada), si está conectado o cuándo fue su última actividad, los datos
personales y desde cuándo está en Nexit. Abajo, escribirle por correo y —solo super_admin— eliminar.

### Invitaciones pendientes, con el botón Invitar encima

Tabla propia: correo, rol propuesto, quién invitó, cuándo se envió ("hace 3 días", con la fecha
exacta al pasar el mouse) y el mensaje que se le dejó. Las dos secciones de abajo comparten ahora un
mismo encabezado —icono, título, contador y una línea que explica para qué sirve la sección— para que
se lean como dos bloques hermanos y no como tres cosas distintas pegadas una tras otra; y sus estados
vacíos son una caja con borde punteado que dice qué va a aparecer ahí, en vez de una fila gris.
Se pueden **cancelar** (`DELETE /api/invitaciones/{id}`, solo super_admin, solo si sigue Pendiente):
antes, una invitación que nunca se aceptaba quedaba atascada para siempre y **bloqueaba volver a
invitar ese correo**, porque el validador rechaza un segundo envío mientras haya una pendiente.

Cancelar borra la fila en vez de marcarla "Cancelada": ese estado no existe en el CHECK constraint de
la tabla y agregarlo obligaría a correr SQL nuevo en producción, para guardar algo que no le sirve a
nadie. Lo que **no** hace es borrar la cuenta que Supabase Auth ya creó al mandar el correo — si esa
persona hace clic en el enlace después, entra sin perfil y la pantalla de registro le dice que no
tiene acceso y le cierra la sesión (`docs/36`), así que no puede hacer nada.

### Auto-protecciones visibles

`docs/11` pedía desde hace tiempo que la interfaz reflejara las tres protecciones que el backend ya
impone con 403 — nadie puede desactivarse, quitarse el rol de super administrador ni eliminarse a sí
mismo. Ahora, sobre la propia cuenta, el botón de eliminar sale apagado con el motivo en el tooltip,
y en el formulario el selector de rol y la casilla "Cuenta activa" salen bloqueados explicando por
qué. También se aclara que el correo no se edita desde ahí (es el de Supabase Auth), y al desmarcar
"Cuenta activa" aparece el aviso de qué implica el plazo de 30 días.

### Solicitudes de eliminación

Los botones ahora solo aparecen en `pendiente_admin`, que es el único estado donde la decisión le
toca a quien mira esta pantalla; en el resto se lee "No te toca a ti" o "Ya se decidió", con el
estado en color. Y tanto aprobar como rechazar pasan por una ventana de confirmación propia
(`ConfirmDialog`, nuevo componente compartido) que dice qué va a pasar de verdad — aprobar **elimina
el registro, sin vuelta atrás** — en vez de los `window.confirm`/`window.prompt` del navegador que
había antes.

## 4. Verificado

294 pruebas de backend pasan (11 nuevas: exportar el equipo, invitar desde Excel fila por fila con
sus casos borde, y cancelar una invitación), más las funcionales que necesitan Docker. El frontend
compila, pasa tipos y ESLint sin nada nuevo.

## 5. Pendiente

- **Nada en base de datos.** Ningún `schema/` nuevo que correr.
- **Reenviar** una invitación sigue sin existir. Con cancelar ya hay salida (cancelar + volver a
  invitar), pero un botón de reenviar sería un clic en vez de dos.
- Los cambios sobre un usuario (cambio de rol, desactivación) **siguen sin quedar en
  `historial_cambios`**, a diferencia de clientes/proveedores/proyectos. Es lo más grande que le
  queda por hacer a este módulo: hoy nadie puede saber quién le cambió el rol a quién.
- `Usuario` sigue siendo la única de las cuatro entidades principales **sin control de concurrencia**
  (`xmin`): dos super administradores editando a la vez, el segundo pisa al primero en silencio.
