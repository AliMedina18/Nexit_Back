# Historias de usuario

A partir de aquí, el trabajo se organiza como historias de usuario: cada una describe una acción concreta de una persona real usando el sistema, con su flujo paso a paso y sus criterios de aceptación, apoyada en lo que ya se documentó en `docs/10` (correos y autenticación) y `docs/11` (el sistema completo, módulo por módulo). Este documento es un backlog vivo — se le van agregando historias nuevas a medida que se van cubriendo los demás módulos (clientes, proveedores, proyectos, calendario, etc.).

Convención de numeración: `HU-XX`, consecutivo, sin reordenar nunca uno ya escrito (si una historia queda obsoleta, se marca como tal en vez de borrarla o renumerar las demás).

## Metodología: primero cerrar la lógica de backend, después diseñar la vista

Esto es intencional y se repite en cada historia de aquí en adelante: **cada `HU-XX` se cierra con un veredicto explícito de qué tan lista está la lógica de backend para soportarla**, antes de pasar a diseñar esa pantalla en el repo del frontend. Una historia puede cerrar en tres estados posibles:

- ✅ **Backend listo** — el código de `Nexit_Back` ya hace todo lo que la historia necesita (o, si el paso vive 100% en Supabase Auth, ya está configurado/documentado cómo debe quedar) — se puede empezar el frontend de esa historia con confianza.
- 🟡 **Backend listo en código, pendiente de un factor externo** — el código ya está escrito y probado, pero depende de algo que todavía no existe (típicamente, el proyecto real de Supabase — ver `docs/09`) — no hay nada más que programar, solo ejecutar ese paso externo.
- 🔴 **Falta backend** — hay que escribir código nuevo en `Nexit_Back` antes de que el frontend tenga algo real contra qué construir esa pantalla.

Ninguna historia pasa a "diseño de frontend" mientras esté en 🔴.

## HU-01 — Iniciar sesión por primera vez y crear mi contraseña

**Como** cualquier persona con una cuenta ya registrada en el sistema (cualquier rol: super_admin, admin, manager o miembro), **quiero** poder entrar por primera vez usando solo mi correo y un código de verificación, y de una vez crear mi contraseña, **para** no depender de que alguien más me la asigne y poder volver a entrar yo sola las próximas veces.

Este es el diseño vigente desde el 2026-08-21 (ver la actualización en `docs/10-correos-autenticacion-y-guia-frontend.md`, sección 2.2) — reemplaza el diseño anterior, donde solo la super administradora tenía contraseña.

### Precondiciones
- La cuenta ya existe en Supabase Auth (alguien con acceso al dashboard la invitó — hoy es manual, ver `docs/10` sección 2.1) y su correo es de un dominio permitido (`agencianextmkt.com`).
- La persona **todavía no ha creado una contraseña** — es su primer ingreso.

### Flujo principal

1. La persona abre la pantalla de login del frontend y escribe su correo.
2. El frontend le ofrece "entrar con código" (es la única opción sensata la primera vez, porque todavía no tiene contraseña) y llama a `supabase.auth.signInWithOtp({ email })`.
3. Supabase le manda un correo con un código de 6 dígitos (plantilla "Magic Link" de Supabase, configurada para mostrar `{{ .Token }}`).
4. La persona escribe el código de 6 dígitos en el frontend. El frontend lo verifica contra Supabase (`verifyOtp`). Si el código es correcto, queda con una sesión válida (tiene JWT), aunque todavía sin contraseña.
5. Como es su primer ingreso, el frontend la lleva directo a una pantalla de **"crea tu contraseña"** — no a la pantalla principal del sistema todavía.
6. La persona escribe su contraseña y la repite en un segundo campo (confirmación). El frontend valida ahí mismo que las dos coincidan y que cumpla la política de contraseña ya definida (mínimo 10 caracteres, con mayúscula, minúscula, número y símbolo — ver `docs/10-correos-autenticacion-y-guia-frontend.md`, sección 7).
7. El frontend guarda la contraseña con `supabase.auth.updateUser({ password })`. Sigue siendo 100% contra Supabase.
8. Con la contraseña ya creada, el frontend lleva a la persona a la pantalla principal del sistema, ya con su sesión activa — sigue directo a HU-03 (qué ve según su rol).

### Flujo alterno — el código no llega o expira
- Supabase expira los códigos OTP después de un tiempo corto (configuración de Supabase, no de este backend). Si expira, la persona pide uno nuevo repitiendo el paso 2.
- Si el código no llega en absoluto, hoy no hay un flujo de "reenviar" documentado aparte de simplemente pedir uno nuevo — pendiente de probar cuando exista el proyecto real de Supabase (ver `docs/10`, sección 5).

### Criterios de aceptación
- No se puede pasar a la pantalla de "crea tu contraseña" sin haber verificado un código válido primero.
- No se puede entrar a la pantalla principal del sistema sin haber creado una contraseña, si es la primera vez.
- La contraseña y su confirmación deben coincidir antes de permitir guardar.
- Una vez creada la contraseña, un nuevo intento de login para ese mismo correo debe poder usar la variante de HU-02 (correo + contraseña), no obligar de nuevo al código.

### Notas técnicas
- Todo este flujo es 100% frontend ↔ Supabase Auth — `Nexit_Back` no interviene en ningún paso de esta historia (ver `docs/10`).
- No hay ningún endpoint de Nexit_Back involucrado aquí — el backend solo entra cuando, ya logueada, la persona empieza a pedir datos de negocio (HU-03 en adelante).

### Estado del backend para esta historia: ✅ Backend listo

**Actualizado 2026-08-23:** el factor externo ya se resolvió — el proyecto real de Supabase existe, el SMTP de Gmail está conectado, y la plantilla "Enlace mágico o OTP" ya muestra `{{ .Token }}` con el diseño de `docs/14-plantilla-correo-otp.md`. Se disparó un `signInWithOtp` real contra `analistacompras@agencianextmkt.com` y el correo llegó de verdad, con el código, por el canal real (no el servicio de pruebas de Supabase). No hay ninguna línea de código nueva que escribir en `Nexit_Back` para esta historia — el intercambio de credenciales completo (pedir el código, verificarlo, crear la contraseña) es Supabase Auth, no este backend.

La única pieza que sí vive en `Nexit_Back` y que esta historia da por hecha (por eso no está en 🔴) es anterior a ella: que la cuenta ya tenga su perfil de negocio registrado con `POST /api/usuarios` (rol, nombre, activo). Esa parte **ya está construida y probada** — es el módulo de usuarios completo (`UsuariosController`, `SuperAdminOnly`, validación de dominio de correo aplicada dos veces — ver `docs/06` y `docs/08`, 120/120 pruebas). No hay nada de permisos de usuarios pendiente de programar.

---

## HU-02 — Iniciar sesión con mi correo y contraseña (ingreso recurrente)

**Como** una persona que ya pasó por HU-01 alguna vez (ya tiene contraseña creada), **quiero** poder entrar directamente con mi correo y mi contraseña, sin tener que pedir un código cada vez, **para** entrar rápido al sistema en mi día a día.

### Flujo principal
1. La persona abre la pantalla de login. Si ya inició sesión antes en ese navegador, el frontend puede prellenar el campo de correo (conveniencia de UI, no algo que dependa del backend).
2. Escribe su correo y su contraseña, y el frontend llama a `supabase.auth.signInWithPassword({ email, password })`.
3. Si son correctos, queda con sesión activa y pasa a HU-03. Si no, el frontend muestra el error que devuelva Supabase (credenciales inválidas) sin indicar cuál de los dos campos falló (por seguridad, no revelar si el correo existe o no).

### Flujo alterno — olvidó su contraseña
Ver `docs/10`, sección 2.2, punto 4: se envía un enlace/código por correo para poder cambiarla (`resetPasswordForEmail` o equivalente de Supabase). Esta historia no desarrolla el detalle paso a paso de esa pantalla todavía — queda anotada como una historia futura (HU pendiente de numerar) cuando se diseñe esa pantalla específica.

### Criterios de aceptación
- Con correo y contraseña correctos, la persona entra directo a la pantalla principal, sin pasar otra vez por el código OTP.
- Con credenciales incorrectas, el mensaje de error no distingue "el correo no existe" de "la contraseña es incorrecta".

### Estado del backend para esta historia: ✅ Backend listo

**Actualizado 2026-08-23:** mismo caso que HU-01 — `signInWithPassword` es Supabase Auth puro, cero código de `Nexit_Back` de por medio, y el factor externo (proyecto real + correo conectado) ya está resuelto y probado. La parte de "olvidé mi contraseña" (flujo alterno de arriba) ya tiene su propia historia — ver **HU-04** más abajo.

---

## HU-03 — Ver el sistema de acuerdo a mi rol (nivel superficial)

**Como** cualquier persona ya logueada (por HU-01 o HU-02), **quiero** que el sistema me muestre solo las secciones que me corresponden según mi rol, **para** no ver ni intentar usar partes del sistema para las que no tengo permiso.

Esta historia se deja deliberadamente **superficial** por ahora — cubre qué secciones aparecen o no en la navegación principal, no el detalle fino de qué botón se habilita dentro de cada pantalla (eso se cubre historia por historia cuando se diseñe cada módulo, apoyado en la matriz completa de `docs/11`, sección 1).

### Flujo principal
1. Justo después de iniciar sesión (HU-01 u HU-02), el frontend ya tiene el JWT de Supabase, del cual puede leer el rol de la persona (`user_role`, el mismo claim que usa `Nexit_Back` — ver `docs/11`, sección 0).
2. El frontend arma el menú principal según ese rol:
   - **Cualquier rol** ve: Clientes, Proveedores, Proyectos, Calendario.
   - **Solo admin y super_admin** ven además: Informes, Catálogos (administración de países/regiones/ciudades/categorías/servicios/fases/estados), y la bandeja de solicitudes de eliminación pendientes de decidir.
   - **Solo super_admin** ve además: Usuarios (alta/edición/baja de cuentas).
   - **manager** ve además una bandeja de "solicitudes de eliminación pendientes para mí" (los proyectos de los que es gerente responsable) — ver `docs/11`, sección 9.
3. El detalle de qué botón exacto se habilita o no dentro de cada una de esas pantallas (por ejemplo, el botón "Eliminar" directo vs. "Solicitar eliminación") se deja para la historia de usuario de cada módulo específico, no para esta.

### Criterios de aceptación
- Un manager o miembro no debe ver en el menú principal las secciones de Usuarios, Informes ni administración de Catálogos.
- Un manager debe ver su bandeja de solicitudes pendientes si tiene al menos un proyecto donde es gerente responsable.
- Ocultar una sección del menú es una ayuda de UI, no la única protección — el backend igual responde `403` si alguien intenta llamar un endpoint restringido directamente (ver `docs/11`, sección 10), así que esta historia no reemplaza esa protección, solo evita que la persona vea opciones que de todas formas le fallarían.

### Estado del backend para esta historia: ✅ Backend listo

Esta historia sí depende directamente de código de `Nexit_Back`, y ya está completo: el claim de rol (`user_role`/`app_role`) lo agrega el Auth Hook de Postgres (`docs/schema/03_auth_hook_custom_claims.sql`, ejecutable en cuanto exista el Supabase real — la lógica en sí ya está escrita), y las dos políticas de autorización que hacen cumplir esos límites en cada endpoint (`SuperAdminOnly`, `AdminOrAbove`) ya están implementadas y probadas (120/120 pruebas, incluidas las de autorización por rol en `docs/06` y `docs/08`). El frontend no tiene que inventar ninguna regla de permisos nueva para armar este menú — solo leer el rol del JWT y aplicar exactamente la matriz de `docs/11`, sección 1.

---

## HU-04 — Recuperar mi contraseña

**Como** una persona que ya tiene contraseña creada (pasó por HU-01 alguna vez) pero no la recuerda, **quiero** poder pedir un código nuevo por correo y elegir una contraseña distinta, **para** volver a entrar al sistema sin depender de que alguien más me la restablezca a mano.

### Precondiciones
- La cuenta ya existe en Supabase Auth y ya tiene una contraseña creada (si nunca la creó, no es este caso — es HU-01, primer ingreso).

### Flujo principal
1. En la pantalla de login, la persona hace clic en "¿Olvidaste tu contraseña?" y escribe su correo.
2. El frontend llama a `supabase.auth.resetPasswordForEmail(email)`.
3. Supabase le manda un correo con un código de 6 dígitos — misma mecánica que HU-01, pero con la plantilla **"Restablecer contraseña"** (`auth/templates/reset-password` en el dashboard), no la de "Enlace mágico o OTP". Esa plantilla también acepta `{{ .Token }}` igual que la otra.
4. La persona escribe el código de 6 dígitos en el frontend. El frontend lo verifica con `supabase.auth.verifyOtp({ email, token, type: 'recovery' })` (nota: `type: 'recovery'`, no `'email'` como en HU-01 — es la diferencia técnica entre las dos historias). Si es correcto, queda con una sesión válida.
5. El frontend la lleva a una pantalla de "elige tu nueva contraseña" (mismo patrón de HU-01, paso 6: campo + confirmación, deben coincidir, y la misma política de contraseña de `docs/10`, sección 7 — mínimo 10 caracteres, mayúscula, minúscula, número y símbolo).
6. El frontend guarda la contraseña nueva con `supabase.auth.updateUser({ password })`.
7. Con la contraseña ya cambiada, el frontend lleva a la persona a la pantalla principal del sistema, con su sesión activa.

### Flujo alterno — el código no llega o expira
- Igual que HU-01: el código expira después de un tiempo corto (configuración de Supabase); si expira, se repite el paso 2.

### Criterios de aceptación
- No se puede pasar a la pantalla de "elige tu nueva contraseña" sin haber verificado un código válido primero.
- La contraseña nueva y su confirmación deben coincidir antes de permitir guardar.
- Después de cambiar la contraseña, la persona entra directo al sistema (no tiene que volver a iniciar sesión por separado).
- Si el correo no corresponde a ninguna cuenta, el mensaje no debe revelar si la cuenta existe o no (mismo principio de seguridad que HU-02).

### Notas técnicas
- Todo este flujo es 100% frontend ↔ Supabase Auth — `Nexit_Back` no interviene en ningún paso, igual que HU-01/HU-02.
- La plantilla de correo "Restablecer contraseña" es una plantilla **distinta** de "Enlace mágico o OTP" dentro del dashboard de Supabase, ya diseñada con el mismo estilo — ver `docs/15-plantilla-correo-restablecer-contrasena.md` y `docs/plantilla_correo_restablecer_contrasena.html`.
- La política de contraseña (paso 5) es la misma que HU-01 — ver `docs/10-correos-autenticacion-y-guia-frontend.md`, sección 7: mínimo 10 caracteres, mayúscula, minúscula, número y símbolo, configurada en Supabase (Authentication → Sign In / Providers → Email) y espejada en el frontend.

### Estado del backend para esta historia: 🟡 Backend listo en código, pendiente de un factor externo

Igual que HU-01/HU-02 antes de esta actualización: el mecanismo (`resetPasswordForEmail` + `verifyOtp` con `type: 'recovery'` + `updateUser`) es Supabase Auth puro, cero código nuevo en `Nexit_Back`. **Actualizado 2026-08-23:** ya está lista la plantilla de correo "Restablecer contraseña" con el mismo diseño de HU-01 (`docs/15`), y ya está definida y documentada la política de contraseña (sección 7 de `docs/10`) que faltaba especificar. Lo único que queda pendiente, puramente externo, es: (1) que pegues la plantilla nueva en el dashboard de Supabase, (2) que ajustes ahí mismo el "Password Requirements" a la regla completa (hoy solo tienes la longitud mínima puesta), y (3) probar el flujo real disparando un `resetPasswordForEmail` y confirmando que el código llega y funciona — igual que ya se probó con HU-01/HU-02. Avísame cuando hagas (1) y (2) y disparo la prueba de (3) de inmediato. En cuanto se confirme, esta historia pasa a ✅ sin tocar `Nexit_Back`.

---

## HU-05 — Enterarme si alguien cambió mi contraseña sin ser yo

**Como** cualquier persona con cuenta en el sistema, **quiero** recibir un correo apenas mi contraseña cambie (ya sea porque la creé por primera vez, la recuperé, o la cambié voluntariamente), **para** darme cuenta de inmediato si ese cambio no lo hice yo.

Esta historia es distinta a las anteriores: no la dispara una acción que el frontend inicia — la dispara Supabase automáticamente, como efecto secundario de HU-01 (crear contraseña la primera vez) o HU-04 (recuperarla). No hay pantalla nueva que diseñar para esta historia; es un correo que llega solo.

### Precondiciones
- La notificación de seguridad "Password changed" está activada en el proyecto de Supabase (Authentication → Emails → Security notifications) — **ya lo hiciste** (2026-08-23).

### Flujo principal
1. En cualquier momento en que `supabase.auth.updateUser({ password })` se ejecuta con éxito (paso final de HU-01 o de HU-04), Supabase dispara automáticamente el correo de la plantilla **"Password changed"**.
2. La persona dueña de la cuenta recibe el correo, confirmando que su contraseña cambió.
3. Si la persona reconoce el cambio, no tiene que hacer nada más.
4. Si la persona **no** hizo ese cambio, el correo le indica contactar de inmediato a un administrador (`analistacompras@agencianextmkt.com`).

### Flujo alterno
- No aplica un "flujo alterno" en el sentido de las otras historias — esta no tiene pasos que puedan fallar del lado de la persona, solo del lado de la entrega del correo (mismo riesgo que cualquier correo transaccional).

### Criterios de aceptación
- Cualquier cambio de contraseña exitoso (primera vez o recuperación) dispara este correo, sin excepción de rol.
- El correo no incluye ningún código ni enlace de acción — es puramente informativo, con un solo llamado a la acción (contactar a un administrador) para el caso de que el cambio no haya sido autorizado.
- El correo se ve con el mismo diseño de marca que las otras dos plantillas de autenticación (`docs/14`, `docs/15`).

### Notas técnicas
- Esta plantilla no usa `{{ .Token }}` — su única variable es `{{ .Email }}`. Ver `docs/16-plantilla-correo-contrasena-cambiada.md` para el detalle completo y por qué el tono es distinto al de las otras dos.
- Vive en una sección separada del dashboard de Supabase ("Security notifications"), no junto a las plantillas de autenticación normales.

### Estado del backend para esta historia: 🟡 Backend listo en código, pendiente de un factor externo

Igual que HU-04: cero código nuevo en `Nexit_Back` — esto es 100% Supabase Auth disparando un correo por su cuenta. **Actualizado 2026-08-23:** ya activaste la notificación de seguridad, y ya está lista la plantilla con el diseño de marca (`docs/16`). Falta: (1) pegar el HTML en Supabase (igual que las otras dos plantillas), y (2) la prueba real — que sí requiere completar un cambio de contraseña de verdad (ver `docs/16`, sección "Cómo se prueba de verdad", incluye una decisión pendiente tuya sobre qué cuenta y qué contraseña usar para esa prueba). En cuanto se confirme que el correo llega bien, esta historia pasa a ✅.

---

## HU-06 — Desactivar, reactivar y eliminar (con respaldo) a un miembro del equipo

**Como** super administrador, **quiero** poder desactivar a alguien del sistema, reactivarlo si me equivoqué, y que se elimine solo después de 30 días desactivado (con un respaldo, no un borrado sin más), **para** dar de baja a alguien con margen de reacción y sin perder el rastro de quién era.

Ver `docs/17-eliminacion-automatica-usuarios.md` para el diseño completo y la investigación de cómo lo resuelven otros productos (Microsoft Entra ID, GitLab, Facebook — todos con una ventana de 30 días, el mismo número que pediste).

### Precondiciones
- Quien ejecuta esto es `super_admin` (`UsuariosController` es exclusivo de ese rol — ver `docs/06`).

### Flujo principal
1. El super administrador edita a alguien (`PUT /api/usuarios/{id}`) y pone `Activo = false`. El backend guarda la fecha exacta de ese cambio (`FechaDesactivacion`) — arranca ahí el conteo de 30 días.
2. Desde ese momento, la cuenta pierde acceso al sistema (el Auth Hook deja de darle un rol válido en su próximo inicio de sesión o renovación de token — hasta 1 hora de margen en el peor caso, ver `docs/17` sección 6) — no solo queda "desactivada en el papel", de verdad no puede usar el sistema.
3. Si en cualquier momento antes de los 30 días alguien vuelve a poner `Activo = true`, el conteo se cancela por completo (`FechaDesactivacion` se limpia) y la cuenta recupera acceso normal.
4. Si nadie la reactiva, al cumplirse los 30 días el proceso automático de `Nexit_Back` la elimina sola: guarda una copia completa en `usuarios_eliminados` (tabla de respaldo, no consultada por la aplicación normal) y borra la fila de `usuarios`.
5. Alternativa al paso 4: el super administrador puede eliminarla de inmediato sin esperar, con `DELETE /api/usuarios/{id}` — mismo respaldo previo, pero queda registrado quién la eliminó (a diferencia de la automática, donde ese campo queda vacío).

### Flujo alterno — la propia cuenta
- Nadie puede desactivarse, quitarse el rol de super_admin, ni eliminarse a sí mismo (protección ya existente, ver `docs/06`) — evita que el sistema se quede sin nadie que lo pueda administrar.

### Criterios de aceptación
- Reactivar antes de los 30 días cancela la eliminación programada por completo, no la pausa ni la reduce.
- Una cuenta desactivada no puede usar ningún endpoint del sistema, no solo los de administración — el bloqueo aplica parejo, sin importar el rol de esa cuenta.
- Antes de cualquier eliminación (automática o manual) siempre queda un respaldo en `usuarios_eliminados` — nunca se borra sin dejar rastro.
- Eliminar (automática o manual) también intenta eliminar la cuenta de Supabase Auth correspondiente, para que no le quede ningún camino de acceso.

### Notas técnicas
- Sin pantalla nueva que diseñar para el paso 4 (el automático) — no es un flujo que el frontend dispare, corre solo dentro del backend.
- El frontend puede mostrar, en la pantalla de usuarios, algo como "se elimina automáticamente el [FechaDesactivacion + 30 días]" para cualquier cuenta desactivada — el campo `FechaDesactivacion` ya viene en la respuesta de `GET /api/usuarios`.

### Estado del backend para esta historia: 🟡 Backend listo en código, pendiente de un factor externo

El código ya está completo, compilado y con pruebas (127 pruebas totales — 120 pasan sin necesitar Docker/base de datos real, incluidas las nuevas de esta historia; las 7 restantes son funcionales que sí necesitan Docker, mismas de siempre, no relacionadas con esto). Lo que falta es puramente externo — dos scripts SQL por correr en tu Supabase real (ver `docs/17`, sección 7): `docs/schema/06_eliminacion_automatica_usuarios.sql` (columna nueva + tabla de respaldo) y volver a correr `docs/schema/03_auth_hook_custom_claims.sql` (se actualizó para revisar `activo`). Opcional: configurar la Service Role Key para que la cuenta de Supabase Auth también se elimine sola (si no, el perfil de negocio sí se elimina igual, solo queda pendiente borrar la cuenta de Auth a mano). En cuanto corras esos dos scripts, esta historia pasa a ✅.

---

## HU-07 — Cerrar sesión

**Como** cualquier persona con cuenta en el sistema, **quiero** poder cerrar sesión desde el botón correspondiente, **para** dejar de tener acceso desde ese dispositivo cuando termino de usar el sistema.

Ver `docs/10-correos-autenticacion-y-guia-frontend.md`, sección 2.3, para el detalle técnico completo dirigido al frontend.

### Precondiciones
- La persona tiene una sesión activa (inició sesión con HU-01 o HU-02).

### Flujo principal
1. La persona hace clic en "cerrar sesión". El frontend llama a `supabase.auth.signOut()` con `scope: 'local'` — termina solo la sesión de ese navegador/dispositivo, deja las demás activas (por ejemplo, si también tiene sesión abierta en el celular, esa no se cierra).
2. Supabase revoca de inmediato el *refresh token* de esa sesión y borra la sesión guardada en el navegador.
3. El frontend redirige a la pantalla de inicio de sesión.

### Flujo alterno — cerrar sesión en todos los dispositivos
- Opcional, no obligatorio para la primera versión: un botón aparte, por ejemplo dentro de la configuración de la cuenta ("cerrar sesión en todos lados"), que llame a `supabase.auth.signOut()` con `scope: 'global'` en vez de `'local'` — termina todas las sesiones de esa cuenta, en cualquier dispositivo.

### Criterios de aceptación
- El botón principal de "cerrar sesión" usa `scope: 'local'` (no cierra otros dispositivos sin que la persona lo pida explícitamente).
- Después de cerrar sesión, el *refresh token* de esa sesión queda revocado de inmediato — no se puede volver a renovar el token desde ese dispositivo sin iniciar sesión de nuevo.

### Notas técnicas
- **Limitación real, verificada en la documentación oficial de Supabase (no una suposición):** cerrar sesión no invalida al instante el *access token* que ya se entregó — ese JWT sigue siendo válido hasta que expira por su cuenta (hasta 1 hora, según la configuración del proyecto). Es el mismo límite ya documentado en `docs/17-eliminacion-automatica-usuarios.md` (sección 6) para cuando se desactiva a alguien: no es un caso aparte, es cómo está construido Supabase Auth (tokens firmados, sin verificar contra una sesión viva en cada petición). Para una persona que simplemente terminó de usar el sistema esto no representa ningún riesgo real. La recomendación concreta para acortar esa ventana en cualquier escenario (logout, desactivación, o token comprometido) está en `docs/17`, sección 6.
- No hace falta ningún endpoint nuevo en `Nexit_Back` para esto — es una llamada 100% del lado de Supabase, igual que el resto de la autenticación (HU-01, HU-02, HU-04).

### Estado del backend para esta historia: ✅ Completo

Cero código nuevo en `Nexit_Back` — cerrar sesión es, igual que iniciar sesión, una operación que resuelve Supabase directamente desde el frontend. No hay ningún factor externo pendiente de tu parte para esta historia.

---

## HU-08 — Enterarme de las solicitudes de eliminación, ver el historial de cambios y marcar en qué proveedores trabajo

**Como** cualquier persona con cuenta en el sistema, **quiero** (1) recibir una notificación cuando alguien me pida eliminar algo que lidero, o cuando alguien más solicite eliminar un cliente/proveedor/proyecto (si soy administradora), (2) poder ver quién editó qué de un proyecto/proveedor/cliente y cuándo, y (3) poder marcar los proveedores con los que trabajo para tener mi propia vista filtrada, **para** no tener que entrar a revisar activamente si hay algo pendiente, saber quién hizo qué cambio, y encontrar rápido "mis" proveedores entre todos los que existen.

Ver `docs/20-notificaciones-historial-y-colaboradores.md` para el diseño completo.

### Flujo principal — notificaciones
1. Alguien solicita eliminar un cliente, proveedor o proyecto (flujo ya existente, `docs/06`).
2. El sistema genera automáticamente una notificación para quien corresponda según la etapa (el gerente responsable si el proyecto tiene uno distinto de quien solicita, o todos los administradores activos si va directo a esa etapa) — incluyendo, si ya había otras solicitudes pendientes para esa misma entidad, cuántas van en total.
3. La persona ve su bandeja (`GET /api/notificaciones`) y puede marcar cada una como leída (`PUT /api/notificaciones/{id}/marcar-leida`) — nunca se borran, quedan como historial permanente.
4. Cuando un administrador decide (aprobar/rechazar), se notifica a quien solicitó, y **automáticamente se resuelven también todas las demás solicitudes que seguían pendientes para esa misma entidad** (el administrador decide una vez por la entidad, no solicitud por solicitud), notificando a cada solicitante por separado.

### Flujo principal — historial de cambios
1. Alguien edita un proyecto, proveedor o cliente.
2. El sistema registra automáticamente, por cada campo que cambió, el valor de antes y el de después, quién lo hizo y cuándo.
3. Cualquier persona autenticada puede consultar ese historial (`GET /api/historial/{tipoEntidad}/{entidadId}`), más reciente primero.

### Flujo principal — "trabajando con este proveedor"
1. Una persona entra a un proveedor y se marca a sí misma como colaboradora (`POST /api/proveedores/{id}/colaboradores`) — cualquiera puede marcarse, no solo un administrador, y varias personas pueden estar marcadas en el mismo proveedor a la vez.
2. Esa marca es pública: en el listado de proveedores se ve quién está trabajando con cada uno (avatares/iniciales).
3. La persona puede filtrar a solo "sus" proveedores (`GET /api/proveedores/mios`) y quitarse en cualquier momento (`DELETE /api/proveedores/{id}/colaboradores`).

### Criterios de aceptación
- Nadie puede marcar como leída una notificación que no es suya (403).
- Resolver una solicitud de eliminación como administrador resuelve también cualquier otra pendiente para la misma entidad, con el mismo resultado y comentario, notificando a cada solicitante individualmente.
- El historial de un registro nunca se sobreescribe: cada edición agrega filas nuevas, no reemplaza las anteriores.
- Marcarse dos veces como colaborador del mismo proveedor no duplica nada.

### Notas técnicas
- Esquema: tablas nuevas `notificaciones`, `historial_cambios`, `proveedor_colaboradores` — ver `docs/schema/07_notificaciones_historial_colaboradores.sql` (Supabase) y la migración de EF Core `AddNotificacionesHistorialColaboradores` (base local).
- Quedaron explícitamente fuera de esta historia, pendientes de más definición: el sistema de prioridad/sugerencias de a qué proveedor/cliente atender primero, y qué mejorar específicamente del informe semanal (que ya existe, `docs/07`).

### Estado del backend para esta historia: ✅ Completo

170 pruebas en total (163 pasan, 7 dependen de Docker en este entorno — nada nuevo), 25 nuevas para esta historia, cero regresiones.

---

## HU-09 — Ver a qué proyecto atender primero

**Como** cualquier persona con acceso a proyectos, **quiero** ver una lista de mis proyectos activos ordenada por qué tan urgente es cada uno, con la razón de por qué quedó en ese lugar, **para** no tener que revisar proyecto por proyecto para darme cuenta de cuál necesita atención ya.

Ver `docs/21-priorizacion-sugerencias-investigacion-y-propuesta.md` para la investigación y `docs/22-sistema-prioridad-proyectos.md` para el diseño de lo construido.

### Flujo principal
1. La persona entra a la vista de "prioridad" (o algo similar en el menú).
2. El frontend llama a `GET /api/proyectos/prioridad`.
3. Ve la lista de proyectos activos (los ya finalizados/cancelados/facturados no aparecen), ordenada de más a menos urgente, cada uno con su puntaje y la lista de razones concretas (ej. "el evento es en 3 días", "la propuesta todavía no se ha enviado").

### Criterios de aceptación
- Un proyecto en estado terminal (Finalizado, Cancelado, No ejecutado, Facturado) nunca aparece en esta lista.
- Cada proyecto siempre viene con al menos su puntaje; si el puntaje es 0, la lista de razones puede venir vacía (no hay ninguna señal de urgencia).
- Los proyectos con más puntaje aparecen primero.

### Notas técnicas
- Es Nivel 1 de la propuesta de `docs/21` — reglas simples, sin IA, tal como se pidió explícitamente probar primero.
- Los pesos de cada señal son un punto de partida, pensados para ajustarse con casos reales más adelante (ver `docs/22`).
- Extendida a proveedores y clientes en HU-10, más abajo.

### Estado del backend para esta historia: ✅ Completo

191 pruebas en total (184 pasan, 7 dependen de Docker en este entorno — nada nuevo), 21 nuevas para esta historia, cero regresiones.

---

## HU-10 — Ver a qué proveedor y a qué cliente atender primero

**Como** cualquier persona con acceso a proveedores o clientes, **quiero** ver esas listas ordenadas por qué tanto vale la pena prestarles atención ahora, con la razón de cada puntaje, **para** no dejar pasar buenos proveedores sin usar ni clientes que llevan tiempo sin actividad.

Ver `docs/23-evaluacion-de-dos-proyectos-de-referencia.md` (por qué se descartó copiar código de los dos proyectos de referencia, pero sí se reutilizó su lógica) y `docs/24-prioridad-proveedores-y-clientes.md` para el diseño de lo construido.

### Flujo principal
1. La persona entra a la vista de "prioridad" de proveedores o de clientes.
2. El frontend llama a `GET /api/proveedores/prioridad` o `GET /api/clientes/prioridad`.
3. Ve la lista ordenada de mayor a menor puntaje, cada uno con la lista de razones concretas (ej. "Bien calificado (Score 5/5) pero sin proyectos en los últimos 120 días").

### Criterios de aceptación
- Un proveedor en estado "Bloqueado" nunca aparece en la lista de prioridad de proveedores.
- Cada proveedor/cliente siempre viene con al menos su puntaje; si es 0, la lista de razones puede venir vacía.
- Los de mayor puntaje aparecen primero.

### Notas técnicas
- Sigue siendo 100% reglas, sin IA — tal como se confirmó explícitamente al pedir esta extensión.
- Los pesos son un punto de partida, ajustable con casos reales (igual que HU-09).
- Se dejó fuera del puntaje de clientes el eje "valor monetario" del modelo RFM (`docs/21`) porque `Cliente` no tiene todavía un campo numérico confiable para eso.

### Estado del backend para esta historia: ✅ Completo

213 pruebas en total (206 pasan, 7 dependen de Docker en este entorno — nada nuevo), 22 nuevas para esta historia, cero regresiones.

---

## HU-11 — Invitar a alguien del equipo desde dentro de Nexit, con aceptar/rechazar

**Como** super administradora, **quiero** invitar a alguien nuevo escribiendo su correo, su rol y un mensaje corto, sin ir al dashboard de Supabase ni copiar ningún UUID a mano, **para** que dar de alta a alguien sea un solo paso en vez de dos acciones manuales separadas.

**Como** persona recién invitada, **quiero** ver esa invitación (con el mensaje de quien me invitó) la primera vez que entro, y decidir si la acepto o la rechazo, **para** que mi perfil se cree solo si acepto, sin que nadie más tenga que crearlo por mí.

Ver `docs/25-invitar-y-registrar-en-un-solo-paso.md` para el diseño completo (esto era el único hueco real de backend que quedó tras revisar "qué falta" del sistema — `docs/10`, sección 5).

### Flujo principal
1. La super administradora llama a `POST /api/invitaciones` (correo, rol, mensaje opcional). El backend valida y dispara la invitación real por la Admin API de Supabase.
2. Supabase le manda el correo a la persona invitada, como siempre; hace clic y crea su contraseña en la página de Supabase.
3. La primera vez que entra a Nexit, `GET /api/invitaciones/mia` le muestra su invitación pendiente.
4. Acepta (`POST /api/invitaciones/{id}/aceptar`, completando nombre y apellido) — su perfil se crea solo, con el rol propuesto. O rechaza (`POST /api/invitaciones/{id}/rechazar`) — no se crea nada.

### Criterios de aceptación
- No se guarda ninguna invitación si la llamada real a Supabase falla.
- Nadie puede aceptar o rechazar una invitación que no es la suya (el correo debe coincidir).
- No se puede responder dos veces la misma invitación, ni invitar dos veces al mismo correo mientras la primera siga pendiente.

### Notas técnicas
- Usa las mismas claves de configuración que `docs/17` (`Supabase:ProjectUrl`, `Supabase:ServiceRoleKey`) — si ya las configuraste para la eliminación automática de cuentas, esto funciona sin nada adicional.
- Esta es también la vía para dar de alta la cuenta de `analistacompras@agencianextmkt.com` que quedó mencionada como pendiente en la nota de abajo, cuando quieras invitarla.

### Estado del backend para esta historia: ✅ Completo, 🟡 para probarla de verdad

El código está completo y probado. Falta el mismo paso externo que `docs/17` — que configures `Supabase:ProjectUrl`/`Supabase:ServiceRoleKey` en `appsettings.Production.json` — para poder invitar a alguien de verdad y ver el correo llegar.

226 pruebas en total (219 pasan, 7 dependen de Docker en este entorno — nada nuevo), 11 nuevas para esta historia, cero regresiones.

---

## Nota de referencia (no es una historia, es contexto para cuando exista el Supabase real)

La usuaria mencionó, al describir HU-01, una futura cuenta de administrador: `analistacompras@agencianextmkt.com` (dominio ya permitido). Todavía no se ha invitado ni registrado en ningún lado — con HU-11 ya se puede hacer en un solo paso desde `POST /api/invitaciones` cuando quieras, sin que esta nota requiera ninguna acción por sí sola.

## Próximas historias a escribir

Siguiendo la estructura de `docs/11`: crear/editar/eliminar un cliente, crear/editar un proveedor (con adjuntos), crear/editar un proyecto (con equipo, proveedores asociados y bitácora de seguimiento, incluida la asignación del gerente responsable), ver el calendario, generar y exportar un informe, administrar catálogos, dar de alta un usuario, y el flujo completo de solicitud/aprobación/rechazo de eliminación (desde las tres perspectivas: quien solicita, el gerente que endosa, el admin que decide).

Cada una de esas se escribirá con el mismo cierre de "Estado del backend" de arriba. Adelantando el veredicto según lo que ya se verificó en `docs/11` y `docs/10` (para no repetir la revisión historia por historia): casi todo el sistema — clientes, proveedores + adjuntos, proyectos + equipo + seguimiento + gerente, calendario, catálogos, informes, usuarios, las cuatro decisiones del flujo de solicitudes de eliminación (solicitar, endosar como gerente, aprobar/rechazar como admin), y desde HU-08 también notificaciones, historial de cambios y "mis proveedores" — ya tiene el código, la validación y el control de permisos completos y probados (163/170 pruebas, el resto depende de Docker en este entorno), así que esas historias deberían cerrar en ✅ o 🟡 (🟡 solo donde dependan de que exista el Supabase real, como los ejemplos de arriba). La única pieza que hoy cerraría en 🔴 (falta backend, no es solo un factor externo) es la que quedó anotada en `docs/10`, sección 5: un endpoint que invite y registre a alguien en un solo paso (hoy son dos acciones manuales separadas). Si te hace falta para una historia futura, se marca 🔴 y se prioriza escribir ese backend antes de esa pantalla, tal como pediste. También quedan pendientes de más definición (no de código ya identificado) el sistema de prioridad/sugerencias y las mejoras al informe semanal — ver `docs/20`.
