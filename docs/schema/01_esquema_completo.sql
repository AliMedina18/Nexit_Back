-- Esquema completo de la base de datos (tablas, columnas, llaves, índices) -- generado
-- automáticamente desde las migraciones de EF Core, no escrito a mano, para que nunca
-- quede desactualizado en silencio. Si agregas una migración nueva, regenera este
-- archivo así (desde la raíz del repo):
--   dotnet ef migrations script --project src\Nexit.Infrastructure --startup-project src\Nexit.API --idempotent --output docs\schema\01_esquema_completo.sql
-- Es idempotente: se puede correr sobre una base vacía o una que ya tenga algunas
-- migraciones aplicadas, sin duplicar nada.

CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    migration_id character varying(150) NOT NULL,
    product_version character varying(32) NOT NULL,
    CONSTRAINT pk___ef_migrations_history PRIMARY KEY (migration_id)
);

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260817212311_InitialCreate') THEN
    CREATE TABLE categorias_proveedor (
        id uuid NOT NULL,
        nombre character varying(255) NOT NULL,
        CONSTRAINT pk_categorias_proveedor PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260817212311_InitialCreate') THEN
    CREATE TABLE dominios_correo_permitidos (
        id uuid NOT NULL,
        dominio character varying(255) NOT NULL,
        CONSTRAINT pk_dominios_correo_permitidos PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260817212311_InitialCreate') THEN
    CREATE TABLE fases_proyecto (
        fase smallint NOT NULL,
        nombre character varying(255) NOT NULL,
        CONSTRAINT pk_fases_proyecto PRIMARY KEY (fase)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260817212311_InitialCreate') THEN
    CREATE TABLE paises (
        id uuid NOT NULL,
        nombre character varying(255) NOT NULL,
        etiqueta_region character varying(100) NOT NULL,
        CONSTRAINT pk_paises PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260817212311_InitialCreate') THEN
    CREATE TABLE servicios (
        id uuid NOT NULL,
        nombre text NOT NULL,
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone,
        created_by uuid,
        CONSTRAINT pk_servicios PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260817212311_InitialCreate') THEN
    CREATE TABLE usuarios (
        id uuid NOT NULL,
        nombre text NOT NULL,
        apellido text NOT NULL,
        email character varying(255) NOT NULL,
        rol text NOT NULL,
        iniciales text,
        activo boolean NOT NULL,
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone,
        CONSTRAINT pk_usuarios PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260817212311_InitialCreate') THEN
    CREATE TABLE estados_proyecto (
        id uuid NOT NULL,
        nombre character varying(255) NOT NULL,
        fase smallint NOT NULL,
        orden smallint NOT NULL,
        CONSTRAINT pk_estados_proyecto PRIMARY KEY (id),
        CONSTRAINT fk_estados_proyecto_fases_proyecto_fase FOREIGN KEY (fase) REFERENCES fases_proyecto (fase) ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260817212311_InitialCreate') THEN
    CREATE TABLE regiones (
        id uuid NOT NULL,
        pais_id uuid NOT NULL,
        nombre character varying(255) NOT NULL,
        CONSTRAINT pk_regiones PRIMARY KEY (id),
        CONSTRAINT fk_regiones_paises_pais_id FOREIGN KEY (pais_id) REFERENCES paises (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260817212311_InitialCreate') THEN
    CREATE TABLE clientes (
        id uuid NOT NULL,
        nombre character varying(255) NOT NULL,
        sector text,
        ciudad text,
        direccion text,
        web text,
        contacto text,
        cargo_contacto text,
        email text,
        valor_referencia text,
        notas text,
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone,
        created_by uuid,
        CONSTRAINT pk_clientes PRIMARY KEY (id),
        CONSTRAINT fk_clientes_usuarios_created_by FOREIGN KEY (created_by) REFERENCES usuarios (id) ON DELETE SET NULL
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260817212311_InitialCreate') THEN
    CREATE TABLE informes_snapshot (
        id uuid NOT NULL,
        tipo text NOT NULL,
        periodo_key text NOT NULL,
        total_proveedores integer NOT NULL,
        total_clientes integer NOT NULL,
        total_proyectos integer NOT NULL,
        proyectos_sin_proveedor integer NOT NULL,
        por_estado jsonb NOT NULL,
        por_brief jsonb NOT NULL,
        created_at timestamp with time zone NOT NULL,
        created_by uuid,
        CONSTRAINT pk_informes_snapshot PRIMARY KEY (id),
        CONSTRAINT fk_informes_snapshot_usuarios_created_by FOREIGN KEY (created_by) REFERENCES usuarios (id) ON DELETE SET NULL
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260817212311_InitialCreate') THEN
    CREATE TABLE ciudades (
        id uuid NOT NULL,
        region_id uuid NOT NULL,
        nombre character varying(255) NOT NULL,
        CONSTRAINT pk_ciudades PRIMARY KEY (id),
        CONSTRAINT fk_ciudades_regiones_region_id FOREIGN KEY (region_id) REFERENCES regiones (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260817212311_InitialCreate') THEN
    CREATE TABLE cliente_telefonos (
        id uuid NOT NULL,
        cliente_id uuid NOT NULL,
        telefono character varying(50) NOT NULL,
        etiqueta text,
        CONSTRAINT pk_cliente_telefonos PRIMARY KEY (id),
        CONSTRAINT fk_cliente_telefonos_clientes_cliente_id FOREIGN KEY (cliente_id) REFERENCES clientes (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260817212311_InitialCreate') THEN
    CREATE TABLE proyectos (
        id uuid NOT NULL,
        nombre character varying(255) NOT NULL,
        cliente_id uuid,
        contacto_proyecto text,
        tipo_proyecto text,
        prioridad text,
        ciudad text,
        sede_next text,
        fecha_solicitud timestamp with time zone,
        fecha_evento timestamp with time zone,
        estado_id uuid NOT NULL,
        porcentaje_avance integer NOT NULL,
        estado_brief text NOT NULL,
        propuesta_estado text NOT NULL,
        numero_factura text,
        pagado boolean NOT NULL,
        fecha_pago timestamp with time zone,
        notas text,
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone,
        created_by uuid,
        CONSTRAINT pk_proyectos PRIMARY KEY (id),
        CONSTRAINT fk_proyectos_clientes_cliente_id FOREIGN KEY (cliente_id) REFERENCES clientes (id) ON DELETE SET NULL,
        CONSTRAINT fk_proyectos_estados_proyecto_estado_id FOREIGN KEY (estado_id) REFERENCES estados_proyecto (id) ON DELETE RESTRICT,
        CONSTRAINT fk_proyectos_usuarios_created_by FOREIGN KEY (created_by) REFERENCES usuarios (id) ON DELETE SET NULL
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260817212311_InitialCreate') THEN
    CREATE TABLE proveedores (
        id uuid NOT NULL,
        nombre character varying(255) NOT NULL,
        pais_id uuid NOT NULL,
        region_id uuid,
        ciudad_id uuid,
        categoria_id uuid NOT NULL,
        estado text NOT NULL,
        contacto text,
        cargo_contacto text,
        email text,
        web text,
        direccion text,
        aforo integer,
        costo_referencia text,
        score integer,
        presupuesto text,
        cobertura text,
        notas text,
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone,
        created_by uuid,
        CONSTRAINT pk_proveedores PRIMARY KEY (id),
        CONSTRAINT fk_proveedores_categorias_proveedor_categoria_id FOREIGN KEY (categoria_id) REFERENCES categorias_proveedor (id) ON DELETE RESTRICT,
        CONSTRAINT fk_proveedores_ciudades_ciudad_id FOREIGN KEY (ciudad_id) REFERENCES ciudades (id) ON DELETE RESTRICT,
        CONSTRAINT fk_proveedores_paises_pais_id FOREIGN KEY (pais_id) REFERENCES paises (id) ON DELETE RESTRICT,
        CONSTRAINT fk_proveedores_regiones_region_id FOREIGN KEY (region_id) REFERENCES regiones (id) ON DELETE RESTRICT,
        CONSTRAINT fk_proveedores_usuarios_created_by FOREIGN KEY (created_by) REFERENCES usuarios (id) ON DELETE SET NULL
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260817212311_InitialCreate') THEN
    CREATE TABLE proyecto_equipo (
        id uuid NOT NULL,
        proyecto_id uuid NOT NULL,
        rol character varying(100) NOT NULL,
        nombre text NOT NULL,
        CONSTRAINT pk_proyecto_equipo PRIMARY KEY (id),
        CONSTRAINT fk_proyecto_equipo_proyectos_proyecto_id FOREIGN KEY (proyecto_id) REFERENCES proyectos (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260817212311_InitialCreate') THEN
    CREATE TABLE proyecto_seguimiento (
        id uuid NOT NULL,
        proyecto_id uuid NOT NULL,
        autor_id uuid,
        area text NOT NULL,
        fecha timestamp with time zone NOT NULL,
        nota text NOT NULL,
        created_at timestamp with time zone NOT NULL,
        CONSTRAINT pk_proyecto_seguimiento PRIMARY KEY (id),
        CONSTRAINT fk_proyecto_seguimiento_proyectos_proyecto_id FOREIGN KEY (proyecto_id) REFERENCES proyectos (id) ON DELETE CASCADE,
        CONSTRAINT fk_proyecto_seguimiento_usuarios_autor_id FOREIGN KEY (autor_id) REFERENCES usuarios (id) ON DELETE SET NULL
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260817212311_InitialCreate') THEN
    CREATE TABLE proveedor_adjuntos (
        id uuid NOT NULL,
        proveedor_id uuid NOT NULL,
        tipo character varying(10) NOT NULL,
        nombre character varying(255) NOT NULL,
        url text,
        storage_path text,
        meta text,
        fecha timestamp with time zone NOT NULL,
        created_at timestamp with time zone NOT NULL,
        CONSTRAINT pk_proveedor_adjuntos PRIMARY KEY (id),
        CONSTRAINT fk_proveedor_adjuntos_proveedores_proveedor_id FOREIGN KEY (proveedor_id) REFERENCES proveedores (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260817212311_InitialCreate') THEN
    CREATE TABLE proveedor_servicios (
        proveedor_id uuid NOT NULL,
        servicio_id uuid NOT NULL,
        CONSTRAINT pk_proveedor_servicios PRIMARY KEY (proveedor_id, servicio_id),
        CONSTRAINT fk_proveedor_servicios_proveedores_proveedor_id FOREIGN KEY (proveedor_id) REFERENCES proveedores (id) ON DELETE CASCADE,
        CONSTRAINT fk_proveedor_servicios_servicios_servicio_id FOREIGN KEY (servicio_id) REFERENCES servicios (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260817212311_InitialCreate') THEN
    CREATE TABLE proveedor_telefonos (
        id uuid NOT NULL,
        proveedor_id uuid NOT NULL,
        telefono character varying(50) NOT NULL,
        etiqueta text,
        CONSTRAINT pk_proveedor_telefonos PRIMARY KEY (id),
        CONSTRAINT fk_proveedor_telefonos_proveedores_proveedor_id FOREIGN KEY (proveedor_id) REFERENCES proveedores (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260817212311_InitialCreate') THEN
    CREATE TABLE proyecto_proveedores (
        proyecto_id uuid NOT NULL,
        proveedor_id uuid NOT NULL,
        CONSTRAINT pk_proyecto_proveedores PRIMARY KEY (proyecto_id, proveedor_id),
        CONSTRAINT fk_proyecto_proveedores_proveedores_proveedor_id FOREIGN KEY (proveedor_id) REFERENCES proveedores (id) ON DELETE CASCADE,
        CONSTRAINT fk_proyecto_proveedores_proyectos_proyecto_id FOREIGN KEY (proyecto_id) REFERENCES proyectos (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260817212311_InitialCreate') THEN
    CREATE UNIQUE INDEX ix_categorias_proveedor_nombre ON categorias_proveedor (nombre);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260817212311_InitialCreate') THEN
    CREATE UNIQUE INDEX ix_ciudades_region_id_nombre ON ciudades (region_id, nombre);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260817212311_InitialCreate') THEN
    CREATE INDEX ix_cliente_telefonos_cliente_id ON cliente_telefonos (cliente_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260817212311_InitialCreate') THEN
    CREATE INDEX ix_clientes_ciudad ON clientes (ciudad);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260817212311_InitialCreate') THEN
    CREATE INDEX ix_clientes_created_by ON clientes (created_by);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260817212311_InitialCreate') THEN
    CREATE UNIQUE INDEX ix_clientes_email ON clientes (email) WHERE email IS NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260817212311_InitialCreate') THEN
    CREATE INDEX ix_clientes_nombre ON clientes (nombre);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260817212311_InitialCreate') THEN
    CREATE UNIQUE INDEX ix_dominios_correo_permitidos_dominio ON dominios_correo_permitidos (dominio);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260817212311_InitialCreate') THEN
    CREATE INDEX ix_estados_proyecto_fase ON estados_proyecto (fase);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260817212311_InitialCreate') THEN
    CREATE UNIQUE INDEX ix_estados_proyecto_nombre ON estados_proyecto (nombre);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260817212311_InitialCreate') THEN
    CREATE UNIQUE INDEX ix_estados_proyecto_orden ON estados_proyecto (orden);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260817212311_InitialCreate') THEN
    CREATE UNIQUE INDEX ix_fases_proyecto_nombre ON fases_proyecto (nombre);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260817212311_InitialCreate') THEN
    CREATE INDEX ix_informes_snapshot_created_by ON informes_snapshot (created_by);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260817212311_InitialCreate') THEN
    CREATE UNIQUE INDEX ix_informes_snapshot_tipo_periodo_key ON informes_snapshot (tipo, periodo_key);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260817212311_InitialCreate') THEN
    CREATE UNIQUE INDEX ix_paises_nombre ON paises (nombre);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260817212311_InitialCreate') THEN
    CREATE INDEX ix_proveedor_adjuntos_proveedor_id ON proveedor_adjuntos (proveedor_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260817212311_InitialCreate') THEN
    CREATE INDEX ix_proveedor_servicios_servicio_id ON proveedor_servicios (servicio_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260817212311_InitialCreate') THEN
    CREATE INDEX ix_proveedor_telefonos_proveedor_id ON proveedor_telefonos (proveedor_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260817212311_InitialCreate') THEN
    CREATE INDEX ix_proveedores_categoria_id ON proveedores (categoria_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260817212311_InitialCreate') THEN
    CREATE INDEX ix_proveedores_ciudad_id ON proveedores (ciudad_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260817212311_InitialCreate') THEN
    CREATE INDEX ix_proveedores_created_by ON proveedores (created_by);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260817212311_InitialCreate') THEN
    CREATE INDEX ix_proveedores_estado ON proveedores (estado);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260817212311_InitialCreate') THEN
    CREATE INDEX ix_proveedores_pais_id ON proveedores (pais_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260817212311_InitialCreate') THEN
    CREATE INDEX ix_proveedores_region_id ON proveedores (region_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260817212311_InitialCreate') THEN
    CREATE INDEX ix_proyecto_equipo_proyecto_id ON proyecto_equipo (proyecto_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260817212311_InitialCreate') THEN
    CREATE INDEX ix_proyecto_proveedores_proveedor_id ON proyecto_proveedores (proveedor_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260817212311_InitialCreate') THEN
    CREATE INDEX ix_proyecto_seguimiento_autor_id ON proyecto_seguimiento (autor_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260817212311_InitialCreate') THEN
    CREATE INDEX ix_proyecto_seguimiento_proyecto_id ON proyecto_seguimiento (proyecto_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260817212311_InitialCreate') THEN
    CREATE INDEX ix_proyectos_cliente_id ON proyectos (cliente_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260817212311_InitialCreate') THEN
    CREATE INDEX ix_proyectos_created_by ON proyectos (created_by);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260817212311_InitialCreate') THEN
    CREATE INDEX ix_proyectos_estado_brief ON proyectos (estado_brief);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260817212311_InitialCreate') THEN
    CREATE INDEX ix_proyectos_estado_id ON proyectos (estado_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260817212311_InitialCreate') THEN
    CREATE INDEX ix_proyectos_fecha_evento ON proyectos (fecha_evento);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260817212311_InitialCreate') THEN
    CREATE INDEX ix_proyectos_prioridad ON proyectos (prioridad);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260817212311_InitialCreate') THEN
    CREATE UNIQUE INDEX ix_regiones_pais_id_nombre ON regiones (pais_id, nombre);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260817212311_InitialCreate') THEN
    CREATE UNIQUE INDEX ix_servicios_nombre ON servicios (nombre);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260817212311_InitialCreate') THEN
    CREATE UNIQUE INDEX ix_usuarios_email ON usuarios (email);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260817212311_InitialCreate') THEN
    INSERT INTO "__EFMigrationsHistory" (migration_id, product_version)
    VALUES ('20260817212311_InitialCreate', '8.0.11');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260817213731_AddUuidDefaults') THEN
    ALTER TABLE servicios ALTER COLUMN id SET DEFAULT (gen_random_uuid());
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260817213731_AddUuidDefaults') THEN
    ALTER TABLE regiones ALTER COLUMN id SET DEFAULT (gen_random_uuid());
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260817213731_AddUuidDefaults') THEN
    ALTER TABLE proyectos ALTER COLUMN id SET DEFAULT (gen_random_uuid());
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260817213731_AddUuidDefaults') THEN
    ALTER TABLE proyecto_seguimiento ALTER COLUMN id SET DEFAULT (gen_random_uuid());
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260817213731_AddUuidDefaults') THEN
    ALTER TABLE proyecto_equipo ALTER COLUMN id SET DEFAULT (gen_random_uuid());
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260817213731_AddUuidDefaults') THEN
    ALTER TABLE proveedores ALTER COLUMN id SET DEFAULT (gen_random_uuid());
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260817213731_AddUuidDefaults') THEN
    ALTER TABLE proveedor_telefonos ALTER COLUMN id SET DEFAULT (gen_random_uuid());
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260817213731_AddUuidDefaults') THEN
    ALTER TABLE proveedor_adjuntos ALTER COLUMN id SET DEFAULT (gen_random_uuid());
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260817213731_AddUuidDefaults') THEN
    ALTER TABLE paises ALTER COLUMN id SET DEFAULT (gen_random_uuid());
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260817213731_AddUuidDefaults') THEN
    ALTER TABLE informes_snapshot ALTER COLUMN id SET DEFAULT (gen_random_uuid());
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260817213731_AddUuidDefaults') THEN
    ALTER TABLE dominios_correo_permitidos ALTER COLUMN id SET DEFAULT (gen_random_uuid());
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260817213731_AddUuidDefaults') THEN
    ALTER TABLE clientes ALTER COLUMN id SET DEFAULT (gen_random_uuid());
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260817213731_AddUuidDefaults') THEN
    ALTER TABLE cliente_telefonos ALTER COLUMN id SET DEFAULT (gen_random_uuid());
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260817213731_AddUuidDefaults') THEN
    ALTER TABLE ciudades ALTER COLUMN id SET DEFAULT (gen_random_uuid());
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260817213731_AddUuidDefaults') THEN
    ALTER TABLE categorias_proveedor ALTER COLUMN id SET DEFAULT (gen_random_uuid());
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260817213731_AddUuidDefaults') THEN
    INSERT INTO "__EFMigrationsHistory" (migration_id, product_version)
    VALUES ('20260817213731_AddUuidDefaults', '8.0.11');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260817213841_AddEstadoProyectoUuidDefault') THEN
    ALTER TABLE estados_proyecto ALTER COLUMN id SET DEFAULT (gen_random_uuid());
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260817213841_AddEstadoProyectoUuidDefault') THEN
    INSERT INTO "__EFMigrationsHistory" (migration_id, product_version)
    VALUES ('20260817213841_AddEstadoProyectoUuidDefault', '8.0.11');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260817214414_AddLocalDatabaseRules') THEN
    ALTER TABLE usuarios ALTER COLUMN updated_at SET DEFAULT (now());
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260817214414_AddLocalDatabaseRules') THEN
    ALTER TABLE usuarios ALTER COLUMN rol SET DEFAULT 'miembro';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260817214414_AddLocalDatabaseRules') THEN
    ALTER TABLE usuarios ALTER COLUMN created_at SET DEFAULT (now());
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260817214414_AddLocalDatabaseRules') THEN
    ALTER TABLE usuarios ALTER COLUMN activo SET DEFAULT TRUE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260817214414_AddLocalDatabaseRules') THEN
    ALTER TABLE proyectos ALTER COLUMN updated_at SET DEFAULT (now());
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260817214414_AddLocalDatabaseRules') THEN
    ALTER TABLE proyectos ALTER COLUMN propuesta_estado SET DEFAULT 'No enviada';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260817214414_AddLocalDatabaseRules') THEN
    ALTER TABLE proyectos ALTER COLUMN porcentaje_avance SET DEFAULT 0;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260817214414_AddLocalDatabaseRules') THEN
    ALTER TABLE proyectos ALTER COLUMN pagado SET DEFAULT FALSE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260817214414_AddLocalDatabaseRules') THEN
    ALTER TABLE proyectos ALTER COLUMN estado_brief SET DEFAULT 'Pendiente por enviar';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260817214414_AddLocalDatabaseRules') THEN
    ALTER TABLE proyectos ALTER COLUMN created_at SET DEFAULT (now());
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260817214414_AddLocalDatabaseRules') THEN
    ALTER TABLE proyecto_seguimiento ALTER COLUMN fecha SET DEFAULT (CURRENT_DATE);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260817214414_AddLocalDatabaseRules') THEN
    ALTER TABLE proyecto_seguimiento ALTER COLUMN created_at SET DEFAULT (now());
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260817214414_AddLocalDatabaseRules') THEN
    ALTER TABLE proyecto_seguimiento ALTER COLUMN area SET DEFAULT 'General';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260817214414_AddLocalDatabaseRules') THEN
    ALTER TABLE proveedores ALTER COLUMN updated_at SET DEFAULT (now());
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260817214414_AddLocalDatabaseRules') THEN
    ALTER TABLE proveedores ALTER COLUMN estado SET DEFAULT 'Activo';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260817214414_AddLocalDatabaseRules') THEN
    ALTER TABLE proveedores ALTER COLUMN created_at SET DEFAULT (now());
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260817214414_AddLocalDatabaseRules') THEN
    ALTER TABLE proveedor_adjuntos ALTER COLUMN fecha SET DEFAULT (CURRENT_DATE);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260817214414_AddLocalDatabaseRules') THEN
    ALTER TABLE proveedor_adjuntos ALTER COLUMN created_at SET DEFAULT (now());
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260817214414_AddLocalDatabaseRules') THEN
    ALTER TABLE informes_snapshot ALTER COLUMN created_at SET DEFAULT (now());
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260817214414_AddLocalDatabaseRules') THEN
    ALTER TABLE usuarios ADD CONSTRAINT ck_usuarios_rol CHECK (rol IN ('admin', 'manager', 'miembro'));
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260817214414_AddLocalDatabaseRules') THEN
    ALTER TABLE proyectos ADD CONSTRAINT ck_proyectos_brief CHECK (estado_brief IN ('Pendiente por enviar', 'Entregado, a espera de respuesta', 'Requiere ajustes', 'Aprobado'));
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260817214414_AddLocalDatabaseRules') THEN
    ALTER TABLE proyectos ADD CONSTRAINT ck_proyectos_pago CHECK (NOT pagado OR fecha_pago IS NOT NULL);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260817214414_AddLocalDatabaseRules') THEN
    ALTER TABLE proyectos ADD CONSTRAINT ck_proyectos_porcentaje CHECK (porcentaje_avance BETWEEN 0 AND 100);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260817214414_AddLocalDatabaseRules') THEN
    ALTER TABLE proyectos ADD CONSTRAINT ck_proyectos_prioridad CHECK (prioridad IS NULL OR prioridad IN ('Alta', 'Media', 'Baja'));
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260817214414_AddLocalDatabaseRules') THEN
    ALTER TABLE proyectos ADD CONSTRAINT ck_proyectos_propuesta CHECK (propuesta_estado IN ('No enviada', 'En proceso', 'Enviada'));
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260817214414_AddLocalDatabaseRules') THEN
    ALTER TABLE proyectos ADD CONSTRAINT ck_proyectos_tipo CHECK (tipo_proyecto IS NULL OR tipo_proyecto IN ('Corporativo', 'Evento social'));
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260817214414_AddLocalDatabaseRules') THEN
    ALTER TABLE proyecto_seguimiento ADD CONSTRAINT ck_proyecto_seguimiento_area CHECK (area IN ('General', 'Creativo', 'Comercial', 'Administrativo'));
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260817214414_AddLocalDatabaseRules') THEN
    ALTER TABLE proyecto_equipo ADD CONSTRAINT ck_proyecto_equipo_rol CHECK (rol IN ('Ejecutivo', 'Comercial', 'Administrativo', 'Diseñador 3D', 'Diseñador gráfico'));
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260817214414_AddLocalDatabaseRules') THEN
    ALTER TABLE proveedores ADD CONSTRAINT ck_proveedores_cobertura CHECK (cobertura IS NULL OR cobertura IN ('Solo ciudad', 'Regional', 'Nacional', 'Internacional'));
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260817214414_AddLocalDatabaseRules') THEN
    ALTER TABLE proveedores ADD CONSTRAINT ck_proveedores_estado CHECK (estado IN ('Activo', 'En evaluación', 'Pausado', 'Bloqueado'));
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260817214414_AddLocalDatabaseRules') THEN
    ALTER TABLE proveedores ADD CONSTRAINT ck_proveedores_presupuesto CHECK (presupuesto IS NULL OR presupuesto IN ('$ Bajo (<20k)', '$$ Medio (20k–100k)', '$$$ Alto (100k–500k)', '$$$$ Premium (>500k)'));
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260817214414_AddLocalDatabaseRules') THEN
    ALTER TABLE proveedores ADD CONSTRAINT ck_proveedores_score CHECK (score IS NULL OR score BETWEEN 1 AND 5);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260817214414_AddLocalDatabaseRules') THEN
    ALTER TABLE proveedor_adjuntos ADD CONSTRAINT ck_proveedor_adjuntos_tipo CHECK (tipo IN ('link', 'file'));
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260817214414_AddLocalDatabaseRules') THEN
    ALTER TABLE informes_snapshot ADD CONSTRAINT ck_informes_snapshot_tipo CHECK (tipo IN ('semanal', 'mensual'));
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260817214414_AddLocalDatabaseRules') THEN
    CREATE OR REPLACE FUNCTION set_updated_at()
    RETURNS trigger AS $$
    BEGIN
      NEW.updated_at = now();
      RETURN NEW;
    END;
    $$ LANGUAGE plpgsql;

    CREATE OR REPLACE FUNCTION check_proveedor_geografia()
    RETURNS trigger AS $$
    DECLARE region_pais_id uuid; ciudad_region_id uuid;
    BEGIN
      IF NEW.region_id IS NOT NULL THEN
        SELECT pais_id INTO region_pais_id FROM regiones WHERE id = NEW.region_id;
        IF region_pais_id IS DISTINCT FROM NEW.pais_id THEN RAISE EXCEPTION 'La región no pertenece al país indicado.'; END IF;
      END IF;
      IF NEW.ciudad_id IS NOT NULL THEN
        SELECT region_id INTO ciudad_region_id FROM ciudades WHERE id = NEW.ciudad_id;
        IF NEW.region_id IS NULL OR ciudad_region_id IS DISTINCT FROM NEW.region_id THEN RAISE EXCEPTION 'La ciudad no pertenece a la región indicada.'; END IF;
      END IF;
      RETURN NEW;
    END;
    $$ LANGUAGE plpgsql;

    CREATE OR REPLACE FUNCTION check_usuario_dominio_correo()
    RETURNS trigger AS $$
    BEGIN
      IF NOT EXISTS (SELECT 1 FROM dominios_correo_permitidos d WHERE lower(NEW.email) LIKE '%@' || lower(d.dominio)) THEN
        RAISE EXCEPTION 'El correo no pertenece a un dominio laboral permitido.';
      END IF;
      RETURN NEW;
    END;
    $$ LANGUAGE plpgsql;

    CREATE OR REPLACE FUNCTION set_estado_proyecto_default()
    RETURNS trigger AS $$
    BEGIN
      IF NEW.estado_id IS NULL THEN
        SELECT id INTO NEW.estado_id FROM estados_proyecto WHERE nombre = 'Planeación interna';
      END IF;
      RETURN NEW;
    END;
    $$ LANGUAGE plpgsql;

    CREATE TRIGGER trg_clientes_updated_at BEFORE UPDATE ON clientes FOR EACH ROW EXECUTE FUNCTION set_updated_at();
    CREATE TRIGGER trg_proveedores_updated_at BEFORE UPDATE ON proveedores FOR EACH ROW EXECUTE FUNCTION set_updated_at();
    CREATE TRIGGER trg_proyectos_updated_at BEFORE UPDATE ON proyectos FOR EACH ROW EXECUTE FUNCTION set_updated_at();
    CREATE TRIGGER trg_usuarios_updated_at BEFORE UPDATE ON usuarios FOR EACH ROW EXECUTE FUNCTION set_updated_at();
    CREATE TRIGGER trg_proveedores_geografia BEFORE INSERT OR UPDATE ON proveedores FOR EACH ROW EXECUTE FUNCTION check_proveedor_geografia();
    CREATE TRIGGER trg_usuarios_dominio_correo BEFORE INSERT OR UPDATE OF email ON usuarios FOR EACH ROW EXECUTE FUNCTION check_usuario_dominio_correo();
    CREATE TRIGGER trg_proyectos_estado_default BEFORE INSERT ON proyectos FOR EACH ROW EXECUTE FUNCTION set_estado_proyecto_default();
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260817214414_AddLocalDatabaseRules') THEN
    INSERT INTO "__EFMigrationsHistory" (migration_id, product_version)
    VALUES ('20260817214414_AddLocalDatabaseRules', '8.0.11');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260817234434_AddConcurrencyAndAuditTracking') THEN
    ALTER TABLE proyectos ADD updated_by uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260817234434_AddConcurrencyAndAuditTracking') THEN
    ALTER TABLE proveedores ADD updated_by uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260817234434_AddConcurrencyAndAuditTracking') THEN
    ALTER TABLE clientes ADD updated_by uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260817234434_AddConcurrencyAndAuditTracking') THEN
    INSERT INTO "__EFMigrationsHistory" (migration_id, product_version)
    VALUES ('20260817234434_AddConcurrencyAndAuditTracking', '8.0.11');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260818015920_AddRbacFourTierRoles') THEN
    ALTER TABLE usuarios DROP CONSTRAINT ck_usuarios_rol;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260818015920_AddRbacFourTierRoles') THEN
    ALTER TABLE proyectos ADD gerente_id uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260818015920_AddRbacFourTierRoles') THEN
    CREATE TABLE solicitudes_eliminacion (
        id uuid NOT NULL DEFAULT (gen_random_uuid()),
        tipo_entidad character varying(20) NOT NULL,
        entidad_id uuid NOT NULL,
        solicitado_por_id uuid NOT NULL,
        motivo text,
        estado character varying(20) NOT NULL DEFAULT 'pendiente_admin',
        gerente_responsable_id uuid,
        aprobado_por_gerente_id uuid,
        aprobado_por_gerente_en timestamp with time zone,
        revisado_por_id uuid,
        revisado_en timestamp with time zone,
        comentario_revision text,
        created_at timestamp with time zone NOT NULL DEFAULT (now()),
        CONSTRAINT pk_solicitudes_eliminacion PRIMARY KEY (id),
        CONSTRAINT ck_solicitudes_eliminacion_estado CHECK (estado IN ('pendiente_gerente', 'pendiente_admin', 'aprobada', 'rechazada')),
        CONSTRAINT ck_solicitudes_eliminacion_tipo CHECK (tipo_entidad IN ('cliente', 'proveedor', 'proyecto')),
        CONSTRAINT fk_solicitudes_eliminacion_usuarios_aprobado_por_gerente_id FOREIGN KEY (aprobado_por_gerente_id) REFERENCES usuarios (id) ON DELETE SET NULL,
        CONSTRAINT fk_solicitudes_eliminacion_usuarios_gerente_responsable_id FOREIGN KEY (gerente_responsable_id) REFERENCES usuarios (id) ON DELETE SET NULL,
        CONSTRAINT fk_solicitudes_eliminacion_usuarios_revisado_por_id FOREIGN KEY (revisado_por_id) REFERENCES usuarios (id) ON DELETE SET NULL,
        CONSTRAINT fk_solicitudes_eliminacion_usuarios_solicitado_por_id FOREIGN KEY (solicitado_por_id) REFERENCES usuarios (id) ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260818015920_AddRbacFourTierRoles') THEN
    ALTER TABLE usuarios ADD CONSTRAINT ck_usuarios_rol CHECK (rol IN ('super_admin', 'admin', 'manager', 'miembro'));
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260818015920_AddRbacFourTierRoles') THEN
    CREATE INDEX ix_proyectos_gerente_id ON proyectos (gerente_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260818015920_AddRbacFourTierRoles') THEN
    CREATE INDEX ix_solicitudes_eliminacion_aprobado_por_gerente_id ON solicitudes_eliminacion (aprobado_por_gerente_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260818015920_AddRbacFourTierRoles') THEN
    CREATE INDEX ix_solicitudes_eliminacion_estado ON solicitudes_eliminacion (estado);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260818015920_AddRbacFourTierRoles') THEN
    CREATE INDEX ix_solicitudes_eliminacion_gerente_responsable_id ON solicitudes_eliminacion (gerente_responsable_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260818015920_AddRbacFourTierRoles') THEN
    CREATE INDEX ix_solicitudes_eliminacion_revisado_por_id ON solicitudes_eliminacion (revisado_por_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260818015920_AddRbacFourTierRoles') THEN
    CREATE INDEX ix_solicitudes_eliminacion_solicitado_por_id ON solicitudes_eliminacion (solicitado_por_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260818015920_AddRbacFourTierRoles') THEN
    CREATE INDEX ix_solicitudes_eliminacion_tipo_entidad_entidad_id ON solicitudes_eliminacion (tipo_entidad, entidad_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260818015920_AddRbacFourTierRoles') THEN
    ALTER TABLE proyectos ADD CONSTRAINT fk_proyectos_usuarios_gerente_id FOREIGN KEY (gerente_id) REFERENCES usuarios (id) ON DELETE SET NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260818015920_AddRbacFourTierRoles') THEN
    INSERT INTO "__EFMigrationsHistory" (migration_id, product_version)
    VALUES ('20260818015920_AddRbacFourTierRoles', '8.0.11');
    END IF;
END $EF$;
COMMIT;

