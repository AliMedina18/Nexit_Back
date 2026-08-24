-- ============================================================
-- Invitaciones de equipo (invitar y registrar en un solo paso)
-- ============================================================
-- Ver docs/25-invitar-y-registrar-en-un-solo-paso.md para el diseño completo.
--
-- Por qué este archivo está escrito a mano en vez de correr `01_esquema_completo.sql`
-- completo contra Supabase (mismo motivo que ya explican los archivos 06 y 07): la
-- migración de EF Core que agrega esta tabla (`AddInvitacionesEquipo`) no está registrada
-- en el `__EFMigrationsHistory` de Supabase porque las tablas anteriores (`usuarios_eliminados`,
-- `notificaciones`, etc.) se aplicaron a mano ahí, no con `dotnet ef database update` -- así
-- que el script completo volvería a intentar crear cosas que ya existen y fallaría. Este
-- archivo saca SOLO la tabla nueva (columnas, constraints e índices calcados exactamente de
-- lo que EF generó) y la envuelve en `IF NOT EXISTS`.
--
-- Cuándo correr esto: en cualquier momento después de 01_esquema_completo.sql -- no tiene
-- dependencia de orden con los demás archivos de esta carpeta.
--
-- Para tu base local `nexit_dev`: NO hace falta correr este archivo -- ahí usa
-- `dotnet ef database update --project src/Nexit.Infrastructure --startup-project src/Nexit.API`.

CREATE TABLE IF NOT EXISTS invitaciones_equipo (
    id               uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    email            character varying(255) NOT NULL,
    rol              character varying(20) NOT NULL,
    mensaje          character varying(500) NULL,
    estado           character varying(20) NOT NULL,
    invitado_por_id  uuid NOT NULL,
    fecha_respuesta  timestamptz NULL,
    created_at       timestamptz NOT NULL DEFAULT now(),
    updated_at       timestamptz NULL,
    created_by       uuid NULL,
    updated_by       uuid NULL,
    CONSTRAINT ck_invitaciones_equipo_estado CHECK (estado IN ('Pendiente', 'Aceptada', 'Rechazada')),
    CONSTRAINT fk_invitaciones_equipo_usuarios_invitado_por_id FOREIGN KEY (invitado_por_id) REFERENCES usuarios (id) ON DELETE RESTRICT
);
CREATE INDEX IF NOT EXISTS ix_invitaciones_equipo_email_estado ON invitaciones_equipo (email, estado);
CREATE INDEX IF NOT EXISTS ix_invitaciones_equipo_invitado_por_id ON invitaciones_equipo (invitado_por_id);

-- RLS -- mismo criterio que el resto del esquema (04_extras_supabase_post_migraciones.sql,
-- sección 2; y el archivo 07): solo el rol de aplicación nexit_app pasa, PostgREST
-- (anon/authenticated) queda bloqueado por defecto al no tener ninguna política.
ALTER TABLE invitaciones_equipo ENABLE ROW LEVEL SECURITY;
DO $$ BEGIN
    CREATE POLICY "solo_nexit_app" ON invitaciones_equipo FOR ALL TO nexit_app USING (true) WITH CHECK (true);
EXCEPTION WHEN duplicate_object THEN NULL; END $$;

-- Verificación:
--   SELECT tablename, rowsecurity FROM pg_tables WHERE tablename = 'invitaciones_equipo';  -- debe quedar en 't'
--   SELECT count(*) FROM invitaciones_equipo;  -- debe correr sin error (0 filas al principio)
