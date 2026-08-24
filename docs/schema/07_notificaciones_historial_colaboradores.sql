-- ============================================================
-- Notificaciones, historial de cambios y "trabajando con este proveedor"
-- ============================================================
-- Ver docs/20-notificaciones-historial-y-colaboradores.md para el diseño completo (y
-- docs/19-diseno-notificaciones-historial-y-mis-proveedores.md para la propuesta original).
-- Resumen de las 3 tablas nuevas:
--   1) notificaciones          -- bandeja por usuario del flujo de solicitudes de eliminación.
--                                  Nunca se borra: "leída" es un estado, no una eliminación.
--   2) historial_cambios       -- quién cambió qué campo de un proyecto/proveedor/cliente y
--                                  cuándo (tipo Google Docs/Excel). Una fila por campo editado.
--   3) proveedor_colaboradores -- "estoy trabajando con este proveedor": cada quien se marca a
--                                  sí mismo, muchos-a-muchos, visible para todos (alimenta la
--                                  vista personal "mis proveedores").
--
-- Por qué este archivo está escrito a mano en vez de generarse con
-- `dotnet ef migrations script` (a diferencia de lo que dice docs/09 y el propio
-- docs/schema/01_esquema_completo.sql): la migración de EF Core que agrega estas 3 tablas
-- (`AddNotificacionesHistorialColaboradores`) quedó "empaquetada" junto con el cambio de
-- `usuarios_eliminados`/`fecha_desactivacion` de HU-07, que en Supabase YA se aplicó a mano
-- con `06_eliminacion_automatica_usuarios.sql` (nunca se registró en `__EFMigrationsHistory`
-- porque no se aplicó con `dotnet ef database update`). Si corrieras el script idempotente
-- completo de esa migración contra Supabase, fallaría al intentar recrear
-- `usuarios_eliminados`. Así que este archivo saca SOLO las 3 tablas nuevas de esa migración
-- (columnas, constraints e índices calcados exactamente de lo que EF generó, para que el
-- modelo de C# y la base real no se desalineen) y las envuelve en `IF NOT EXISTS`, igual que
-- ya se hizo en el archivo 06 para el mismo tipo de situación.
--
-- Cuándo correr esto: en cualquier momento después de 01_esquema_completo.sql -- no tiene
-- dependencia de orden con los demás archivos de esta carpeta.
--
-- Para tu base local `nexit_dev`: NO hace falta correr este archivo -- ahí usa
-- `dotnet ef database update --project src/Nexit.Infrastructure --startup-project src/Nexit.API`,
-- que aplica esto (y el resto de migraciones pendientes) de una sola vez a través del
-- mecanismo normal de EF Core.

-- 1) notificaciones
CREATE TABLE IF NOT EXISTS notificaciones (
    id                        uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    usuario_destinatario_id   uuid NOT NULL,
    tipo                      character varying(50) NOT NULL,
    titulo                    character varying(255) NOT NULL,
    mensaje                   text NOT NULL,
    tipo_entidad              character varying(20) NULL,
    entidad_id                uuid NULL,
    solicitud_id              uuid NULL,
    leida                     boolean NOT NULL DEFAULT false,
    fecha_creacion            timestamptz NOT NULL DEFAULT now(),
    fecha_leida               timestamptz NULL,
    CONSTRAINT ck_notificaciones_tipo CHECK (tipo IN ('solicitud_eliminacion_creada', 'solicitud_eliminacion_endosada', 'solicitud_eliminacion_decidida')),
    CONSTRAINT fk_notificaciones_usuarios_usuario_destinatario_id FOREIGN KEY (usuario_destinatario_id) REFERENCES usuarios (id) ON DELETE CASCADE
);
CREATE INDEX IF NOT EXISTS ix_notificaciones_usuario_destinatario_id_leida ON notificaciones (usuario_destinatario_id, leida);

-- 2) historial_cambios
CREATE TABLE IF NOT EXISTS historial_cambios (
    id              uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tipo_entidad    character varying(20) NOT NULL,
    entidad_id      uuid NOT NULL,
    usuario_id      uuid NOT NULL,
    accion          character varying(20) NOT NULL,
    campo           character varying(100) NULL,
    valor_anterior  text NULL,
    valor_nuevo     text NULL,
    fecha           timestamptz NOT NULL DEFAULT now(),
    CONSTRAINT ck_historial_cambios_tipo_entidad CHECK (tipo_entidad IN ('proyecto', 'proveedor', 'cliente')),
    CONSTRAINT fk_historial_cambios_usuarios_usuario_id FOREIGN KEY (usuario_id) REFERENCES usuarios (id) ON DELETE RESTRICT
);
CREATE INDEX IF NOT EXISTS ix_historial_cambios_tipo_entidad_entidad_id_fecha ON historial_cambios (tipo_entidad, entidad_id, fecha);
CREATE INDEX IF NOT EXISTS ix_historial_cambios_usuario_id ON historial_cambios (usuario_id);

-- 3) proveedor_colaboradores
CREATE TABLE IF NOT EXISTS proveedor_colaboradores (
    proveedor_id    uuid NOT NULL,
    usuario_id      uuid NOT NULL,
    fecha_agregado  timestamptz NOT NULL DEFAULT now(),
    CONSTRAINT pk_proveedor_colaboradores PRIMARY KEY (proveedor_id, usuario_id),
    CONSTRAINT fk_proveedor_colaboradores_proveedores_proveedor_id FOREIGN KEY (proveedor_id) REFERENCES proveedores (id) ON DELETE CASCADE,
    CONSTRAINT fk_proveedor_colaboradores_usuarios_usuario_id FOREIGN KEY (usuario_id) REFERENCES usuarios (id) ON DELETE CASCADE
);
CREATE INDEX IF NOT EXISTS ix_proveedor_colaboradores_usuario_id ON proveedor_colaboradores (usuario_id);

-- 4) RLS -- mismo criterio que el resto del esquema (04_extras_supabase_post_migraciones.sql,
--    sección 2): solo el rol de aplicación nexit_app pasa, PostgREST (anon/authenticated)
--    queda bloqueado por defecto al no tener ninguna política.
ALTER TABLE notificaciones ENABLE ROW LEVEL SECURITY;
ALTER TABLE historial_cambios ENABLE ROW LEVEL SECURITY;
ALTER TABLE proveedor_colaboradores ENABLE ROW LEVEL SECURITY;

DO $$ BEGIN
    CREATE POLICY "solo_nexit_app" ON notificaciones FOR ALL TO nexit_app USING (true) WITH CHECK (true);
EXCEPTION WHEN duplicate_object THEN NULL; END $$;
DO $$ BEGIN
    CREATE POLICY "solo_nexit_app" ON historial_cambios FOR ALL TO nexit_app USING (true) WITH CHECK (true);
EXCEPTION WHEN duplicate_object THEN NULL; END $$;
DO $$ BEGIN
    CREATE POLICY "solo_nexit_app" ON proveedor_colaboradores FOR ALL TO nexit_app USING (true) WITH CHECK (true);
EXCEPTION WHEN duplicate_object THEN NULL; END $$;

-- Verificación:
--   SELECT tablename, rowsecurity FROM pg_tables WHERE tablename IN ('notificaciones', 'historial_cambios', 'proveedor_colaboradores');  -- las 3 deben quedar en 't'
--   SELECT count(*) FROM notificaciones;  -- debe correr sin error (0 filas al principio)
