-- ============================================================================
-- 24_borrar_solo_proyectos.sql
--
-- Borra ÚNICAMENTE los proyectos (y lo que cuelga de ellos: proyecto_equipo,
-- proyecto_proveedores, proyecto_seguimiento, por FK ON DELETE CASCADE) --
-- NO toca clientes ni proveedores. Es para recargar solo Proyectos desde cero
-- (después del intento que dejó 31 proyectos a medias por los 527 que
-- fallaron con "el cliente no existe", ver docs/34) sin tener que volver a
-- borrar/recargar Clientes ni Proveedores.
--
-- Probado de verdad contra una base de prueba: se insertó un proyecto con una
-- fila en cada una de las 3 tablas que cuelgan de él, se corrió el script, y
-- las 4 tablas quedaron en 0 -- clientes y proveedores no se tocaron.
-- Se puede correr más de una vez sin problema (si ya está vacío, no hace nada).
--
-- Es IRREVERSIBLE sobre la tabla `proyectos`.
-- ============================================================================

BEGIN;

DELETE FROM proyectos;

-- Limpieza de referencias sueltas de proyecto (no son llave foránea real, ver
-- el mismo comentario en schema/19).
DELETE FROM solicitudes_eliminacion WHERE tipo_entidad = 'proyecto';
DELETE FROM notificaciones WHERE tipo_entidad = 'proyecto';

COMMIT;

-- Verificación rápida después de correr esto.
SELECT 'proyectos' AS tabla, count(*) FROM proyectos
UNION ALL SELECT 'proyecto_equipo', count(*) FROM proyecto_equipo
UNION ALL SELECT 'proyecto_proveedores', count(*) FROM proyecto_proveedores
UNION ALL SELECT 'proyecto_seguimiento', count(*) FROM proyecto_seguimiento
UNION ALL SELECT 'clientes (no debería cambiar)', count(*) FROM clientes
UNION ALL SELECT 'proveedores (no debería cambiar)', count(*) FROM proveedores;
