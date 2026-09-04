-- ============================================================================
-- 16_datos_reales_clientes_proveedores_proyectos.sql
--
-- Carga los clientes, proveedores y proyectos REALES que ya estaban en el
-- prototipo (Nexit__Standalone.html) hacia la base de datos de producción
-- (Supabase). Los datos se extrajeron directamente de las constantes
-- embebidas en el HTML del prototipo (PROVIDERS / CLIENTS / PROJECTS), no
-- fueron inventados.
--
-- REQUISITOS ANTES DE CORRER ESTE SCRIPT (en este orden):
--   1. docs/schema/14_cliente_ubicacion_catalogo_y_estado.sql   (agrega
--      pais_id/region_id/ciudad_id/estado a "clientes")
--   2. docs/schema/15_fix_propuesta_estado_proyectos.sql         (corrige
--      ck_proyectos_propuesta para permitir 'Aprobada'/'Rechazada' --
--      ver nota más abajo. Este archivo también trae ese mismo ajuste
--      como bloque de seguridad, por si se te olvida correr el 15 antes,
--      pero de todas formas corre el 15 primero para que quede también
--      registrado en __EFMigrationsHistory)
--   3. docs/schema/seed_geografia_categorias_estados.sql         (si tu
--      base de producción todavía no tiene países/departamentos/ciudades/
--      categorías de proveedor/estados de proyecto sembrados -- si ya lo
--      corriste antes, sáltatelo)
--   4. Este archivo.
--
-- Es SEGURO correr este script más de una vez, y también es seguro correrlo
-- si YA existen registros con estos mismos nombres (por ejemplo, datos de
-- prueba/placeholder cargados antes): cada cliente/proveedor/proyecto se
-- busca primero por nombre exacto; si ya existe, se ACTUALIZA con estos
-- datos reales (no se crea un duplicado y no se deja la fila vieja a
-- medias); si no existe, se crea. Cada dato hijo (teléfonos, servicios,
-- adjuntos, equipo, bitácora, proveedores asignados) también se revisa
-- antes de insertarse para no duplicar. No borra teléfonos ni datos hijos
-- viejos que ya existieran con otro valor (por ejemplo un teléfono de
-- prueba) -- solo agrega los reales que falten.
--
-- NOTA IMPORTANTE -- bug real encontrado al preparar estos datos:
-- el campo "Estado de la propuesta" de un proyecto en el prototipo puede
-- valer 'Aprobada' o 'Rechazada' (además de 'No enviada' y 'Enviada'), pero
-- la restricción de la base de datos antes de esta revisión
-- (ck_proyectos_propuesta) SOLO permitía ('No enviada', 'En proceso',
-- 'Enviada') -- 'En proceso' ni siquiera se usa en el prototipo, y
-- 'Aprobada'/'Rechazada' faltaban. Con la restricción vieja, este script
-- fallaría al intentar cargar los 4 proyectos reales que ya tienen
-- propuesta "Aprobada". docs/schema/15_fix_propuesta_estado_proyectos.sql
-- corrige esa restricción para que coincida con las opciones reales del
-- formulario ('No enviada', 'Enviada', 'Aprobada', 'Rechazada'). El código
-- de la aplicación (backend y frontend) se corrigió por separado para que
-- ya no vuelva a desalinearse.
-- ============================================================================

-- Bloque de seguridad (ver nota arriba): repite el ajuste de
-- 15_fix_propuesta_estado_proyectos.sql por si ese archivo no se corrió
-- todavía. Si ya lo corriste, este bloque no hace nada distinto (deja la
-- restricción exactamente igual).
DO $blk$
BEGIN
    ALTER TABLE proyectos DROP CONSTRAINT IF EXISTS ck_proyectos_propuesta;
    ALTER TABLE proyectos ADD CONSTRAINT ck_proyectos_propuesta
        CHECK (propuesta_estado IN ('No enviada', 'Enviada', 'Aprobada', 'Rechazada'));
END $blk$;

-- ============================== CLIENTES ==============================
-- Cliente: Grupo Vitalis
DO $blk$
DECLARE v_cliente_id uuid;
BEGIN
    SELECT id INTO v_cliente_id FROM clientes WHERE nombre = 'Grupo Vitalis' LIMIT 1;
    IF v_cliente_id IS NULL THEN
        v_cliente_id := gen_random_uuid();
        INSERT INTO clientes (id, nombre, sector, ciudad, contacto, cargo_contacto, email, notas, pais_id, region_id, ciudad_id, estado, created_at, updated_at)
        VALUES (v_cliente_id, 'Grupo Vitalis', 'Consumo masivo', 'Bogotá', 'Ricardo Gómez', 'Gerente de Marketing', 'ricardo@vitalis.com', 'Cliente frecuente, siempre pide activaciones BTL.',
            (SELECT id FROM paises WHERE nombre = 'Colombia'),
            (SELECT r.id FROM regiones r JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'Colombia' AND r.nombre = 'Bogotá D.C.'),
            (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'Colombia' AND r.nombre = 'Bogotá D.C.' AND ci.nombre = 'Bogotá'),
            'Activo', now(), now());
    ELSE
        UPDATE clientes SET
            sector = 'Consumo masivo',
            ciudad = 'Bogotá',
            contacto = 'Ricardo Gómez',
            cargo_contacto = 'Gerente de Marketing',
            email = 'ricardo@vitalis.com',
            notas = 'Cliente frecuente, siempre pide activaciones BTL.',
            pais_id = (SELECT id FROM paises WHERE nombre = 'Colombia'),
            region_id = (SELECT r.id FROM regiones r JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'Colombia' AND r.nombre = 'Bogotá D.C.'),
            ciudad_id = (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'Colombia' AND r.nombre = 'Bogotá D.C.' AND ci.nombre = 'Bogotá'),
            estado = 'Activo',
            updated_at = now()
        WHERE id = v_cliente_id;
    END IF;

    INSERT INTO cliente_telefonos (id, cliente_id, telefono)
    SELECT gen_random_uuid(), v_cliente_id, v.t FROM (VALUES ('+57 310 555 2233')) AS v(t)
    WHERE NOT EXISTS (SELECT 1 FROM cliente_telefonos WHERE cliente_id = v_cliente_id AND telefono = v.t);
END $blk$;

-- Cliente: Cámara de Comercio Bogotá
DO $blk$
DECLARE v_cliente_id uuid;
BEGIN
    SELECT id INTO v_cliente_id FROM clientes WHERE nombre = 'Cámara de Comercio Bogotá' LIMIT 1;
    IF v_cliente_id IS NULL THEN
        v_cliente_id := gen_random_uuid();
        INSERT INTO clientes (id, nombre, sector, ciudad, contacto, cargo_contacto, email, notas, pais_id, region_id, ciudad_id, estado, created_at, updated_at)
        VALUES (v_cliente_id, 'Cámara de Comercio Bogotá', 'Gremios y asociaciones', 'Bogotá', 'Paula Méndez', 'Coordinadora de Eventos', 'paula@ccb.org.co', 'Organiza ferias anuales, procesos de compra públicos.',
            (SELECT id FROM paises WHERE nombre = 'Colombia'),
            (SELECT r.id FROM regiones r JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'Colombia' AND r.nombre = 'Bogotá D.C.'),
            (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'Colombia' AND r.nombre = 'Bogotá D.C.' AND ci.nombre = 'Bogotá'),
            'Activo', now(), now());
    ELSE
        UPDATE clientes SET
            sector = 'Gremios y asociaciones',
            ciudad = 'Bogotá',
            contacto = 'Paula Méndez',
            cargo_contacto = 'Coordinadora de Eventos',
            email = 'paula@ccb.org.co',
            notas = 'Organiza ferias anuales, procesos de compra públicos.',
            pais_id = (SELECT id FROM paises WHERE nombre = 'Colombia'),
            region_id = (SELECT r.id FROM regiones r JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'Colombia' AND r.nombre = 'Bogotá D.C.'),
            ciudad_id = (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'Colombia' AND r.nombre = 'Bogotá D.C.' AND ci.nombre = 'Bogotá'),
            estado = 'Activo',
            updated_at = now()
        WHERE id = v_cliente_id;
    END IF;

    INSERT INTO cliente_telefonos (id, cliente_id, telefono)
    SELECT gen_random_uuid(), v_cliente_id, v.t FROM (VALUES ('+57 300 444 7788'), ('+57 601 444 0000')) AS v(t)
    WHERE NOT EXISTS (SELECT 1 FROM cliente_telefonos WHERE cliente_id = v_cliente_id AND telefono = v.t);
END $blk$;

-- Cliente: Marca Ron Real
DO $blk$
DECLARE v_cliente_id uuid;
BEGIN
    SELECT id INTO v_cliente_id FROM clientes WHERE nombre = 'Marca Ron Real' LIMIT 1;
    IF v_cliente_id IS NULL THEN
        v_cliente_id := gen_random_uuid();
        INSERT INTO clientes (id, nombre, sector, ciudad, contacto, cargo_contacto, email, notas, pais_id, region_id, ciudad_id, estado, created_at, updated_at)
        VALUES (v_cliente_id, 'Marca Ron Real', 'Bebidas', 'Cartagena', 'Camila Ortiz', 'Brand Manager', 'camila@ronreal.com', 'En negociación para activación itinerante Costa Caribe.',
            (SELECT id FROM paises WHERE nombre = 'Colombia'),
            (SELECT r.id FROM regiones r JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'Colombia' AND r.nombre = 'Bolívar'),
            (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'Colombia' AND r.nombre = 'Bolívar' AND ci.nombre = 'Cartagena'),
            'Prospecto', now(), now());
    ELSE
        UPDATE clientes SET
            sector = 'Bebidas',
            ciudad = 'Cartagena',
            contacto = 'Camila Ortiz',
            cargo_contacto = 'Brand Manager',
            email = 'camila@ronreal.com',
            notas = 'En negociación para activación itinerante Costa Caribe.',
            pais_id = (SELECT id FROM paises WHERE nombre = 'Colombia'),
            region_id = (SELECT r.id FROM regiones r JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'Colombia' AND r.nombre = 'Bolívar'),
            ciudad_id = (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'Colombia' AND r.nombre = 'Bolívar' AND ci.nombre = 'Cartagena'),
            estado = 'Prospecto',
            updated_at = now()
        WHERE id = v_cliente_id;
    END IF;

    INSERT INTO cliente_telefonos (id, cliente_id, telefono)
    SELECT gen_random_uuid(), v_cliente_id, v.t FROM (VALUES ('+57 315 222 9090')) AS v(t)
    WHERE NOT EXISTS (SELECT 1 FROM cliente_telefonos WHERE cliente_id = v_cliente_id AND telefono = v.t);
END $blk$;

-- Cliente: Nova Cosméticos
DO $blk$
DECLARE v_cliente_id uuid;
BEGIN
    SELECT id INTO v_cliente_id FROM clientes WHERE nombre = 'Nova Cosméticos' LIMIT 1;
    IF v_cliente_id IS NULL THEN
        v_cliente_id := gen_random_uuid();
        INSERT INTO clientes (id, nombre, sector, ciudad, contacto, cargo_contacto, email, notas, pais_id, region_id, ciudad_id, estado, created_at, updated_at)
        VALUES (v_cliente_id, 'Nova Cosméticos', 'Belleza y cuidado personal', 'Medellín', 'Sergio Palacio', 'Director de Comunicaciones', 'sergio@novacosmeticos.com', 'Cierre de proyecto exitoso, pagó a tiempo.',
            (SELECT id FROM paises WHERE nombre = 'Colombia'),
            (SELECT r.id FROM regiones r JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'Colombia' AND r.nombre = 'Antioquia'),
            (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'Colombia' AND r.nombre = 'Antioquia' AND ci.nombre = 'Medellín'),
            'Activo', now(), now());
    ELSE
        UPDATE clientes SET
            sector = 'Belleza y cuidado personal',
            ciudad = 'Medellín',
            contacto = 'Sergio Palacio',
            cargo_contacto = 'Director de Comunicaciones',
            email = 'sergio@novacosmeticos.com',
            notas = 'Cierre de proyecto exitoso, pagó a tiempo.',
            pais_id = (SELECT id FROM paises WHERE nombre = 'Colombia'),
            region_id = (SELECT r.id FROM regiones r JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'Colombia' AND r.nombre = 'Antioquia'),
            ciudad_id = (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'Colombia' AND r.nombre = 'Antioquia' AND ci.nombre = 'Medellín'),
            estado = 'Activo',
            updated_at = now()
        WHERE id = v_cliente_id;
    END IF;

    INSERT INTO cliente_telefonos (id, cliente_id, telefono)
    SELECT gen_random_uuid(), v_cliente_id, v.t FROM (VALUES ('+57 312 888 1122')) AS v(t)
    WHERE NOT EXISTS (SELECT 1 FROM cliente_telefonos WHERE cliente_id = v_cliente_id AND telefono = v.t);
END $blk$;

-- Cliente: Grupo Financiero Andes
DO $blk$
DECLARE v_cliente_id uuid;
BEGIN
    SELECT id INTO v_cliente_id FROM clientes WHERE nombre = 'Grupo Financiero Andes' LIMIT 1;
    IF v_cliente_id IS NULL THEN
        v_cliente_id := gen_random_uuid();
        INSERT INTO clientes (id, nombre, sector, ciudad, contacto, cargo_contacto, email, notas, pais_id, region_id, ciudad_id, estado, created_at, updated_at)
        VALUES (v_cliente_id, 'Grupo Financiero Andes', 'Finanzas', 'Cuauhtémoc', 'Laura Nieto', 'Gerente de Experiencia', 'laura@gfandes.mx', 'Requiere protocolos de seguridad estrictos en sus eventos.',
            (SELECT id FROM paises WHERE nombre = 'México'),
            (SELECT r.id FROM regiones r JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND r.nombre = 'Ciudad de México'),
            (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND r.nombre = 'Ciudad de México' AND ci.nombre = 'Cuauhtémoc'),
            'Activo', now(), now());
    ELSE
        UPDATE clientes SET
            sector = 'Finanzas',
            ciudad = 'Cuauhtémoc',
            contacto = 'Laura Nieto',
            cargo_contacto = 'Gerente de Experiencia',
            email = 'laura@gfandes.mx',
            notas = 'Requiere protocolos de seguridad estrictos en sus eventos.',
            pais_id = (SELECT id FROM paises WHERE nombre = 'México'),
            region_id = (SELECT r.id FROM regiones r JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND r.nombre = 'Ciudad de México'),
            ciudad_id = (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND r.nombre = 'Ciudad de México' AND ci.nombre = 'Cuauhtémoc'),
            estado = 'Activo',
            updated_at = now()
        WHERE id = v_cliente_id;
    END IF;

    INSERT INTO cliente_telefonos (id, cliente_id, telefono)
    SELECT gen_random_uuid(), v_cliente_id, v.t FROM (VALUES ('+52 55 4433 1200')) AS v(t)
    WHERE NOT EXISTS (SELECT 1 FROM cliente_telefonos WHERE cliente_id = v_cliente_id AND telefono = v.t);
END $blk$;

-- Cliente: Tech Solutions LatAm
DO $blk$
DECLARE v_cliente_id uuid;
BEGIN
    SELECT id INTO v_cliente_id FROM clientes WHERE nombre = 'Tech Solutions LatAm' LIMIT 1;
    IF v_cliente_id IS NULL THEN
        v_cliente_id := gen_random_uuid();
        INSERT INTO clientes (id, nombre, sector, ciudad, contacto, cargo_contacto, email, notas, pais_id, region_id, ciudad_id, estado, created_at, updated_at)
        VALUES (v_cliente_id, 'Tech Solutions LatAm', 'Tecnología', 'Los Angeles', 'Daniel Restrepo', 'Head of Marketing', 'daniel@techsolutions.com', 'Primer contacto en feria, evaluando propuesta.',
            (SELECT id FROM paises WHERE nombre = 'Estados Unidos'),
            (SELECT r.id FROM regiones r JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'Estados Unidos' AND r.nombre = 'California'),
            (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'Estados Unidos' AND r.nombre = 'California' AND ci.nombre = 'Los Angeles'),
            'Prospecto', now(), now());
    ELSE
        UPDATE clientes SET
            sector = 'Tecnología',
            ciudad = 'Los Angeles',
            contacto = 'Daniel Restrepo',
            cargo_contacto = 'Head of Marketing',
            email = 'daniel@techsolutions.com',
            notas = 'Primer contacto en feria, evaluando propuesta.',
            pais_id = (SELECT id FROM paises WHERE nombre = 'Estados Unidos'),
            region_id = (SELECT r.id FROM regiones r JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'Estados Unidos' AND r.nombre = 'California'),
            ciudad_id = (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'Estados Unidos' AND r.nombre = 'California' AND ci.nombre = 'Los Angeles'),
            estado = 'Prospecto',
            updated_at = now()
        WHERE id = v_cliente_id;
    END IF;

    INSERT INTO cliente_telefonos (id, cliente_id, telefono)
    SELECT gen_random_uuid(), v_cliente_id, v.t FROM (VALUES ('+1 310 555 4499')) AS v(t)
    WHERE NOT EXISTS (SELECT 1 FROM cliente_telefonos WHERE cliente_id = v_cliente_id AND telefono = v.t);
END $blk$;

-- Cliente: Universidad del Pacífico
DO $blk$
DECLARE v_cliente_id uuid;
BEGIN
    SELECT id INTO v_cliente_id FROM clientes WHERE nombre = 'Universidad del Pacífico' LIMIT 1;
    IF v_cliente_id IS NULL THEN
        v_cliente_id := gen_random_uuid();
        INSERT INTO clientes (id, nombre, sector, ciudad, contacto, cargo_contacto, email, notas, pais_id, region_id, ciudad_id, estado, created_at, updated_at)
        VALUES (v_cliente_id, 'Universidad del Pacífico', 'Educación', 'Cali', 'Marcela Uribe', 'Directora de Bienestar', 'marcela@unipacifico.edu.co', 'Eventos institucionales dos veces al año.',
            (SELECT id FROM paises WHERE nombre = 'Colombia'),
            (SELECT r.id FROM regiones r JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'Colombia' AND r.nombre = 'Valle del Cauca'),
            (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'Colombia' AND r.nombre = 'Valle del Cauca' AND ci.nombre = 'Cali'),
            'Activo', now(), now());
    ELSE
        UPDATE clientes SET
            sector = 'Educación',
            ciudad = 'Cali',
            contacto = 'Marcela Uribe',
            cargo_contacto = 'Directora de Bienestar',
            email = 'marcela@unipacifico.edu.co',
            notas = 'Eventos institucionales dos veces al año.',
            pais_id = (SELECT id FROM paises WHERE nombre = 'Colombia'),
            region_id = (SELECT r.id FROM regiones r JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'Colombia' AND r.nombre = 'Valle del Cauca'),
            ciudad_id = (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'Colombia' AND r.nombre = 'Valle del Cauca' AND ci.nombre = 'Cali'),
            estado = 'Activo',
            updated_at = now()
        WHERE id = v_cliente_id;
    END IF;

    INSERT INTO cliente_telefonos (id, cliente_id, telefono)
    SELECT gen_random_uuid(), v_cliente_id, v.t FROM (VALUES ('+57 315 220 4477'), ('+57 602 555 3300')) AS v(t)
    WHERE NOT EXISTS (SELECT 1 FROM cliente_telefonos WHERE cliente_id = v_cliente_id AND telefono = v.t);
END $blk$;

-- Cliente: Constructora Horizonte
DO $blk$
DECLARE v_cliente_id uuid;
BEGIN
    SELECT id INTO v_cliente_id FROM clientes WHERE nombre = 'Constructora Horizonte' LIMIT 1;
    IF v_cliente_id IS NULL THEN
        v_cliente_id := gen_random_uuid();
        INSERT INTO clientes (id, nombre, sector, ciudad, contacto, cargo_contacto, email, notas, pais_id, region_id, ciudad_id, estado, created_at, updated_at)
        VALUES (v_cliente_id, 'Constructora Horizonte', 'Construcción', 'Cuauhtémoc', 'Bernardo Leal', 'Gerente Comercial', 'bernardo@horizonte.mx', 'No hay actividad desde hace un año.',
            (SELECT id FROM paises WHERE nombre = 'México'),
            (SELECT r.id FROM regiones r JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND r.nombre = 'Ciudad de México'),
            (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND r.nombre = 'Ciudad de México' AND ci.nombre = 'Cuauhtémoc'),
            'Inactivo', now(), now());
    ELSE
        UPDATE clientes SET
            sector = 'Construcción',
            ciudad = 'Cuauhtémoc',
            contacto = 'Bernardo Leal',
            cargo_contacto = 'Gerente Comercial',
            email = 'bernardo@horizonte.mx',
            notas = 'No hay actividad desde hace un año.',
            pais_id = (SELECT id FROM paises WHERE nombre = 'México'),
            region_id = (SELECT r.id FROM regiones r JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND r.nombre = 'Ciudad de México'),
            ciudad_id = (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND r.nombre = 'Ciudad de México' AND ci.nombre = 'Cuauhtémoc'),
            estado = 'Inactivo',
            updated_at = now()
        WHERE id = v_cliente_id;
    END IF;

    INSERT INTO cliente_telefonos (id, cliente_id, telefono)
    SELECT gen_random_uuid(), v_cliente_id, v.t FROM (VALUES ('+52 55 4400 5566')) AS v(t)
    WHERE NOT EXISTS (SELECT 1 FROM cliente_telefonos WHERE cliente_id = v_cliente_id AND telefono = v.t);
END $blk$;

-- ============================== PROVEEDORES ==============================
-- Proveedor: Grupo Luminary
DO $blk$
DECLARE v_proveedor_id uuid;
BEGIN
    SELECT id INTO v_proveedor_id FROM proveedores WHERE nombre = 'Grupo Luminary' LIMIT 1;
    IF v_proveedor_id IS NULL THEN
        v_proveedor_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, score, presupuesto, cobertura, notas, created_at, updated_at)
        VALUES (v_proveedor_id, 'Grupo Luminary',
            (SELECT id FROM paises WHERE nombre = 'México'),
            (SELECT r.id FROM regiones r JOIN paises pa ON r.pais_id = pa.id WHERE pa.nombre = 'México' AND r.nombre = 'Ciudad de México'),
            (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises pa ON r.pais_id = pa.id WHERE pa.nombre = 'México' AND r.nombre = 'Ciudad de México' AND ci.nombre = 'Benito Juárez'),
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Iluminación y sonido'),
            'Activo', 'Rafael Torres', NULL, 'rafael@luminary.mx', 5, '$$$ Alto (100k–500k)', 'Nacional', 'Go-to para eventos premium. Descuento 10% con contrato anual. Pedir a Rafael 3 semanas antes.', now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'México'),
            region_id = (SELECT r.id FROM regiones r JOIN paises pa ON r.pais_id = pa.id WHERE pa.nombre = 'México' AND r.nombre = 'Ciudad de México'),
            ciudad_id = (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises pa ON r.pais_id = pa.id WHERE pa.nombre = 'México' AND r.nombre = 'Ciudad de México' AND ci.nombre = 'Benito Juárez'),
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Iluminación y sonido'),
            estado = 'Activo',
            contacto = 'Rafael Torres',
            email = 'rafael@luminary.mx',
            score = 5,
            presupuesto = '$$$ Alto (100k–500k)',
            cobertura = 'Nacional',
            notas = 'Go-to para eventos premium. Descuento 10% con contrato anual. Pedir a Rafael 3 semanas antes.',
            updated_at = now()
        WHERE id = v_proveedor_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_proveedor_id, '+52 55 8823 4401'
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_proveedor_id AND telefono = '+52 55 8823 4401');

    INSERT INTO servicios (id, nombre, created_at) VALUES (gen_random_uuid(), 'Iluminación arquitectónica', now()) ON CONFLICT (nombre) DO NOTHING;
    INSERT INTO proveedor_servicios (proveedor_id, servicio_id)
    SELECT v_proveedor_id, id FROM servicios WHERE nombre = 'Iluminación arquitectónica'
    ON CONFLICT (proveedor_id, servicio_id) DO NOTHING;
    INSERT INTO servicios (id, nombre, created_at) VALUES (gen_random_uuid(), 'sonido line array', now()) ON CONFLICT (nombre) DO NOTHING;
    INSERT INTO proveedor_servicios (proveedor_id, servicio_id)
    SELECT v_proveedor_id, id FROM servicios WHERE nombre = 'sonido line array'
    ON CONFLICT (proveedor_id, servicio_id) DO NOTHING;
    INSERT INTO servicios (id, nombre, created_at) VALUES (gen_random_uuid(), 'generadores', now()) ON CONFLICT (nombre) DO NOTHING;
    INSERT INTO proveedor_servicios (proveedor_id, servicio_id)
    SELECT v_proveedor_id, id FROM servicios WHERE nombre = 'generadores'
    ON CONFLICT (proveedor_id, servicio_id) DO NOTHING;

    INSERT INTO proveedor_adjuntos (id, proveedor_id, tipo, nombre, url, meta, fecha, created_at)
    SELECT gen_random_uuid(), v_proveedor_id, 'link', 'Portafolio 2024', 'https://drive.google.com', 'drive.google.com', '2024-03-10'::timestamptz, now()
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_adjuntos WHERE proveedor_id = v_proveedor_id AND nombre = 'Portafolio 2024');
    INSERT INTO proveedor_adjuntos (id, proveedor_id, tipo, nombre, url, meta, fecha, created_at)
    SELECT gen_random_uuid(), v_proveedor_id, 'link', 'Contrato marco vigente', 'https://docs.google.com', 'docs.google.com', '2024-01-15'::timestamptz, now()
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_adjuntos WHERE proveedor_id = v_proveedor_id AND nombre = 'Contrato marco vigente');
END $blk$;

-- Proveedor: EvenTech México
DO $blk$
DECLARE v_proveedor_id uuid;
BEGIN
    SELECT id INTO v_proveedor_id FROM proveedores WHERE nombre = 'EvenTech México' LIMIT 1;
    IF v_proveedor_id IS NULL THEN
        v_proveedor_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, score, presupuesto, cobertura, notas, created_at, updated_at)
        VALUES (v_proveedor_id, 'EvenTech México',
            (SELECT id FROM paises WHERE nombre = 'México'),
            (SELECT r.id FROM regiones r JOIN paises pa ON r.pais_id = pa.id WHERE pa.nombre = 'México' AND r.nombre = 'Nuevo León'),
            (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises pa ON r.pais_id = pa.id WHERE pa.nombre = 'México' AND r.nombre = 'Nuevo León' AND ci.nombre = 'Monterrey'),
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Tecnología e interactividad'),
            'Activo', 'Sofía Mendoza', NULL, 'sofia@eventech.mx', 5, '$$$ Alto (100k–500k)', 'Nacional', 'Requieren brief técnico detallado. Solo trabajan con 50% de anticipo.', now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'México'),
            region_id = (SELECT r.id FROM regiones r JOIN paises pa ON r.pais_id = pa.id WHERE pa.nombre = 'México' AND r.nombre = 'Nuevo León'),
            ciudad_id = (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises pa ON r.pais_id = pa.id WHERE pa.nombre = 'México' AND r.nombre = 'Nuevo León' AND ci.nombre = 'Monterrey'),
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Tecnología e interactividad'),
            estado = 'Activo',
            contacto = 'Sofía Mendoza',
            email = 'sofia@eventech.mx',
            score = 5,
            presupuesto = '$$$ Alto (100k–500k)',
            cobertura = 'Nacional',
            notas = 'Requieren brief técnico detallado. Solo trabajan con 50% de anticipo.',
            updated_at = now()
        WHERE id = v_proveedor_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_proveedor_id, '+52 81 7744 9900'
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_proveedor_id AND telefono = '+52 81 7744 9900');

    INSERT INTO servicios (id, nombre, created_at) VALUES (gen_random_uuid(), 'Video mapping', now()) ON CONFLICT (nombre) DO NOTHING;
    INSERT INTO proveedor_servicios (proveedor_id, servicio_id)
    SELECT v_proveedor_id, id FROM servicios WHERE nombre = 'Video mapping'
    ON CONFLICT (proveedor_id, servicio_id) DO NOTHING;
    INSERT INTO servicios (id, nombre, created_at) VALUES (gen_random_uuid(), 'realidad aumentada', now()) ON CONFLICT (nombre) DO NOTHING;
    INSERT INTO proveedor_servicios (proveedor_id, servicio_id)
    SELECT v_proveedor_id, id FROM servicios WHERE nombre = 'realidad aumentada'
    ON CONFLICT (proveedor_id, servicio_id) DO NOTHING;
    INSERT INTO servicios (id, nombre, created_at) VALUES (gen_random_uuid(), 'pantallas LED', now()) ON CONFLICT (nombre) DO NOTHING;
    INSERT INTO proveedor_servicios (proveedor_id, servicio_id)
    SELECT v_proveedor_id, id FROM servicios WHERE nombre = 'pantallas LED'
    ON CONFLICT (proveedor_id, servicio_id) DO NOTHING;
    INSERT INTO servicios (id, nombre, created_at) VALUES (gen_random_uuid(), 'gamificación', now()) ON CONFLICT (nombre) DO NOTHING;
    INSERT INTO proveedor_servicios (proveedor_id, servicio_id)
    SELECT v_proveedor_id, id FROM servicios WHERE nombre = 'gamificación'
    ON CONFLICT (proveedor_id, servicio_id) DO NOTHING;

    INSERT INTO proveedor_adjuntos (id, proveedor_id, tipo, nombre, url, meta, fecha, created_at)
    SELECT gen_random_uuid(), v_proveedor_id, 'link', 'Catálogo de equipos', 'https://eventech.mx/catalogo', 'eventech.mx', '2024-02-20'::timestamptz, now()
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_adjuntos WHERE proveedor_id = v_proveedor_id AND nombre = 'Catálogo de equipos');
END $blk$;

-- Proveedor: Flash Studios
DO $blk$
DECLARE v_proveedor_id uuid;
BEGIN
    SELECT id INTO v_proveedor_id FROM proveedores WHERE nombre = 'Flash Studios' LIMIT 1;
    IF v_proveedor_id IS NULL THEN
        v_proveedor_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, score, presupuesto, cobertura, notas, created_at, updated_at)
        VALUES (v_proveedor_id, 'Flash Studios',
            (SELECT id FROM paises WHERE nombre = 'México'),
            (SELECT r.id FROM regiones r JOIN paises pa ON r.pais_id = pa.id WHERE pa.nombre = 'México' AND r.nombre = 'Ciudad de México'),
            (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises pa ON r.pais_id = pa.id WHERE pa.nombre = 'México' AND r.nombre = 'Ciudad de México' AND ci.nombre = 'Coyoacán'),
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Fotografía y video'),
            'Activo', 'Diego Ramírez', NULL, 'diego@flashstudios.mx', 5, '$$ Medio (20k–100k)', 'Nacional', 'Entrega en 48h. Pack completo con edición incluida. Los mejores para recaps ejecutivos.', now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'México'),
            region_id = (SELECT r.id FROM regiones r JOIN paises pa ON r.pais_id = pa.id WHERE pa.nombre = 'México' AND r.nombre = 'Ciudad de México'),
            ciudad_id = (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises pa ON r.pais_id = pa.id WHERE pa.nombre = 'México' AND r.nombre = 'Ciudad de México' AND ci.nombre = 'Coyoacán'),
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Fotografía y video'),
            estado = 'Activo',
            contacto = 'Diego Ramírez',
            email = 'diego@flashstudios.mx',
            score = 5,
            presupuesto = '$$ Medio (20k–100k)',
            cobertura = 'Nacional',
            notas = 'Entrega en 48h. Pack completo con edición incluida. Los mejores para recaps ejecutivos.',
            updated_at = now()
        WHERE id = v_proveedor_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_proveedor_id, '+52 55 6677 8899'
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_proveedor_id AND telefono = '+52 55 6677 8899');

    INSERT INTO servicios (id, nombre, created_at) VALUES (gen_random_uuid(), 'Fotografía editorial', now()) ON CONFLICT (nombre) DO NOTHING;
    INSERT INTO proveedor_servicios (proveedor_id, servicio_id)
    SELECT v_proveedor_id, id FROM servicios WHERE nombre = 'Fotografía editorial'
    ON CONFLICT (proveedor_id, servicio_id) DO NOTHING;
    INSERT INTO servicios (id, nombre, created_at) VALUES (gen_random_uuid(), 'video recap', now()) ON CONFLICT (nombre) DO NOTHING;
    INSERT INTO proveedor_servicios (proveedor_id, servicio_id)
    SELECT v_proveedor_id, id FROM servicios WHERE nombre = 'video recap'
    ON CONFLICT (proveedor_id, servicio_id) DO NOTHING;
    INSERT INTO servicios (id, nombre, created_at) VALUES (gen_random_uuid(), 'transmisión en vivo', now()) ON CONFLICT (nombre) DO NOTHING;
    INSERT INTO proveedor_servicios (proveedor_id, servicio_id)
    SELECT v_proveedor_id, id FROM servicios WHERE nombre = 'transmisión en vivo'
    ON CONFLICT (proveedor_id, servicio_id) DO NOTHING;
    INSERT INTO servicios (id, nombre, created_at) VALUES (gen_random_uuid(), 'drone', now()) ON CONFLICT (nombre) DO NOTHING;
    INSERT INTO proveedor_servicios (proveedor_id, servicio_id)
    SELECT v_proveedor_id, id FROM servicios WHERE nombre = 'drone'
    ON CONFLICT (proveedor_id, servicio_id) DO NOTHING;

END $blk$;

-- Proveedor: Sabores & Experiencias
DO $blk$
DECLARE v_proveedor_id uuid;
BEGIN
    SELECT id INTO v_proveedor_id FROM proveedores WHERE nombre = 'Sabores & Experiencias' LIMIT 1;
    IF v_proveedor_id IS NULL THEN
        v_proveedor_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, score, presupuesto, cobertura, notas, created_at, updated_at)
        VALUES (v_proveedor_id, 'Sabores & Experiencias',
            (SELECT id FROM paises WHERE nombre = 'México'),
            (SELECT r.id FROM regiones r JOIN paises pa ON r.pais_id = pa.id WHERE pa.nombre = 'México' AND r.nombre = 'Jalisco'),
            (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises pa ON r.pais_id = pa.id WHERE pa.nombre = 'México' AND r.nombre = 'Jalisco' AND ci.nombre = 'Guadalajara'),
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Catering y F&B'),
            'Activo', 'Carmen Ríos', NULL, 'carmen@sabores.com.mx', 4, '$$ Medio (20k–100k)', 'Regional', 'Excelente presentación. Han fallado en eventos masivos (+500 pax). Ideales para ejecutivos.', now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'México'),
            region_id = (SELECT r.id FROM regiones r JOIN paises pa ON r.pais_id = pa.id WHERE pa.nombre = 'México' AND r.nombre = 'Jalisco'),
            ciudad_id = (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises pa ON r.pais_id = pa.id WHERE pa.nombre = 'México' AND r.nombre = 'Jalisco' AND ci.nombre = 'Guadalajara'),
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Catering y F&B'),
            estado = 'Activo',
            contacto = 'Carmen Ríos',
            email = 'carmen@sabores.com.mx',
            score = 4,
            presupuesto = '$$ Medio (20k–100k)',
            cobertura = 'Regional',
            notas = 'Excelente presentación. Han fallado en eventos masivos (+500 pax). Ideales para ejecutivos.',
            updated_at = now()
        WHERE id = v_proveedor_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_proveedor_id, '+52 33 5511 2233'
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_proveedor_id AND telefono = '+52 33 5511 2233');

    INSERT INTO servicios (id, nombre, created_at) VALUES (gen_random_uuid(), 'Catering gourmet', now()) ON CONFLICT (nombre) DO NOTHING;
    INSERT INTO proveedor_servicios (proveedor_id, servicio_id)
    SELECT v_proveedor_id, id FROM servicios WHERE nombre = 'Catering gourmet'
    ON CONFLICT (proveedor_id, servicio_id) DO NOTHING;
    INSERT INTO servicios (id, nombre, created_at) VALUES (gen_random_uuid(), 'barras de cocteles', now()) ON CONFLICT (nombre) DO NOTHING;
    INSERT INTO proveedor_servicios (proveedor_id, servicio_id)
    SELECT v_proveedor_id, id FROM servicios WHERE nombre = 'barras de cocteles'
    ON CONFLICT (proveedor_id, servicio_id) DO NOTHING;
    INSERT INTO servicios (id, nombre, created_at) VALUES (gen_random_uuid(), 'coffee breaks', now()) ON CONFLICT (nombre) DO NOTHING;
    INSERT INTO proveedor_servicios (proveedor_id, servicio_id)
    SELECT v_proveedor_id, id FROM servicios WHERE nombre = 'coffee breaks'
    ON CONFLICT (proveedor_id, servicio_id) DO NOTHING;
    INSERT INTO servicios (id, nombre, created_at) VALUES (gen_random_uuid(), 'food trucks', now()) ON CONFLICT (nombre) DO NOTHING;
    INSERT INTO proveedor_servicios (proveedor_id, servicio_id)
    SELECT v_proveedor_id, id FROM servicios WHERE nombre = 'food trucks'
    ON CONFLICT (proveedor_id, servicio_id) DO NOTHING;

END $blk$;

-- Proveedor: Crea Eventos Bogotá
DO $blk$
DECLARE v_proveedor_id uuid;
BEGIN
    SELECT id INTO v_proveedor_id FROM proveedores WHERE nombre = 'Crea Eventos Bogotá' LIMIT 1;
    IF v_proveedor_id IS NULL THEN
        v_proveedor_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, score, presupuesto, cobertura, notas, created_at, updated_at)
        VALUES (v_proveedor_id, 'Crea Eventos Bogotá',
            (SELECT id FROM paises WHERE nombre = 'Colombia'),
            (SELECT r.id FROM regiones r JOIN paises pa ON r.pais_id = pa.id WHERE pa.nombre = 'Colombia' AND r.nombre = 'Bogotá D.C.'),
            (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises pa ON r.pais_id = pa.id WHERE pa.nombre = 'Colombia' AND r.nombre = 'Bogotá D.C.' AND ci.nombre = 'Bogotá'),
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Escenografía y montaje'),
            'Activo', 'Laura Quintero', NULL, 'laura@creaev.co', 5, '$$$ Alto (100k–500k)', 'Nacional', 'Referencia de Coca-Cola Colombia. Los mejores en escenografía del país. Reservar con 1 mes.', now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'Colombia'),
            region_id = (SELECT r.id FROM regiones r JOIN paises pa ON r.pais_id = pa.id WHERE pa.nombre = 'Colombia' AND r.nombre = 'Bogotá D.C.'),
            ciudad_id = (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises pa ON r.pais_id = pa.id WHERE pa.nombre = 'Colombia' AND r.nombre = 'Bogotá D.C.' AND ci.nombre = 'Bogotá'),
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Escenografía y montaje'),
            estado = 'Activo',
            contacto = 'Laura Quintero',
            email = 'laura@creaev.co',
            score = 5,
            presupuesto = '$$$ Alto (100k–500k)',
            cobertura = 'Nacional',
            notas = 'Referencia de Coca-Cola Colombia. Los mejores en escenografía del país. Reservar con 1 mes.',
            updated_at = now()
        WHERE id = v_proveedor_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_proveedor_id, '+57 310 444 8800'
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_proveedor_id AND telefono = '+57 310 444 8800');

    INSERT INTO servicios (id, nombre, created_at) VALUES (gen_random_uuid(), 'Escenografías custom', now()) ON CONFLICT (nombre) DO NOTHING;
    INSERT INTO proveedor_servicios (proveedor_id, servicio_id)
    SELECT v_proveedor_id, id FROM servicios WHERE nombre = 'Escenografías custom'
    ON CONFLICT (proveedor_id, servicio_id) DO NOTHING;
    INSERT INTO servicios (id, nombre, created_at) VALUES (gen_random_uuid(), 'stands', now()) ON CONFLICT (nombre) DO NOTHING;
    INSERT INTO proveedor_servicios (proveedor_id, servicio_id)
    SELECT v_proveedor_id, id FROM servicios WHERE nombre = 'stands'
    ON CONFLICT (proveedor_id, servicio_id) DO NOTHING;
    INSERT INTO servicios (id, nombre, created_at) VALUES (gen_random_uuid(), 'estructuras', now()) ON CONFLICT (nombre) DO NOTHING;
    INSERT INTO proveedor_servicios (proveedor_id, servicio_id)
    SELECT v_proveedor_id, id FROM servicios WHERE nombre = 'estructuras'
    ON CONFLICT (proveedor_id, servicio_id) DO NOTHING;
    INSERT INTO servicios (id, nombre, created_at) VALUES (gen_random_uuid(), 'carpas premium', now()) ON CONFLICT (nombre) DO NOTHING;
    INSERT INTO proveedor_servicios (proveedor_id, servicio_id)
    SELECT v_proveedor_id, id FROM servicios WHERE nombre = 'carpas premium'
    ON CONFLICT (proveedor_id, servicio_id) DO NOTHING;

    INSERT INTO proveedor_adjuntos (id, proveedor_id, tipo, nombre, url, meta, fecha, created_at)
    SELECT gen_random_uuid(), v_proveedor_id, 'link', 'Fotos evento Coca-Cola 2023', 'https://drive.google.com', 'drive.google.com', '2023-11-05'::timestamptz, now()
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_adjuntos WHERE proveedor_id = v_proveedor_id AND nombre = 'Fotos evento Coca-Cola 2023');
END $blk$;

-- Proveedor: Sonido Élite Colombia
DO $blk$
DECLARE v_proveedor_id uuid;
BEGIN
    SELECT id INTO v_proveedor_id FROM proveedores WHERE nombre = 'Sonido Élite Colombia' LIMIT 1;
    IF v_proveedor_id IS NULL THEN
        v_proveedor_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, score, presupuesto, cobertura, notas, created_at, updated_at)
        VALUES (v_proveedor_id, 'Sonido Élite Colombia',
            (SELECT id FROM paises WHERE nombre = 'Colombia'),
            (SELECT r.id FROM regiones r JOIN paises pa ON r.pais_id = pa.id WHERE pa.nombre = 'Colombia' AND r.nombre = 'Antioquia'),
            (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises pa ON r.pais_id = pa.id WHERE pa.nombre = 'Colombia' AND r.nombre = 'Antioquia' AND ci.nombre = 'Medellín'),
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Iluminación y sonido'),
            'Activo', 'Andrés Mejía', NULL, 'andres@sonidoelite.co', 4, '$$ Medio (20k–100k)', 'Regional', 'Cubren Antioquia y Eje Cafetero. Precios competitivos. Muy puntuales.', now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'Colombia'),
            region_id = (SELECT r.id FROM regiones r JOIN paises pa ON r.pais_id = pa.id WHERE pa.nombre = 'Colombia' AND r.nombre = 'Antioquia'),
            ciudad_id = (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises pa ON r.pais_id = pa.id WHERE pa.nombre = 'Colombia' AND r.nombre = 'Antioquia' AND ci.nombre = 'Medellín'),
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Iluminación y sonido'),
            estado = 'Activo',
            contacto = 'Andrés Mejía',
            email = 'andres@sonidoelite.co',
            score = 4,
            presupuesto = '$$ Medio (20k–100k)',
            cobertura = 'Regional',
            notas = 'Cubren Antioquia y Eje Cafetero. Precios competitivos. Muy puntuales.',
            updated_at = now()
        WHERE id = v_proveedor_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_proveedor_id, '+57 304 777 5500'
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_proveedor_id AND telefono = '+57 304 777 5500');

    INSERT INTO servicios (id, nombre, created_at) VALUES (gen_random_uuid(), 'Sonido line array', now()) ON CONFLICT (nombre) DO NOTHING;
    INSERT INTO proveedor_servicios (proveedor_id, servicio_id)
    SELECT v_proveedor_id, id FROM servicios WHERE nombre = 'Sonido line array'
    ON CONFLICT (proveedor_id, servicio_id) DO NOTHING;
    INSERT INTO servicios (id, nombre, created_at) VALUES (gen_random_uuid(), 'iluminación escénica', now()) ON CONFLICT (nombre) DO NOTHING;
    INSERT INTO proveedor_servicios (proveedor_id, servicio_id)
    SELECT v_proveedor_id, id FROM servicios WHERE nombre = 'iluminación escénica'
    ON CONFLICT (proveedor_id, servicio_id) DO NOTHING;
    INSERT INTO servicios (id, nombre, created_at) VALUES (gen_random_uuid(), 'backline', now()) ON CONFLICT (nombre) DO NOTHING;
    INSERT INTO proveedor_servicios (proveedor_id, servicio_id)
    SELECT v_proveedor_id, id FROM servicios WHERE nombre = 'backline'
    ON CONFLICT (proveedor_id, servicio_id) DO NOTHING;

END $blk$;

-- Proveedor: Cartagena Sound Pro
DO $blk$
DECLARE v_proveedor_id uuid;
BEGIN
    SELECT id INTO v_proveedor_id FROM proveedores WHERE nombre = 'Cartagena Sound Pro' LIMIT 1;
    IF v_proveedor_id IS NULL THEN
        v_proveedor_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, score, presupuesto, cobertura, notas, created_at, updated_at)
        VALUES (v_proveedor_id, 'Cartagena Sound Pro',
            (SELECT id FROM paises WHERE nombre = 'Colombia'),
            (SELECT r.id FROM regiones r JOIN paises pa ON r.pais_id = pa.id WHERE pa.nombre = 'Colombia' AND r.nombre = 'Bolívar'),
            (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises pa ON r.pais_id = pa.id WHERE pa.nombre = 'Colombia' AND r.nombre = 'Bolívar' AND ci.nombre = 'Cartagena'),
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Iluminación y sonido'),
            'Activo', 'Iván Herrera', NULL, 'ivan@ctgsound.co', 4, '$$ Medio (20k–100k)', 'Regional', 'Excelentes para eventos en la costa. Conocen bien los venues del centro histórico.', now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'Colombia'),
            region_id = (SELECT r.id FROM regiones r JOIN paises pa ON r.pais_id = pa.id WHERE pa.nombre = 'Colombia' AND r.nombre = 'Bolívar'),
            ciudad_id = (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises pa ON r.pais_id = pa.id WHERE pa.nombre = 'Colombia' AND r.nombre = 'Bolívar' AND ci.nombre = 'Cartagena'),
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Iluminación y sonido'),
            estado = 'Activo',
            contacto = 'Iván Herrera',
            email = 'ivan@ctgsound.co',
            score = 4,
            presupuesto = '$$ Medio (20k–100k)',
            cobertura = 'Regional',
            notas = 'Excelentes para eventos en la costa. Conocen bien los venues del centro histórico.',
            updated_at = now()
        WHERE id = v_proveedor_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_proveedor_id, '+57 312 555 7788'
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_proveedor_id AND telefono = '+57 312 555 7788');

    INSERT INTO servicios (id, nombre, created_at) VALUES (gen_random_uuid(), 'Sonido line array', now()) ON CONFLICT (nombre) DO NOTHING;
    INSERT INTO proveedor_servicios (proveedor_id, servicio_id)
    SELECT v_proveedor_id, id FROM servicios WHERE nombre = 'Sonido line array'
    ON CONFLICT (proveedor_id, servicio_id) DO NOTHING;
    INSERT INTO servicios (id, nombre, created_at) VALUES (gen_random_uuid(), 'iluminación eventos', now()) ON CONFLICT (nombre) DO NOTHING;
    INSERT INTO proveedor_servicios (proveedor_id, servicio_id)
    SELECT v_proveedor_id, id FROM servicios WHERE nombre = 'iluminación eventos'
    ON CONFLICT (proveedor_id, servicio_id) DO NOTHING;
    INSERT INTO servicios (id, nombre, created_at) VALUES (gen_random_uuid(), 'DJ técnico', now()) ON CONFLICT (nombre) DO NOTHING;
    INSERT INTO proveedor_servicios (proveedor_id, servicio_id)
    SELECT v_proveedor_id, id FROM servicios WHERE nombre = 'DJ técnico'
    ON CONFLICT (proveedor_id, servicio_id) DO NOTHING;

END $blk$;

-- Proveedor: Barranquilla Produce
DO $blk$
DECLARE v_proveedor_id uuid;
BEGIN
    SELECT id INTO v_proveedor_id FROM proveedores WHERE nombre = 'Barranquilla Produce' LIMIT 1;
    IF v_proveedor_id IS NULL THEN
        v_proveedor_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, score, presupuesto, cobertura, notas, created_at, updated_at)
        VALUES (v_proveedor_id, 'Barranquilla Produce',
            (SELECT id FROM paises WHERE nombre = 'Colombia'),
            (SELECT r.id FROM regiones r JOIN paises pa ON r.pais_id = pa.id WHERE pa.nombre = 'Colombia' AND r.nombre = 'Atlántico'),
            (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises pa ON r.pais_id = pa.id WHERE pa.nombre = 'Colombia' AND r.nombre = 'Atlántico' AND ci.nombre = 'Barranquilla'),
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Producción audiovisual'),
            'Activo', 'Claudia Rueda', NULL, 'claudia@bqproduce.co', 4, '$$ Medio (20k–100k)', 'Regional', 'Muy activos durante el Carnaval. Tienen contactos con todos los venues de Barranquilla.', now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'Colombia'),
            region_id = (SELECT r.id FROM regiones r JOIN paises pa ON r.pais_id = pa.id WHERE pa.nombre = 'Colombia' AND r.nombre = 'Atlántico'),
            ciudad_id = (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises pa ON r.pais_id = pa.id WHERE pa.nombre = 'Colombia' AND r.nombre = 'Atlántico' AND ci.nombre = 'Barranquilla'),
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Producción audiovisual'),
            estado = 'Activo',
            contacto = 'Claudia Rueda',
            email = 'claudia@bqproduce.co',
            score = 4,
            presupuesto = '$$ Medio (20k–100k)',
            cobertura = 'Regional',
            notas = 'Muy activos durante el Carnaval. Tienen contactos con todos los venues de Barranquilla.',
            updated_at = now()
        WHERE id = v_proveedor_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_proveedor_id, '+57 300 888 4412'
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_proveedor_id AND telefono = '+57 300 888 4412');

    INSERT INTO servicios (id, nombre, created_at) VALUES (gen_random_uuid(), 'Producción audiovisual', now()) ON CONFLICT (nombre) DO NOTHING;
    INSERT INTO proveedor_servicios (proveedor_id, servicio_id)
    SELECT v_proveedor_id, id FROM servicios WHERE nombre = 'Producción audiovisual'
    ON CONFLICT (proveedor_id, servicio_id) DO NOTHING;
    INSERT INTO servicios (id, nombre, created_at) VALUES (gen_random_uuid(), 'streaming', now()) ON CONFLICT (nombre) DO NOTHING;
    INSERT INTO proveedor_servicios (proveedor_id, servicio_id)
    SELECT v_proveedor_id, id FROM servicios WHERE nombre = 'streaming'
    ON CONFLICT (proveedor_id, servicio_id) DO NOTHING;
    INSERT INTO servicios (id, nombre, created_at) VALUES (gen_random_uuid(), 'LED wall', now()) ON CONFLICT (nombre) DO NOTHING;
    INSERT INTO proveedor_servicios (proveedor_id, servicio_id)
    SELECT v_proveedor_id, id FROM servicios WHERE nombre = 'LED wall'
    ON CONFLICT (proveedor_id, servicio_id) DO NOTHING;
    INSERT INTO servicios (id, nombre, created_at) VALUES (gen_random_uuid(), 'operación técnica', now()) ON CONFLICT (nombre) DO NOTHING;
    INSERT INTO proveedor_servicios (proveedor_id, servicio_id)
    SELECT v_proveedor_id, id FROM servicios WHERE nombre = 'operación técnica'
    ON CONFLICT (proveedor_id, servicio_id) DO NOTHING;

END $blk$;

-- Proveedor: LA Event Tech
DO $blk$
DECLARE v_proveedor_id uuid;
BEGIN
    SELECT id INTO v_proveedor_id FROM proveedores WHERE nombre = 'LA Event Tech' LIMIT 1;
    IF v_proveedor_id IS NULL THEN
        v_proveedor_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, score, presupuesto, cobertura, notas, created_at, updated_at)
        VALUES (v_proveedor_id, 'LA Event Tech',
            (SELECT id FROM paises WHERE nombre = 'Estados Unidos'),
            (SELECT r.id FROM regiones r JOIN paises pa ON r.pais_id = pa.id WHERE pa.nombre = 'Estados Unidos' AND r.nombre = 'California'),
            (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises pa ON r.pais_id = pa.id WHERE pa.nombre = 'Estados Unidos' AND r.nombre = 'California' AND ci.nombre = 'Los Angeles'),
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Tecnología e interactividad'),
            'Activo', 'Mark Sullivan', NULL, 'mark@laeventtech.com', 4, '$$$$ Premium (>500k)', 'Internacional', 'Para proyectos binacionales con presencia en LA. Hablan español.', now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'Estados Unidos'),
            region_id = (SELECT r.id FROM regiones r JOIN paises pa ON r.pais_id = pa.id WHERE pa.nombre = 'Estados Unidos' AND r.nombre = 'California'),
            ciudad_id = (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises pa ON r.pais_id = pa.id WHERE pa.nombre = 'Estados Unidos' AND r.nombre = 'California' AND ci.nombre = 'Los Angeles'),
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Tecnología e interactividad'),
            estado = 'Activo',
            contacto = 'Mark Sullivan',
            email = 'mark@laeventtech.com',
            score = 4,
            presupuesto = '$$$$ Premium (>500k)',
            cobertura = 'Internacional',
            notas = 'Para proyectos binacionales con presencia en LA. Hablan español.',
            updated_at = now()
        WHERE id = v_proveedor_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_proveedor_id, '+1 310 444 9988'
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_proveedor_id AND telefono = '+1 310 444 9988');

    INSERT INTO servicios (id, nombre, created_at) VALUES (gen_random_uuid(), 'AR/VR experiencial', now()) ON CONFLICT (nombre) DO NOTHING;
    INSERT INTO proveedor_servicios (proveedor_id, servicio_id)
    SELECT v_proveedor_id, id FROM servicios WHERE nombre = 'AR/VR experiencial'
    ON CONFLICT (proveedor_id, servicio_id) DO NOTHING;
    INSERT INTO servicios (id, nombre, created_at) VALUES (gen_random_uuid(), 'pantallas holográficas', now()) ON CONFLICT (nombre) DO NOTHING;
    INSERT INTO proveedor_servicios (proveedor_id, servicio_id)
    SELECT v_proveedor_id, id FROM servicios WHERE nombre = 'pantallas holográficas'
    ON CONFLICT (proveedor_id, servicio_id) DO NOTHING;
    INSERT INTO servicios (id, nombre, created_at) VALUES (gen_random_uuid(), 'mapping avanzado', now()) ON CONFLICT (nombre) DO NOTHING;
    INSERT INTO proveedor_servicios (proveedor_id, servicio_id)
    SELECT v_proveedor_id, id FROM servicios WHERE nombre = 'mapping avanzado'
    ON CONFLICT (proveedor_id, servicio_id) DO NOTHING;

END $blk$;

-- Proveedor: SegurEvent
DO $blk$
DECLARE v_proveedor_id uuid;
BEGIN
    SELECT id INTO v_proveedor_id FROM proveedores WHERE nombre = 'SegurEvent' LIMIT 1;
    IF v_proveedor_id IS NULL THEN
        v_proveedor_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, score, presupuesto, cobertura, notas, created_at, updated_at)
        VALUES (v_proveedor_id, 'SegurEvent',
            (SELECT id FROM paises WHERE nombre = 'México'),
            (SELECT r.id FROM regiones r JOIN paises pa ON r.pais_id = pa.id WHERE pa.nombre = 'México' AND r.nombre = 'Ciudad de México'),
            (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises pa ON r.pais_id = pa.id WHERE pa.nombre = 'México' AND r.nombre = 'Ciudad de México' AND ci.nombre = 'Cuauhtémoc'),
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Seguridad y protocolo'),
            'Bloqueado', 'Bernardo Leal', NULL, 'bleal@segurevent.mx', 2, '$ Bajo (<20k)', 'Solo ciudad', 'BLOQUEADO. Falla en Evento Farma 2024: 2 guardias no se presentaron.', now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'México'),
            region_id = (SELECT r.id FROM regiones r JOIN paises pa ON r.pais_id = pa.id WHERE pa.nombre = 'México' AND r.nombre = 'Ciudad de México'),
            ciudad_id = (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises pa ON r.pais_id = pa.id WHERE pa.nombre = 'México' AND r.nombre = 'Ciudad de México' AND ci.nombre = 'Cuauhtémoc'),
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Seguridad y protocolo'),
            estado = 'Bloqueado',
            contacto = 'Bernardo Leal',
            email = 'bleal@segurevent.mx',
            score = 2,
            presupuesto = '$ Bajo (<20k)',
            cobertura = 'Solo ciudad',
            notas = 'BLOQUEADO. Falla en Evento Farma 2024: 2 guardias no se presentaron.',
            updated_at = now()
        WHERE id = v_proveedor_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_proveedor_id, '+52 55 4400 5566'
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_proveedor_id AND telefono = '+52 55 4400 5566');

    INSERT INTO servicios (id, nombre, created_at) VALUES (gen_random_uuid(), 'Control de acceso', now()) ON CONFLICT (nombre) DO NOTHING;
    INSERT INTO proveedor_servicios (proveedor_id, servicio_id)
    SELECT v_proveedor_id, id FROM servicios WHERE nombre = 'Control de acceso'
    ON CONFLICT (proveedor_id, servicio_id) DO NOTHING;
    INSERT INTO servicios (id, nombre, created_at) VALUES (gen_random_uuid(), 'guardias', now()) ON CONFLICT (nombre) DO NOTHING;
    INSERT INTO proveedor_servicios (proveedor_id, servicio_id)
    SELECT v_proveedor_id, id FROM servicios WHERE nombre = 'guardias'
    ON CONFLICT (proveedor_id, servicio_id) DO NOTHING;
    INSERT INTO servicios (id, nombre, created_at) VALUES (gen_random_uuid(), 'protocolo de emergencias', now()) ON CONFLICT (nombre) DO NOTHING;
    INSERT INTO proveedor_servicios (proveedor_id, servicio_id)
    SELECT v_proveedor_id, id FROM servicios WHERE nombre = 'protocolo de emergencias'
    ON CONFLICT (proveedor_id, servicio_id) DO NOTHING;

    INSERT INTO proveedor_adjuntos (id, proveedor_id, tipo, nombre, url, meta, fecha, created_at)
    SELECT gen_random_uuid(), v_proveedor_id, 'link', 'Reporte de incidente Farma 2024', 'https://docs.google.com', 'docs.google.com', '2024-05-18'::timestamptz, now()
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_adjuntos WHERE proveedor_id = v_proveedor_id AND nombre = 'Reporte de incidente Farma 2024');
END $blk$;

-- Proveedor: Terraza Río Cali
DO $blk$
DECLARE v_proveedor_id uuid;
BEGIN
    SELECT id INTO v_proveedor_id FROM proveedores WHERE nombre = 'Terraza Río Cali' LIMIT 1;
    IF v_proveedor_id IS NULL THEN
        v_proveedor_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, score, presupuesto, cobertura, notas, created_at, updated_at)
        VALUES (v_proveedor_id, 'Terraza Río Cali',
            (SELECT id FROM paises WHERE nombre = 'Colombia'),
            (SELECT r.id FROM regiones r JOIN paises pa ON r.pais_id = pa.id WHERE pa.nombre = 'Colombia' AND r.nombre = 'Valle del Cauca'),
            (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises pa ON r.pais_id = pa.id WHERE pa.nombre = 'Colombia' AND r.nombre = 'Valle del Cauca' AND ci.nombre = 'Cali'),
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Rooftop'),
            'Activo', 'Marcela Uribe', NULL, 'marcela@terrazario.co', 4, '$$ Medio (20k–100k)', 'Solo ciudad', 'Vista a la ciudad. Ideal para lanzamientos pequeños.', now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'Colombia'),
            region_id = (SELECT r.id FROM regiones r JOIN paises pa ON r.pais_id = pa.id WHERE pa.nombre = 'Colombia' AND r.nombre = 'Valle del Cauca'),
            ciudad_id = (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises pa ON r.pais_id = pa.id WHERE pa.nombre = 'Colombia' AND r.nombre = 'Valle del Cauca' AND ci.nombre = 'Cali'),
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Rooftop'),
            estado = 'Activo',
            contacto = 'Marcela Uribe',
            email = 'marcela@terrazario.co',
            score = 4,
            presupuesto = '$$ Medio (20k–100k)',
            cobertura = 'Solo ciudad',
            notas = 'Vista a la ciudad. Ideal para lanzamientos pequeños.',
            updated_at = now()
        WHERE id = v_proveedor_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_proveedor_id, '+57 315 220 4477'
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_proveedor_id AND telefono = '+57 315 220 4477');

    INSERT INTO servicios (id, nombre, created_at) VALUES (gen_random_uuid(), 'Espacio rooftop para hasta 150 personas', now()) ON CONFLICT (nombre) DO NOTHING;
    INSERT INTO proveedor_servicios (proveedor_id, servicio_id)
    SELECT v_proveedor_id, id FROM servicios WHERE nombre = 'Espacio rooftop para hasta 150 personas'
    ON CONFLICT (proveedor_id, servicio_id) DO NOTHING;
    INSERT INTO servicios (id, nombre, created_at) VALUES (gen_random_uuid(), 'bar incluido', now()) ON CONFLICT (nombre) DO NOTHING;
    INSERT INTO proveedor_servicios (proveedor_id, servicio_id)
    SELECT v_proveedor_id, id FROM servicios WHERE nombre = 'bar incluido'
    ON CONFLICT (proveedor_id, servicio_id) DO NOTHING;

END $blk$;

-- Proveedor: Hotel Intercontinental Cali
DO $blk$
DECLARE v_proveedor_id uuid;
BEGIN
    SELECT id INTO v_proveedor_id FROM proveedores WHERE nombre = 'Hotel Intercontinental Cali' LIMIT 1;
    IF v_proveedor_id IS NULL THEN
        v_proveedor_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, score, presupuesto, cobertura, notas, created_at, updated_at)
        VALUES (v_proveedor_id, 'Hotel Intercontinental Cali',
            (SELECT id FROM paises WHERE nombre = 'Colombia'),
            (SELECT r.id FROM regiones r JOIN paises pa ON r.pais_id = pa.id WHERE pa.nombre = 'Colombia' AND r.nombre = 'Valle del Cauca'),
            (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises pa ON r.pais_id = pa.id WHERE pa.nombre = 'Colombia' AND r.nombre = 'Valle del Cauca' AND ci.nombre = 'Cali'),
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Hotel'),
            'Activo', 'Jorge Salcedo', NULL, 'eventos@intercali.com', 5, '$$$ Alto (100k–500k)', 'Solo ciudad', 'El más solicitado por clientes corporativos grandes.', now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'Colombia'),
            region_id = (SELECT r.id FROM regiones r JOIN paises pa ON r.pais_id = pa.id WHERE pa.nombre = 'Colombia' AND r.nombre = 'Valle del Cauca'),
            ciudad_id = (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises pa ON r.pais_id = pa.id WHERE pa.nombre = 'Colombia' AND r.nombre = 'Valle del Cauca' AND ci.nombre = 'Cali'),
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Hotel'),
            estado = 'Activo',
            contacto = 'Jorge Salcedo',
            email = 'eventos@intercali.com',
            score = 5,
            presupuesto = '$$$ Alto (100k–500k)',
            cobertura = 'Solo ciudad',
            notas = 'El más solicitado por clientes corporativos grandes.',
            updated_at = now()
        WHERE id = v_proveedor_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_proveedor_id, '+57 2 485 0000'
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_proveedor_id AND telefono = '+57 2 485 0000');

    INSERT INTO servicios (id, nombre, created_at) VALUES (gen_random_uuid(), 'Salones para hasta 500 personas', now()) ON CONFLICT (nombre) DO NOTHING;
    INSERT INTO proveedor_servicios (proveedor_id, servicio_id)
    SELECT v_proveedor_id, id FROM servicios WHERE nombre = 'Salones para hasta 500 personas'
    ON CONFLICT (proveedor_id, servicio_id) DO NOTHING;
    INSERT INTO servicios (id, nombre, created_at) VALUES (gen_random_uuid(), 'catering propio', now()) ON CONFLICT (nombre) DO NOTHING;
    INSERT INTO proveedor_servicios (proveedor_id, servicio_id)
    SELECT v_proveedor_id, id FROM servicios WHERE nombre = 'catering propio'
    ON CONFLICT (proveedor_id, servicio_id) DO NOTHING;
    INSERT INTO servicios (id, nombre, created_at) VALUES (gen_random_uuid(), 'hospedaje', now()) ON CONFLICT (nombre) DO NOTHING;
    INSERT INTO proveedor_servicios (proveedor_id, servicio_id)
    SELECT v_proveedor_id, id FROM servicios WHERE nombre = 'hospedaje'
    ON CONFLICT (proveedor_id, servicio_id) DO NOTHING;

END $blk$;

-- Proveedor: La Central Gastrobar
DO $blk$
DECLARE v_proveedor_id uuid;
BEGIN
    SELECT id INTO v_proveedor_id FROM proveedores WHERE nombre = 'La Central Gastrobar' LIMIT 1;
    IF v_proveedor_id IS NULL THEN
        v_proveedor_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, score, presupuesto, cobertura, notas, created_at, updated_at)
        VALUES (v_proveedor_id, 'La Central Gastrobar',
            (SELECT id FROM paises WHERE nombre = 'Colombia'),
            (SELECT r.id FROM regiones r JOIN paises pa ON r.pais_id = pa.id WHERE pa.nombre = 'Colombia' AND r.nombre = 'Valle del Cauca'),
            (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises pa ON r.pais_id = pa.id WHERE pa.nombre = 'Colombia' AND r.nombre = 'Valle del Cauca' AND ci.nombre = 'Cali'),
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Gastro bar'),
            'En evaluación', 'Natalia Ospina', NULL, 'eventos@lacentral.co', 3, '$ Bajo (<20k)', 'Solo ciudad', 'Aún sin contrato formal. Pendiente evaluación de capacidad.', now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'Colombia'),
            region_id = (SELECT r.id FROM regiones r JOIN paises pa ON r.pais_id = pa.id WHERE pa.nombre = 'Colombia' AND r.nombre = 'Valle del Cauca'),
            ciudad_id = (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises pa ON r.pais_id = pa.id WHERE pa.nombre = 'Colombia' AND r.nombre = 'Valle del Cauca' AND ci.nombre = 'Cali'),
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Gastro bar'),
            estado = 'En evaluación',
            contacto = 'Natalia Ospina',
            email = 'eventos@lacentral.co',
            score = 3,
            presupuesto = '$ Bajo (<20k)',
            cobertura = 'Solo ciudad',
            notas = 'Aún sin contrato formal. Pendiente evaluación de capacidad.',
            updated_at = now()
        WHERE id = v_proveedor_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_proveedor_id, '+57 300 555 1122'
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_proveedor_id AND telefono = '+57 300 555 1122');

    INSERT INTO servicios (id, nombre, created_at) VALUES (gen_random_uuid(), 'Espacio para activaciones pequeñas', now()) ON CONFLICT (nombre) DO NOTHING;
    INSERT INTO proveedor_servicios (proveedor_id, servicio_id)
    SELECT v_proveedor_id, id FROM servicios WHERE nombre = 'Espacio para activaciones pequeñas'
    ON CONFLICT (proveedor_id, servicio_id) DO NOTHING;
    INSERT INTO servicios (id, nombre, created_at) VALUES (gen_random_uuid(), 'DJ', now()) ON CONFLICT (nombre) DO NOTHING;
    INSERT INTO proveedor_servicios (proveedor_id, servicio_id)
    SELECT v_proveedor_id, id FROM servicios WHERE nombre = 'DJ'
    ON CONFLICT (proveedor_id, servicio_id) DO NOTHING;
    INSERT INTO servicios (id, nombre, created_at) VALUES (gen_random_uuid(), 'coctelería', now()) ON CONFLICT (nombre) DO NOTHING;
    INSERT INTO proveedor_servicios (proveedor_id, servicio_id)
    SELECT v_proveedor_id, id FROM servicios WHERE nombre = 'coctelería'
    ON CONFLICT (proveedor_id, servicio_id) DO NOTHING;

END $blk$;

-- Proveedor: Talento BTL Bogotá
DO $blk$
DECLARE v_proveedor_id uuid;
BEGIN
    SELECT id INTO v_proveedor_id FROM proveedores WHERE nombre = 'Talento BTL Bogotá' LIMIT 1;
    IF v_proveedor_id IS NULL THEN
        v_proveedor_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, score, presupuesto, cobertura, notas, created_at, updated_at)
        VALUES (v_proveedor_id, 'Talento BTL Bogotá',
            (SELECT id FROM paises WHERE nombre = 'Colombia'),
            (SELECT r.id FROM regiones r JOIN paises pa ON r.pais_id = pa.id WHERE pa.nombre = 'Colombia' AND r.nombre = 'Bogotá D.C.'),
            (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises pa ON r.pais_id = pa.id WHERE pa.nombre = 'Colombia' AND r.nombre = 'Bogotá D.C.' AND ci.nombre = 'Bogotá'),
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Personal BTL (impulsadoras, edecanes, meseros)'),
            'Activo', 'Diana Castañeda', NULL, 'diana@talentobtl.co', 5, '$$ Medio (20k–100k)', 'Nacional', 'Personal muy bien entrenado. Piden mínimo 5 días de anticipación.', now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'Colombia'),
            region_id = (SELECT r.id FROM regiones r JOIN paises pa ON r.pais_id = pa.id WHERE pa.nombre = 'Colombia' AND r.nombre = 'Bogotá D.C.'),
            ciudad_id = (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises pa ON r.pais_id = pa.id WHERE pa.nombre = 'Colombia' AND r.nombre = 'Bogotá D.C.' AND ci.nombre = 'Bogotá'),
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Personal BTL (impulsadoras, edecanes, meseros)'),
            estado = 'Activo',
            contacto = 'Diana Castañeda',
            email = 'diana@talentobtl.co',
            score = 5,
            presupuesto = '$$ Medio (20k–100k)',
            cobertura = 'Nacional',
            notas = 'Personal muy bien entrenado. Piden mínimo 5 días de anticipación.',
            updated_at = now()
        WHERE id = v_proveedor_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_proveedor_id, '+57 311 400 2255'
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_proveedor_id AND telefono = '+57 311 400 2255');

    INSERT INTO servicios (id, nombre, created_at) VALUES (gen_random_uuid(), 'Impulsadoras', now()) ON CONFLICT (nombre) DO NOTHING;
    INSERT INTO proveedor_servicios (proveedor_id, servicio_id)
    SELECT v_proveedor_id, id FROM servicios WHERE nombre = 'Impulsadoras'
    ON CONFLICT (proveedor_id, servicio_id) DO NOTHING;
    INSERT INTO servicios (id, nombre, created_at) VALUES (gen_random_uuid(), 'edecanes bilingües', now()) ON CONFLICT (nombre) DO NOTHING;
    INSERT INTO proveedor_servicios (proveedor_id, servicio_id)
    SELECT v_proveedor_id, id FROM servicios WHERE nombre = 'edecanes bilingües'
    ON CONFLICT (proveedor_id, servicio_id) DO NOTHING;
    INSERT INTO servicios (id, nombre, created_at) VALUES (gen_random_uuid(), 'meseros', now()) ON CONFLICT (nombre) DO NOTHING;
    INSERT INTO proveedor_servicios (proveedor_id, servicio_id)
    SELECT v_proveedor_id, id FROM servicios WHERE nombre = 'meseros'
    ON CONFLICT (proveedor_id, servicio_id) DO NOTHING;
    INSERT INTO servicios (id, nombre, created_at) VALUES (gen_random_uuid(), 'coordinadores de piso', now()) ON CONFLICT (nombre) DO NOTHING;
    INSERT INTO proveedor_servicios (proveedor_id, servicio_id)
    SELECT v_proveedor_id, id FROM servicios WHERE nombre = 'coordinadores de piso'
    ON CONFLICT (proveedor_id, servicio_id) DO NOTHING;

END $blk$;

-- Proveedor: Promo Regalos Corporativos
DO $blk$
DECLARE v_proveedor_id uuid;
BEGIN
    SELECT id INTO v_proveedor_id FROM proveedores WHERE nombre = 'Promo Regalos Corporativos' LIMIT 1;
    IF v_proveedor_id IS NULL THEN
        v_proveedor_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, score, presupuesto, cobertura, notas, created_at, updated_at)
        VALUES (v_proveedor_id, 'Promo Regalos Corporativos',
            (SELECT id FROM paises WHERE nombre = 'Colombia'),
            (SELECT r.id FROM regiones r JOIN paises pa ON r.pais_id = pa.id WHERE pa.nombre = 'Colombia' AND r.nombre = 'Bogotá D.C.'),
            (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises pa ON r.pais_id = pa.id WHERE pa.nombre = 'Colombia' AND r.nombre = 'Bogotá D.C.' AND ci.nombre = 'Bogotá'),
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Merchandising y regalos corporativos'),
            'Activo', 'Felipe Angulo', NULL, 'felipe@promoregalos.co', 4, '$$ Medio (20k–100k)', 'Nacional', 'Buen precio en volumen. Tiempo de producción: 2 semanas.', now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'Colombia'),
            region_id = (SELECT r.id FROM regiones r JOIN paises pa ON r.pais_id = pa.id WHERE pa.nombre = 'Colombia' AND r.nombre = 'Bogotá D.C.'),
            ciudad_id = (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises pa ON r.pais_id = pa.id WHERE pa.nombre = 'Colombia' AND r.nombre = 'Bogotá D.C.' AND ci.nombre = 'Bogotá'),
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Merchandising y regalos corporativos'),
            estado = 'Activo',
            contacto = 'Felipe Angulo',
            email = 'felipe@promoregalos.co',
            score = 4,
            presupuesto = '$$ Medio (20k–100k)',
            cobertura = 'Nacional',
            notas = 'Buen precio en volumen. Tiempo de producción: 2 semanas.',
            updated_at = now()
        WHERE id = v_proveedor_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_proveedor_id, '+57 320 660 9911'
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_proveedor_id AND telefono = '+57 320 660 9911');

    INSERT INTO servicios (id, nombre, created_at) VALUES (gen_random_uuid(), 'Merchandising personalizado', now()) ON CONFLICT (nombre) DO NOTHING;
    INSERT INTO proveedor_servicios (proveedor_id, servicio_id)
    SELECT v_proveedor_id, id FROM servicios WHERE nombre = 'Merchandising personalizado'
    ON CONFLICT (proveedor_id, servicio_id) DO NOTHING;
    INSERT INTO servicios (id, nombre, created_at) VALUES (gen_random_uuid(), 'kits de bienvenida', now()) ON CONFLICT (nombre) DO NOTHING;
    INSERT INTO proveedor_servicios (proveedor_id, servicio_id)
    SELECT v_proveedor_id, id FROM servicios WHERE nombre = 'kits de bienvenida'
    ON CONFLICT (proveedor_id, servicio_id) DO NOTHING;
    INSERT INTO servicios (id, nombre, created_at) VALUES (gen_random_uuid(), 'textiles bordados', now()) ON CONFLICT (nombre) DO NOTHING;
    INSERT INTO proveedor_servicios (proveedor_id, servicio_id)
    SELECT v_proveedor_id, id FROM servicios WHERE nombre = 'textiles bordados'
    ON CONFLICT (proveedor_id, servicio_id) DO NOTHING;

END $blk$;

-- Proveedor: RigStage Producción Técnica
DO $blk$
DECLARE v_proveedor_id uuid;
BEGIN
    SELECT id INTO v_proveedor_id FROM proveedores WHERE nombre = 'RigStage Producción Técnica' LIMIT 1;
    IF v_proveedor_id IS NULL THEN
        v_proveedor_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, score, presupuesto, cobertura, notas, created_at, updated_at)
        VALUES (v_proveedor_id, 'RigStage Producción Técnica',
            (SELECT id FROM paises WHERE nombre = 'Colombia'),
            (SELECT r.id FROM regiones r JOIN paises pa ON r.pais_id = pa.id WHERE pa.nombre = 'Colombia' AND r.nombre = 'Antioquia'),
            (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises pa ON r.pais_id = pa.id WHERE pa.nombre = 'Colombia' AND r.nombre = 'Antioquia' AND ci.nombre = 'Medellín'),
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Producción técnica (rigging, generadores, carpas)'),
            'Activo', 'Camilo Restrepo', NULL, 'camilo@rigstage.co', 5, '$$$ Alto (100k–500k)', 'Nacional', 'Los más confiables para eventos masivos al aire libre.', now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'Colombia'),
            region_id = (SELECT r.id FROM regiones r JOIN paises pa ON r.pais_id = pa.id WHERE pa.nombre = 'Colombia' AND r.nombre = 'Antioquia'),
            ciudad_id = (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises pa ON r.pais_id = pa.id WHERE pa.nombre = 'Colombia' AND r.nombre = 'Antioquia' AND ci.nombre = 'Medellín'),
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Producción técnica (rigging, generadores, carpas)'),
            estado = 'Activo',
            contacto = 'Camilo Restrepo',
            email = 'camilo@rigstage.co',
            score = 5,
            presupuesto = '$$$ Alto (100k–500k)',
            cobertura = 'Nacional',
            notas = 'Los más confiables para eventos masivos al aire libre.',
            updated_at = now()
        WHERE id = v_proveedor_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_proveedor_id, '+57 314 778 3300'
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_proveedor_id AND telefono = '+57 314 778 3300');

    INSERT INTO servicios (id, nombre, created_at) VALUES (gen_random_uuid(), 'Rigging', now()) ON CONFLICT (nombre) DO NOTHING;
    INSERT INTO proveedor_servicios (proveedor_id, servicio_id)
    SELECT v_proveedor_id, id FROM servicios WHERE nombre = 'Rigging'
    ON CONFLICT (proveedor_id, servicio_id) DO NOTHING;
    INSERT INTO servicios (id, nombre, created_at) VALUES (gen_random_uuid(), 'estructuras truss', now()) ON CONFLICT (nombre) DO NOTHING;
    INSERT INTO proveedor_servicios (proveedor_id, servicio_id)
    SELECT v_proveedor_id, id FROM servicios WHERE nombre = 'estructuras truss'
    ON CONFLICT (proveedor_id, servicio_id) DO NOTHING;
    INSERT INTO servicios (id, nombre, created_at) VALUES (gen_random_uuid(), 'generadores eléctricos', now()) ON CONFLICT (nombre) DO NOTHING;
    INSERT INTO proveedor_servicios (proveedor_id, servicio_id)
    SELECT v_proveedor_id, id FROM servicios WHERE nombre = 'generadores eléctricos'
    ON CONFLICT (proveedor_id, servicio_id) DO NOTHING;
    INSERT INTO servicios (id, nombre, created_at) VALUES (gen_random_uuid(), 'carpas industriales', now()) ON CONFLICT (nombre) DO NOTHING;
    INSERT INTO proveedor_servicios (proveedor_id, servicio_id)
    SELECT v_proveedor_id, id FROM servicios WHERE nombre = 'carpas industriales'
    ON CONFLICT (proveedor_id, servicio_id) DO NOTHING;

END $blk$;

-- ============================== PROYECTOS ==============================
-- Proyecto: Lanzamiento Vitalis (cliente: Grupo Vitalis)
DO $blk$
DECLARE v_proyecto_id uuid;
BEGIN
    SELECT id INTO v_proyecto_id FROM proyectos WHERE nombre = 'Lanzamiento Vitalis' LIMIT 1;
    IF v_proyecto_id IS NULL THEN
        v_proyecto_id := gen_random_uuid();
        INSERT INTO proyectos (id, nombre, cliente_id, contacto_proyecto, tipo_proyecto, prioridad, ciudad, sede_next, fecha_solicitud, fecha_evento, estado_id, porcentaje_avance, estado_brief, propuesta_estado, numero_factura, pagado, fecha_pago, notas, created_at, updated_at)
        VALUES (v_proyecto_id, 'Lanzamiento Vitalis',
            (SELECT id FROM clientes WHERE nombre = 'Grupo Vitalis' LIMIT 1),
            'Ricardo Gómez', 'Corporativo', 'Alta', 'Ciudad de México', 'Ciudad de México',
            '2026-07-01'::timestamptz, '2026-09-12'::timestamptz,
            (SELECT id FROM estados_proyecto WHERE nombre = 'Confirmado'),
            55, 'Entregado, a espera de respuesta', 'Aprobada', NULL, FALSE, NULL, 'Activación de marca en CDMX, 2 días. Requiere escenografía premium.', now(), now());
    ELSE
        UPDATE proyectos SET
            cliente_id = (SELECT id FROM clientes WHERE nombre = 'Grupo Vitalis' LIMIT 1),
            contacto_proyecto = 'Ricardo Gómez',
            tipo_proyecto = 'Corporativo',
            prioridad = 'Alta',
            ciudad = 'Ciudad de México',
            sede_next = 'Ciudad de México',
            fecha_solicitud = '2026-07-01'::timestamptz,
            fecha_evento = '2026-09-12'::timestamptz,
            estado_id = (SELECT id FROM estados_proyecto WHERE nombre = 'Confirmado'),
            porcentaje_avance = 55,
            estado_brief = 'Entregado, a espera de respuesta',
            propuesta_estado = 'Aprobada',
            numero_factura = NULL,
            pagado = FALSE,
            fecha_pago = NULL,
            notas = 'Activación de marca en CDMX, 2 días. Requiere escenografía premium.',
            updated_at = now()
        WHERE id = v_proyecto_id;
    END IF;

    INSERT INTO proyecto_equipo (id, proyecto_id, rol, nombre)
    SELECT gen_random_uuid(), v_proyecto_id, 'Ejecutivo', 'Viviana Salazar'
    WHERE NOT EXISTS (SELECT 1 FROM proyecto_equipo WHERE proyecto_id = v_proyecto_id AND rol = 'Ejecutivo' AND nombre = 'Viviana Salazar');
    INSERT INTO proyecto_equipo (id, proyecto_id, rol, nombre)
    SELECT gen_random_uuid(), v_proyecto_id, 'Diseñador 3D', 'Carlos Fajardo'
    WHERE NOT EXISTS (SELECT 1 FROM proyecto_equipo WHERE proyecto_id = v_proyecto_id AND rol = 'Diseñador 3D' AND nombre = 'Carlos Fajardo');
    INSERT INTO proyecto_equipo (id, proyecto_id, rol, nombre)
    SELECT gen_random_uuid(), v_proyecto_id, 'Diseñador gráfico', 'Ana Duarte'
    WHERE NOT EXISTS (SELECT 1 FROM proyecto_equipo WHERE proyecto_id = v_proyecto_id AND rol = 'Diseñador gráfico' AND nombre = 'Ana Duarte');

    INSERT INTO proyecto_seguimiento (id, proyecto_id, area, fecha, nota, created_at)
    SELECT gen_random_uuid(), v_proyecto_id, 'General', '2026-07-02'::timestamptz, 'Brief inicial recibido del cliente.', now()
    WHERE NOT EXISTS (SELECT 1 FROM proyecto_seguimiento WHERE proyecto_id = v_proyecto_id AND fecha = '2026-07-02'::timestamptz AND nota = 'Brief inicial recibido del cliente.');
    INSERT INTO proyecto_seguimiento (id, proyecto_id, area, fecha, nota, created_at)
    SELECT gen_random_uuid(), v_proyecto_id, 'Creativo', '2026-08-10'::timestamptz, 'Primer render de escenografía enviado a revisión.', now()
    WHERE NOT EXISTS (SELECT 1 FROM proyecto_seguimiento WHERE proyecto_id = v_proyecto_id AND fecha = '2026-08-10'::timestamptz AND nota = 'Primer render de escenografía enviado a revisión.');

    INSERT INTO proyecto_proveedores (proyecto_id, proveedor_id)
    SELECT v_proyecto_id, id FROM proveedores WHERE nombre = 'Grupo Luminary'
    ON CONFLICT (proyecto_id, proveedor_id) DO NOTHING;
    INSERT INTO proyecto_proveedores (proyecto_id, proveedor_id)
    SELECT v_proyecto_id, id FROM proveedores WHERE nombre = 'Flash Studios'
    ON CONFLICT (proyecto_id, proveedor_id) DO NOTHING;
END $blk$;

-- Proyecto: Feria Comercial Andina (cliente: Cámara de Comercio Bogotá)
DO $blk$
DECLARE v_proyecto_id uuid;
BEGIN
    SELECT id INTO v_proyecto_id FROM proyectos WHERE nombre = 'Feria Comercial Andina' LIMIT 1;
    IF v_proyecto_id IS NULL THEN
        v_proyecto_id := gen_random_uuid();
        INSERT INTO proyectos (id, nombre, cliente_id, contacto_proyecto, tipo_proyecto, prioridad, ciudad, sede_next, fecha_solicitud, fecha_evento, estado_id, porcentaje_avance, estado_brief, propuesta_estado, numero_factura, pagado, fecha_pago, notas, created_at, updated_at)
        VALUES (v_proyecto_id, 'Feria Comercial Andina',
            (SELECT id FROM clientes WHERE nombre = 'Cámara de Comercio Bogotá' LIMIT 1),
            'Paula Méndez', 'Corporativo', 'Alta', 'Bogotá', 'Bogotá',
            '2026-06-01'::timestamptz, '2026-08-20'::timestamptz,
            (SELECT id FROM estados_proyecto WHERE nombre = 'En curso'),
            80, 'Aprobado', 'Aprobada', NULL, FALSE, NULL, 'Stand principal + zona de prensa. Coordinar con seguridad por aforo alto.', now(), now());
    ELSE
        UPDATE proyectos SET
            cliente_id = (SELECT id FROM clientes WHERE nombre = 'Cámara de Comercio Bogotá' LIMIT 1),
            contacto_proyecto = 'Paula Méndez',
            tipo_proyecto = 'Corporativo',
            prioridad = 'Alta',
            ciudad = 'Bogotá',
            sede_next = 'Bogotá',
            fecha_solicitud = '2026-06-01'::timestamptz,
            fecha_evento = '2026-08-20'::timestamptz,
            estado_id = (SELECT id FROM estados_proyecto WHERE nombre = 'En curso'),
            porcentaje_avance = 80,
            estado_brief = 'Aprobado',
            propuesta_estado = 'Aprobada',
            numero_factura = NULL,
            pagado = FALSE,
            fecha_pago = NULL,
            notas = 'Stand principal + zona de prensa. Coordinar con seguridad por aforo alto.',
            updated_at = now()
        WHERE id = v_proyecto_id;
    END IF;

    INSERT INTO proyecto_equipo (id, proyecto_id, rol, nombre)
    SELECT gen_random_uuid(), v_proyecto_id, 'Ejecutivo', 'Viviana Salazar'
    WHERE NOT EXISTS (SELECT 1 FROM proyecto_equipo WHERE proyecto_id = v_proyecto_id AND rol = 'Ejecutivo' AND nombre = 'Viviana Salazar');
    INSERT INTO proyecto_equipo (id, proyecto_id, rol, nombre)
    SELECT gen_random_uuid(), v_proyecto_id, 'Diseñador 3D', 'Carlos Fajardo'
    WHERE NOT EXISTS (SELECT 1 FROM proyecto_equipo WHERE proyecto_id = v_proyecto_id AND rol = 'Diseñador 3D' AND nombre = 'Carlos Fajardo');

    INSERT INTO proyecto_seguimiento (id, proyecto_id, area, fecha, nota, created_at)
    SELECT gen_random_uuid(), v_proyecto_id, 'Comercial', '2026-06-15'::timestamptz, 'Contrato firmado por la Cámara de Comercio.', now()
    WHERE NOT EXISTS (SELECT 1 FROM proyecto_seguimiento WHERE proyecto_id = v_proyecto_id AND fecha = '2026-06-15'::timestamptz AND nota = 'Contrato firmado por la Cámara de Comercio.');

    INSERT INTO proyecto_proveedores (proyecto_id, proveedor_id)
    SELECT v_proyecto_id, id FROM proveedores WHERE nombre = 'Crea Eventos Bogotá'
    ON CONFLICT (proyecto_id, proveedor_id) DO NOTHING;
    INSERT INTO proyecto_proveedores (proyecto_id, proveedor_id)
    SELECT v_proyecto_id, id FROM proveedores WHERE nombre = 'Cartagena Sound Pro'
    ON CONFLICT (proyecto_id, proveedor_id) DO NOTHING;
    INSERT INTO proyecto_proveedores (proyecto_id, proveedor_id)
    SELECT v_proyecto_id, id FROM proveedores WHERE nombre = 'SegurEvent'
    ON CONFLICT (proyecto_id, proveedor_id) DO NOTHING;
END $blk$;

-- Proyecto: Activación Costa Caribe (cliente: Marca Ron Real)
DO $blk$
DECLARE v_proyecto_id uuid;
BEGIN
    SELECT id INTO v_proyecto_id FROM proyectos WHERE nombre = 'Activación Costa Caribe' LIMIT 1;
    IF v_proyecto_id IS NULL THEN
        v_proyecto_id := gen_random_uuid();
        INSERT INTO proyectos (id, nombre, cliente_id, contacto_proyecto, tipo_proyecto, prioridad, ciudad, sede_next, fecha_solicitud, fecha_evento, estado_id, porcentaje_avance, estado_brief, propuesta_estado, numero_factura, pagado, fecha_pago, notas, created_at, updated_at)
        VALUES (v_proyecto_id, 'Activación Costa Caribe',
            (SELECT id FROM clientes WHERE nombre = 'Marca Ron Real' LIMIT 1),
            NULL, 'Evento social', 'Media', 'Cartagena', 'Bogotá',
            '2026-08-01'::timestamptz, '2026-11-02'::timestamptz,
            (SELECT id FROM estados_proyecto WHERE nombre = 'Planeación interna'),
            15, 'Pendiente por enviar', 'No enviada', NULL, FALSE, NULL, 'Evento itinerante entre Cartagena y Barranquilla.', now(), now());
    ELSE
        UPDATE proyectos SET
            cliente_id = (SELECT id FROM clientes WHERE nombre = 'Marca Ron Real' LIMIT 1),
            contacto_proyecto = NULL,
            tipo_proyecto = 'Evento social',
            prioridad = 'Media',
            ciudad = 'Cartagena',
            sede_next = 'Bogotá',
            fecha_solicitud = '2026-08-01'::timestamptz,
            fecha_evento = '2026-11-02'::timestamptz,
            estado_id = (SELECT id FROM estados_proyecto WHERE nombre = 'Planeación interna'),
            porcentaje_avance = 15,
            estado_brief = 'Pendiente por enviar',
            propuesta_estado = 'No enviada',
            numero_factura = NULL,
            pagado = FALSE,
            fecha_pago = NULL,
            notas = 'Evento itinerante entre Cartagena y Barranquilla.',
            updated_at = now()
        WHERE id = v_proyecto_id;
    END IF;

    INSERT INTO proyecto_equipo (id, proyecto_id, rol, nombre)
    SELECT gen_random_uuid(), v_proyecto_id, 'Ejecutivo', 'Viviana Salazar'
    WHERE NOT EXISTS (SELECT 1 FROM proyecto_equipo WHERE proyecto_id = v_proyecto_id AND rol = 'Ejecutivo' AND nombre = 'Viviana Salazar');
    INSERT INTO proyecto_equipo (id, proyecto_id, rol, nombre)
    SELECT gen_random_uuid(), v_proyecto_id, 'Diseñador gráfico', 'Ana Duarte'
    WHERE NOT EXISTS (SELECT 1 FROM proyecto_equipo WHERE proyecto_id = v_proyecto_id AND rol = 'Diseñador gráfico' AND nombre = 'Ana Duarte');

    INSERT INTO proyecto_proveedores (proyecto_id, proveedor_id)
    SELECT v_proyecto_id, id FROM proveedores WHERE nombre = 'Cartagena Sound Pro'
    ON CONFLICT (proyecto_id, proveedor_id) DO NOTHING;
    INSERT INTO proyecto_proveedores (proyecto_id, proveedor_id)
    SELECT v_proyecto_id, id FROM proveedores WHERE nombre = 'Barranquilla Produce'
    ON CONFLICT (proyecto_id, proveedor_id) DO NOTHING;
END $blk$;

-- Proyecto: Rueda de Prensa Nova (cliente: Nova Cosméticos)
DO $blk$
DECLARE v_proyecto_id uuid;
BEGIN
    SELECT id INTO v_proyecto_id FROM proyectos WHERE nombre = 'Rueda de Prensa Nova' LIMIT 1;
    IF v_proyecto_id IS NULL THEN
        v_proyecto_id := gen_random_uuid();
        INSERT INTO proyectos (id, nombre, cliente_id, contacto_proyecto, tipo_proyecto, prioridad, ciudad, sede_next, fecha_solicitud, fecha_evento, estado_id, porcentaje_avance, estado_brief, propuesta_estado, numero_factura, pagado, fecha_pago, notas, created_at, updated_at)
        VALUES (v_proyecto_id, 'Rueda de Prensa Nova',
            (SELECT id FROM clientes WHERE nombre = 'Nova Cosméticos' LIMIT 1),
            'Sergio Palacio', 'Corporativo', 'Baja', 'Medellín', 'Bogotá',
            '2026-05-01'::timestamptz, '2026-06-15'::timestamptz,
            (SELECT id FROM estados_proyecto WHERE nombre = 'Facturado'),
            100, 'Aprobado', 'Aprobada', 'FAC-2026-0142', TRUE, '2026-06-20'::timestamptz, 'Evento ya cerrado. Factura enviada y pagada por el cliente.', now(), now());
    ELSE
        UPDATE proyectos SET
            cliente_id = (SELECT id FROM clientes WHERE nombre = 'Nova Cosméticos' LIMIT 1),
            contacto_proyecto = 'Sergio Palacio',
            tipo_proyecto = 'Corporativo',
            prioridad = 'Baja',
            ciudad = 'Medellín',
            sede_next = 'Bogotá',
            fecha_solicitud = '2026-05-01'::timestamptz,
            fecha_evento = '2026-06-15'::timestamptz,
            estado_id = (SELECT id FROM estados_proyecto WHERE nombre = 'Facturado'),
            porcentaje_avance = 100,
            estado_brief = 'Aprobado',
            propuesta_estado = 'Aprobada',
            numero_factura = 'FAC-2026-0142',
            pagado = TRUE,
            fecha_pago = '2026-06-20'::timestamptz,
            notas = 'Evento ya cerrado. Factura enviada y pagada por el cliente.',
            updated_at = now()
        WHERE id = v_proyecto_id;
    END IF;

    INSERT INTO proyecto_equipo (id, proyecto_id, rol, nombre)
    SELECT gen_random_uuid(), v_proyecto_id, 'Ejecutivo', 'Viviana Salazar'
    WHERE NOT EXISTS (SELECT 1 FROM proyecto_equipo WHERE proyecto_id = v_proyecto_id AND rol = 'Ejecutivo' AND nombre = 'Viviana Salazar');
    INSERT INTO proyecto_equipo (id, proyecto_id, rol, nombre)
    SELECT gen_random_uuid(), v_proyecto_id, 'Diseñador 3D', 'Carlos Fajardo'
    WHERE NOT EXISTS (SELECT 1 FROM proyecto_equipo WHERE proyecto_id = v_proyecto_id AND rol = 'Diseñador 3D' AND nombre = 'Carlos Fajardo');
    INSERT INTO proyecto_equipo (id, proyecto_id, rol, nombre)
    SELECT gen_random_uuid(), v_proyecto_id, 'Diseñador gráfico', 'Ana Duarte'
    WHERE NOT EXISTS (SELECT 1 FROM proyecto_equipo WHERE proyecto_id = v_proyecto_id AND rol = 'Diseñador gráfico' AND nombre = 'Ana Duarte');

    INSERT INTO proyecto_seguimiento (id, proyecto_id, area, fecha, nota, created_at)
    SELECT gen_random_uuid(), v_proyecto_id, 'Administrativo', '2026-06-20'::timestamptz, 'Pago recibido, proyecto cerrado.', now()
    WHERE NOT EXISTS (SELECT 1 FROM proyecto_seguimiento WHERE proyecto_id = v_proyecto_id AND fecha = '2026-06-20'::timestamptz AND nota = 'Pago recibido, proyecto cerrado.');

    INSERT INTO proyecto_proveedores (proyecto_id, proveedor_id)
    SELECT v_proyecto_id, id FROM proveedores WHERE nombre = 'EvenTech México'
    ON CONFLICT (proyecto_id, proveedor_id) DO NOTHING;
    INSERT INTO proyecto_proveedores (proyecto_id, proveedor_id)
    SELECT v_proyecto_id, id FROM proveedores WHERE nombre = 'Sabores & Experiencias'
    ON CONFLICT (proyecto_id, proveedor_id) DO NOTHING;
END $blk$;

-- Proyecto: Convención Andes 2026 (cliente: Grupo Financiero Andes)
DO $blk$
DECLARE v_proyecto_id uuid;
BEGIN
    SELECT id INTO v_proyecto_id FROM proyectos WHERE nombre = 'Convención Andes 2026' LIMIT 1;
    IF v_proyecto_id IS NULL THEN
        v_proyecto_id := gen_random_uuid();
        INSERT INTO proyectos (id, nombre, cliente_id, contacto_proyecto, tipo_proyecto, prioridad, ciudad, sede_next, fecha_solicitud, fecha_evento, estado_id, porcentaje_avance, estado_brief, propuesta_estado, numero_factura, pagado, fecha_pago, notas, created_at, updated_at)
        VALUES (v_proyecto_id, 'Convención Andes 2026',
            (SELECT id FROM clientes WHERE nombre = 'Grupo Financiero Andes' LIMIT 1),
            'Laura Nieto', 'Corporativo', 'Alta', 'Ciudad de México', 'Ciudad de México',
            '2026-07-15'::timestamptz, '2026-10-05'::timestamptz,
            (SELECT id FROM estados_proyecto WHERE nombre = 'Planeación'),
            35, 'Requiere ajustes', 'Enviada', NULL, FALSE, NULL, 'Alta seguridad, 400 invitados VIP.', now(), now());
    ELSE
        UPDATE proyectos SET
            cliente_id = (SELECT id FROM clientes WHERE nombre = 'Grupo Financiero Andes' LIMIT 1),
            contacto_proyecto = 'Laura Nieto',
            tipo_proyecto = 'Corporativo',
            prioridad = 'Alta',
            ciudad = 'Ciudad de México',
            sede_next = 'Ciudad de México',
            fecha_solicitud = '2026-07-15'::timestamptz,
            fecha_evento = '2026-10-05'::timestamptz,
            estado_id = (SELECT id FROM estados_proyecto WHERE nombre = 'Planeación'),
            porcentaje_avance = 35,
            estado_brief = 'Requiere ajustes',
            propuesta_estado = 'Enviada',
            numero_factura = NULL,
            pagado = FALSE,
            fecha_pago = NULL,
            notas = 'Alta seguridad, 400 invitados VIP.',
            updated_at = now()
        WHERE id = v_proyecto_id;
    END IF;

    INSERT INTO proyecto_equipo (id, proyecto_id, rol, nombre)
    SELECT gen_random_uuid(), v_proyecto_id, 'Ejecutivo', 'Leandro Peña'
    WHERE NOT EXISTS (SELECT 1 FROM proyecto_equipo WHERE proyecto_id = v_proyecto_id AND rol = 'Ejecutivo' AND nombre = 'Leandro Peña');

    INSERT INTO proyecto_proveedores (proyecto_id, proveedor_id)
    SELECT v_proyecto_id, id FROM proveedores WHERE nombre = 'SegurEvent'
    ON CONFLICT (proyecto_id, proveedor_id) DO NOTHING;
    INSERT INTO proyecto_proveedores (proyecto_id, proveedor_id)
    SELECT v_proyecto_id, id FROM proveedores WHERE nombre = 'Hotel Intercontinental Cali'
    ON CONFLICT (proyecto_id, proveedor_id) DO NOTHING;
END $blk$;

-- Proyecto: Demo Día Tech Solutions (cliente: Tech Solutions LatAm)
DO $blk$
DECLARE v_proyecto_id uuid;
BEGIN
    SELECT id INTO v_proyecto_id FROM proyectos WHERE nombre = 'Demo Día Tech Solutions' LIMIT 1;
    IF v_proyecto_id IS NULL THEN
        v_proyecto_id := gen_random_uuid();
        INSERT INTO proyectos (id, nombre, cliente_id, contacto_proyecto, tipo_proyecto, prioridad, ciudad, sede_next, fecha_solicitud, fecha_evento, estado_id, porcentaje_avance, estado_brief, propuesta_estado, numero_factura, pagado, fecha_pago, notas, created_at, updated_at)
        VALUES (v_proyecto_id, 'Demo Día Tech Solutions',
            (SELECT id FROM clientes WHERE nombre = 'Tech Solutions LatAm' LIMIT 1),
            'Daniel Restrepo', 'Corporativo', 'Media', 'Los Angeles', 'Bogotá',
            '2026-08-15'::timestamptz, '2026-12-01'::timestamptz,
            (SELECT id FROM estados_proyecto WHERE nombre = 'Planeación interna'),
            5, 'Pendiente por enviar', 'No enviada', NULL, FALSE, NULL, 'Propuesta preliminar enviada, a espera de aprobación de presupuesto.', now(), now());
    ELSE
        UPDATE proyectos SET
            cliente_id = (SELECT id FROM clientes WHERE nombre = 'Tech Solutions LatAm' LIMIT 1),
            contacto_proyecto = 'Daniel Restrepo',
            tipo_proyecto = 'Corporativo',
            prioridad = 'Media',
            ciudad = 'Los Angeles',
            sede_next = 'Bogotá',
            fecha_solicitud = '2026-08-15'::timestamptz,
            fecha_evento = '2026-12-01'::timestamptz,
            estado_id = (SELECT id FROM estados_proyecto WHERE nombre = 'Planeación interna'),
            porcentaje_avance = 5,
            estado_brief = 'Pendiente por enviar',
            propuesta_estado = 'No enviada',
            numero_factura = NULL,
            pagado = FALSE,
            fecha_pago = NULL,
            notas = 'Propuesta preliminar enviada, a espera de aprobación de presupuesto.',
            updated_at = now()
        WHERE id = v_proyecto_id;
    END IF;

    INSERT INTO proyecto_equipo (id, proyecto_id, rol, nombre)
    SELECT gen_random_uuid(), v_proyecto_id, 'Ejecutivo', 'Viviana Salazar'
    WHERE NOT EXISTS (SELECT 1 FROM proyecto_equipo WHERE proyecto_id = v_proyecto_id AND rol = 'Ejecutivo' AND nombre = 'Viviana Salazar');

END $blk$;

-- Proyecto: Graduación Unipacífico (cliente: Universidad del Pacífico)
DO $blk$
DECLARE v_proyecto_id uuid;
BEGIN
    SELECT id INTO v_proyecto_id FROM proyectos WHERE nombre = 'Graduación Unipacífico' LIMIT 1;
    IF v_proyecto_id IS NULL THEN
        v_proyecto_id := gen_random_uuid();
        INSERT INTO proyectos (id, nombre, cliente_id, contacto_proyecto, tipo_proyecto, prioridad, ciudad, sede_next, fecha_solicitud, fecha_evento, estado_id, porcentaje_avance, estado_brief, propuesta_estado, numero_factura, pagado, fecha_pago, notas, created_at, updated_at)
        VALUES (v_proyecto_id, 'Graduación Unipacífico',
            (SELECT id FROM clientes WHERE nombre = 'Universidad del Pacífico' LIMIT 1),
            'Marcela Uribe', 'Evento social', 'Media', 'Cali', 'Bogotá',
            '2026-05-20'::timestamptz, '2026-07-18'::timestamptz,
            (SELECT id FROM estados_proyecto WHERE nombre = 'Ejecutado, pendiente facturar'),
            95, 'Aprobado', 'Aprobada', NULL, FALSE, NULL, 'Ceremonia con 600 graduandos, salón principal.', now(), now());
    ELSE
        UPDATE proyectos SET
            cliente_id = (SELECT id FROM clientes WHERE nombre = 'Universidad del Pacífico' LIMIT 1),
            contacto_proyecto = 'Marcela Uribe',
            tipo_proyecto = 'Evento social',
            prioridad = 'Media',
            ciudad = 'Cali',
            sede_next = 'Bogotá',
            fecha_solicitud = '2026-05-20'::timestamptz,
            fecha_evento = '2026-07-18'::timestamptz,
            estado_id = (SELECT id FROM estados_proyecto WHERE nombre = 'Ejecutado, pendiente facturar'),
            porcentaje_avance = 95,
            estado_brief = 'Aprobado',
            propuesta_estado = 'Aprobada',
            numero_factura = NULL,
            pagado = FALSE,
            fecha_pago = NULL,
            notas = 'Ceremonia con 600 graduandos, salón principal.',
            updated_at = now()
        WHERE id = v_proyecto_id;
    END IF;

    INSERT INTO proyecto_equipo (id, proyecto_id, rol, nombre)
    SELECT gen_random_uuid(), v_proyecto_id, 'Ejecutivo', 'Leandro Peña'
    WHERE NOT EXISTS (SELECT 1 FROM proyecto_equipo WHERE proyecto_id = v_proyecto_id AND rol = 'Ejecutivo' AND nombre = 'Leandro Peña');
    INSERT INTO proyecto_equipo (id, proyecto_id, rol, nombre)
    SELECT gen_random_uuid(), v_proyecto_id, 'Diseñador 3D', 'Carlos Fajardo'
    WHERE NOT EXISTS (SELECT 1 FROM proyecto_equipo WHERE proyecto_id = v_proyecto_id AND rol = 'Diseñador 3D' AND nombre = 'Carlos Fajardo');

    INSERT INTO proyecto_seguimiento (id, proyecto_id, area, fecha, nota, created_at)
    SELECT gen_random_uuid(), v_proyecto_id, 'General', '2026-07-19'::timestamptz, 'Evento ejecutado sin novedades. Pendiente enviar factura.', now()
    WHERE NOT EXISTS (SELECT 1 FROM proyecto_seguimiento WHERE proyecto_id = v_proyecto_id AND fecha = '2026-07-19'::timestamptz AND nota = 'Evento ejecutado sin novedades. Pendiente enviar factura.');

    INSERT INTO proyecto_proveedores (proyecto_id, proveedor_id)
    SELECT v_proyecto_id, id FROM proveedores WHERE nombre = 'Terraza Río Cali'
    ON CONFLICT (proyecto_id, proveedor_id) DO NOTHING;
    INSERT INTO proyecto_proveedores (proyecto_id, proveedor_id)
    SELECT v_proyecto_id, id FROM proveedores WHERE nombre = 'La Central Gastrobar'
    ON CONFLICT (proyecto_id, proveedor_id) DO NOTHING;
END $blk$;

