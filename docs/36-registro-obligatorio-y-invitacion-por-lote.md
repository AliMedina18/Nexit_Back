# Registro obligatorio antes de usar el sistema, e invitar a varios correos de una vez

*2026-09-08. Cierra la mitad de HU-11 que nunca se construyó en el frontend, y el agujero de
seguridad que esa mitad faltante dejaba abierto.*

## 1. El problema: se podía usar Nexit sin haberse registrado nunca

`docs/25` dejó el backend de invitaciones completo: `POST /api/invitaciones` dispara la invitación
real por Supabase, y la persona invitada acepta o rechaza con `GET /api/invitaciones/mia` +
`POST /api/invitaciones/{id}/aceptar`. Lo que nunca se construyó fue la pantalla: en `Nexit_Front`
esos tres endpoints existían en `invitaciones-service.ts` y **no los llamaba nadie**.

La consecuencia no era solo "falta una pantalla". Era esto:

1. La super administradora invita a alguien. Supabase le manda el correo, la persona hace clic y
   crea su contraseña. Ya tiene cuenta en Supabase Auth.
2. Entra a Nexit. Todavía no tiene fila en `usuarios` — su perfil de negocio no existe.
3. El Auth Hook (`docs/schema/03_auth_hook_custom_claims.sql`) le pone `user_role = "miembro"` por
   defecto justamente para ese caso, "para no romper políticas que sí esperan el claim".
4. Con ese claim pasa el `[Authorize]` de `BaseController`. Y `miembro` es un rol real del sistema:
   podía **leer clientes, proveedores y proyectos, escribir en la bitácora de seguimiento y crear
   solicitudes de eliminación**, sin haberse registrado, sin que nadie le hubiera aprobado nada más
   que un correo.

O sea: aceptar la invitación era, en la práctica, opcional. El sistema no lo pedía en ningún
momento y tampoco lo exigía.

## 2. La corrección, en dos capas

### Capa 1 (la que manda): `PerfilRequeridoFilter` en el backend

Filtro global de MVC (`src/Nexit.API/Filters/PerfilRequeridoFilter.cs`), registrado en `Program.cs`
junto a `AddControllers`. Antes de que corra cualquier acción, comprueba que quien llama tenga fila
en `usuarios`:

| Situación | Respuesta |
|---|---|
| Sin fila en `usuarios` | `403` con `codigo: "perfil_requerido"` |
| Con fila, pero `activo = false` | `403` con `codigo: "cuenta_inactiva"` |
| Con fila y activa | pasa |

El campo `codigo` es nuevo en `ErrorResponse` y existe solo para esto: el frontend necesita
distinguir "todavía no te registraste" (mandarla a `/registro`) de "no tienes permiso" (mostrar el
error), y el texto del mensaje no es algo sobre lo que se pueda programar.

**Las únicas seis acciones eximidas** (atributo `[PermitirSinPerfil]`) son exactamente las que hacen
falta para conseguir un perfil:

- `AuthController` completo — `estado-cuenta` y `confirmar-contrasena`, apoyo al login, ocurren
  antes de que exista ningún perfil.
- `GET /api/usuarios/me` — es el endpoint con el que el frontend averigua si la persona ya está
  registrada. Si el filtro lo bloqueara con `403`, nunca podría distinguirlo de "sin permiso"; así
  responde `404`, que es lo que significa de verdad.
- `GET /api/invitaciones/mia`, `POST .../aceptar`, `POST .../rechazar` — el camino del registro.

Esa lista está fijada en una prueba (`tests/Nexit.Tests/Security/PerfilRequeridoTests.cs`, mismo
espíritu que `ControllersRequierenAutorizacionTests`): eximir una acción nueva rompe la prueba a
propósito. Con un filtro global, el riesgo no es olvidarse de proteger algo — es eximir de más.

**Efecto secundario bueno:** esto también cierra la ventana de hasta una hora que documentaba
`Program.cs` para las cuentas desactivadas. El claim `user_active` solo se actualiza cuando el token
se renueva, así que alguien recién desactivado conservaba acceso hasta que su token venciera solo.
El filtro lee el estado real de la fila, así que la desactivación surte efecto en la siguiente
petición — que es lo que pedía el criterio de aceptación de HU-06 ("una cuenta desactivada no puede
usar ningún endpoint del sistema").

**Costo:** una consulta por clave primaria por petición, que devuelve un solo booleano
(`IUsuarioRepository.EstaActivoAsync`). Con el volumen de Next (decenas de cuentas) es despreciable,
y a cambio no depende de que el token se haya renovado ni de correr SQL nuevo en Supabase.

### Capa 2: la pantalla `/registro` en el frontend

`Nexit_Front/src/app/registro/page.tsx`. El `auth-store` ahora distingue cinco estados de perfil
(`EstadoPerfil`), no solo "hay sesión / no hay sesión":

- `cargando` — todavía no se sabe; el dashboard no se pinta.
- `completo` — uso normal.
- `sin-perfil` — `GET /api/usuarios/me` respondió `404`; el layout del dashboard manda a `/registro`.
- `inactivo` — la cuenta fue desactivada; se le cierra la sesión.
- `indeterminado` — no se pudo averiguar (backend caído, sin red). **A propósito no bloquea:** una
  caída del backend no puede mandar a todo el equipo a la pantalla de registro.

En `/registro` la persona ve quién la invitó, con qué rol y el mensaje que le dejaron, escribe su
nombre y apellido (los escribe ella, decisión de `docs/25`) y acepta — o rechaza y sale.

**Detalle que no es obvio:** al aceptar, el frontend llama `supabase.auth.refreshSession()` antes de
seguir. El rol viaja en el JWT y lo pone el Auth Hook leyendo `usuarios`; el token que esa persona
tiene en la mano se emitió *antes* de que existiera su fila, así que dice `miembro` sin importar el
rol que se le haya propuesto. Sin ese refresco, alguien invitado como `admin` entraría con permisos
de miembro hasta que su token venciera solo.

## 3. Invitar a varios correos de una vez

Pedido de la usuaria: que invitar se parezca a escribir un correo en Gmail — varios destinatarios en
un solo campo, un solo envío.

`POST /api/invitaciones/lote` (`SuperAdminOnly`), cuerpo `{ emails: [...], rol, mensaje }`, hasta 25
por envío. Todos los correos del lote comparten rol y mensaje; si hacen falta roles distintos, son
dos envíos, que es más claro que una tabla de correo+rol dentro de un modal.

**Nunca es todo-o-nada.** La respuesta es `200` incluso si algunos fallan, con dos listas separadas:

```json
{
  "enviadas": [ { "email": "una@agencianextmkt.com", "rol": "manager", "estado": "Pendiente" } ],
  "fallidas": [ { "email": "otra@dominio-ajeno.com", "motivo": "El correo no pertenece a un dominio laboral permitido." } ]
}
```

La razón es de uso real: quien invita escribe cinco correos de corrido, y un dedazo en uno solo no
puede hacer perder el envío entero. El caso de uso reutiliza correo por correo el mismo validador
(`CrearInvitacionValidator`) y el mismo `CrearInvitacionUseCase` de la invitación individual, así
que no hay una segunda copia de las reglas que se pueda desincronizar. Los repetidos dentro del
mismo envío se ignoran en silencio (escribir dos veces el mismo correo es un error de dedo, no dos
invitaciones); los repetidos contra la base los sigue detectando el validador de siempre.

En el frontend, `InviteModal` pasó a ser un campo de fichas: se escribe un correo y con Enter, coma,
punto y coma o espacio se convierte en ficha; se pueden pegar varios de golpe; Retroceso con el
campo vacío borra la última (el gesto que ya espera cualquiera que haya usado el campo "Para" de un
correo). Si algo falla, el modal **no se cierra** — deja las fichas fallidas puestas con su motivo
al lado, que es justo la información que hace falta para corregir.

## 4. Qué se probó

- `PerfilRequeridoTests` (2, unitarias por reflexión) — la lista de acciones eximidas es exactamente
  la esperada, ni una más ni una menos.
- `PerfilRequeridoFunctionalTests` (8, contra Postgres real) — sin perfil: `403` en clientes,
  proveedores, proyectos, catálogos y notificaciones; `404` (no `403`) en `/api/usuarios/me`; sin
  bloqueo en `/api/invitaciones/mia`; y una cuenta desactivada pierde el acceso en la siguiente
  petición aunque su token no traiga `user_active=false`.
- `InvitacionesTests` (4 nuevas) — el lote invita a todos los válidos; un correo inválido no impide
  invitar a los demás; una falla de Supabase en un correo no tumba el resto; los repetidos y
  espacios del mismo envío se colapsan en uno.

## 5. Pendiente

- **Nada en base de datos.** Este trabajo no agrega ni cambia ninguna columna: no hay `schema/`
  nuevo que correr en producción.
- La lista de dominios permitidos del frontend (`src/lib/dominios-correo.ts`, extraída de
  `login/page.tsx` para compartirla con el modal de invitar) sigue siendo una copia de validación de
  UX del catálogo `dominios_correo_permitidos`. Si algún día se agrega un dominio, hay que tocar los
  dos lados — ya está anotado dentro del propio archivo.
- Sigue sin haber **cancelar ni reenviar** una invitación (`docs/25` lo dejó fuera a propósito). Con
  la pantalla de registro ya construida, se vuelve más visible: una invitación pendiente que nunca
  se acepta se queda ahí para siempre y bloquea invitar de nuevo a ese correo.
