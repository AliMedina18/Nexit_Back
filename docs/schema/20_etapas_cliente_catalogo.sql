-- ============================================================================
-- 20_etapas_cliente_catalogo.sql
--
-- Aplica lo que quedó pendiente del documento 33 (catálogo de etapas de
-- cliente E1-E6) y que NUNCA se generó como migración real de EF Core --
-- ver "Hallazgo aparte" en docs/34-limpieza-carga-datos-historicos.md. El
-- código C# (Cliente.EtapaId, CatalogosController.GetEtapasCliente) ya
-- asume que existen la tabla `etapas_cliente` y la columna
-- `clientes.etapa_id`, pero en producción nunca se crearon -- por eso
-- fallan con "column c.etapa_id does not exist" / "relation etapas_cliente
-- does not exist" (2026-09-05, al importar/listar clientes).
--
-- Crea exactamente lo que `NexitDbContext.cs` espera (mismos nombres,
-- tipos y constraints que si hubiera salido de `dotnet ef migrations add
-- AddEtapaClienteCatalogo`), y siembra las 6 etapas reales de
-- docs/33-etapas-cliente-catalogo.md (idéntico al bloque ya existente en
-- docs/schema/seed_geografia_categorias_estados.sql, que nunca se pudo
-- correr en producción porque la tabla no existía).
--
-- Idempotente -- se puede correr más de una vez sin duplicar nada ni
-- fallar si algo ya existe parcialmente.
-- ============================================================================

CREATE TABLE IF NOT EXISTS etapas_cliente (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    nombre character varying(255) NOT NULL,
    orden smallint NOT NULL,
    porcentaje_proceso smallint NOT NULL DEFAULT 0
);

DO $$ BEGIN
    ALTER TABLE etapas_cliente ADD CONSTRAINT ck_etapas_cliente_porcentaje CHECK (porcentaje_proceso BETWEEN 0 AND 100);
EXCEPTION WHEN duplicate_object THEN NULL; END $$;

CREATE UNIQUE INDEX IF NOT EXISTS ix_etapas_cliente_nombre ON etapas_cliente (nombre);
CREATE UNIQUE INDEX IF NOT EXISTS ix_etapas_cliente_orden ON etapas_cliente (orden);

ALTER TABLE clientes ADD COLUMN IF NOT EXISTS etapa_id uuid;
CREATE INDEX IF NOT EXISTS ix_clientes_etapa_id ON clientes (etapa_id);

DO $$ BEGIN
    ALTER TABLE clientes ADD CONSTRAINT fk_clientes_etapas_cliente_etapa_id FOREIGN KEY (etapa_id) REFERENCES etapas_cliente (id) ON DELETE RESTRICT;
EXCEPTION WHEN duplicate_object THEN NULL; END $$;

-- Semilla real (docs/33) -- idéntico al bloque de seed_geografia_categorias_estados.sql.
-- ON CONFLICT DO NOTHING es seguro aquí porque, a diferencia de clientes/proveedores,
-- "nombre" y "orden" sí tienen restricción UNIQUE real.
INSERT INTO etapas_cliente (nombre, orden, porcentaje_proceso) VALUES
    ('Contacto inicial', 1, 10),
    ('Reconocimiento', 2, 30),
    ('Oportunidad de negocio', 3, 60),
    ('Cierre y pre-producción', 4, 80),
    ('Finalización', 5, 90),
    ('Facturación', 6, 100)
ON CONFLICT (nombre) DO NOTHING;
