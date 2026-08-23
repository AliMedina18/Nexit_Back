-- ============================================================
-- Corrige el aviso de seguridad "Function Search Path Mutable"
-- (Supabase Security Advisor, detectado 2026-08-23)
-- ============================================================
-- Qué señala este aviso: ninguna de las 5 funciones de abajo tenía un search_path
-- fijo, así que Postgres resolvía los nombres de tabla sin calificar (regiones,
-- ciudades, dominios_correo_permitidos, estados_proyecto, usuarios) usando el
-- search_path que estuviera activo en el momento de ejecutarse -- en teoría, alguien
-- con permiso de crear objetos podría crear una tabla/función con el mismo nombre en
-- otro esquema antes en la ruta de búsqueda y "secuestrar" la función. Fijar el
-- search_path (pg_catalog, public -- solo esquemas de confianza, en ese orden) cierra
-- ese hueco sin tener que reescribir ninguna función ni calificar cada referencia.
--
-- Se puede correr las veces que haga falta (ALTER FUNCTION es idempotente) y no
-- afecta triggers, permisos (GRANT/REVOKE) ni el Auth Hook ya activado -- solo fija
-- esta propiedad de cada función.

ALTER FUNCTION public.set_updated_at() SET search_path = pg_catalog, public;
ALTER FUNCTION public.check_proveedor_geografia() SET search_path = pg_catalog, public;
ALTER FUNCTION public.check_usuario_dominio_correo() SET search_path = pg_catalog, public;
ALTER FUNCTION public.set_estado_proyecto_default() SET search_path = pg_catalog, public;
ALTER FUNCTION public.custom_access_token_hook(jsonb) SET search_path = pg_catalog, public;

-- Verificación: en el SQL Editor, corre esto y confirma que las 5 filas muestran
-- "search_path=pg_catalog, public" en la columna proconfig:
--   SELECT proname, proconfig FROM pg_proc
--   WHERE pronamespace = 'public'::regnamespace
--     AND proname IN ('set_updated_at', 'check_proveedor_geografia',
--                      'check_usuario_dominio_correo', 'set_estado_proyecto_default',
--                      'custom_access_token_hook');
-- El aviso debería desaparecer del Security Advisor la próxima vez que lo abras
-- (a veces tarda un momento en refrescar el resultado del lint).
