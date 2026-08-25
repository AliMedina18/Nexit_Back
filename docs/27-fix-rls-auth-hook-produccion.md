# Bug real encontrado y corregido: el Auth Hook no podía leer el rol de nadie en producción

## Qué pasó

Al activar el Auth Hook en el proyecto real de Supabase (`docs/09`, sección 4) y probarlo pidiendo un token real para la cuenta de super_admin, el claim `user_role` sí aparecía en el JWT — pero decía `"miembro"` en vez de `"super_admin"`, aunque la fila en `usuarios` tenía el rol correcto. Es decir: **el Auth Hook estaba activo pero no podía leer el rol de nadie**, y le asignaba a todo el mundo el valor por defecto ("miembro") sin importar su rol real en la base.

## Causa

La tabla `usuarios` tiene Row Level Security activado (`docs/schema/04_extras_supabase_post_migraciones.sql`), y la única política que existe (`solo_nexit_app`) le da acceso exclusivamente al rol `nexit_app` — el rol de la aplicación. El Auth Hook, en cambio, se ejecuta como el rol interno de Supabase `supabase_auth_admin`. El script `03_auth_hook_custom_claims.sql` ya le había dado ese rol un `GRANT SELECT` a nivel de tabla, pero **un GRANT de tabla no basta cuando RLS está activo** — sin una política que explícitamente permita a `supabase_auth_admin`, su `SELECT` dentro de la función siempre devolvía cero filas, y la función caía al valor por defecto.

Este es un caso conocido documentado por el propio Supabase: cualquier tabla que un Auth Hook consulte, si tiene RLS activo, necesita su propia política para `supabase_auth_admin` — el GRANT de tabla y la política de RLS son dos capas independientes.

## Impacto real que tenía este bug

Mientras no se corrigiera, **ningún endpoint protegido por rol habría funcionado para nadie en producción** — ni siquiera para la super administradora real. Cualquier política de autorización (`SuperAdminOnly`, `AdminOrAbove`, etc.) habría rechazado a todo el mundo, porque el JWT de cualquiera, sin importar su rol real, siempre traía `user_role: miembro`. Se encontró antes de invitar a nadie más y antes de desplegar el backend, así que no llegó a afectar a ningún usuario real.

## Corrección

Una política adicional, de solo lectura, para `supabase_auth_admin`:

```sql
CREATE POLICY "auth_hook_lee_rol" ON usuarios
  FOR SELECT TO supabase_auth_admin
  USING (true);
```

Ejecutada directamente en el SQL Editor de Supabase el 2026-08-25.

## Verificación

Se pidió un token nuevo para la cuenta de super_admin (`analistacompras@agencianextmkt.com`) después de aplicar la política, y se decodificó: `user_role` ya muestra `"super_admin"` correctamente. Confirmado en la misma sesión que se encontró el bug.

## Dónde queda esto en el esquema de referencia

Falta agregar esta política a `docs/schema/04_extras_supabase_post_migraciones.sql` (o a un script nuevo, `09_fix_rls_auth_hook.sql`) para que una futura reconstrucción del proyecto desde cero no repita este mismo bug — pendiente de agregar al repo.
