-- ============================================================================
-- 22_rls_cliente_proveedor_emails.sql
--
-- Arregla un bug real de docs/schema/21_cliente_proveedor_emails.sql: esa
-- migración creó las tablas `cliente_emails` y `proveedor_emails` pero se le
-- olvidó activarles Row Level Security y agregarles la política de
-- "solo_nexit_app" que sí tienen TODAS las demás tablas de negocio (incluidas
-- cliente_telefonos y proveedor_telefonos, que son justo las que estas dos
-- tablas nuevas están imitando -- ver docs/schema/04_extras_supabase_post_migraciones.sql).
--
-- Efecto del bug: el rol de la aplicación (`nexit_app`, ver
-- docs/schema/02_rol_aplicacion_minimo_privilegio.sql) tiene el GRANT de tabla
-- para insertar en cliente_emails/proveedor_emails, pero como Postgres activa
-- RLS con cero políticas, igual bloquea todo. Esto se veía en el backend como:
--
--   Npgsql.PostgresException 42501: new row violates row-level security
--   policy for table "cliente_emails"
--
-- y en el frontend como el mensaje genérico "La operación no pudo completarse
-- por una restricción de datos." al crear/importar un cliente o proveedor con
-- al menos un correo.
--
-- Reproducido y verificado en una base de prueba antes de entregar esto:
--   1. Con RLS activado y sin política (como quedó por el bug de schema/21),
--      un INSERT como nexit_app falla con el mismo error 42501 de arriba.
--   2. Con esta política agregada, el mismo INSERT funciona.
--
-- Idempotente -- se puede correr más de una vez sin fallar (usa
-- IF NOT EXISTS / verifica antes de crear la política).
-- ============================================================================

BEGIN;

ALTER TABLE cliente_emails ENABLE ROW LEVEL SECURITY;
ALTER TABLE proveedor_emails ENABLE ROW LEVEL SECURITY;

DO $$ BEGIN
    CREATE POLICY "solo_nexit_app" ON cliente_emails
        FOR ALL TO nexit_app USING (true) WITH CHECK (true);
EXCEPTION WHEN duplicate_object THEN NULL; END $$;

DO $$ BEGIN
    CREATE POLICY "solo_nexit_app" ON proveedor_emails
        FOR ALL TO nexit_app USING (true) WITH CHECK (true);
EXCEPTION WHEN duplicate_object THEN NULL; END $$;

COMMIT;

-- --- Verificación -------------------------------------------------------------
-- Debe mostrar 't' (true) en rowsecurity, y una fila de política por tabla.
SELECT relname AS tabla, relrowsecurity AS rls_activo
FROM pg_class
WHERE relname IN ('cliente_emails', 'proveedor_emails');

SELECT tablename, policyname, roles
FROM pg_policies
WHERE tablename IN ('cliente_emails', 'proveedor_emails');
