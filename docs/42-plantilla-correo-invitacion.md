# Plantilla del correo de invitación

Cuarta plantilla del set de correos de Nexit, con el mismo sistema visual que las otras tres (`docs/14`, `docs/16`, y la de restablecer contraseña) — acento `#6fceca`, el mismo encabezado con el ícono y "Nexit", tipografía Arial, fondo blanco, sin emojis, mismo pie discreto. Antes de esto, "Invite user" se había quedado con el texto por defecto de Supabase (sin el diseño del resto del sistema).

## Qué tiene distinto esta plantilla

- **Es la única de las cuatro con un botón**, no un código. Las otras tres (`Magic Link`/OTP y las dos de seguridad) muestran algo que la persona lee o solo informan; esta necesita que la persona haga clic para entrar por primera vez, así que usa `{{ .ConfirmationURL }}` (la variable que Supabase ya expone para esta plantilla, confirmado en `docs/09`) en un botón con el color de marca, más el enlace en texto plano debajo por si el botón no se ve bien en algún cliente de correo (Outlook de escritorio clásico, sobre todo).
- **Sí muestra quién invitó, a qué rol, y el motivo -- actualizado 2026-09-09.** La primera versión no los mostraba porque el backend solo le mandaba a Supabase el correo de la persona invitada. Ahora `CrearInvitacionUseCase` arma esos tres datos (el nombre de quien invita, la etiqueta del rol -- "Admin"/"Director"/"Miembro" -- y el mensaje que escribió quien invita) y se los manda a Supabase como metadata de la cuenta (`data` en el body de `POST /auth/v1/invite`); la plantilla los lee con `{{ .Data.invitadoPor }}`, `{{ .Data.rol }}` y `{{ .Data.mensaje }}`. El mensaje es opcional en el backend (puede no venir), así que ese bloque va dentro de un `{{ if .Data.mensaje }}...{{ end }}` -- si no hay mensaje, el correo simplemente no muestra esa parte, en vez de mostrar un hueco vacío.
- El resto -- estructura, colores, tipografía, pie -- es igual a las otras tres, para que las cuatro se vean del mismo sistema.

## Archivo

`docs/plantilla_correo_invitacion.html` -- HTML completo, listo para copiar y pegar tal cual.

## Cómo aplicarla en Supabase

1. Entra a tu proyecto → **Authentication → Emails**.
2. Selecciona la plantilla **"Invite user"**.
3. Cambia a la vista de código fuente (ícono `</>` o "Source").
4. Borra el contenido que haya y pega el HTML completo de `docs/plantilla_correo_invitacion.html`.
5. En el campo **Subject**, pon:
   ```
   Te invitaron a unirte a Nexit
   ```
6. Guarda. Los cambios pueden tardar unos minutos en reflejarse en la próxima invitación que se mande.

## Cómo se prueba

Se prueba invitando a un correo real desde **Usuarios → Invitar** dentro de Nexit (el mismo flujo que ya usaste para probar el envío de invitaciones). No hace falta nada especial de Supabase para probarla, a diferencia de la de "Contraseña cambiada" (`docs/16`).

## Referencias

- `docs/09-crear-proyecto-supabase-paso-a-paso.md`, sección 2.2 punto 6 -- confirma que "Invite user" usa `{{ .ConfirmationURL }}` y que la contraseña se crea dentro de Nexit, no en la página de Supabase.
- `docs/14-plantilla-correo-otp.md` y `docs/16-plantilla-correo-contrasena-cambiada.md` -- las otras dos plantillas del mismo sistema visual, con las referencias de diseño de correo que se investigaron para todo el set.
