# Dar de alta a alguien por los dos caminos, y que la campanita sirva

*2026-09-08, tercera pasada sobre el módulo de usuarios (`docs/36`, `docs/37`). Alicia revisó la
pantalla otra vez y pidió, sobre todo, poder dar de alta a alguien sin depender de que responda un
correo.*

## 1. Registrar a alguien manualmente

Hasta ahora la única vía desde la aplicación era invitar: se manda el correo, la persona hace clic,
crea su contraseña y completa sus datos. Funciona, pero **depende de que la otra persona responda**.
Alicia lo dijo claro: "a veces uno también puede crear un usuario manualmente".

El obstáculo real era que un usuario de Nexit no puede existir sin su cuenta en Supabase Auth, y esa
cuenta la creaba Supabase al aceptar la invitación. La salida es la Admin API: **`POST
/auth/v1/admin/users`** crea la cuenta directamente. Con eso, `POST /api/usuarios/registrar`
(`SuperAdminOnly`) hace las dos cosas en un solo paso:

1. Crea la cuenta en Supabase Auth con ese correo.
2. Crea el perfil en `usuarios` **usando el UUID que Supabase devolvió** — eso es lo esencial: el id
   del perfil tiene que ser el de la cuenta de Auth, porque es el que va a traer el JWT con el que
   esa persona se autentique. Está fijado en una prueba.

**La cuenta se crea sin contraseña, a propósito.** Poner una desde el panel significaría que quien
administra conoce la contraseña de otra persona. En vez de eso, se crea con el correo ya confirmado y
sin credencial: la primera vez, esa persona pide su código en la pantalla de inicio de sesión y ahí
crea su propia contraseña — exactamente el mismo camino de quien llega por invitación (`docs/30`), y
funciona sin tocar nada porque `contrasena_configurada` arranca en `false`.

Si Supabase falla (correo ya registrado, clave sin configurar, red caída), **no se guarda ningún
perfil**: un perfil sin cuenta de acceso es una fila que nadie podría usar jamás. Mismo criterio que
`CrearInvitacionUseCase`.

Invitar **no se retira**: son dos caminos para la misma decisión. Invitar sirve cuando se prefiere
que la propia persona escriba su nombre; registrar sirve cuando hay prisa o la persona ya está
sentada al lado.

## 2. Invitar y registrar son dos formularios, no dos pestañas

Primero se hicieron como un solo modal con pestañas, razonando que eran "la misma decisión tomada de
dos maneras". Alicia lo rechazó de inmediato y tenía razón: **invitar es pedirle a alguien que se
sume; registrar es darlo por hecho.** Piden datos distintos (registrar necesita el nombre y el
apellido, invitar necesita un mensaje), tienen consecuencias distintas y se usan en momentos
distintos. Meterlos en un mismo formulario obligaba a entender la diferencia antes de poder ver
siquiera qué pedía cada uno.

Quedaron separados:

- **"Nuevo usuario"** (con ícono de persona) en la barra superior, donde las otras pantallas tienen
  su "Nuevo proyecto" → abre **Registrar usuario**.
- **"Invitar"** en el encabezado de la sección de invitaciones pendientes, que es donde tiene
  sentido → abre **Invitar al equipo**.

`PageToolbarConfig` tuvo que aceptar `addLabel`/`onAdd`/`addIcon` opcionales, porque Usuarios fue la
primera pantalla que usó la barra de Excel sin botón negro, y la primera que necesitó un ícono
distinto del "+".

## 3. Los formularios, más limpios

Alicia fue específica sobre lo que sobraba, y tenía razón en todo: quien administra el sistema no
necesita que la interfaz le explique las reglas que ya conoce.

- Fuera el párrafo *"tu rol y tu acceso no se pueden tocar desde aquí…"* en la propia cuenta. Los
  controles salen deshabilitados y con eso basta; para lo demás están las validaciones.
- Fuera la descripción de cada rol dentro del formulario (las cuatro tarjetas con "manda en todo",
  "administra el sistema"…). El rol volvió a ser un selector normal. Esa explicación se queda solo
  en el **panel de perfil**, que es donde uno mira a una persona para entender hasta dónde llega.
- Fuera los textos de ayuda de más ("Opcional. Vacío, se arman con el nombre").
- **Fuera el campo "Iniciales", en todos lados.** No tenía razón de existir: las iniciales son la
  primera letra del nombre y la del apellido, siempre. Ahora se arman solas y los dos formularios
  las muestran en vivo en una tarjeta de vista previa mientras se escribe -- que es también la
  respuesta silenciosa a "¿y dónde pongo las iniciales?". Al guardar se manda `iniciales: null` para
  que no quede colgado un valor viejo escrito a mano.
- **Nombre, apellido y correo llevan asterisco** — el correo es obligatorio, aunque en la edición no
  se pueda cambiar (es el de Supabase Auth).
- En el formulario de invitar, fuera la instrucción *"escribe un correo y pulsa Enter para
  agregarlo"*, y **el mensaje pasó a ser obligatorio**: si a alguien le va a llegar un correo
  invitándolo a un sistema interno, merece leer de quién viene y para qué.
- Los errores de validación dejaron de ser una línea roja suelta: ahora son una caja con su ícono,
  igual en los tres formularios.

## 4. La campanita

Dos cosas distintas, las dos pedidas.

**Que las invitaciones también notifiquen.** Aceptar o rechazar una invitación ahora le crea una
notificación a quien invitó (`invitacion_aceptada` / `invitacion_rechazada`). Antes la única forma de
enterarse era entrar a Usuarios y notar que esa invitación había desaparecido de la lista de
pendientes — que nadie revisa a diario, así que en la práctica no se enteraba. Las solicitudes de
eliminación ya notificaban desde `docs/20`.

**Que el panel se pueda leer.** Era una lista plana de título + mensaje. Ahora cada fila trae **su
ícono según el tipo** (una cosa es "alguien quiere borrar un proveedor" y otra "alguien entró al
equipo") y **cuánto hace que llegó**; las no leídas se separan de las anteriores en dos grupos; hay
**"Marcar todas"**; y el estado vacío explica qué va a aparecer ahí. Marcar todas se hace fila por
fila contra `marcar-leida` porque el backend no tiene endpoint de lote — con el volumen real de la
bandeja no se nota, y evita una migración por algo que puede no hacer falta nunca.

## 5. La pasada de diseño

Tres rondas de "se ve feo" seguidas obligaron a dejar de retocar y hacer una pasada de diseño de
verdad, con la disciplina de una guía anti-plantilla en vez de a ojo. Lo que salió de ahí:

- **Se leyó primero el sistema que ya existe** (`src/styles/globals.css`: papel `#f4f3ef`, tinta
  `#0c0c0c`, verde `#00f675` solo decorativo, radios de 3-4 px, Archivo + IBM Plex Mono, sin
  librería de animación) y se trabajó **dentro** de él. Ni una paleta nueva, ni una tipografía
  nueva, ni un valor de color suelto fuera de los tokens.
- **Los modales ganaron encabezado propio**: etiqueta monoespaciada, título y una línea que dice qué
  hace ese formulario, sobre la banda color papel en vez del blanco del cuerpo. Separa "de qué se
  trata esto" de "qué tengo que llenar" sin una sola línea divisoria más.
- **Vista previa en los dos**: al invitar se ve el correo tal como le va a llegar a la otra persona,
  con el nombre real de quien invita y el mensaje escrito -- la única forma de notar que quedó
  cortante antes de mandárselo a cinco personas; al registrar se ve la fila como va a aparecer en la
  lista, con su avatar.
- **Todos los estados**, no solo el de reposo: cada control interactivo tiene reposo, hover, foco
  visible, pulsado, deshabilitado, cargando y error.
- **Verificado a 320, 375, 414 y 768 px**: sin desplazamiento horizontal, sin botones partidos en
  dos líneas, las dos columnas de los formularios se apilan solas.

## 6. Textos que decían lo obvio

- El título de la pantalla es **"Gestión de usuarios"** (como "Gestión de clientes"), no "Usuarios".
- El menú de Excel dice **"Importar usuarios"** / **"Exportar usuarios"**, no "invitar desde Excel" ni
  "exportar el equipo".
- Los filtros dicen **"Todos los roles"**, "Todos los estados" y "Toda la actividad" — no "Cualquier
  rol", que no es como nadie habla.
- Las solicitudes de eliminación ya no se describen como "quien no puede borrar un registro lo pide
  aquí" sino por lo que de verdad son: *"cuando alguien quiere eliminar un cliente, un proveedor o un
  proyecto, lo pide aquí con su motivo y tú decides"*. El motivo es la razón de ser del flujo, y no
  se mencionaba.

## 7. Verificado

298 pruebas de backend pasan (4 nuevas: el registro manual usa el UUID de Supabase; no guarda perfil
si Supabase falla; aceptar y rechazar una invitación notifican a quien invitó), más las funcionales
que necesitan Docker. El frontend compila, pasa tipos, ESLint y sus 58 pruebas.

## 8. Pendiente

- **Nada en base de datos.** Ningún `schema/` nuevo.
- El archivo `Nexit_Front/src/app/(dashboard)/usuarios/InviteModal.tsx` quedó reemplazado por
  `NuevoUsuarioModal.tsx` y se movió a `_to_delete/` (el puente al computador no puede borrar
  archivos) — se puede borrar sin más.
- **Avisarle a la persona que su cuenta fue desactivada** sigue sin existir. Alicia lo mencionó
  ("eso se le informa a la persona"): hoy el aviso de los 30 días solo se ve en la pantalla de quien
  administra. Mandarle un correo requeriría que este backend enviara correos, cosa que nunca ha
  hecho (`docs/10`) — habría que decidir si se hace con una plantilla más de Supabase.
- Sigue faltando lo más grande del módulo: **los cambios sobre un usuario no quedan en
  `historial_cambios`**, así que un cambio de rol o una desactivación no dejan rastro de quién lo
  hizo.
