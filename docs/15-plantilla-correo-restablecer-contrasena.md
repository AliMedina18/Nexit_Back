# Plantilla del correo para restablecer contraseña (HU-04)

Mismo diseño que `docs/14-plantilla-correo-otp.md` (el correo de login), pero para la plantilla **separada** que Supabase usa cuando alguien pide recuperar su contraseña (`resetPasswordForEmail`). Es una plantilla distinta dentro del dashboard — aunque el HTML se ve casi igual, Supabase la dispara en un momento distinto y con un texto que sí debe ser diferente (aquí no es "tu primer ingreso", es "pediste cambiar tu contraseña").

## Qué cambia respecto a la plantilla de login

- El texto de introducción: "Recibimos una solicitud para restablecer tu contraseña en Nexit..." en vez de "Es tu código para entrar por primera vez...".
- La etiqueta sobre el código dice "TU CÓDIGO DE RECUPERACIÓN" en vez de "TU CÓDIGO DE ACCESO".
- La línea de tranquilidad es más específica: aclara que si no fuiste tú, la contraseña actual sigue funcionando y no se hace ningún cambio — importante aquí porque, a diferencia del login, este correo sí implica que *alguien* pidió cambiar algo, y conviene dejar claro que no pasa nada si se ignora.
- El asunto sugerido también cambia (ver abajo).

El resto — color de acento `#6fceca`, el ícono (cuadrito con check, hecho en HTML/CSS puro, no imagen — así no se rompe en Gmail como pasó con la primera versión de la de login), la tipografía, el fondo blanco, sin emojis — es exactamente igual, para que los dos correos se vean como del mismo sistema.

## Archivo

`docs/plantilla_correo_restablecer_contrasena.html` — HTML completo, listo para copiar y pegar.

## Cómo aplicarla en Supabase

1. Entra a tu proyecto → **Authentication → Emails**.
2. Esta vez selecciona la plantilla **"Reset Password"** ("Restablecer contraseña" en la versión en español del dashboard) — **no** la de "Magic Link"/"Enlace mágico o OTP", esa es la de login y ya quedó lista aparte.
3. Cambia a la vista de código fuente (ícono `</>` o "Source").
4. Borra el contenido que haya y pega el HTML completo de `docs/plantilla_correo_restablecer_contrasena.html`.
5. En el campo **Subject** (asunto), pon:
   ```
   Restablece tu contraseña en Nexit: {{ .Token }}
   ```
6. Guarda.

## Por qué usa `{{ .Token }}` y no un enlace

Igual que en la plantilla de login (ver `docs/14`, última sección): el diseño de este sistema usa el código de 6 dígitos escrito a mano en todos los flujos de autenticación, nunca un enlace de "un clic". HU-04 (`docs/12-historias-de-usuario.md`) ya especifica que el frontend llama `verifyOtp({ email, token, type: 'recovery' })` con ese mismo código — así que la plantilla de recuperación tiene que mostrar `{{ .Token }}`, igual que la de login, para que ambas historias usen la misma mecánica.

## Prueba pendiente

Falta disparar un `resetPasswordForEmail` real contra `analistacompras@agencianextmkt.com` una vez esta plantilla ya esté pegada en el dashboard, para confirmar que el correo llega bien formateado — el mismo tipo de prueba que ya se hizo con la de login. Avísame cuando la hayas pegado y lo pruebo.
