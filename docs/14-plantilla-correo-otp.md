# Plantilla del correo con el código OTP

Investigué prácticas recomendadas de Supabase y ejemplos de correos de código de verificación antes de armar esto (ver Referencias) — la plantilla sigue el estándar de correos HTML (tablas, estilos en línea, sin CSS moderno que Gmail/Outlook no soportan bien) y usa las variables que Supabase realmente expone para la plantilla "Magic Link" / "Enlace mágico o OTP".

**Actualización 2026-08-23 — segunda versión, con mejor diseño y redacción.** Se revisaron ejemplos y guías de correos de verificación de plataformas conocidas (ver Referencias) para mejorar tres cosas puntuales: (1) el asunto, que ahora muestra el código directamente (Supabase sí permite variables como `{{ .Token }}` en el asunto, igual que en el cuerpo); (2) la redacción, con un saludo más cercano pero profesional y una explicación más clara de por qué llega el correo; (3) el diseño visual, con una franja de color de acento arriba, una etiqueta pequeña sobre el código ("TU CÓDIGO DE ACCESO"), y un bloque aparte con el aviso de seguridad para que no se pierda entre el resto del texto.

**Actualización 2026-08-23 — tercera versión, con el color de marca y sin emojis.** Cambios pedidos directamente:
- Color de acento: `#6fceca` (el color real del sitio de Next que diste), usado en la franja superior, el ícono, y el resaltado del bloque del código. El fondo se mantiene blanco, como pediste.
- Se armó un pequeño logotipo para el software: un ícono (un check dentro de un cuadrado redondeado, en `#6fceca`) junto a la palabra **"Nexit"** en tipografía bold — el correo ahora se presenta como que viene del software (Nexit), no de la compañía (Next). El ícono es una imagen (PNG embebida directo en el HTML, no depende de un link externo), no un emoji.
- Se quitaron los dos emojis (👋 y 🔒) que tenía la versión anterior — no queda ninguno en el correo.
- Se quitó el aviso "no compartas este código con nadie" — quedaba con tono de alarma; el texto ahora solo explica que es el código para entrar por primera vez y crear la contraseña, sin generar preocupación.
- Se quitó la línea "Este es un mensaje automático, no respondas..." del pie — ahora el pie solo dice "Nexit es el sistema interno de Next.", una mención discreta, sin ser el protagonista.
- Se mantuvo, más corta, la línea de "si no lo solicitaste, ignóralo" — es información útil y no suena alarmante, a diferencia de la de "no lo compartas".

**Nota técnica sobre el ícono:** va embebido como imagen PNG en base64 directo en el HTML (no depende de que cargues nada externo). Se ve bien en Gmail, Apple Mail y la mayoría de clientes modernos. Outlook de escritorio (la versión clásica, no la web) a veces no muestra imágenes embebidas así — en ese caso, el correo se sigue viendo bien, solo faltaría el ícono junto a la palabra "Nexit". Si te preocupa ese caso puntual, se puede resolver más adelante subiendo el ícono a un link público en vez de embebido.

## Archivo

`docs/plantilla_correo_otp.html` — el HTML completo, listo para copiar y pegar tal cual.

## Cómo aplicarla en Supabase

1. Entra a tu proyecto → **Authentication → Emails**.
2. Selecciona la plantilla **"Magic Link"** (es la que se usa para el código OTP de HU-01/HU-02, ver `docs/10`, sección 2.2).
3. Verás un editor con vista "Rich text" y otra de código fuente/HTML — cambia a la vista de **código fuente** (el ícono `</>` o "Source", según la versión del dashboard).
4. Borra el contenido que haya y pega el HTML completo de `docs/plantilla_correo_otp.html`.
5. En el campo **Subject** (asunto), pon exactamente esto (sí acepta la misma variable que el cuerpo, así que el código aparece directo en la vista previa del correo, sin tener que abrirlo):
   ```
   Tu código de acceso a Nexit: {{ .Token }}
   ```
6. Guarda. Los cambios del dashboard pueden tardar unos minutos en reflejarse en el próximo correo que se envíe.

## Qué tiene la plantilla

- Usa `{{ .Token }}` — la variable real que Supabase reemplaza por el código de 6 dígitos (confirmado contra la documentación oficial).
- Diseño de una sola columna, máximo 600px, con el código bien grande y espaciado para que no se preste a error al transcribirlo.
- Aviso de seguridad estándar ("si no fuiste tú, ignora este correo") — buena práctica para este tipo de correos.
- Sin logo real de Next (no tengo un archivo de logo) — por ahora dice "NEXT | Nexit" en texto. Si me pasas un logo (PNG/SVG, fondo transparente idealmente), lo puedo incrustar directo en el HTML como imagen embebida en base64, para que no dependa de un link externo que algunos clientes de correo bloquean.
- Colores neutros (gris oscuro/blanco) — fácil de ajustar si quieres los colores de marca de Next; solo dime los códigos de color (hex) o compárteme algo con la paleta y lo actualizo.

## Nota técnica (por qué no incluye el enlace de "clic para entrar")

Supabase recomienda incluir también `{{ .ConfirmationURL }}` como alternativa al código, por si algún filtro corporativo "pre-visita" el enlace y lo invalida antes de que la persona haga clic. No lo agregué porque el diseño de login de este proyecto (`docs/10`) usa solo el código escrito a mano, nunca el enlace — agregarlo abriría una segunda forma de iniciar sesión que el frontend no maneja todavía. Si en algún momento quieres soportar también "iniciar sesión con un clic", se agrega fácil.

## Referencias

- [Email Templates | Supabase Docs](https://supabase.com/docs/guides/auth/auth-email-templates) — variables disponibles y tipos de plantilla.
- [Custom Email Notification Templates in Supabase (2026)](https://www.pingram.io/blog/supabase-custom-email-notification-templates) — buenas prácticas y trampas comunes (2026).
- [OTP Verification Email Template — Redwiat (GitHub)](https://github.com/Redwiat/otp-verification-email-template) — referencia de diseño para correos de código OTP.
- [Email Verification Examples: 11 High-Converting Templates | mailfloss](https://mailfloss.com/email-verification-examples-11-templates-for-2025/) — patrones de diseño (jerarquía visual, formato del código, colores).
- [3 Powerful OTP Email Templates & Examples | MailMaestro](https://www.maestrolabs.com/email-templates/3-powerful-otp-email-templates-examples) — ejemplos de redacción y asuntos para este tipo de correo.
- [The Best 48 Verification Email Examples & Designs | Really Good Emails](https://reallygoodemails.com/categories/verification) — galería de referencia de correos de verificación reales.
