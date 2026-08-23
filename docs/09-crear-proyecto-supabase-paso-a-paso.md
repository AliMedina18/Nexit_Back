# Crear el proyecto de Supabase, paso a paso

> **Actualización 2026-08-21 — este documento quedó parcialmente superado.** La usuaria decidió NO seguir la ruta de "gratis ahora, migrar a Pro después" que describe este documento — en su lugar, el proyecto se crea **directo en plan Pro**, dimensionado para hasta 25 personas, con un proveedor de correo transaccional conectado **antes** de invitar a nadie. Ver `docs/13-correo-transaccional-y-supabase-pro.md` para la ruta vigente (reordena y ajusta los pasos de este documento, especialmente las secciones 1, 5 y 11 de abajo). Las secciones 3, 4, 7, 8 y 9 (esquema, hook, invitar usuarios, bootstrap del super_admin) siguen siendo válidas tal cual. La sección 10 (diseño de login) ya estaba superada desde antes por `docs/10`, sección 2.2.

Este documento es la guía que se usó para crear la primera versión (gratuita) del proyecto de Supabase de Nexit, y que se vuelve a seguir tal cual cuando llegue el correo corporativo y se migre al plan Pro. Por eso está escrita para repetirse dos veces: la única parte que cambia entre una vuelta y la otra es qué correo se usa para la cuenta de Supabase y el plan que se elige — el resto de los pasos (esquema, roles, hook, usuarios) es idéntico.

Contexto de por qué existen tantos pasos: este backend (Nexit_Back) nunca crea contraseñas ni envía invitaciones — eso lo hace Supabase Auth. El backend solo valida el token (JWT) que Supabase ya firmó, y guarda el "perfil de negocio" de cada persona (nombre, rol, activo/inactivo) en la tabla `usuarios`. Por eso el orden importa: primero existe la cuenta en Supabase Auth, y solo después se registra su perfil en este sistema.

## 1. Crear la cuenta y el proyecto (plan gratuito)

1. Entra a [supabase.com](https://supabase.com) y crea una cuenta (con el correo que tengas disponible por ahora — cuando llegue el correo corporativo de la empresa, este paso se repite con ese correo para la versión Pro).
2. Crea una organización nueva si te la pide (el plan "Free" no tiene costo).
3. Click en **New Project**. Datos que te va a pedir:
   - **Name**: `nexit` o `nexit-produccion` (lo que prefieras, es solo una etiqueta).
   - **Database Password**: genera una contraseña fuerte y **guárdala aparte de inmediato** (en un gestor de contraseñas, no en el chat ni en un archivo de texto suelto) — la vas a necesitar más adelante para el rol `nexit_app`, no para esta contraseña maestra directamente, pero Supabase la pide igual para el rol `postgres` interno.
   - **Region**: elige la más cercana a donde están tus usuarios (Colombia/México) — normalmente `South America (São Paulo)` o `East US` si São Paulo no está disponible en el plan gratuito.
   - **Pricing Plan**: Free.
4. Espera 1-2 minutos mientras Supabase aprovisiona el proyecto.

## 2. Guardar las credenciales del proyecto

Dentro del proyecto ya creado, ve a **Project Settings → API**. Ahí vas a encontrar tres datos que hay que guardar (en un gestor de contraseñas o variables de entorno — nunca en Git):

- **Project URL** (algo como `https://xxxxxxxx.supabase.co`) — este valor, seguido de `/auth/v1`, es el `Jwt:Authority` del backend.
- **anon public key** — la usará el futuro frontend para hablar con Supabase Auth directamente (login, pedir código, etc.).
- **service_role key** — solo para tareas administrativas hechas a mano por ti (nunca la pongas en el frontend ni en este backend). Con esta se pueden invitar usuarios por API en vez de por el dashboard, si más adelante quieres automatizarlo.

## 3. Aplicar el esquema completo

**Esto cambió el 2026-08-20:** `nexus_schema_v2.sql` (el script SQL viejo, escrito a mano) había quedado desactualizado frente a lo que el código realmente espera -- por ejemplo, le faltaba la columna `updated_by` que sí existe en el código desde la migración `AddConcurrencyAndAuditTracking`. Es el mismo tipo de problema que arreglamos en tu base local `nexit_dev` (una base que "dice" estar al día pero no lo está). Para que esto no vuelva a pasar, `docs/schema/01_esquema_completo.sql` **no está escrito a mano** -- se generó directamente desde las migraciones de EF Core con `dotnet ef migrations script`, así que es imposible que diga algo distinto de lo que el código real crea. Si más adelante agregas una migración nueva, se regenera con un solo comando (la instrucción exacta está en la primera línea del propio archivo).

**Corrección 2026-08-23:** el orden que estaba escrito aquí abajo (01 → 04 → 02 → 03 → seed) tiene un error que solo se ve al correrlo contra un Supabase real (nunca se probó en ese orden exacto antes — la base local `nexit_dev` no pasa por los pasos 2-5, así que este error quedó sin detectar hasta ahora): el paso 04 crea una `CREATE POLICY ... TO nexit_app` para cada tabla, lo que requiere que el rol `nexit_app` **ya exista** — pero ese rol lo crea el paso 02, que en el orden viejo corría después. Eso hace que 04 falle con `role "nexit_app" does not exist` si se corre antes que 02. El orden correcto, que ya no tiene esa dependencia invertida, es el de la lista de abajo (02 antes que 04).

Corre estos 5 archivos, **en este orden**, cada uno en el **SQL Editor** de Supabase:

1. `docs/schema/01_esquema_completo.sql` -- crea las 21 tablas de negocio, con sus columnas, llaves, índices y triggers. Ya se probó de punta a punta contra un Postgres real antes de entregártelo.
2. `docs/schema/02_rol_aplicacion_minimo_privilegio.sql` -- **antes de correrlo**, reemplaza `'CAMBIAR_ESTA_CONTRASENA'` por una contraseña nueva generada (por ejemplo con `openssl rand -base64 32` en cualquier terminal), y guárdala -- es la que usará el backend para conectarse (rol `nexit_app`, no el superusuario). De aquí en adelante, la cadena de conexión del backend (sección 6) usa `nexit_app`, no `postgres`. Va antes que el paso 3 porque ese paso crea políticas que apuntan a este rol -- si no existe todavía, el paso 3 falla.
3. `docs/schema/04_extras_supabase_post_migraciones.sql` -- las cosas que el paso 1 no puede crear porque dependen del esquema `auth` de Supabase (no existe en tu Postgres local ni en las pruebas) o del rol `nexit_app` recién creado: que `usuarios.id` quede enlazado a la cuenta real de Supabase Auth, Row Level Security como segunda barrera, y la política que le da paso a `nexit_app` en cada tabla.
4. `docs/schema/03_auth_hook_custom_claims.sql` -- crea la función que agrega el rol (`user_role`) al token cuando alguien inicia sesión.
5. `docs/schema/seed_geografia_categorias_estados.sql` -- carga los catálogos iniciales (países, categorías de proveedor, estados de proyecto, y los dominios de correo permitidos: `agencianextmkt.com` y, mientras se confirma o se retira, `nextexperiencial.com`).

(Los números del 1 al 5 de arriba son el orden en que los corres -- no coinciden con los números en los nombres de archivo, porque los nombres reflejan cuándo se creó cada uno durante el proyecto, no el orden de ejecución.)

**¿Y para tu base local `nexit_dev`?** Ahí puedes seguir usando `dotnet ef database update` tal cual lo hiciste hace un rato (sin `--connection`, usa la de `appsettings.Development.json`) -- es exactamente equivalente a correr el archivo 01 en Supabase, solo que en tu máquina no hace falta pasar por el SQL Editor. Los pasos 2 a 5 (RLS, rol de aplicación, hook, catálogos) son específicos de Supabase y no aplican a tu base local.

`nexus_schema_v2.sql` no se corre en ningún paso de este flujo -- por eso se sacó de `docs/schema/` (donde están los archivos que sí se ejecutan) y se movió a `docs/schema/referencia/nexus_schema_v2.sql`. Ahí queda solo como documento de lectura para entender el diseño completo de un vistazo, no como script ejecutable.

## 4. Activar el Auth Hook de roles

Sin este paso, nadie —ni siquiera el super administrador— va a poder pasar las políticas que exigen un rol, porque el token no trae el claim `user_role` hasta que se active.

1. Ve a **Authentication → Hooks**.
2. En "Customize Access Token (JWT) Claims hook", elige **Postgres**.
3. Selecciona la función `public.custom_access_token_hook` (ya la creó el script del paso 3).
4. Guarda y confirma que quede **Enabled**.

## 5. Configurar el correo de invitaciones y del código de acceso

Ve a **Authentication → Emails**. Ahí están las plantillas que Supabase envía. Dos cosas a revisar:

- **Invite user**: la plantilla que le llega a alguien cuando la invitas por primera vez — puedes dejarla con el texto por defecto o adaptarla con el nombre de Next.
- **Magic Link**: esta es la plantilla que se usa también para el código de acceso (OTP). Por defecto trae un enlace (`{{ .ConfirmationURL }}`), pero también incluye la variable `{{ .Token }}` con el código de 6 dígitos — para que el flujo sea "te llega un código y lo escribes" (como pediste) en vez de "haz click en un enlace", edita la plantilla para mostrar `{{ .Token }}` de forma visible.

Nota sobre el plan gratuito: Supabase usa un servidor de correo compartido por defecto con un límite bajo de envíos por hora (pensado para pruebas, no para producción). Mientras sean solo tú y Yuliana no debería ser un problema; si más adelante se suma más gente y empiezan a fallar los envíos, la solución es configurar un proveedor de correo propio (SMTP) en **Project Settings → Auth → SMTP Settings** — eso se puede dejar para cuando se cree el proyecto definitivo con el correo corporativo.

## 6. Conectar el backend a las credenciales reales

En el backend, estos valores nunca van commiteados a Git (así están hoy en `appsettings.json`, solo con placeholders). Se configuran como variables de entorno o en `appsettings.Production.json` (que está en `.gitignore`):

```
ConnectionStrings:DefaultConnection = Host=db.<tu-proyecto>.supabase.co;Database=postgres;Username=nexit_app;Password=<la contraseña del paso 3.2>;Port=5432;SSL Mode=Require
Jwt:Authority = https://<tu-proyecto>.supabase.co/auth/v1
Jwt:Audience = authenticated
```

`Jwt:LegacySharedSecret` se deja vacío — ese modo es solo para proyectos viejos de Supabase que todavía firman con un secreto compartido (no aplica a un proyecto nuevo).

## 7. Invitar a los dos primeros usuarios reales

Ve a **Authentication → Users → Invite user** e invita, una por una:

1. Tu propia cuenta (super administradora) — con el correo que vayas a usar para entrar.
2. `yuliana.navarro@agencianextmkt.com` — Yuliana Navarro.

Cada persona invitada recibe un correo para aceptar la invitación y (en el caso de tu cuenta) establecer una contraseña. **A Yuliana, cuando acepte la invitación, no le pongas ni le compartas ninguna contraseña** — así, más adelante, su único camino de entrada real es pedir el código por correo (ver sección 9). Después de que cada una acepte, en la lista de **Authentication → Users** vas a ver su UUID (el identificador que Supabase le asignó) — cópialo, lo necesitas en el siguiente paso.

## 8. El primer super_admin: por qué hay que crearlo a mano una sola vez

Aquí hay un detalle importante que conviene entender antes de hacerlo: `POST /api/usuarios` (el endpoint que registra el perfil de negocio de alguien en la tabla `usuarios`) está protegido y solo lo puede llamar alguien que **ya sea** `super_admin`. La primera vez, nadie lo es todavía — ni siquiera tú — así que no puedes usar la API normal para registrarte a ti misma. Por eso, solo para esta primera cuenta, se inserta directamente por SQL:

En **SQL Editor**, con tu propio UUID (el que copiaste en el paso 7) y tu correo real:

```sql
INSERT INTO usuarios (id, nombre, apellido, email, rol, activo)
VALUES ('<tu-uuid-de-auth.users>', '<tu nombre>', '<tu apellido>', '<tu correo real>', 'super_admin', true);
```

Después de correr esto, inicia sesión (o refresca tu sesión si ya habías entrado antes de este paso) para que tu token nuevo traiga el claim `user_role: super_admin`. Puedes confirmarlo copiando el `access_token` y pegándolo en [jwt.io](https://jwt.io) — el payload debe mostrar `"user_role": "super_admin"`.

## 9. Registrar a Yuliana por la API normal (ya con permisos)

Con tu sesión ya como `super_admin`, este segundo registro sí se hace por la API, como cualquier usuario futuro — es el flujo que va a usarse de aquí en adelante para todo el equipo:

```
POST /api/usuarios
{
  "id": "<uuid que Supabase le asignó a Yuliana en el paso 7>",
  "nombre": "Yuliana",
  "apellido": "Navarro",
  "email": "yuliana.navarro@agencianextmkt.com",
  "rol": "admin",
  "activo": true
}
```

Esta llamada ya pasa por las dos validaciones que se dejaron listas en el backend: que el correo no esté repetido, y que el dominio (`agencianextmkt.com`) esté en la lista de dominios permitidos — si alguien intenta registrar un correo de otro dominio, el backend lo rechaza con un 400 antes de tocar la base de datos.

## 10. Cómo va a funcionar el inicio de sesión

Esto es una decisión de diseño, no algo que Supabase active con un solo interruptor por usuario — vale la pena entender cómo queda armado:

- **Super administradora (tú)**: entra con correo y **contraseña** (`signInWithPassword` del lado de Supabase Auth). Es la única cuenta a la que se le define y comparte una contraseña real.
- **Todas las demás cuentas (Yuliana, y quien se sume después)**: entran pidiendo un **código de acceso por correo** (`signInWithOtp` — un código de 6 dígitos, la plantilla del paso 5). Nunca se les asigna ni se les comparte una contraseña, así que ese camino de entrada simplemente no existe para ellas.

Supabase no tiene un campo "este usuario solo puede usar código" — el control real está en dos lugares: que a esas cuentas nunca se les dé una contraseña (así que aunque exista el campo, no lo pueden usar), y que el futuro frontend solo les muestre la opción de "enviarme un código" en vez de un campo de contraseña. Esto es trabajo pendiente del frontend (todavía no construido, ver `docs/README.md`), pero ya queda documentado aquí como el diseño a seguir cuando se construya esa pantalla.

## 11. Cuando llegue el correo corporativo (versión Pro)

Cuando tengan el correo de la empresa que va a quedar como cuenta de facturación:

1. Repite el paso 1 completo (nueva cuenta de Supabase con ese correo, proyecto nuevo, plan Pro en vez de Free).
2. Repite los pasos 2 a 9 tal cual — el esquema, el rol de mínimo privilegio, el hook, los usuarios y el diseño de login son exactamente los mismos.
3. Si para ese momento el proyecto gratuito ya tiene datos reales cargados (clientes, proyectos, etc.) que quieras conservar, avísame antes de este paso — hay que exportarlos del proyecto viejo e importarlos al nuevo, no se migran solos por estar en el mismo Supabase.
4. Con el plan Pro conviene además configurar un SMTP propio (sección 5) para no depender del límite de envíos del plan gratuito.
