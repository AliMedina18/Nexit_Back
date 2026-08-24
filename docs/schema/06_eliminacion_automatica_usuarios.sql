-- ============================================================
-- Eliminación automática de usuarios inactivos (30 días) + respaldo
-- ============================================================
-- Ver docs/17-eliminacion-automatica-usuarios.md para el diseño completo. Resumen: al
-- desactivar una cuenta (usuarios.activo = false) empieza a correr un plazo de 30 días
-- (configurable); si nadie la reactiva antes, un proceso del backend la elimina sola,
-- pero primero guarda una copia en usuarios_eliminados (nunca se borra sin dejar rastro).
--
-- Cuándo correr esto: en cualquier momento después de 01_esquema_completo.sql -- no tiene
-- dependencia de orden con los demás archivos de esta carpeta (a diferencia de 02/04/03,
-- ver la nota de 04_extras_supabase_post_migraciones.sql).

-- 1) Columna nueva en usuarios: cuándo se desactivó (null mientras está activa). La usa
--    el backend para calcular cuándo se cumplen los 30 días -- ver
--    EliminarUsuariosInactivosUseCase en el código.
ALTER TABLE usuarios ADD COLUMN IF NOT EXISTS fecha_desactivacion timestamptz NULL;

-- 2) Tabla de respaldo. No la consulta la aplicación normal -- es solo un archivo de
--    auditoría/recuperación por si hace falta reconstruir quién era una cuenta ya
--    eliminada. eliminado_por_id queda NULL cuando la eliminación fue automática (a
--    diferencia de una eliminación manual con DELETE /api/usuarios/{id}).
CREATE TABLE IF NOT EXISTS usuarios_eliminados (
  id                  uuid PRIMARY KEY DEFAULT gen_random_uuid(),
  usuario_id_original uuid NOT NULL,
  nombre              varchar(255) NOT NULL,
  apellido            varchar(255) NOT NULL,
  email               varchar(255) NOT NULL,
  rol                 varchar(20) NOT NULL,
  iniciales           varchar(10) NULL,
  fecha_alta_original timestamptz NOT NULL,
  fecha_desactivacion timestamptz NULL,
  fecha_eliminacion   timestamptz NOT NULL DEFAULT now(),
  eliminado_por_id    uuid NULL
);
CREATE INDEX IF NOT EXISTS ix_usuarios_eliminados_usuario_id_original ON usuarios_eliminados(usuario_id_original);

-- 3) RLS -- mismo criterio que el resto del esquema (04_extras_supabase_post_migraciones.sql,
--    sección 2): solo el rol de aplicación nexit_app pasa, PostgREST (anon/authenticated)
--    queda bloqueado por defecto al no tener ninguna política.
ALTER TABLE usuarios_eliminados ENABLE ROW LEVEL SECURITY;
CREATE POLICY "solo_nexit_app" ON usuarios_eliminados FOR ALL TO nexit_app USING (true) WITH CHECK (true);

-- Verificación:
--   SELECT column_name FROM information_schema.columns WHERE table_name = 'usuarios' AND column_name = 'fecha_desactivacion';
--   SELECT tablename, rowsecurity FROM pg_tables WHERE tablename = 'usuarios_eliminados';  -- debe ser 't'
