-- ============================================================================
-- 21_cliente_proveedor_emails.sql
--
-- "Pues hay que tener en cuenta que [el cliente] puede tener varios correos"
-- (2026-09-06) -- Cliente.Email y Proveedor.Email dejan de ser una sola columna
-- y pasan a ser una lista simple, sin "principal", exactamente como ya
-- funciona Telefono hoy (ver cliente_telefonos / proveedor_telefonos). Ver
-- docs/34-limpieza-carga-datos-historicos.md para el resto del contexto.
--
-- Qué hace, en orden:
--   1. Crea `cliente_emails` / `proveedor_emails` (mismos nombres, tipos y
--      constraints que si hubiera salido de `dotnet ef migrations add`).
--   2. Copia hacia ellas cualquier correo que ya exista en `clientes.email` /
--      `proveedores.email`.
--   3. Borra la columna vieja y su índice.
--
-- Asimetría a propósito entre las dos tablas nuevas (igual que ya pasa con las
-- columnas viejas): `cliente_emails.email` SÍ tiene índice único (clientes.email
-- ya lo tenía), `proveedor_emails.email` NO (proveedores.email tampoco lo
-- tenía -- de hecho ya hay un caso real de dos proveedores con el mismo correo
-- en producción, "orquestaperezprado@gmail.com", que este script no toca).
--
-- Idempotente -- se puede correr más de una vez sin duplicar nada ni fallar si
-- algo ya existe o ya se corrió antes (una vez que `clientes.email` ya no
-- exista, el segundo INSERT simplemente no encuentra nada que copiar).
-- ============================================================================

BEGIN;

-- --- Clientes ---------------------------------------------------------------

CREATE TABLE IF NOT EXISTS cliente_emails (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    cliente_id uuid NOT NULL,
    email character varying(255) NOT NULL,
    etiqueta text
);

DO $$ BEGIN
    ALTER TABLE cliente_emails ADD CONSTRAINT fk_cliente_emails_clientes_cliente_id
        FOREIGN KEY (cliente_id) REFERENCES clientes (id) ON DELETE CASCADE;
EXCEPTION WHEN duplicate_object THEN NULL; END $$;

CREATE UNIQUE INDEX IF NOT EXISTS ix_cliente_emails_email ON cliente_emails (email);
CREATE INDEX IF NOT EXISTS ix_cliente_emails_cliente_id ON cliente_emails (cliente_id);

-- Copia los correos existentes. ON CONFLICT DO NOTHING es solo defensivo: como
-- clientes.email YA es único hoy, no debería haber nada que choque.
DO $$ BEGIN
    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'clientes' AND column_name = 'email') THEN
        INSERT INTO cliente_emails (cliente_id, email)
        SELECT id, btrim(email) FROM clientes WHERE email IS NOT NULL AND btrim(email) <> ''
        ON CONFLICT (email) DO NOTHING;
    END IF;
END $$;

DROP INDEX IF EXISTS ix_clientes_email;
ALTER TABLE clientes DROP COLUMN IF EXISTS email;

-- --- Proveedores --------------------------------------------------------------

CREATE TABLE IF NOT EXISTS proveedor_emails (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    proveedor_id uuid NOT NULL,
    email character varying(255) NOT NULL,
    etiqueta text
);

DO $$ BEGIN
    ALTER TABLE proveedor_emails ADD CONSTRAINT fk_proveedor_emails_proveedores_proveedor_id
        FOREIGN KEY (proveedor_id) REFERENCES proveedores (id) ON DELETE CASCADE;
EXCEPTION WHEN duplicate_object THEN NULL; END $$;

CREATE INDEX IF NOT EXISTS ix_proveedor_emails_proveedor_id ON proveedor_emails (proveedor_id);
-- Sin índice único a propósito -- ver comentario arriba.

DO $$ BEGIN
    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'proveedores' AND column_name = 'email') THEN
        INSERT INTO proveedor_emails (proveedor_id, email)
        SELECT id, btrim(email) FROM proveedores WHERE email IS NOT NULL AND btrim(email) <> '';
    END IF;
END $$;

ALTER TABLE proveedores DROP COLUMN IF EXISTS email;

COMMIT;

-- --- Verificación -------------------------------------------------------------
SELECT 'clientes' AS tabla, count(*) AS filas_con_email_migradas FROM cliente_emails
UNION ALL
SELECT 'proveedores', count(*) FROM proveedor_emails;
