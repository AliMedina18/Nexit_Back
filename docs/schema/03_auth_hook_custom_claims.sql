-- ============================================================
-- Auth Hook de Supabase: agregar el rol de usuarios.rol al JWT
-- ============================================================
-- Por qué existe este archivo (hallazgo H3 de la auditoría de seguridad, 2026-08-17):
-- la política "AdminOnly" del backend (Nexit.API/Program.cs) exige que el JWT tenga un
-- claim `user_role=admin` (o `app_role=admin`, o el rol JWT estándar `admin`). Sin esta
-- función, Supabase Auth nunca agrega ese claim al token — el JWT solo trae lo básico
-- (sub, email, aud, exp, etc.), nada sobre la tabla `usuarios`. Sin este hook, la
-- política "AdminOnly" no la puede cumplir NADIE, ni siquiera un admin real: los
-- endpoints de eliminar y de administrar catálogos quedarían inutilizables para todos.
--
-- Esta función usa el mecanismo "Custom Access Token Hook" de Supabase Auth, disponible
-- en el plan gratuito (Auth Hooks: Custom Access Token (JWT) — confirmado en la página
-- de precios de Supabase). Se ejecuta en Postgres, sin necesidad de una Edge Function.
--
-- Pasos para activarlo (una vez que exista el proyecto de Supabase):
-- 1) Ejecutar este script completo contra la base de datos del proyecto.
-- 2) En el dashboard de Supabase: Authentication -> Hooks -> "Customize Access Token
--    (JWT) Claims hook" -> elegir "Postgres" -> seleccionar la función
--    public.custom_access_token_hook -> guardar.
-- 3) Los tokens NUEVOS que se emitan después de esto ya traen el claim `user_role`. Los
--    tokens ya emitidos antes de activar el hook no lo tienen hasta que el usuario
--    vuelva a iniciar sesión (o se le refresque el token).

CREATE OR REPLACE FUNCTION public.custom_access_token_hook(event jsonb)
RETURNS jsonb
LANGUAGE plpgsql
STABLE
AS $$
DECLARE
  claims jsonb;
  rol_usuario text;
BEGIN
  SELECT rol INTO rol_usuario FROM public.usuarios WHERE id = (event->>'user_id')::uuid;

  claims := COALESCE(event->'claims', '{}'::jsonb);

  IF rol_usuario IS NOT NULL THEN
    claims := jsonb_set(claims, '{user_role}', to_jsonb(rol_usuario));
  ELSE
    -- Si el usuario de auth.users todavía no tiene fila en public.usuarios (ej. está a
    -- mitad del flujo de registro), se le asigna el rol menos privilegiado por defecto
    -- en vez de dejarlo sin claim (que además rompería políticas que sí esperan el claim).
    claims := jsonb_set(claims, '{user_role}', '"miembro"');
  END IF;

  event := jsonb_set(event, '{claims}', claims);
  RETURN event;
END;
$$;

-- Solo el propio sistema de Auth de Supabase (supabase_auth_admin) puede ejecutar esta
-- función; ni los usuarios autenticados ni el rol anónimo deben poder invocarla directo.
GRANT EXECUTE ON FUNCTION public.custom_access_token_hook TO supabase_auth_admin;
REVOKE EXECUTE ON FUNCTION public.custom_access_token_hook FROM authenticated, anon, public;

-- La función necesita poder leer public.usuarios aunque se ejecute en el contexto de
-- supabase_auth_admin (que no tiene GRANT sobre las tablas de la aplicación).
GRANT USAGE ON SCHEMA public TO supabase_auth_admin;
GRANT SELECT ON public.usuarios TO supabase_auth_admin;

-- Verificación (después de activar el hook en el dashboard): inicia sesión con un
-- usuario que ya tenga fila en public.usuarios, copia el JWT (access_token) y decodifícalo
-- en jwt.io — el payload debe traer "user_role": "super_admin" / "admin" / "manager" /
-- "miembro" según corresponda (modelo de 4 niveles, ver docs/06-modelo-permisos-roles.md).
-- Si un admin real no ve el claim, revisa que el hook esté "Enabled" en el dashboard y
-- que el usuario haya iniciado sesión DESPUÉS de activarlo.
