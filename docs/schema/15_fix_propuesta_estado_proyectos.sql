-- Corrige la restricción ck_proyectos_propuesta -- estaba desalineada tanto
-- del formulario real (ProjectFormModal.tsx / prototipo Nexit__Standalone.html,
-- que ofrecen 'No enviada' | 'Enviada' | 'Aprobada' | 'Rechazada') como de los
-- datos reales que ya existían en el prototipo (varios proyectos con
-- propuesta "Aprobada"). Con la restricción vieja
-- ('No enviada', 'En proceso', 'Enviada') no era posible guardar un proyecto
-- con propuesta aprobada o rechazada -- 'En proceso' tampoco se usa en
-- ningún lado del sistema.
--
-- Corresponde a la migración de EF Core FixProyectoPropuestaEstadoValues.
-- Idempotente: se puede correr más de una vez sin error.

DO $$
BEGIN
    ALTER TABLE proyectos DROP CONSTRAINT IF EXISTS ck_proyectos_propuesta;
    ALTER TABLE proyectos ADD CONSTRAINT ck_proyectos_propuesta
        CHECK (propuesta_estado IN ('No enviada', 'Enviada', 'Aprobada', 'Rechazada'));
END $$;

INSERT INTO "__EFMigrationsHistory" (migration_id, product_version)
VALUES ('20260903200926_FixProyectoPropuestaEstadoValues', '8.0.11')
ON CONFLICT (migration_id) DO NOTHING;
