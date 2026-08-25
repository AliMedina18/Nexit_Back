-- ============================================================
-- Fix: el Auth Hook (03_auth_hook_custom_claims.sql) no podía leer
-- el rol de nadie porque RLS bloqueaba a supabase_auth_admin
-- ============================================================
-- Ver docs/27-fix-rls-auth-hook-produccion.md para el detalle completo.
--
-- El GRANT SELECT que ya le daba 03_auth_hook_custom_claims.sql al rol
-- supabase_auth_admin no basta por sí solo: usuarios tiene Row Level
-- Security activado (04_extras_supabase_post_migraciones.sql), y la
-- única política que existía (solo_nexit_app) solo cubre al rol de la
-- aplicación. Sin una política propia, la consulta interna del hook
-- siempre devolvía cero filas y todo el mundo quedaba con el rol por
-- defecto ("miembro"), sin importar su rol real -- encontrado y
-- corregido en producción el 2026-08-25, antes de invitar a nadie.
--
-- Sí se ejecuta, una vez, sin orden fijo (después de 03 y 04).

CREATE POLICY "auth_hook_lee_rol" ON usuarios
  FOR SELECT TO supabase_auth_admin
  USING (true);

-- Verificación: pide un token nuevo para una cuenta con rol conocido
-- (supabase.auth.signInWithPassword o el flujo de OTP), decodifícalo en
-- jwt.io, y confirma que "user_role" trae el rol real de esa cuenta, no
-- "miembro" por defecto -- salvo que su rol real SÍ sea miembro.
