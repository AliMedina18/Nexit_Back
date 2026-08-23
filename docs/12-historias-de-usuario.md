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

## Nota de referencia (no es una historia, es contexto para cuando exista el Supabase real)

La usuaria mencionó, al describir HU-01, una futura cuenta de administrador: `analistacompras@agencianextmkt.com` (dominio ya permitido). Todavía no se ha invitado ni registrado en ningún lado — queda anotada aquí solo como referencia para cuando se ejecute el alta real de usuarios (`docs/09`, secciones 7-9), no requiere ninguna acción en este documento.

## Próximas historias a escribir

Siguiendo la estructura de `docs/11`: crear/editar/eliminar un cliente, crear/editar un proveedor (con adjuntos), crear/editar un proyecto (con equipo, proveedores asociados y bitácora de seguimiento, incluida la asignación del gerente responsable), ver el calendario, generar y exportar un informe, administrar catálogos, dar de alta un usuario, y el flujo completo de solicitud/aprobación/rechazo de eliminación (desde las tres perspectivas: quien solicita, el gerente que endosa, el admin que decide).

Cada una de esas se escribirá con el mismo cierre de "Estado del backend" de arriba. Adelantando el veredicto según lo que ya se verificó en `docs/11` y `docs/10` (para no repetir la revisión historia por historia): casi todo el sistema — clientes, proveedores + adjuntos, proyectos + equipo + seguimiento + gerente, calendario, catálogos, informes, usuarios, y las cuatro decisiones del flujo de solicitudes de eliminación (solicitar, endosar como gerente, aprobar/rechazar como admin) — ya tiene el código, la validación y el control de permisos completos y probados (120/120 pruebas), así que esas historias deberían cerrar en ✅ o 🟡 (🟡 solo donde dependan de que exista el Supabase real, como los ejemplos de arriba). Las únicas piezas que hoy cerrarían en 🔴 (falta backend, no es solo un factor externo) son las que ya quedaron anotadas en `docs/10`, sección 5: un endpoint que invite y registre a alguien en un solo paso (hoy son dos acciones manuales separadas), y notificaciones (de cualquier tipo, no solo correo) para el flujo de solicitudes de eliminación — hoy nadie se entera de una solicitud nueva sin entrar a revisar activamente. Si alguna de esas dos te hace falta para una historia futura, se marca 🔴 y se prioriza escribir ese backend antes de esa pantalla, tal como pediste.
