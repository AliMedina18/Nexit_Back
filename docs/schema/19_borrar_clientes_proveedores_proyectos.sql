-- ============================================================================
-- 19_borrar_clientes_proveedores_proyectos.sql
--
-- Borra TODOS los clientes, proveedores y proyectos actuales (y todo lo que
-- cuelga de ellos: teléfonos, equipo de proyecto, seguimiento, relación
-- proyecto-proveedor, servicios/adjuntos de proveedor), para dejar la base
-- limpia y recargar desde cero con los datos reales ya depurados
-- (docs/datos/Clientes_importar.xlsx, Proveedores_importar.xlsx,
-- Proyectos_importar.xlsx + docs/schema/18_proveedores_reales.sql).
--
-- Decisión de Alicia (2026-09-05): no hace falta respaldo antes de borrar
-- porque el sistema todavía no lo usan usuarios finales.
--
-- NO TOCA: catálogos (países, regiones, ciudades, categorías de proveedor,
-- fases/estados de proyecto, etapas de cliente, usuarios, servicios,
-- dominios de correo permitidos) -- nada de eso se borra, solo los datos
-- de clientes/proveedores/proyectos.
--
-- Es IRREVERSIBLE. Probado de verdad contra una base de prueba con datos
-- de ejemplo (cliente + proyecto + proveedor + sus filas hijas en las 6
-- tablas relacionadas) antes de entregarse -- ver docs/34 para el detalle
-- de la prueba. Verificado: no hay ningún FK con "Restrict" en esta
-- dirección, así que el orden de abajo (proyectos -> proveedores ->
-- clientes) no puede fallar por restricción de llave foránea; se elige
-- ese orden solo para que ningún proyecto quede transitoriamente con
-- cliente_id en null antes de borrarlo también.
--
-- Se puede correr más de una vez sin problema (si ya está vacío, no hace
-- nada).
-- ============================================================================

BEGIN;

-- 1) Proyectos: se lleva en cascada proyecto_equipo, proyecto_proveedores
--    (la relación con el proveedor, no el proveedor en sí) y
--    proyecto_seguimiento.
DELETE FROM proyectos;

-- 2) Proveedores: se lleva en cascada proveedor_telefonos,
--    proveedor_servicios, proveedor_adjuntos y proveedor_colaboradores.
DELETE FROM proveedores;

-- 3) Clientes: se lleva en cascada cliente_telefonos.
DELETE FROM clientes;

-- 4) Limpieza de referencias sueltas: estas 2 tablas guardan el tipo de
--    entidad + su id como texto/uuid (no son llave foránea real, por eso
--    no se borran solas al borrar arriba). En un sistema sin usuarios
--    finales todavía deberían estar vacías, pero se limpian por si acaso
--    quedó algo de pruebas.
DELETE FROM solicitudes_eliminacion WHERE tipo_entidad IN ('cliente', 'proveedor', 'proyecto');
DELETE FROM notificaciones WHERE tipo_entidad IN ('cliente', 'proveedor', 'proyecto');

COMMIT;

-- Verificación rápida después de correr esto: las 5 tablas deben quedar en 0.
SELECT 'clientes' AS tabla, count(*) FROM clientes
UNION ALL SELECT 'proveedores', count(*) FROM proveedores
UNION ALL SELECT 'proyectos', count(*) FROM proyectos
UNION ALL SELECT 'proyecto_equipo', count(*) FROM proyecto_equipo
UNION ALL SELECT 'proyecto_proveedores', count(*) FROM proyecto_proveedores
UNION ALL SELECT 'proyecto_seguimiento', count(*) FROM proyecto_seguimiento
UNION ALL SELECT 'proveedor_telefonos', count(*) FROM proveedor_telefonos
UNION ALL SELECT 'cliente_telefonos', count(*) FROM cliente_telefonos;
