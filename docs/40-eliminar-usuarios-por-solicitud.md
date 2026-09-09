# 40 · Eliminar usuarios por solicitud (y la base de datos al día)

**Fecha:** 2026-09-08
**Alicia:** *"recuerda que lo que te digo, eliminar un usuario de una vez le llega una notificación al administrador y al super administrador, son ellos dos. Y le tiene que aparecer en la tabla de solicitud de eliminación en la parte de gestión de usuario al administrador y al super administrador."*

Dos cosas en un mismo documento porque van juntas: el circuito nuevo para eliminar personas, y el
script de base de datos que hacía falta para que gestión de usuarios funcione de verdad — incluido
un error que llevaba semanas rompiendo la pantalla en silencio.

---

## 1. Lo que estaba roto (y desde cuándo)

### `relation "invitaciones_equipo" does not exist`

Cada `GET /api/invitaciones` devolvía **500**. La tabla se creó en el repo el **2026-08-24**
(migración `AddInvitacionesEquipo` + `docs/schema/08_invitaciones_equipo.sql`), pero el archivo 08
nunca se llegó a correr contra Supabase. O sea: no es algo que se rompiera hoy, llevaba así desde
agosto — la pantalla de invitaciones nunca cargó en esa base.

No hay forma de que el backend lo arregle solo. En este proyecto Supabase **no** corre migraciones
de EF (su `__EFMigrationsHistory` está desfasado a propósito, ver la cabecera de los archivos 06, 07
y 08): se actualiza corriendo los scripts de `docs/schema` a mano.

### El CHECK de notificaciones rechazaba las de invitación

`ck_notificaciones_tipo` solo aceptaba los tres tipos de solicitudes de eliminación. Desde que
aceptar o rechazar una invitación genera una notificación (`invitacion_aceptada` /
`invitacion_rechazada`, docs/38), la base la rechazaba al guardar. Nunca llegó a saltar en producción
solo porque la tabla de invitaciones no existía.

### Nadie con uso real del sistema se podía eliminar

Tres llaves foráneas hacia `usuarios` estaban en `ON DELETE RESTRICT`:

| Tabla | Columna |
|---|---|
| `historial_cambios` | `usuario_id` |
| `invitaciones_equipo` | `invitado_por_id` |
| `solicitudes_eliminacion` | `solicitado_por_id` |

Con eso, eliminar a alguien que alguna vez editó un cliente, invitó a un compañero o pidió una
eliminación fallaba con violación de llave foránea. Y `historial_cambios` se escribe en **cada**
creación y edición, así que en la práctica cualquiera con una semana de uso era ineliminable. La
limpieza automática de los 30 días (docs/17) se atascaba igual, en silencio, dentro del background
service.

Ahora son `ON DELETE SET NULL` y las tres columnas son nullables. El historial no se borra nunca:
la fila se conserva con el "quién" en blanco, y quién era esa persona queda en `usuarios_eliminados`.

---

## 2. Qué correr, y contra qué base

Un solo archivo, una sola vez, **contra la base a la que apunta el backend que estés usando**:

```
docs/schema/25_gestion_usuarios_al_dia.sql
```

Esto es lo que más confusión causó: `dotnet run` arranca en entorno **Development**, y ahí manda
`appsettings.Development.json` → **Postgres local, base `nexit_dev`**. El error 42P01 salía de esa
base, no de Supabase. Hay que correrlo en las dos si se usan las dos:

| Backend | Base | Cómo |
|---|---|---|
| `dotnet run` / F5 (Development) | `nexit_dev` en localhost | pgAdmin/DBeaver, o `psql -h localhost -U postgres -d nexit_dev -f docs/schema/25_gestion_usuarios_al_dia.sql` |
| Publicado (Production) | Supabase | SQL Editor de Supabase |

Es idempotente (se puede correr dos veces sin daño) y no borra datos. Al final del archivo hay
consultas de verificación comentadas, y al principio una de diagnóstico por si quieres mirar antes de
tocar nada.

**Verificado contra Postgres 16 real** (2026-09-09) sobre una base que reproducía el estado de
`nexit_dev`: sin `invitaciones_equipo`, con los CHECK viejos, con los tres FK en RESTRICT y con una
persona que tenía historial y una solicitud a su nombre. Antes del script, borrarla fallaba con
violación de llave foránea; después, se borra y tanto el historial como la solicitud sobreviven con el
"quién" en `NULL`. Segunda pasada: sin errores. Los CHECK siguen rechazando valores inválidos.

El bloque de RLS se salta solo cuando el rol `nexit_app` no existe (es el caso de `nexit_dev`): sin
eso, el script entero se caía en la base local con `role "nexit_app" does not exist`.

**No** corras la migración `20260908222937_AddEliminacionUsuarioPorSolicitud` contra Supabase: el
snapshot del modelo venía atrasado respecto a los scripts 20/21/22, así que esa migración también
crea `etapas_cliente`, `cliente_emails` y `proveedor_emails`, que allá ya existen, y fallaría. Esa
migración es para la base local:

```bash
dotnet ef database update --project src/Nexit.Infrastructure --startup-project src/Nexit.API
```

---

## 3. El circuito nuevo: eliminar personas

### Antes

`DELETE /api/usuarios/{id}`, exclusivo del super_admin, borraba en el acto.

### Ahora

**Ese endpoint ya no existe.** Una cuenta solo puede desaparecer por dos caminos:

1. **Alguien lo pide, alguien lo aprueba.** `POST /api/solicitudeseliminacion` con
   `{ tipoEntidad: "usuario", entidadId, motivo }`. Es el mismo circuito que ya existía para
   clientes, proveedores y proyectos (docs/19), y ahora acepta `'usuario'` como cuarto tipo.
2. **La limpieza automática de los 30 días** (docs/17), para cuentas desactivadas y olvidadas.

### Quién puede pedirlo, y sobre quién

| Regla | Por qué |
|---|---|
| Solo `admin` y `super_admin` pueden pedirlo | Son los únicos que entran a gestión de usuarios |
| Nadie puede pedir la suya propia | Darse de baja a uno mismo no es esto; para eso está desactivar |
| La cuenta del `super_admin` no se puede pedir | Si el sistema se queda sin ella, no hay quien vuelva a dar de alta a nadie |
| Quien aprueba no puede aprobar la eliminación de su propia cuenta | Misma razón, por si alguien la pidió antes |

El frontend apaga el botón en los dos primeros casos con el motivo en el `title`, para que nadie
haga clic solo para recibir un 403.

### A quién le llega la notificación

A **todos los administradores y al super administrador** — que es lo que pidió Alicia — menos dos:

- **quien la pidió**, porque ya lo sabe;
- **la persona en cuestión**, porque avisarle "pidieron eliminarte" desde su propia campana sería
  cruel y además no puede hacer nada al respecto.

La solicitud aparece en la tabla **Solicitudes de eliminación** de gestión de usuarios, que ven
tanto el administrador como el super administrador (`GET /api/solicitudeseliminacion` es
`AdminOrAbove`).

### Qué pasa al aprobar

`AprobarComoAdminUseCase`, caso `"usuario"`, hace exactamente lo mismo que la limpieza automática:

1. respaldo en `usuarios_eliminados` (con `eliminado_por_id` = quien aprobó);
2. borrado del perfil;
3. `SaveChangesAsync`;
4. y solo entonces, la cuenta de Supabase Auth — porque es lo único de aquí que no se puede deshacer.

Si el perfil ya no existe (alguien más lo eliminó antes), la solicitud se marca aprobada sin volver
a intentar nada y sin tocar Supabase.

---

## 4. Textos

`NotificacionFactory.Articulo(tipoEntidad)` traduce el tipo a algo que se pueda leer: "un cliente",
"un proveedor", "un proyecto", **"una cuenta de usuario"**. Existe porque los títulos se construían
interpolando `TipoEntidad` a pelo, y *"Solicitud para eliminar un usuario"* sonaba a inventario.

En la pantalla, el botón dice **"Pedir eliminación"** y el diálogo dice *"Un administrador revisa la
solicitud y decide si se elimina o no. Te llega una notificación con la respuesta."*

Alicia rechazó la primera versión, que empezaba con *"Esto no elimina nada todavía"*: **no se explica
por lo que NO pasa, se explica por lo que sí va a pasar** — quién lo revisa, qué decide y cómo se
entera quien lo pidió. El mismo texto quedó en `DeleteAction` (clientes, proveedores, proyectos),
que decía lo mismo con otras palabras.

El **motivo es obligatorio** en los dos diálogos: es lo único que quien revisa tiene para decidir.

Y la notificación de vuelta ("aprobaron / rechazaron tu solicitud") ahora dice primero el resultado y
después el comentario de quien decidió. Antes, si esa persona escribía algo, la notificación era solo
ese texto suelto y quien la recibía no sabía si le habían dicho que sí o que no.

---

## 5. Archivos tocados

**Base de datos**

- `docs/schema/25_gestion_usuarios_al_dia.sql` *(nuevo — esto es lo que se corre en Supabase)*
- `src/Nexit.Infrastructure/Migrations/20260908222937_AddEliminacionUsuarioPorSolicitud.cs` *(nuevo — solo para la base local)*
- `src/Nexit.Infrastructure/Data/NexitDbContext.cs`

**Backend**

- `src/Nexit.Core/Constants/TiposEntidadEliminable.cs` *(nuevo)*
- `src/Nexit.Core/Entities/SolicitudEliminacion.cs`, `HistorialCambio.cs`, `InvitacionEquipo.cs`
- `src/Nexit.Application/UseCases/SolicitudesEliminacion/SolicitudEliminacionUseCases.cs`
- `src/Nexit.Application/UseCases/Notificaciones/NotificacionUseCases.cs`
- `src/Nexit.Application/UseCases/Usuarios/UsuarioUseCases.cs` *(se fue `EliminarUsuarioUseCase`)*
- `src/Nexit.Application/Validators/SolicitudesEliminacion/CrearSolicitudEliminacionValidator.cs`
- `src/Nexit.API/Controllers/UsuariosController.cs` *(se fue el `[HttpDelete]`)*

**Frontend**

- `src/app/(dashboard)/usuarios/page.tsx`, `UsuarioDetail.tsx`
- `src/services/api/usuarios-service.ts` *(se fue `remove`)*
- `src/types/api.ts`
- `src/components/ui/form.tsx` *(`Field` acepta `icon`)*

---

## 6. Lo que queda pendiente

- El **cuarto recuadro** de los KPI sigue sin decidir.
- Los cambios sobre un usuario **no quedan en `historial_cambios`** (solo los de cliente/proveedor/
  proyecto). El CHECK `ck_historial_cambios_tipo_entidad` tampoco acepta `'usuario'` todavía.
- No se puede **reenviar** una invitación, y las invitaciones **no vencen**.
- Las carpetas `_to_delete/` de los dos repos hay que borrarlas a mano (el puente no puede borrar
  archivos).
