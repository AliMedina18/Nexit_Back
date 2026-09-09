-- ============================================================
-- Gestión de usuarios al día (2026-09-08)
-- ============================================================
-- Corre esto UNA VEZ en el SQL Editor de Supabase. Es idempotente: si lo corres dos veces no pasa
-- nada, y si una parte ya estaba aplicada la salta sola. No borra datos.
--
-- Qué arregla, en orden de urgencia:
--
--   1. `relation "invitaciones_equipo" does not exist` (error 42P01). La tabla se creó en el repo el
--      2026-08-24 (migración AddInvitacionesEquipo + docs/schema/08_invitaciones_equipo.sql), pero
--      el archivo 08 nunca se llegó a correr contra esta base. Por eso GET /api/invitaciones devuelve
--      500 desde entonces. La sección 1 la crea.
--
--   2. `ck_notificaciones_tipo` solo aceptaba los tres tipos de solicitudes de eliminación. Desde que
--      aceptar o rechazar una invitación genera una notificación (`invitacion_aceptada` /
--      `invitacion_rechazada`), la base la rechazaba al guardar. La sección 2 amplía el CHECK.
--
--   3. `ck_solicitudes_eliminacion_tipo` solo aceptaba cliente/proveedor/proyecto. Eliminar a una
--      persona ahora pasa por el mismo circuito de solicitud + decisión (docs/40), así que hace falta
--      `'usuario'`. La sección 3 lo agrega.
--
--   4. Tres llaves foráneas hacia `usuarios` estaban en ON DELETE RESTRICT: `historial_cambios`,
--      `invitaciones_equipo` y `solicitudes_eliminacion`. Con eso, eliminar a alguien que alguna vez
--      editó un cliente, invitó a un compañero o pidió una eliminación fallaba con violación de llave
--      foránea -- es decir, cualquiera con uso real del sistema era ineliminable, y la limpieza
--      automática de los 30 días (docs/17) también se atascaba en silencio. La sección 4 las pasa a
--      ON DELETE SET NULL: las filas de historial/invitaciones/solicitudes se conservan, y quién era
--      esa persona queda en el respaldo de `usuarios_eliminados`.
--
-- Equivalente en el repo: migración 20260908222937_AddEliminacionUsuarioPorSolicitud. NO corras esa
-- migración contra Supabase -- arrastra también las tablas de los scripts 20/21/22, que acá ya
-- existen, y fallaría. Para la base local `nexit_dev` sí:
--   dotnet ef database update --project src/Nexit.Infrastructure --startup-project src/Nexit.API


-- ------------------------------------------------------------
-- 0. Diagnóstico -- opcional, corre solo esto primero si quieres ver qué falta antes de tocar nada.
-- ------------------------------------------------------------
-- SELECT 'invitaciones_equipo'  AS objeto, to_regclass('public.invitaciones_equipo')  IS NOT NULL AS existe
-- UNION ALL SELECT 'notificaciones',        to_regclass('public.notificaciones')        IS NOT NULL
-- UNION ALL SELECT 'historial_cambios',     to_regclass('public.historial_cambios')     IS NOT NULL
-- UNION ALL SELECT 'usuarios_eliminados',   to_regclass('public.usuarios_eliminados')   IS NOT NULL
-- UNION ALL SELECT 'presencia_usuarios',    to_regclass('public.presencia_usuarios')    IS NOT NULL
-- UNION ALL SELECT 'solicitudes_eliminacion', to_regclass('public.solicitudes_eliminacion') IS NOT NULL;


-- ------------------------------------------------------------
-- 1. invitaciones_equipo (era docs/schema/08, que nunca se corrió acá)
-- ------------------------------------------------------------
CREATE TABLE IF NOT EXISTS invitaciones_equipo (
    id               uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    email            character varying(255) NOT NULL,
    rol              character varying(20) NOT NULL,
    mensaje          character varying(500) NULL,
    estado           character varying(20) NOT NULL,
    invitado_por_id  uuid NULL,
    fecha_respuesta  timestamptz NULL,
    created_at       timestamptz NOT NULL DEFAULT now(),
    updated_at       timestamptz NULL,
    created_by       uuid NULL,
    updated_by       uuid NULL,
    CONSTRAINT ck_invitaciones_equipo_estado CHECK (estado IN ('Pendiente', 'Aceptada', 'Rechazada')),
    CONSTRAINT fk_invitaciones_equipo_usuarios_invitado_por_id FOREIGN KEY (invitado_por_id) REFERENCES usuarios (id) ON DELETE SET NULL
);
CREATE INDEX IF NOT EXISTS ix_invitaciones_equipo_email_estado ON invitaciones_equipo (email, estado);
CREATE INDEX IF NOT EXISTS ix_invitaciones_equipo_invitado_por_id ON invitaciones_equipo (invitado_por_id);

-- RLS -- mismo criterio que el resto del esquema (04, sección 2; y el archivo 07): solo el rol de
-- aplicación nexit_app pasa; PostgREST (anon/authenticated) queda bloqueado al no tener política.
ALTER TABLE invitaciones_equipo ENABLE ROW LEVEL SECURITY;
DO $$ BEGIN
    CREATE POLICY "solo_nexit_app" ON invitaciones_equipo FOR ALL TO nexit_app USING (true) WITH CHECK (true);
EXCEPTION WHEN duplicate_object THEN NULL; END $$;


-- ------------------------------------------------------------
-- 2. notificaciones: aceptar los dos tipos de invitación
-- ------------------------------------------------------------
ALTER TABLE notificaciones DROP CONSTRAINT IF EXISTS ck_notificaciones_tipo;
ALTER TABLE notificaciones ADD CONSTRAINT ck_notificaciones_tipo CHECK (tipo IN (
    'solicitud_eliminacion_creada',
    'solicitud_eliminacion_endosada',
    'solicitud_eliminacion_decidida',
    'invitacion_aceptada',
    'invitacion_rechazada'
));


-- ------------------------------------------------------------
-- 3. solicitudes_eliminacion: aceptar tipo_entidad = 'usuario'
-- ------------------------------------------------------------
ALTER TABLE solicitudes_eliminacion DROP CONSTRAINT IF EXISTS ck_solicitudes_eliminacion_tipo;
ALTER TABLE solicitudes_eliminacion ADD CONSTRAINT ck_solicitudes_eliminacion_tipo
    CHECK (tipo_entidad IN ('cliente', 'proveedor', 'proyecto', 'usuario'));


-- ------------------------------------------------------------
-- 4. RESTRICT -> SET NULL en las tres llaves foráneas que bloqueaban eliminar personas
-- ------------------------------------------------------------
-- historial_cambios: la fila de auditoría no se borra nunca, pero tampoco puede impedir que se
-- elimine una cuenta. Queda con usuario_id NULL y la aplicación la muestra como "Usuario eliminado".
ALTER TABLE historial_cambios ALTER COLUMN usuario_id DROP NOT NULL;
ALTER TABLE historial_cambios DROP CONSTRAINT IF EXISTS fk_historial_cambios_usuarios_usuario_id;
ALTER TABLE historial_cambios ADD CONSTRAINT fk_historial_cambios_usuarios_usuario_id
    FOREIGN KEY (usuario_id) REFERENCES usuarios (id) ON DELETE SET NULL;

-- invitaciones_equipo: quien invitó puede irse del equipo; sus invitaciones se conservan sin dueño.
ALTER TABLE invitaciones_equipo ALTER COLUMN invitado_por_id DROP NOT NULL;
ALTER TABLE invitaciones_equipo DROP CONSTRAINT IF EXISTS fk_invitaciones_equipo_usuarios_invitado_por_id;
ALTER TABLE invitaciones_equipo ADD CONSTRAINT fk_invitaciones_equipo_usuarios_invitado_por_id
    FOREIGN KEY (invitado_por_id) REFERENCES usuarios (id) ON DELETE SET NULL;

-- solicitudes_eliminacion: igual -- la solicitud sobrevive a quien la pidió.
ALTER TABLE solicitudes_eliminacion ALTER COLUMN solicitado_por_id DROP NOT NULL;
ALTER TABLE solicitudes_eliminacion DROP CONSTRAINT IF EXISTS fk_solicitudes_eliminacion_usuarios_solicitado_por_id;
ALTER TABLE solicitudes_eliminacion ADD CONSTRAINT fk_solicitudes_eliminacion_usuarios_solicitado_por_id
    FOREIGN KEY (solicitado_por_id) REFERENCES usuarios (id) ON DELETE SET NULL;


-- ------------------------------------------------------------
-- Verificación -- las cuatro deben responder lo que dice el comentario.
-- ------------------------------------------------------------
-- SELECT count(*) FROM invitaciones_equipo;                                   -- corre sin error (0 al principio)
-- SELECT rowsecurity FROM pg_tables WHERE tablename = 'invitaciones_equipo';  -- t
-- SELECT pg_get_constraintdef(oid) FROM pg_constraint WHERE conname = 'ck_solicitudes_eliminacion_tipo';
--   -- debe incluir 'usuario'
-- SELECT conname, confdeltype FROM pg_constraint
--  WHERE conname IN ('fk_historial_cambios_usuarios_usuario_id',
--                    'fk_invitaciones_equipo_usuarios_invitado_por_id',
--                    'fk_solicitudes_eliminacion_usuarios_solicitado_por_id');
--   -- confdeltype debe ser 'n' (SET NULL) en las tres, no 'r' (RESTRICT)
