-- Corresponde a la migración de EF Core "AddClienteUbicacionCatalogoYEstado" (2026-09-03).
-- Cierra la brecha frente al mockup aprobado (Claude Diseño): el formulario de cliente ahí
-- pide país/departamento/ciudad en cascada (mismas tablas de catálogo que ya usa Proveedor:
-- paises/regiones/ciudades) y un "Estado" (Activo/Prospecto/Inactivo) -- ninguno de los dos
-- existía en `clientes`, que solo tenía la columna `ciudad` de texto libre.
--
-- Las 3 columnas de catálogo son NULLABLE (a diferencia de Proveedor, donde pais_id es
-- obligatorio): los clientes que ya existen antes de este cambio no tienen forma confiable de
-- inferir su país desde el texto libre de `ciudad`, así que quedan sin catálogo hasta que
-- alguien los edite desde el formulario nuevo. La columna `ciudad` de texto libre NO se toca --
-- el frontend prioriza el nombre resuelto de `ciudad_id` cuando existe, y usa `ciudad` como
-- respaldo para todo lo creado antes de hoy.
--
-- Se ejecuta, una vez, sin orden fijo -- no depende de ningún otro script de esta carpeta
-- (sí depende, como toda esta carpeta, de que paises/regiones/ciudades ya existan --
-- ver seed_geografia_categorias_estados.sql).
--
-- Para tu base local `nexit_dev`: NO hace falta correr este archivo -- ahí usa
-- `dotnet ef database update --project src/Nexit.Infrastructure --startup-project src/Nexit.API`,
-- que aplica esto (y el resto de migraciones pendientes) de una sola vez a través del
-- mecanismo normal de EF Core.
--
-- Columnas -- con IF NOT EXISTS, mismo patrón que ya usan schema/06, /07, /10, /11 y /12, para
-- que correr esto dos veces por error no falle con "column already exists".
ALTER TABLE clientes ADD COLUMN IF NOT EXISTS pais_id uuid;
ALTER TABLE clientes ADD COLUMN IF NOT EXISTS region_id uuid;
ALTER TABLE clientes ADD COLUMN IF NOT EXISTS ciudad_id uuid;
ALTER TABLE clientes ADD COLUMN IF NOT EXISTS estado text NOT NULL DEFAULT 'Activo';

-- Índices -- CREATE INDEX sí soporta IF NOT EXISTS de forma nativa en Postgres.
CREATE INDEX IF NOT EXISTS ix_clientes_pais_id ON clientes (pais_id);
CREATE INDEX IF NOT EXISTS ix_clientes_region_id ON clientes (region_id);
CREATE INDEX IF NOT EXISTS ix_clientes_ciudad_id ON clientes (ciudad_id);
CREATE INDEX IF NOT EXISTS ix_clientes_estado ON clientes (estado);

-- Constraint y llaves foráneas -- Postgres no tiene "ADD CONSTRAINT IF NOT EXISTS", así que se
-- envuelven en un bloque que ignora el error si ya existen (para poder correr este archivo más
-- de una vez sin que falle, igual que las columnas de arriba).
DO $$ BEGIN
    ALTER TABLE clientes ADD CONSTRAINT ck_clientes_estado CHECK (estado IN ('Activo', 'Prospecto', 'Inactivo'));
EXCEPTION WHEN duplicate_object THEN NULL; END $$;

DO $$ BEGIN
    ALTER TABLE clientes ADD CONSTRAINT fk_clientes_paises_pais_id FOREIGN KEY (pais_id) REFERENCES paises (id) ON DELETE RESTRICT;
EXCEPTION WHEN duplicate_object THEN NULL; END $$;

DO $$ BEGIN
    ALTER TABLE clientes ADD CONSTRAINT fk_clientes_regiones_region_id FOREIGN KEY (region_id) REFERENCES regiones (id) ON DELETE RESTRICT;
EXCEPTION WHEN duplicate_object THEN NULL; END $$;

DO $$ BEGIN
    ALTER TABLE clientes ADD CONSTRAINT fk_clientes_ciudades_ciudad_id FOREIGN KEY (ciudad_id) REFERENCES ciudades (id) ON DELETE RESTRICT;
EXCEPTION WHEN duplicate_object THEN NULL; END $$;
