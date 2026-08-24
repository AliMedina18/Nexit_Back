# Correos, autenticación y qué necesita el frontend

Este documento responde una pregunta muy puntual que surgió al empezar a construir las vistas en el repo del frontend (aparte de Nexit_Back): **¿qué hace exactamente el backend con el envío de correos, y qué le falta?** Se escribe ahora (2026-08-21) porque la respuesta corta — "el backend no envía ningún correo, eso lo hace Supabase" — necesita el detalle completo para poder diseñar las pantallas correctas del lado del frontend.

> **Actualización 2026-08-21 (mismo día, sesión de historias de usuario):** la usuaria cambió el diseño de login descrito en la sección 2.2 original. **Ya no es "solo la super administradora tiene contraseña"** — ahora **todos los roles** pasan por código OTP la primera vez y ahí mismo crean su contraseña, para entrar con ella de ahí en adelante. La sección 2.2 y la tabla de la sección 6 ya reflejan el diseño nuevo; ver `docs/12-historias-de-usuario.md` (HU-01) para el flujo completo paso a paso.

> **Actualización 2026-08-23:** se definió la política concreta de contraseñas (antes quedaba como "a decidir en el diseño visual") — ver la nueva sección 7.



## 1. La respuesta corta

**Nexit_Back (este backend en C#) nunca envía un correo.** No hay ningún servicio de correo, no hay SMTP configurado, no hay ninguna clase `EmailService` ni nada parecido en el código — se verificó buscando en todo el repo (`Services`, `Interfaces`, dependencias del proyecto) y no existe. Todo lo que tiene que ver con "enviarle un correo a alguien" (invitaciones, códigos de acceso) lo hace **Supabase Auth**, que es un servicio aparte, y lo hace **directamente contra el navegador del usuario** — sin pasar por este backend en absoluto.

Este backend solo hace dos cosas relacionadas con correo, y ninguna de las dos es "enviarlo":
1. **Valida** que el correo de un usuario nuevo pertenezca a un dominio laboral permitido (`agencianextmkt.com`) antes de guardar su perfil — ver sección 4.
2. **Verifica** el token JWT que Supabase Auth ya firmó, para saber quién está haciendo cada petición — ver sección 3.

Esto tiene una implicación directa para el frontend: **el login y el registro de acceso NO se hacen contra `Nexit_Back`, se hacen contra Supabase directamente**, usando el SDK de Supabase (`@supabase/supabase-js` u otro). El backend entra en escena *después* de que la persona ya inició sesión, para todo lo que es la lógica de negocio (clientes, proveedores, proyectos, etc.).

## 2. Los dos flujos de correo que existen hoy (ambos, 100% de Supabase Auth)

### 2.1. Invitación de un usuario nuevo

**Actualizado 2026-08-24 (ver `docs/25`):** ya existe una forma de hacer esto en un solo paso, desde dentro de Nexit, sin ir al dashboard de Supabase. La super administradora llama a `POST /api/invitaciones` (correo, rol, mensaje opcional) y el backend dispara la invitación real de Supabase por su cuenta, usando la Admin API con la Service Role Key. El resto del correo lo sigue mandando Supabase, igual que siempre:

1. Supabase le envía el correo de invitación a la persona.
2. La persona invitada hace click en el enlace del correo y llega a una página **de Supabase** (no del frontend de Nexit) donde establece su contraseña.
3. La primera vez que esa persona entra a Nexit, el frontend llama a `GET /api/invitaciones/mia` -- si hay una invitación pendiente para su correo, se le muestra (con el rol propuesto y el mensaje de quien la invitó) para que la **acepte o rechace**.
4. Si acepta (`POST /api/invitaciones/{id}/aceptar`, completando su nombre y apellido), su perfil de negocio se crea automáticamente con el rol que se le propuso -- usando su propio UUID de Supabase Auth, sin que nadie tenga que copiarlo a mano. Si rechaza (`POST /api/invitaciones/{id}/rechazar`), no se crea ningún perfil.

El proceso manual anterior (invitar desde el dashboard de Supabase + `POST /api/usuarios` con el UUID a mano) sigue funcionando igual si alguna vez lo necesitas, pero ya no es el único camino.

### 2.2. Inicio de sesión — diseño vigente (actualizado 2026-08-21): OTP la primera vez, contraseña de ahí en adelante, para todos los roles

Ya no hay una variante distinta por rol — **el mismo flujo de dos etapas aplica a super_admin, admin, manager y miembro por igual**:

1. **Primera vez que esa cuenta entra al sistema:** la persona escribe su correo, Supabase le manda un **código de 6 dígitos** por correo (`signInWithOtp`, plantilla "Magic Link" con `{{ .Token }}`), y lo escribe en el frontend. Verificar ese código ya la deja con una sesión válida de Supabase (con su JWT), aunque todavía no tiene contraseña.
2. **Crear su contraseña, en esa misma sesión recién verificada:** el frontend le muestra una pantalla para definirla — con confirmación ("repite la contraseña") y con la política de fortaleza que ya está fijada, no pendiente de decidir: **ver sección 7** —, y la guarda llamando a `supabase.auth.updateUser({ password })`. Esto sigue siendo 100% Supabase, no pasa por `Nexit_Back`.
3. **De ahí en adelante, esa cuenta entra con correo (predeterminado/recordado) + contraseña** (`signInWithPassword`) — el código OTP ya no vuelve a pedirse en logins normales.
4. **Cambiar la contraseña más adelante** ("olvidé mi contraseña" o un cambio voluntario): se envía un código por correo para poder cambiarla (`resetPasswordForEmail`, plantilla separada "Reset Password"/"Restablecer contraseña") — esto ya no es un caso de un solo usuario (como decía la versión anterior de este documento), aplica a cualquier cuenta porque ahora todas tienen contraseña. Paso a paso completo: `docs/12-historias-de-usuario.md`, HU-04.

Ninguna de estas etapas pasa por `Nexit_Back` — el intercambio de credenciales es 100% frontend ↔ Supabase. El backend solo entra cuando, ya con el JWT que Supabase entregó, el frontend llama a un endpoint como `GET /api/proyectos` con `Authorization: Bearer <token>`.

**Implicación para cómo se da de alta a alguien (ver también sección 2.1):** con este diseño, ya no hace falta que el enlace de invitación de Supabase sea el que le pida la contraseña a la persona (eso era necesario en el diseño anterior, exclusivo de la super_admin) — el propio flujo de OTP + "crea tu contraseña" del frontend cubre ese primer ingreso para cualquiera. Sigue haciendo falta que la cuenta exista en Supabase Auth antes de poder pedirle un código (ver sección 5 sobre el alta en dos pasos).

**Detalle importante para el frontend:** ya no existe una cuenta "sin camino de contraseña" — toda cuenta, sin importar el rol, termina teniendo una después de su primer ingreso. La pantalla de login ya no necesita decidir "esta cuenta usa código o contraseña" por adelantado: simplemente ofrece "iniciar con código" (para quien todavía no tiene contraseña) e "iniciar con contraseña" (para quien ya la tiene), y el correo puede quedar recordado del ingreso anterior para prellenar el campo.

### 2.3. Cerrar sesión (agregado 2026-08-23)

Igual que el resto de la autenticación, es 100% Supabase — el botón de "cerrar sesión" del frontend llama a `supabase.auth.signOut()` y ya. `Nexit_Back` no interviene, no hay ningún endpoint de logout que llamar aparte de eso. Paso a paso completo: `docs/12-historias-de-usuario.md`, HU-07.

Lo único que sí hay que decidir es el `scope` de esa llamada — Supabase soporta tres:

- **`local`** (recomendado para el botón normal de "cerrar sesión"): termina solo la sesión de ese navegador/dispositivo, deja las demás activas. Es el comportamiento que la gente espera de un botón de logout — cerrar sesión en el computador de la oficina no debería sacarte también del celular.
- **`global`**: termina TODAS las sesiones de esa cuenta, en todos los dispositivos.
- **`others`**: termina todas menos la actual (útil para un botón tipo "cerrar sesión en mis otros dispositivos", si algún día se quiere agregar).

Con `local` como comportamiento del botón principal, `global` queda como una opción razonable para ofrecer aparte (no obligatoria para la primera versión) en algo como "cerrar sesión en todos lados" dentro de la configuración de la cuenta.

**Límite real de Supabase, verificado en su documentación oficial (no es una suposición):** cerrar sesión revoca el *refresh token* de inmediato, pero el *access token* (el JWT que ya se entregó) **sigue siendo válido hasta que expira por su cuenta** — Supabase lo dice explícitamente: "el usuario no se desloguea de inmediato, solo cuando el Access Token expira". Es el mismo límite que ya quedó documentado en `docs/17-eliminacion-automatica-usuarios.md` (sección 6) para cuando se desactiva a alguien — no es un caso aparte, es como está construido Supabase Auth (tokens JWT sin estado, verificados por firma, no contra una sesión viva en cada petición). Ver la sección 6 de `docs/17` para la recomendación concreta de cómo acortar esa ventana (bajar el tiempo de expiración del token en el dashboard).

## 3. Cómo se valida el login del lado del backend (sin correo de por medio)

Una vez el frontend ya tiene el JWT de Supabase, cada petición a la API lo manda en el header `Authorization`. El backend (`Program.cs`) lo valida contra Supabase (modo recomendado: claves asimétricas vía JWKS, usando `Jwt:Authority` = `https://<proyecto>.supabase.co/auth/v1`) y lee del token el rol de negocio (`user_role`, agregado por un Auth Hook de Postgres — no algo que Supabase traiga de fábrica). Con ese rol decide qué endpoints puede usar la persona (`SuperAdminOnly`, `AdminOrAbove`, o simplemente autenticado). Nada de esto involucra correo — se menciona aquí solo porque es la pieza que conecta el login (sección 2.2) con el resto del sistema.

## 4. Dónde entra el correo en las reglas de negocio (validación, no envío)

Al crear un usuario (`POST /api/usuarios`, exclusivo de `super_admin`), el backend valida:
- Que el correo tenga formato válido y no esté repetido.
- Que el correo termine en un dominio de la tabla `dominios_correo_permitidos` (hoy: únicamente `agencianextmkt.com`, confirmado 2026-08-23 — ver `docs/schema/seed_geografia_categorias_estados.sql`). Esto se valida **dos veces**: una vez en la aplicación (`CreateUsuarioValidator`, responde `400` antes de tocar la base) y otra vez como respaldo en un trigger de Postgres (`check_usuario_dominio_correo`).

Esto es una validación de **quién puede tener perfil en el sistema**, no tiene ningún efecto sobre el envío del correo de invitación (que ya pasó antes, manualmente, en Supabase — sección 2.1). Si alguien intenta registrar un perfil con un correo de dominio no permitido, el error es del backend de Nexit, no de Supabase.

## 5. Qué falta — importante para no asumir que ya existe

Esto es lo que el frontend **no puede dar por hecho** que el backend resuelve, porque no está construido:

- **Actualizado 2026-08-24 — esto ya se construyó, ver `docs/25`.** Ya existe `POST /api/invitaciones` (solo super administradora): invita por correo y registra el perfil en un solo paso, sin que nadie tenga que copiar ningún UUID a mano ni pasar por el dashboard de Supabase. La persona invitada ve su invitación pendiente con `GET /api/invitaciones/mia` la primera vez que entra, y decide aceptarla (`POST /api/invitaciones/{id}/aceptar`, crea su perfil sola) o rechazarla (`POST /api/invitaciones/{id}/rechazar`). Detalle completo, incluida la configuración de Supabase que necesita, en `docs/25`.
- **Actualizado 2026-08-24 — esto ya se resolvió a medias.** Desde `docs/20` (HU-08) sí existe notificación **en bandeja** (`GET /api/notificaciones`, `PUT /api/notificaciones/{id}/marcar-leida`) para el flujo de solicitudes de eliminación: se genera sola al crear una solicitud, al aprobar/rechazar como gerente, y al aprobar/rechazar como admin. Lo que **sigue sin existir** es el correo: nadie recibe un email cuando pasa algo de esto, solo lo ve si entra a mirar su bandeja de notificaciones (o si el frontend implementa poll periódico, no hay websockets ni push). Si en algún momento quieres correo real acá también, sí sería backend nuevo (integrar un proveedor transaccional para este flujo específico, ver `docs/13`).
- **No hay reenvío de invitación ni de código** documentado como flujo separado — se asume que el `signInWithOtp`/`resetPasswordForEmail` de Supabase se puede volver a llamar si el correo no llega, ya probado para el caso de login (ver `docs/12`, HU-01).

## 6. Resumen para diseñar las pantallas del frontend

| Pantalla / flujo | Contra qué habla | Qué hace |
|---|---|---|
| Login, primera vez (cualquier rol) | Supabase directamente (SDK) | Correo → código de 6 dígitos → `signInWithOtp` → con la sesión ya verificada, crear contraseña con `updateUser({password})` (política de contraseña: sección 7) |
| Login, veces siguientes (cualquier rol) | Supabase directamente (SDK) | Correo (recordado/prellenado) + contraseña → `signInWithPassword` |
| Cambiar/recuperar contraseña (cualquier rol) | Supabase directamente (SDK) | Código por correo → `resetPasswordForEmail` + `verifyOtp(type: 'recovery')` + `updateUser({password})` — paso a paso: `docs/12`, HU-04 |
| Después del login (cualquier rol) | `Nexit_Back` | Todas las pantallas de negocio (clientes, proveedores, proyectos, calendario, informes, usuarios, solicitudes de eliminación) usan el JWT de Supabase como `Bearer token` contra los endpoints ya documentados en `docs/06`, `docs/07` |
| Alta de un nuevo miembro del equipo | `Nexit_Back` (ver `docs/25`) | Super admin invita con `POST /api/invitaciones` (correo + rol + mensaje opcional); la persona invitada ve `GET /api/invitaciones/mia` al entrar por primera vez y acepta/rechaza -- si acepta, su perfil se crea solo |
| Notificar solicitudes de eliminación pendientes | `GET /api/notificaciones/mias` (ver `docs/20`) | Ya llega como notificación en bandeja, generada sola; sin correo todavía, y sin push (el frontend puede seguir consultando `GET /api/solicitudes-eliminacion/pendientes-para-mi`/`GET /api/solicitudes-eliminacion` directamente si prefiere) |

## 7. Política de contraseñas (agregado 2026-08-23)

Esto se aplica en dos flujos: crear la contraseña por primera vez (HU-01, paso 6) y elegir una nueva al recuperarla (HU-04, paso 5) — misma regla en los dos casos.

**Regla:**
- Longitud mínima: **10 caracteres**.
- Debe incluir al menos: una letra mayúscula, una letra minúscula, un número y un símbolo (por ejemplo `! @ # $ % ^ & *`).
- El campo de confirmación ("repite tu contraseña") debe coincidir exactamente antes de dejar guardar — esto ya estaba especificado en HU-01/HU-04, no cambia.

**Por qué esto no es una validación de `Nexit_Back`:** la contraseña nunca llega a este backend — se crea y se guarda 100% dentro de Supabase Auth (`updateUser({password})`, sección 2.2). No hay ningún DTO ni endpoint en `Nexit_Back` que reciba una contraseña (se revisó `Nexit.Application/DTOs/Usuarios/UsuarioDtos.cs` y `UsuarioValidators.cs` — ninguno tiene un campo de contraseña, y así debe quedarse: es justamente el diseño de que Supabase, no este backend, custodie las credenciales). Por eso la regla de arriba se aplica en **dos lugares que deben coincidir entre sí**, ninguno de los dos es código C#:

1. **Supabase (la fuente de verdad — ahí se guarda y valida la contraseña de verdad):**
   Dashboard → **Authentication → Sign In / Providers → Email** → sección de contraseña:
   - **Minimum password length:** `10`.
   - **Password Requirements:** elegir la opción **"Lowercase, uppercase letters, digits and symbols"** (la más estricta de las que ofrece Supabase) — reemplaza la que ya tenías puesta (que, por lo que describiste, solo cubría longitud mínima).
   - Opcional, recomendado porque el proyecto ya está en plan Pro: activar **"Leaked password protection"** (revisa contra la base de HaveIBeenPwned que la contraseña elegida no esté filtrada en una fuga conocida — un correo/contraseña ya expuesto en otro sitio no podría usarse aquí).
   
   Esto es un cambio de configuración en el dashboard, no algo que yo pueda ejecutar por API — no tengo (ni debo tener) una sesión con tu cuenta de Supabase. Son un par de clics en esa pantalla.

2. **Frontend:** debe validar exactamente la misma regla (longitud 10 + mayúscula + minúscula + número + símbolo) *antes* de llamar a Supabase, para mostrar el error al instante en vez de esperar la respuesta del servidor. Si el frontend valida algo distinto a lo que Supabase exige, alguien podría pasar la validación visual y aun así recibir un error de Supabase al guardar — por eso las dos reglas tienen que ser exactamente iguales.

En cuanto actualices esa pantalla de Supabase, esta pieza queda completamente cerrada — no hay ninguna otra parte pendiente de programar para la política de contraseñas.

## 8. Referencias

- `docs/06-modelo-permisos-roles.md` — matriz de permisos completa y el flujo de solicitudes de eliminación.
- `docs/09-crear-proyecto-supabase-paso-a-paso.md` — sección 5 (plantillas de correo) y sección 11 (SMTP propio con plan Pro) siguen vigentes. **Su sección 10 (diseño de login) quedó desactualizada** por el cambio del 2026-08-21 — el diseño vigente es el de la sección 2.2 de este documento, no el de esa sección 10.
- `docs/12-historias-de-usuario.md` — HU-01 (login primera vez), HU-02 (login recurrente) y HU-04 (recuperar contraseña) — el flujo paso a paso de cada una.
- `docs/14-plantilla-correo-otp.md` y `docs/15-plantilla-correo-restablecer-contrasena.md` — las plantillas de correo de cada flujo.
- `src/Nexit.API/Controllers/UsuariosController.cs`, `SolicitudesEliminacionController.cs` — endpoints mencionados arriba.
- `src/Nexit.Application/Validators/Usuarios/UsuarioValidators.cs` — validación de dominio de correo.
