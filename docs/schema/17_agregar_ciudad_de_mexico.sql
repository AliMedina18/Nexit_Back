-- ============================================================================
-- 17_agregar_ciudad_de_mexico.sql
--
-- PROPUESTO, NO APLICADO todavía -- ver docs/34-limpieza-carga-datos-historicos.md.
--
-- El catálogo de ciudades sembrado (docs/schema/seed_geografia_categorias_estados.sql)
-- lista, bajo el estado "Ciudad de México", solo sus alcaldías (Benito Juárez,
-- Miguel Hidalgo, Coyoacán, Cuauhtémoc, Álvaro Obregón, Tlalpan, Iztapalapa) --
-- nunca "Ciudad de México" como ciudad en sí. 86 de los 145 proveedores del
-- Excel real de la compañera tienen "CDMX" como ciudad, y Proveedor.Ciudad se
-- resuelve por nombre EXACTO contra este catálogo (docs/31) -- sin esta fila,
-- esos 86 proveedores quedan con Ciudad vacía al importar (no se rechaza el
-- proveedor completo por esto, solo se pierde el dato de ciudad).
--
-- Es seguro correr esto más de una vez: si la fila ya existe, no hace nada.
--
-- Para tu base local `nexit_dev`: esto es un dato de catálogo, no una
-- migración de EF Core -- se corre directo con psql/tu cliente de Postgres
-- de siempre, igual que seed_geografia_categorias_estados.sql.
-- ============================================================================
INSERT INTO ciudades (region_id, nombre)
SELECT r.id, 'Ciudad de México'
FROM regiones r
JOIN paises p ON p.id = r.pais_id
WHERE p.nombre = 'México' AND r.nombre = 'Ciudad de México'
  AND NOT EXISTS (
    SELECT 1 FROM ciudades c WHERE c.region_id = r.id AND c.nombre = 'Ciudad de México'
  );
