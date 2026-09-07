-- ============================================================================
-- 23_borrar_solo_clientes.sql
--
-- Borra ÚNICAMENTE los clientes (y lo que cuelga de ellos: cliente_telefonos,
-- cliente_emails, por FK ON DELETE CASCADE) -- NO toca proveedores ni
-- proyectos. A diferencia de docs/schema/19 (que borra clientes + proveedores
-- + proyectos juntos), este script es a propósito más angosto: es para
-- recargar solo Clientes desde cero (después de que una segunda corrida del
-- import dejó clientes duplicados, ver docs/34) sin arriesgar los 138
-- proveedores reales ya cargados por schema/18.
--
-- Por qué es seguro sin tocar Proyectos: `proyectos.cliente_id` tiene
-- DeleteBehavior.SetNull (ver NexitDbContext.cs línea ~110) -- si algún
-- proyecto ya apuntara a un cliente borrado, Postgres simplemente le pone
-- cliente_id en null en vez de fallar o borrar el proyecto. Hoy no debería
-- haber ningún proyecto todavía (no se ha importado Proyectos_importar.xlsx),
-- así que en la práctica esto no debería tocar ninguna fila de `proyectos`.
--
-- Probado de verdad contra una base de prueba: se insertó un cliente con
-- teléfono, correo y un proyecto apuntándole; se corrió este script; el
-- cliente y su teléfono/correo desaparecieron, el proyecto siguió existiendo
-- con cliente_id en null (no se borró), y la tabla `proveedores` no se tocó.
-- Se puede correr más de una vez sin problema (si ya está vacío, no hace nada).
--
-- Es IRREVERSIBLE sobre la tabla `clientes`.
-- ============================================================================

BEGIN;

DELETE FROM clientes;

-- Limpieza de referencias sueltas de cliente (no son llave foránea real, ver
-- el mismo comentario en schema/19).
DELETE FROM solicitudes_eliminacion WHERE tipo_entidad = 'cliente';
DELETE FROM notificaciones WHERE tipo_entidad = 'cliente';

COMMIT;

-- Verificación rápida después de correr esto.
SELECT 'clientes' AS tabla, count(*) FROM clientes
UNION ALL SELECT 'cliente_telefonos', count(*) FROM cliente_telefonos
UNION ALL SELECT 'cliente_emails', count(*) FROM cliente_emails
UNION ALL SELECT 'proveedores (no debería cambiar)', count(*) FROM proveedores;
