# Invitar y registrar en un solo paso (con aceptar/rechazar)

Esto es lo que quedó pendiente en `docs/10`, sección 5 (el único hueco real de backend que quedó
tras revisar "qué falta" del sistema completo) — y quedó construido con la idea que describiste:
que se invite desde dentro de la plataforma, con un mensajito, y que la persona invitada pueda
aceptar o rechazar.

## Por qué no era tan simple como "un botón que haga las dos cosas"

El correo de verdad para alguien que todavía no tiene cuenta **lo sigue mandando Supabase** — este
backend nunca ha enviado correos (`docs/10`) y eso no cambió; no hay forma de "notificarle dentro
de la plataforma" a alguien que aún no puede entrar a la plataforma. Lo que sí cambió es todo lo
demás: antes había que ir al dashboard de Supabase a mano para invitar, y luego, por separado,
alguien tenía que copiar el UUID que Supabase le asignó y pegarlo en `POST /api/usuarios`. Ahora
todo eso lo hace el backend solo, y la parte de "aceptar o rechazar con un mensaje" sí pasa
completamente dentro de Nexit, la primera vez que la persona invitada inicia sesión.

## Cómo funciona, paso a paso

1. **La super administradora invita desde Nexit.** `POST /api/invitaciones` con el correo, el rol
   propuesto (admin/manager/miembro) y, si quiere, un mensaje corto ("bienvenida al equipo, nos
   vemos el lunes"). El backend valida el correo (dominio permitido, que no exista ya un usuario o
   una invitación pendiente con ese correo) y llama directo a la Admin API de Supabase para
   disparar la invitación real — nada de eso lo hace la super administradora a mano.
2. **Supabase le manda el correo a la persona**, como siempre. Hace clic, y en la página de
   Supabase (no de Nexit) establece su contraseña.
3. **La primera vez que esa persona entra a Nexit**, el frontend llama a `GET /api/invitaciones/mia`.
   Como esa persona todavía no tiene fila en `usuarios`, el Auth Hook le da el rol por defecto
   `miembro` (`docs/schema/03_auth_hook_custom_claims.sql`) — pero eso alcanza para que el backend
   la reconozca como alguien autenticado y le busque su invitación pendiente por correo.
4. **Ve la invitación:** quién la invitó, con qué rol propuesto, y el mensaje. Decide:
   - **Aceptar** (`POST /api/invitaciones/{id}/aceptar`, completando su nombre y apellido) — el
     backend crea su perfil de negocio automáticamente, con el rol que se le propuso, usando su
     propio UUID (el mismo que usó para autenticarse) — **esto es lo que elimina el paso manual de
     copiar el UUID.**
   - **Rechazar** (`POST /api/invitaciones/{id}/rechazar`) — no se crea ningún perfil. La cuenta de
     Supabase Auth queda existiendo (ya la creó Supabase al aceptar el correo), pero sin perfil en
     Nexit no puede hacer nada dentro del sistema.

```json
// GET /api/invitaciones/mia
{
  "id": "...",
  "email": "nueva@agencianextmkt.com",
  "rol": "manager",
  "mensaje": "Bienvenida al equipo, nos vemos el lunes",
  "estado": "Pendiente",
  "invitadoPorNombre": "Alicia Medina",
  "createdAt": "2026-08-24T...",
  "fechaRespuesta": null
}
```

## Configuración que necesitas revisar (importante)

Esto usa **las mismas dos claves** que ya documentó `docs/17` para la eliminación automática de
cuentas (`Supabase:ProjectUrl` y `Supabase:ServiceRoleKey`, en `appsettings.Production.json`, que
ya está en `.gitignore`). Si ya las configuraste para que la eliminación automática borre cuentas
de Supabase Auth, **esta función nueva ya funciona sin que hagas nada más**. Si todavía no las
configuraste, invitar a alguien falla con un mensaje claro explicando exactamente eso (no falla en
silencio) — la puedes probar hasta que las configures.

## Endpoints nuevos

| Endpoint | Quién | Qué hace |
|---|---|---|
| `POST /api/invitaciones` | Solo super admin | Crea la invitación y dispara el correo real por Supabase |
| `GET /api/invitaciones` | Solo super admin | Ver todas las invitaciones (pendientes, aceptadas, rechazadas) |
| `GET /api/invitaciones/mia` | Cualquier autenticado | La invitación pendiente que le corresponde por su correo (404 si no hay ninguna) |
| `POST /api/invitaciones/{id}/aceptar` | Cualquier autenticado | Acepta su propia invitación (correo debe coincidir) — crea su perfil |
| `POST /api/invitaciones/{id}/rechazar` | Cualquier autenticado | Rechaza su propia invitación (correo debe coincidir) |

## Decisiones de diseño

- **El nombre y apellido los escribe la propia persona invitada al aceptar**, no quien invita — más
  preciso (nadie mejor que ella sabe cómo escribir su nombre) y menos trabajo para quien invita.
- **Se valida que el correo de quien acepta/rechaza coincida con el de la invitación** — nadie puede
  aceptar o rechazar una invitación que no es suya, aunque conozca el ID.
- **Si Supabase no está configurado o falla, no se guarda ninguna invitación "fantasma"** — primero
  se dispara la invitación real, y solo si eso funciona se guarda el registro en la base. Así nunca
  queda una invitación "Pendiente" en Nexit de la que la persona nunca se enteró.
- **Tabla nueva, sin tocar nada existente** — `invitaciones_equipo`, con Row Level Security igual
  que el resto del esquema (solo el rol de aplicación `nexit_app` puede tocarla).
- **El proceso manual de antes sigue funcionando** (invitar desde el dashboard de Supabase +
  `POST /api/usuarios` con el UUID a mano) — esto no lo reemplaza, solo agrega el camino más rápido.

## Lo que se dejó fuera, a propósito

- **No hay reenvío de invitación** todavía como acción separada — si el correo no llega, hoy tocaría
  invitar de nuevo desde cero (que fallaría porque ya existe una invitación Pendiente para ese
  correo) o resolverlo desde el dashboard de Supabase directamente. Se puede agregar si hace falta.
- **No hay vencimiento automático de una invitación vieja** — una invitación Pendiente queda así
  indefinidamente hasta que se acepte o rechace. Si más adelante quieres que expiren solas después
  de X días, es un cambio pequeño (mismo patrón que la eliminación automática de `docs/17`).

## Verificación

226 pruebas en total (219 pasan, 7 dependen de Docker en este entorno — el mismo grupo de siempre),
11 nuevas para esta pieza: que la invitación se dispare antes de guardarse (y que no quede nada
guardado si Supabase falla), que se pueda consultar/aceptar/rechazar la propia invitación, que se
bloquee aceptar/rechazar la de alguien más, que no se pueda responder dos veces la misma invitación,
y que quien ya tiene perfil no pueda aceptar otra.
