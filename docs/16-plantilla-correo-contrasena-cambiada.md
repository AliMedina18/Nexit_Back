# Plantilla del correo de seguridad "Contraseña cambiada"

Esta es la tercera plantilla del set de autenticación, y es distinta a las otras dos en un punto importante: **no la dispara una persona pidiendo algo** (como el código de login o el de recuperación) — Supabase la manda **sola, automáticamente**, justo después de que una contraseña ya cambió de verdad (`updateUser({ password })`, sin importar si vino de HU-01 —primera vez— o de HU-04 —recuperación—). Es una notificación de seguridad: el objetivo es que, si alguien más cambió tu contraseña sin que fueras tú, te enteres de inmediato.

Vive en un grupo aparte del dashboard de Supabase llamado **"Security notifications"**, junto a otras parecidas (cambio de correo, MFA agregada/quitada, etc.) — ya la activaste ahí, según lo que confirmaste.

## Qué tiene distinto esta plantilla

- **No lleva ningún código.** Esta plantilla no usa `{{ .Token }}` — no hay nada que la persona tenga que escribir en ningún lado, es puramente informativa. La única variable que usa es `{{ .Email }}`, para mostrar a qué cuenta le cambió la contraseña.
- **El tono sí es más directo que las otras dos**, a propósito: mientras que en los correos de código dijiste explícitamente que no querías nada alarmante ("vamos a asustar a la persona"), aquí el propósito del correo *es* que la persona reaccione si no fue ella — así que el bloque de "¿no fuiste tú?" es más notorio, con un enlace directo de contacto (`analistacompras@agencianextmkt.com`) en vez de solo un texto suelto. Se mantiene sin ser alarmista (mismo color de marca, sin rojo ni íconos de advertencia), pero sí es claro sobre qué hacer.
- El resto — acento `#6fceca`, ícono en HTML/CSS puro (sin imagen), tipografía, fondo blanco, sin emojis, pie discreto — es igual a las otras dos, para que las tres se vean como del mismo sistema.

## Archivo

`docs/plantilla_correo_contrasena_cambiada.html` — HTML completo, listo para copiar y pegar.

## Cómo aplicarla en Supabase

1. Entra a tu proyecto → **Authentication → Emails**.
2. Busca la sección **"Security notifications"** (donde ya activaste esta notificación) y selecciona la plantilla **"Password changed"** ("Contraseña cambiada").
3. Cambia a la vista de código fuente (ícono `</>` o "Source").
4. Borra el contenido que haya y pega el HTML completo de `docs/plantilla_correo_contrasena_cambiada.html`.
5. En el campo **Subject**, pon:
   ```
   Tu contraseña en Nexit fue actualizada
   ```
   (Sin variable en el asunto esta vez — no hay código que mostrar, a diferencia de las otras dos plantillas.)
6. Guarda.

## Cómo se prueba de verdad

A diferencia de las otras dos (que se disparan con una sola llamada pública a la API de Supabase), esta **solo se dispara cuando una contraseña cambia de verdad** — no hay un endpoint público que la mande "de prueba" sin cambiar nada. Para probarla de punta a punta hay que completar el flujo real de HU-04 una vez:
1. Disparar `resetPasswordForEmail` para `analistacompras@agencianextmkt.com` (yo lo hago).
2. Tú me pasas el código de 6 dígitos que llegue a ese correo.
3. Yo verifico ese código y, con la sesión que eso genera, cambio la contraseña de esa cuenta a una que tú me indiques.
4. Ese cambio real dispara automáticamente este correo — revisamos que llegue bien.

**Importante:** el paso 3 cambia de verdad la contraseña de la cuenta `analistacompras@agencianextmkt.com` (la super_admin). No es una prueba "de mentiras" — al terminar, esa va a ser la contraseña real de esa cuenta. Dime si quieres seguir con esa cuenta o prefieres que probemos con otra, y qué contraseña quieres que quede puesta.

## Referencias

- [Introducing Seven New Email Templates for Supabase Auth](https://supabase.com/blog/introducing-seven-new-email-templates-for-auth) — anuncio oficial de esta plantilla y las demás notificaciones de seguridad.
- [Email Templates | Supabase Docs](https://supabase.com/docs/guides/auth/auth-email-templates) — variables disponibles por tipo de plantilla.
