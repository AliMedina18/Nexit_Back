-- ============================================================================
-- 18_proveedores_reales.sql
--
-- Carga los 145 proveedores reales del Excel de la compañera de la usuaria
-- (docs/34-limpieza-carga-datos-historicos.md) -- primera vez que se cargan
-- proveedores reales, no hay nada previo con que se puedan duplicar.
--
-- Generado automáticamente desde limpieza_excel_nexit.py -- misma fuente de
-- datos que docs/datos/Proveedores_importar.xlsx, para que ambos coincidan
-- exactamente. Idempotente (patrón de schema/16: busca por nombre, actualiza
-- si existe, inserta si no) por seguridad, aunque en teoría es la primera vez.
--
-- PENDIENTE antes de correr esto: aplicar primero docs/schema/17 (agrega
-- "Ciudad de México" al catálogo) si se quiere que los 86 proveedores con
-- CDMX queden con ciudad asignada -- si no, se cargan igual pero sin ciudad.
--
-- NO PROBADO contra una base real (no hay acceso a Postgres desde donde se
-- generó esto) -- correr primero contra una copia/backup o nexit_dev antes
-- de producción, y revisar unas cuantas filas al azar.
-- ============================================================================

-- Proveedor: MUSEO DEL CHICO
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'MUSEO DEL CHICO' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'MUSEO DEL CHICO',
            (SELECT id FROM paises WHERE nombre = 'Colombia'),
            (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'Colombia' AND ci.nombre = 'Bogotá' LIMIT 1),
            (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'Colombia' AND ci.nombre = 'Bogotá' LIMIT 1),
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Salón de eventos'),
            'Activo', 'PILAR', 'Comercial', 'comercial1@museodelchico.com', 'https://www.museodelchico.com/', 'Ak 7 #93 - 01, Chapinero, Bogotá', NULL, NULL, 'Hemos hecho fiestas fin de año como la de Geopark y eventos de mas de 2.000 personas siendo una zona residencial hayq ue tener mucho cuidado con el ruido a mas tardsar 11pm eventos', now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'Colombia'),
            region_id = (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'Colombia' AND ci.nombre = 'Bogotá' LIMIT 1),
            ciudad_id = (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'Colombia' AND ci.nombre = 'Bogotá' LIMIT 1),
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Salón de eventos'),
            contacto = COALESCE(proveedores.contacto, 'PILAR'),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, 'Comercial'),
            email = COALESCE(proveedores.email, 'comercial1@museodelchico.com'),
            web = COALESCE(proveedores.web, 'https://www.museodelchico.com/'),
            direccion = COALESCE(proveedores.direccion, 'Ak 7 #93 - 01, Chapinero, Bogotá'),
            aforo = COALESCE(proveedores.aforo, NULL),
            costo_referencia = COALESCE(proveedores.costo_referencia, NULL),
            notas = COALESCE(proveedores.notas, 'Hemos hecho fiestas fin de año como la de Geopark y eventos de mas de 2.000 personas siendo una zona residencial hayq ue tener mucho cuidado con el ruido a mas tardsar 11pm eventos'),
            updated_at = now()
        WHERE id = v_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_id, vv.t FROM (VALUES ('319 4175342')) AS vv(t)
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_id AND telefono = vv.t);
END $blk$;

-- Proveedor: COLPLAY SAS
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'COLPLAY SAS' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'COLPLAY SAS',
            (SELECT id FROM paises WHERE nombre = 'Colombia'),
            NULL,
            NULL,
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Entretenimiento y artistas'),
            'Activo', 'Cristian Ramirez', 'comercial', 'colplaysas@hotmail.com', NULL, NULL, NULL, NULL, 'Alquiler de maquinas · Ciudad original en el Excel: "Cota Cundinamarca"', now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'Colombia'),
            region_id = NULL,
            ciudad_id = NULL,
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Entretenimiento y artistas'),
            contacto = COALESCE(proveedores.contacto, 'Cristian Ramirez'),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, 'comercial'),
            email = COALESCE(proveedores.email, 'colplaysas@hotmail.com'),
            web = COALESCE(proveedores.web, NULL),
            direccion = COALESCE(proveedores.direccion, NULL),
            aforo = COALESCE(proveedores.aforo, NULL),
            costo_referencia = COALESCE(proveedores.costo_referencia, NULL),
            notas = COALESCE(proveedores.notas, 'Alquiler de maquinas · Ciudad original en el Excel: "Cota Cundinamarca"'),
            updated_at = now()
        WHERE id = v_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_id, vv.t FROM (VALUES ('3507779899')) AS vv(t)
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_id AND telefono = vv.t);
END $blk$;

-- Proveedor: MIL ALEGRIAS
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'MIL ALEGRIAS' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'MIL ALEGRIAS',
            (SELECT id FROM paises WHERE nombre = 'Colombia'),
            (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'Colombia' AND ci.nombre = 'Manizales' LIMIT 1),
            (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'Colombia' AND ci.nombre = 'Manizales' LIMIT 1),
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Logística y transporte'),
            'Activo', 'María Jesenia Rodríguez Giraldo', NULL, 'mil.alegrias@hotmail.com.', 'https://facebook.com/milalegriasrecreacion.', NULL, NULL, NULL, NULL, now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'Colombia'),
            region_id = (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'Colombia' AND ci.nombre = 'Manizales' LIMIT 1),
            ciudad_id = (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'Colombia' AND ci.nombre = 'Manizales' LIMIT 1),
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Logística y transporte'),
            contacto = COALESCE(proveedores.contacto, 'María Jesenia Rodríguez Giraldo'),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, NULL),
            email = COALESCE(proveedores.email, 'mil.alegrias@hotmail.com.'),
            web = COALESCE(proveedores.web, 'https://facebook.com/milalegriasrecreacion.'),
            direccion = COALESCE(proveedores.direccion, NULL),
            aforo = COALESCE(proveedores.aforo, NULL),
            costo_referencia = COALESCE(proveedores.costo_referencia, NULL),
            notas = COALESCE(proveedores.notas, NULL),
            updated_at = now()
        WHERE id = v_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_id, vv.t FROM (VALUES ('3113855941')) AS vv(t)
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_id AND telefono = vv.t);
END $blk$;

-- Proveedor: SOCIEDAD EMPRENDEDORA SAS
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'SOCIEDAD EMPRENDEDORA SAS' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'SOCIEDAD EMPRENDEDORA SAS',
            (SELECT id FROM paises WHERE nombre = 'Colombia'),
            NULL,
            NULL,
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Mobiliario y decoración'),
            'Activo', 'Gloria', NULL, 'ventas@puffestilorelax.com', 'www.puffestilorelax.com', 'CALLE 39 BIS SUR # 68 L 45', NULL, NULL, 'PUFF', now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'Colombia'),
            region_id = NULL,
            ciudad_id = NULL,
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Mobiliario y decoración'),
            contacto = COALESCE(proveedores.contacto, 'Gloria'),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, NULL),
            email = COALESCE(proveedores.email, 'ventas@puffestilorelax.com'),
            web = COALESCE(proveedores.web, 'www.puffestilorelax.com'),
            direccion = COALESCE(proveedores.direccion, 'CALLE 39 BIS SUR # 68 L 45'),
            aforo = COALESCE(proveedores.aforo, NULL),
            costo_referencia = COALESCE(proveedores.costo_referencia, NULL),
            notas = COALESCE(proveedores.notas, 'PUFF'),
            updated_at = now()
        WHERE id = v_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_id, vv.t FROM (VALUES ('3167572001')) AS vv(t)
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_id AND telefono = vv.t);
END $blk$;

-- Proveedor: PEPE GANGA
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'PEPE GANGA' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'PEPE GANGA',
            (SELECT id FROM paises WHERE nombre = 'Colombia'),
            NULL,
            NULL,
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Entretenimiento y artistas'),
            'Activo', 'Yaris Osorio', 'jecutiva comercial de Pepe Ganga', 'asistentelogistico@pepeganga.com', NULL, NULL, NULL, NULL, NULL, now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'Colombia'),
            region_id = NULL,
            ciudad_id = NULL,
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Entretenimiento y artistas'),
            contacto = COALESCE(proveedores.contacto, 'Yaris Osorio'),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, 'jecutiva comercial de Pepe Ganga'),
            email = COALESCE(proveedores.email, 'asistentelogistico@pepeganga.com'),
            web = COALESCE(proveedores.web, NULL),
            direccion = COALESCE(proveedores.direccion, NULL),
            aforo = COALESCE(proveedores.aforo, NULL),
            costo_referencia = COALESCE(proveedores.costo_referencia, NULL),
            notas = COALESCE(proveedores.notas, NULL),
            updated_at = now()
        WHERE id = v_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_id, vv.t FROM (VALUES ('321 9690029')) AS vv(t)
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_id AND telefono = vv.t);
END $blk$;

-- Proveedor: LUGANO
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'LUGANO' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'LUGANO',
            (SELECT id FROM paises WHERE nombre = 'Colombia'),
            NULL,
            NULL,
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Merchandising y regalos corporativos'),
            'Activo', 'ADRIANA SARMIENTO VARGAS', NULL, NULL, NULL, NULL, NULL, NULL, 'con productos de entrega inmediata como morrales, accesorios, botellas, loncheras, maletas de viaje, sombrillas, entre otros. otellas de hidratación premium Cool Gear, complementos para actividades al aire libre, artículos de viaje, línea Nat Geo, porcelana fina inglesa Wilmax, cristal checo Bohemia, entre otros.', now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'Colombia'),
            region_id = NULL,
            ciudad_id = NULL,
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Merchandising y regalos corporativos'),
            contacto = COALESCE(proveedores.contacto, 'ADRIANA SARMIENTO VARGAS'),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, NULL),
            email = COALESCE(proveedores.email, NULL),
            web = COALESCE(proveedores.web, NULL),
            direccion = COALESCE(proveedores.direccion, NULL),
            aforo = COALESCE(proveedores.aforo, NULL),
            costo_referencia = COALESCE(proveedores.costo_referencia, NULL),
            notas = COALESCE(proveedores.notas, 'con productos de entrega inmediata como morrales, accesorios, botellas, loncheras, maletas de viaje, sombrillas, entre otros. otellas de hidratación premium Cool Gear, complementos para actividades al aire libre, artículos de viaje, línea Nat Geo, porcelana fina inglesa Wilmax, cristal checo Bohemia, entre otros.'),
            updated_at = now()
        WHERE id = v_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_id, vv.t FROM (VALUES ('310 7624306')) AS vv(t)
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_id AND telefono = vv.t);
END $blk$;

-- Proveedor: SABOR LATINO ORQUESTA Y EVENTOS
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'SABOR LATINO ORQUESTA Y EVENTOS' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'SABOR LATINO ORQUESTA Y EVENTOS',
            (SELECT id FROM paises WHERE nombre = 'Colombia'),
            NULL,
            NULL,
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Entretenimiento y artistas'),
            'Activo', 'FRAY FRANCISCO GAMBOA TABORDA', NULL, NULL, 'https://www.instagram.com/saborlatinofraygamboa?utm_source=qr&igsh=MTFmZjVrdWhreWlreg==', 'CRA 9 # 5 A 42', NULL, NULL, 'CUBRIMIENTO NACIONAL E INTERNACIONAL · Ciudad original en el Excel: "FUNZA  CUNDINAMARCA"', now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'Colombia'),
            region_id = NULL,
            ciudad_id = NULL,
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Entretenimiento y artistas'),
            contacto = COALESCE(proveedores.contacto, 'FRAY FRANCISCO GAMBOA TABORDA'),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, NULL),
            email = COALESCE(proveedores.email, NULL),
            web = COALESCE(proveedores.web, 'https://www.instagram.com/saborlatinofraygamboa?utm_source=qr&igsh=MTFmZjVrdWhreWlreg=='),
            direccion = COALESCE(proveedores.direccion, 'CRA 9 # 5 A 42'),
            aforo = COALESCE(proveedores.aforo, NULL),
            costo_referencia = COALESCE(proveedores.costo_referencia, NULL),
            notas = COALESCE(proveedores.notas, 'CUBRIMIENTO NACIONAL E INTERNACIONAL · Ciudad original en el Excel: "FUNZA  CUNDINAMARCA"'),
            updated_at = now()
        WHERE id = v_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_id, vv.t FROM (VALUES ('3106665229')) AS vv(t)
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_id AND telefono = vv.t);
END $blk$;

-- Proveedor: LoreCake
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'LoreCake' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'LoreCake',
            (SELECT id FROM paises WHERE nombre = 'Colombia'),
            (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'Colombia' AND ci.nombre = 'Bogotá' LIMIT 1),
            (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'Colombia' AND ci.nombre = 'Bogotá' LIMIT 1),
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Catering y F&B'),
            'Activo', 'Lorena Virviescas', NULL, 'orenasabogalvirviescas91@gmail.com', 'https://www.instagram.com/lorecakeoficial?igsh=MXRvbjZkbXBjY3ZpdQ==', NULL, NULL, NULL, NULL, now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'Colombia'),
            region_id = (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'Colombia' AND ci.nombre = 'Bogotá' LIMIT 1),
            ciudad_id = (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'Colombia' AND ci.nombre = 'Bogotá' LIMIT 1),
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Catering y F&B'),
            contacto = COALESCE(proveedores.contacto, 'Lorena Virviescas'),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, NULL),
            email = COALESCE(proveedores.email, 'orenasabogalvirviescas91@gmail.com'),
            web = COALESCE(proveedores.web, 'https://www.instagram.com/lorecakeoficial?igsh=MXRvbjZkbXBjY3ZpdQ=='),
            direccion = COALESCE(proveedores.direccion, NULL),
            aforo = COALESCE(proveedores.aforo, NULL),
            costo_referencia = COALESCE(proveedores.costo_referencia, NULL),
            notas = COALESCE(proveedores.notas, NULL),
            updated_at = now()
        WHERE id = v_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_id, vv.t FROM (VALUES ('3102207339')) AS vv(t)
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_id AND telefono = vv.t);
END $blk$;

-- Proveedor: Moxe SAS
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'Moxe SAS' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'Moxe SAS',
            (SELECT id FROM paises WHERE nombre = 'Colombia'),
            NULL,
            NULL,
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Catering y F&B'),
            'Activo', 'Alfonso Jaramillo', 'sales manager', 'ajaramillo@moxefoods.com', 'www.moxefoods.com', NULL, NULL, NULL, NULL, now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'Colombia'),
            region_id = NULL,
            ciudad_id = NULL,
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Catering y F&B'),
            contacto = COALESCE(proveedores.contacto, 'Alfonso Jaramillo'),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, 'sales manager'),
            email = COALESCE(proveedores.email, 'ajaramillo@moxefoods.com'),
            web = COALESCE(proveedores.web, 'www.moxefoods.com'),
            direccion = COALESCE(proveedores.direccion, NULL),
            aforo = COALESCE(proveedores.aforo, NULL),
            costo_referencia = COALESCE(proveedores.costo_referencia, NULL),
            notas = COALESCE(proveedores.notas, NULL),
            updated_at = now()
        WHERE id = v_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_id, vv.t FROM (VALUES ('3175563755')) AS vv(t)
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_id AND telefono = vv.t);
END $blk$;

-- Proveedor: Súper Bowling Medellín SAS
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'Súper Bowling Medellín SAS' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'Súper Bowling Medellín SAS',
            (SELECT id FROM paises WHERE nombre = 'Colombia'),
            (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'Colombia' AND ci.nombre = 'Medellín' LIMIT 1),
            (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'Colombia' AND ci.nombre = 'Medellín' LIMIT 1),
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Salón de eventos'),
            'Activo', 'María de los Ángeles Santander Giménez', 'Asesora comercial', 'comercial.medellin@superbowling.co', NULL, 'Cra 27 #23sur 120 Envigado', NULL, NULL, NULL, now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'Colombia'),
            region_id = (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'Colombia' AND ci.nombre = 'Medellín' LIMIT 1),
            ciudad_id = (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'Colombia' AND ci.nombre = 'Medellín' LIMIT 1),
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Salón de eventos'),
            contacto = COALESCE(proveedores.contacto, 'María de los Ángeles Santander Giménez'),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, 'Asesora comercial'),
            email = COALESCE(proveedores.email, 'comercial.medellin@superbowling.co'),
            web = COALESCE(proveedores.web, NULL),
            direccion = COALESCE(proveedores.direccion, 'Cra 27 #23sur 120 Envigado'),
            aforo = COALESCE(proveedores.aforo, NULL),
            costo_referencia = COALESCE(proveedores.costo_referencia, NULL),
            notas = COALESCE(proveedores.notas, NULL),
            updated_at = now()
        WHERE id = v_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_id, vv.t FROM (VALUES ('3008848279')) AS vv(t)
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_id AND telefono = vv.t);
END $blk$;

-- Proveedor: Home Burgers
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'Home Burgers' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'Home Burgers',
            (SELECT id FROM paises WHERE nombre = 'Colombia'),
            (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'Colombia' AND ci.nombre = 'Bogotá' LIMIT 1),
            (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'Colombia' AND ci.nombre = 'Bogotá' LIMIT 1),
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Otro'),
            'Activo', 'Carlos Olaya', 'Coordinador de mercadeo', 'colaya@homeburgers.com', 'https://homeburgers.com/', 'Cra 5 #67-12', NULL, NULL, 'Ciudad de cubrimiento: Bogotá y Medellín', now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'Colombia'),
            region_id = (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'Colombia' AND ci.nombre = 'Bogotá' LIMIT 1),
            ciudad_id = (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'Colombia' AND ci.nombre = 'Bogotá' LIMIT 1),
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Otro'),
            contacto = COALESCE(proveedores.contacto, 'Carlos Olaya'),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, 'Coordinador de mercadeo'),
            email = COALESCE(proveedores.email, 'colaya@homeburgers.com'),
            web = COALESCE(proveedores.web, 'https://homeburgers.com/'),
            direccion = COALESCE(proveedores.direccion, 'Cra 5 #67-12'),
            aforo = COALESCE(proveedores.aforo, NULL),
            costo_referencia = COALESCE(proveedores.costo_referencia, NULL),
            notas = COALESCE(proveedores.notas, 'Ciudad de cubrimiento: Bogotá y Medellín'),
            updated_at = now()
        WHERE id = v_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_id, vv.t FROM (VALUES ('3219046153')) AS vv(t)
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_id AND telefono = vv.t);
END $blk$;

-- Proveedor: SONESTA HOTEL CALI - HOTELES DE OCCIDENTE
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'SONESTA HOTEL CALI - HOTELES DE OCCIDENTE' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'SONESTA HOTEL CALI - HOTELES DE OCCIDENTE',
            (SELECT id FROM paises WHERE nombre = 'Colombia'),
            (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'Colombia' AND ci.nombre = 'Cali' LIMIT 1),
            (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'Colombia' AND ci.nombre = 'Cali' LIMIT 1),
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Salón de eventos'),
            'Activo', 'KEVIN BARRARA', 'COORDINADOR DE EVENTOS', 'EVENTOS.SONESTACALI@GHLHOTELES.COM', NULL, 'CALLE 18N #4N - 08', NULL, NULL, NULL, now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'Colombia'),
            region_id = (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'Colombia' AND ci.nombre = 'Cali' LIMIT 1),
            ciudad_id = (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'Colombia' AND ci.nombre = 'Cali' LIMIT 1),
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Salón de eventos'),
            contacto = COALESCE(proveedores.contacto, 'KEVIN BARRARA'),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, 'COORDINADOR DE EVENTOS'),
            email = COALESCE(proveedores.email, 'EVENTOS.SONESTACALI@GHLHOTELES.COM'),
            web = COALESCE(proveedores.web, NULL),
            direccion = COALESCE(proveedores.direccion, 'CALLE 18N #4N - 08'),
            aforo = COALESCE(proveedores.aforo, NULL),
            costo_referencia = COALESCE(proveedores.costo_referencia, NULL),
            notas = COALESCE(proveedores.notas, NULL),
            updated_at = now()
        WHERE id = v_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_id, vv.t FROM (VALUES ('3154637705')) AS vv(t)
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_id AND telefono = vv.t);
END $blk$;

-- Proveedor: VILASECA SAS
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'VILASECA SAS' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'VILASECA SAS',
            (SELECT id FROM paises WHERE nombre = 'Colombia'),
            (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'Colombia' AND ci.nombre = 'Bogotá' LIMIT 1),
            (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'Colombia' AND ci.nombre = 'Bogotá' LIMIT 1),
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Producción audiovisual'),
            'Activo', 'NELSON VIVAS', 'ASESOR COMERCIAL', 'contactenos@vilaseca.com.co', 'https://www.vilaseca.co/?srsltid=AfmBOoqSqnuOjbRMsvSqixZtxJYnVCx9RSL526zwIuWFydCKGu7W1fec', 'CARRERA 34 #10-28', NULL, NULL, 'CUBRIMIENTO CAPITALES A NIVEL NACIONAL', now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'Colombia'),
            region_id = (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'Colombia' AND ci.nombre = 'Bogotá' LIMIT 1),
            ciudad_id = (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'Colombia' AND ci.nombre = 'Bogotá' LIMIT 1),
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Producción audiovisual'),
            contacto = COALESCE(proveedores.contacto, 'NELSON VIVAS'),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, 'ASESOR COMERCIAL'),
            email = COALESCE(proveedores.email, 'contactenos@vilaseca.com.co'),
            web = COALESCE(proveedores.web, 'https://www.vilaseca.co/?srsltid=AfmBOoqSqnuOjbRMsvSqixZtxJYnVCx9RSL526zwIuWFydCKGu7W1fec'),
            direccion = COALESCE(proveedores.direccion, 'CARRERA 34 #10-28'),
            aforo = COALESCE(proveedores.aforo, NULL),
            costo_referencia = COALESCE(proveedores.costo_referencia, NULL),
            notas = COALESCE(proveedores.notas, 'CUBRIMIENTO CAPITALES A NIVEL NACIONAL'),
            updated_at = now()
        WHERE id = v_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_id, vv.t FROM (VALUES ('3174280927'), ('6017470070')) AS vv(t)
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_id AND telefono = vv.t);
END $blk$;

-- Proveedor: Ghl Hotel Barranquilla
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'Ghl Hotel Barranquilla' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'Ghl Hotel Barranquilla',
            (SELECT id FROM paises WHERE nombre = 'Colombia'),
            (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'Colombia' AND ci.nombre = 'Barranquilla' LIMIT 1),
            (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'Colombia' AND ci.nombre = 'Barranquilla' LIMIT 1),
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Salón de eventos'),
            'Activo', 'Manuela Rios', 'Asistente mercadeo', 'asistentemercadeo.ghlbarranquilla@ghlhoteles.com', 'https://www.ghlgrandbarranquilla.com', 'Cl 106# 50-11', NULL, NULL, 'CUBRIMIENTO BARRANQUILLA', now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'Colombia'),
            region_id = (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'Colombia' AND ci.nombre = 'Barranquilla' LIMIT 1),
            ciudad_id = (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'Colombia' AND ci.nombre = 'Barranquilla' LIMIT 1),
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Salón de eventos'),
            contacto = COALESCE(proveedores.contacto, 'Manuela Rios'),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, 'Asistente mercadeo'),
            email = COALESCE(proveedores.email, 'asistentemercadeo.ghlbarranquilla@ghlhoteles.com'),
            web = COALESCE(proveedores.web, 'https://www.ghlgrandbarranquilla.com'),
            direccion = COALESCE(proveedores.direccion, 'Cl 106# 50-11'),
            aforo = COALESCE(proveedores.aforo, NULL),
            costo_referencia = COALESCE(proveedores.costo_referencia, NULL),
            notas = COALESCE(proveedores.notas, 'CUBRIMIENTO BARRANQUILLA'),
            updated_at = now()
        WHERE id = v_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_id, vv.t FROM (VALUES ('3182852983'), ('605 3856060')) AS vv(t)
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_id AND telefono = vv.t);
END $blk$;

-- Proveedor: Tequendama hotel Medellin
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'Tequendama hotel Medellin' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'Tequendama hotel Medellin',
            (SELECT id FROM paises WHERE nombre = 'Colombia'),
            NULL,
            NULL,
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Salón de eventos'),
            'Activo', 'Mariana Arango', 'Ejecutiva comercial', 'Info@tequendamamedellin.com', NULL, 'Calle 50#70-124', NULL, NULL, 'Medellin, Carmen de viboral, Bogotá, Villavicencio,Cartagena,santa marta.', now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'Colombia'),
            region_id = NULL,
            ciudad_id = NULL,
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Salón de eventos'),
            contacto = COALESCE(proveedores.contacto, 'Mariana Arango'),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, 'Ejecutiva comercial'),
            email = COALESCE(proveedores.email, 'Info@tequendamamedellin.com'),
            web = COALESCE(proveedores.web, NULL),
            direccion = COALESCE(proveedores.direccion, 'Calle 50#70-124'),
            aforo = COALESCE(proveedores.aforo, NULL),
            costo_referencia = COALESCE(proveedores.costo_referencia, NULL),
            notas = COALESCE(proveedores.notas, 'Medellin, Carmen de viboral, Bogotá, Villavicencio,Cartagena,santa marta.'),
            updated_at = now()
        WHERE id = v_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_id, vv.t FROM (VALUES ('3212585034')) AS vv(t)
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_id AND telefono = vv.t);
END $blk$;

-- Proveedor: ACADEMIA DE ACORDEON
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'ACADEMIA DE ACORDEON' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'ACADEMIA DE ACORDEON',
            (SELECT id FROM paises WHERE nombre = 'Colombia'),
            (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'Colombia' AND ci.nombre = 'Soacha' LIMIT 1),
            (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'Colombia' AND ci.nombre = 'Soacha' LIMIT 1),
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Entretenimiento y artistas'),
            'Activo', 'Néstor Iván Rincon Vasquez', 'Director', 'Academiadeacordeonivanpirucho@hotmail.com', 'www.academiadeacordeonivanpirucho.com', NULL, NULL, NULL, 'Cubrimiemto Bogotá Cundinamarca', now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'Colombia'),
            region_id = (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'Colombia' AND ci.nombre = 'Soacha' LIMIT 1),
            ciudad_id = (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'Colombia' AND ci.nombre = 'Soacha' LIMIT 1),
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Entretenimiento y artistas'),
            contacto = COALESCE(proveedores.contacto, 'Néstor Iván Rincon Vasquez'),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, 'Director'),
            email = COALESCE(proveedores.email, 'Academiadeacordeonivanpirucho@hotmail.com'),
            web = COALESCE(proveedores.web, 'www.academiadeacordeonivanpirucho.com'),
            direccion = COALESCE(proveedores.direccion, NULL),
            aforo = COALESCE(proveedores.aforo, NULL),
            costo_referencia = COALESCE(proveedores.costo_referencia, NULL),
            notas = COALESCE(proveedores.notas, 'Cubrimiemto Bogotá Cundinamarca'),
            updated_at = now()
        WHERE id = v_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_id, vv.t FROM (VALUES ('3142656911')) AS vv(t)
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_id AND telefono = vv.t);
END $blk$;

-- Proveedor: ST FOOD SAS
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'ST FOOD SAS' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'ST FOOD SAS',
            (SELECT id FROM paises WHERE nombre = 'Colombia'),
            (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'Colombia' AND ci.nombre = 'Cali' LIMIT 1),
            (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'Colombia' AND ci.nombre = 'Cali' LIMIT 1),
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Salón de eventos'),
            'Activo', 'Karol Andrea Arango Benavidez', 'Auxiliar contable', 'notificacionesjudicialesstfood@gmail.com', 'https://www.lossantosfoodtrucks.com/', 'CR 2 13 36', NULL, NULL, NULL, now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'Colombia'),
            region_id = (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'Colombia' AND ci.nombre = 'Cali' LIMIT 1),
            ciudad_id = (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'Colombia' AND ci.nombre = 'Cali' LIMIT 1),
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Salón de eventos'),
            contacto = COALESCE(proveedores.contacto, 'Karol Andrea Arango Benavidez'),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, 'Auxiliar contable'),
            email = COALESCE(proveedores.email, 'notificacionesjudicialesstfood@gmail.com'),
            web = COALESCE(proveedores.web, 'https://www.lossantosfoodtrucks.com/'),
            direccion = COALESCE(proveedores.direccion, 'CR 2 13 36'),
            aforo = COALESCE(proveedores.aforo, NULL),
            costo_referencia = COALESCE(proveedores.costo_referencia, NULL),
            notas = COALESCE(proveedores.notas, NULL),
            updated_at = now()
        WHERE id = v_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_id, vv.t FROM (VALUES ('3173808128')) AS vv(t)
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_id AND telefono = vv.t);
END $blk$;

-- Proveedor: Alquiler de equipos audiovisuales
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'Alquiler de equipos audiovisuales' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'Alquiler de equipos audiovisuales',
            (SELECT id FROM paises WHERE nombre = 'Colombia'),
            (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'Colombia' AND ci.nombre = 'Cartagena' LIMIT 1),
            (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'Colombia' AND ci.nombre = 'Cartagena' LIMIT 1),
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Otro'),
            'Activo', 'Juan Diego franco Ospina', 'GERENTE', 'eventoproducciones@hotmail.com', NULL, NULL, NULL, NULL, 'costa atlántica', now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'Colombia'),
            region_id = (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'Colombia' AND ci.nombre = 'Cartagena' LIMIT 1),
            ciudad_id = (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'Colombia' AND ci.nombre = 'Cartagena' LIMIT 1),
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Otro'),
            contacto = COALESCE(proveedores.contacto, 'Juan Diego franco Ospina'),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, 'GERENTE'),
            email = COALESCE(proveedores.email, 'eventoproducciones@hotmail.com'),
            web = COALESCE(proveedores.web, NULL),
            direccion = COALESCE(proveedores.direccion, NULL),
            aforo = COALESCE(proveedores.aforo, NULL),
            costo_referencia = COALESCE(proveedores.costo_referencia, NULL),
            notas = COALESCE(proveedores.notas, 'costa atlántica'),
            updated_at = now()
        WHERE id = v_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_id, vv.t FROM (VALUES ('3173833118')) AS vv(t)
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_id AND telefono = vv.t);
END $blk$;

-- Proveedor: Graficas Ducal Ltda
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'Graficas Ducal Ltda' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'Graficas Ducal Ltda',
            (SELECT id FROM paises WHERE nombre = 'Colombia'),
            (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'Colombia' AND ci.nombre = 'Bogotá' LIMIT 1),
            (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'Colombia' AND ci.nombre = 'Bogotá' LIMIT 1),
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Otro'),
            'Activo', 'Andrea Avellaneda', 'Asesora Comercial.', 'andrea@graficasducal.com', 'www.graficasducal.com', 'carrera 16 # 24 -17', NULL, NULL, 'COBERTURA colombia', now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'Colombia'),
            region_id = (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'Colombia' AND ci.nombre = 'Bogotá' LIMIT 1),
            ciudad_id = (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'Colombia' AND ci.nombre = 'Bogotá' LIMIT 1),
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Otro'),
            contacto = COALESCE(proveedores.contacto, 'Andrea Avellaneda'),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, 'Asesora Comercial.'),
            email = COALESCE(proveedores.email, 'andrea@graficasducal.com'),
            web = COALESCE(proveedores.web, 'www.graficasducal.com'),
            direccion = COALESCE(proveedores.direccion, 'carrera 16 # 24 -17'),
            aforo = COALESCE(proveedores.aforo, NULL),
            costo_referencia = COALESCE(proveedores.costo_referencia, NULL),
            notas = COALESCE(proveedores.notas, 'COBERTURA colombia'),
            updated_at = now()
        WHERE id = v_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_id, vv.t FROM (VALUES ('317 4336312')) AS vv(t)
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_id AND telefono = vv.t);
END $blk$;

-- Proveedor: FACES
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'FACES' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'FACES',
            (SELECT id FROM paises WHERE nombre = 'México'),
            (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Toluca' LIMIT 1),
            (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Toluca' LIMIT 1),
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Personal BTL (impulsadoras, edecanes, meseros)'),
            'Activo', 'LEONARNO HERRERA', NULL, 'faces_toluca@yahoo.com.mx', NULL, NULL, NULL, NULL, NULL, now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'México'),
            region_id = (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Toluca' LIMIT 1),
            ciudad_id = (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Toluca' LIMIT 1),
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Personal BTL (impulsadoras, edecanes, meseros)'),
            contacto = COALESCE(proveedores.contacto, 'LEONARNO HERRERA'),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, NULL),
            email = COALESCE(proveedores.email, 'faces_toluca@yahoo.com.mx'),
            web = COALESCE(proveedores.web, NULL),
            direccion = COALESCE(proveedores.direccion, NULL),
            aforo = COALESCE(proveedores.aforo, NULL),
            costo_referencia = COALESCE(proveedores.costo_referencia, NULL),
            notas = COALESCE(proveedores.notas, NULL),
            updated_at = now()
        WHERE id = v_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_id, vv.t FROM (VALUES ('7222642765')) AS vv(t)
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_id AND telefono = vv.t);
END $blk$;

-- Proveedor: PAT MODELS
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'PAT MODELS' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'PAT MODELS',
            (SELECT id FROM paises WHERE nombre = 'México'),
            (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Toluca' LIMIT 1),
            (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Toluca' LIMIT 1),
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Personal BTL (impulsadoras, edecanes, meseros)'),
            'Activo', 'FRANCISCO HERNANDEZ', NULL, 'patmodelstoluca@gmail.com', NULL, NULL, NULL, NULL, NULL, now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'México'),
            region_id = (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Toluca' LIMIT 1),
            ciudad_id = (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Toluca' LIMIT 1),
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Personal BTL (impulsadoras, edecanes, meseros)'),
            contacto = COALESCE(proveedores.contacto, 'FRANCISCO HERNANDEZ'),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, NULL),
            email = COALESCE(proveedores.email, 'patmodelstoluca@gmail.com'),
            web = COALESCE(proveedores.web, NULL),
            direccion = COALESCE(proveedores.direccion, NULL),
            aforo = COALESCE(proveedores.aforo, NULL),
            costo_referencia = COALESCE(proveedores.costo_referencia, NULL),
            notas = COALESCE(proveedores.notas, NULL),
            updated_at = now()
        WHERE id = v_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_id, vv.t FROM (VALUES ('7223755605')) AS vv(t)
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_id AND telefono = vv.t);
END $blk$;

-- Proveedor: LOS TOROS
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'LOS TOROS' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'LOS TOROS',
            (SELECT id FROM paises WHERE nombre = 'México'),
            (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Toluca' LIMIT 1),
            (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Toluca' LIMIT 1),
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Catering y F&B'),
            'Activo', 'JOSUE REYES', 'DIRECTOR GENERAL', 'contacto.taquerialostoros@gmail.com', 'lostoros.mx', 'Frontera San Isidro 204, San Isidro, Metepec', NULL, NULL, NULL, now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'México'),
            region_id = (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Toluca' LIMIT 1),
            ciudad_id = (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Toluca' LIMIT 1),
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Catering y F&B'),
            contacto = COALESCE(proveedores.contacto, 'JOSUE REYES'),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, 'DIRECTOR GENERAL'),
            email = COALESCE(proveedores.email, 'contacto.taquerialostoros@gmail.com'),
            web = COALESCE(proveedores.web, 'lostoros.mx'),
            direccion = COALESCE(proveedores.direccion, 'Frontera San Isidro 204, San Isidro, Metepec'),
            aforo = COALESCE(proveedores.aforo, NULL),
            costo_referencia = COALESCE(proveedores.costo_referencia, NULL),
            notas = COALESCE(proveedores.notas, NULL),
            updated_at = now()
        WHERE id = v_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_id, vv.t FROM (VALUES ('7229079034'), ('7224219375')) AS vv(t)
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_id AND telefono = vv.t);
END $blk$;

-- Proveedor: ANDAMIOS AMARILLOS
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'ANDAMIOS AMARILLOS' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'ANDAMIOS AMARILLOS',
            (SELECT id FROM paises WHERE nombre = 'México'),
            (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Producción técnica (rigging, generadores, carpas)'),
            'Activo', 'MARY', NULL, NULL, 'andamiosamarillos.com', NULL, NULL, NULL, 'Ciudad original en el Excel: "CDMX"', now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'México'),
            region_id = (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            ciudad_id = (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Producción técnica (rigging, generadores, carpas)'),
            contacto = COALESCE(proveedores.contacto, 'MARY'),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, NULL),
            email = COALESCE(proveedores.email, NULL),
            web = COALESCE(proveedores.web, 'andamiosamarillos.com'),
            direccion = COALESCE(proveedores.direccion, NULL),
            aforo = COALESCE(proveedores.aforo, NULL),
            costo_referencia = COALESCE(proveedores.costo_referencia, NULL),
            notas = COALESCE(proveedores.notas, 'Ciudad original en el Excel: "CDMX"'),
            updated_at = now()
        WHERE id = v_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_id, vv.t FROM (VALUES ('8116114084')) AS vv(t)
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_id AND telefono = vv.t);
END $blk$;

-- Proveedor: ANDAMIOS JINES
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'ANDAMIOS JINES' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'ANDAMIOS JINES',
            (SELECT id FROM paises WHERE nombre = 'México'),
            (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Producción técnica (rigging, generadores, carpas)'),
            'Activo', 'CAROLINA', NULL, NULL, 'andamiosjines.com.mx', NULL, NULL, NULL, 'Ciudad original en el Excel: "CDMX"', now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'México'),
            region_id = (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            ciudad_id = (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Producción técnica (rigging, generadores, carpas)'),
            contacto = COALESCE(proveedores.contacto, 'CAROLINA'),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, NULL),
            email = COALESCE(proveedores.email, NULL),
            web = COALESCE(proveedores.web, 'andamiosjines.com.mx'),
            direccion = COALESCE(proveedores.direccion, NULL),
            aforo = COALESCE(proveedores.aforo, NULL),
            costo_referencia = COALESCE(proveedores.costo_referencia, NULL),
            notas = COALESCE(proveedores.notas, 'Ciudad original en el Excel: "CDMX"'),
            updated_at = now()
        WHERE id = v_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_id, vv.t FROM (VALUES ('5557921129')) AS vv(t)
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_id AND telefono = vv.t);
END $blk$;

-- Proveedor: ANDAMIOS ORTEGA
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'ANDAMIOS ORTEGA' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'ANDAMIOS ORTEGA',
            (SELECT id FROM paises WHERE nombre = 'México'),
            (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Producción técnica (rigging, generadores, carpas)'),
            'Activo', 'DIANA ORTEGA', NULL, NULL, 'andamios-ortega.com', NULL, NULL, NULL, 'Ciudad original en el Excel: "CDMX"', now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'México'),
            region_id = (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            ciudad_id = (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Producción técnica (rigging, generadores, carpas)'),
            contacto = COALESCE(proveedores.contacto, 'DIANA ORTEGA'),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, NULL),
            email = COALESCE(proveedores.email, NULL),
            web = COALESCE(proveedores.web, 'andamios-ortega.com'),
            direccion = COALESCE(proveedores.direccion, NULL),
            aforo = COALESCE(proveedores.aforo, NULL),
            costo_referencia = COALESCE(proveedores.costo_referencia, NULL),
            notas = COALESCE(proveedores.notas, 'Ciudad original en el Excel: "CDMX"'),
            updated_at = now()
        WHERE id = v_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_id, vv.t FROM (VALUES ('5572120052')) AS vv(t)
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_id AND telefono = vv.t);
END $blk$;

-- Proveedor: ANDAMIOS TLALPAN
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'ANDAMIOS TLALPAN' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'ANDAMIOS TLALPAN',
            (SELECT id FROM paises WHERE nombre = 'México'),
            (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Producción técnica (rigging, generadores, carpas)'),
            'Activo', 'VICTOR SANCHEZ', NULL, NULL, NULL, NULL, NULL, NULL, 'Ciudad original en el Excel: "CDMX"', now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'México'),
            region_id = (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            ciudad_id = (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Producción técnica (rigging, generadores, carpas)'),
            contacto = COALESCE(proveedores.contacto, 'VICTOR SANCHEZ'),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, NULL),
            email = COALESCE(proveedores.email, NULL),
            web = COALESCE(proveedores.web, NULL),
            direccion = COALESCE(proveedores.direccion, NULL),
            aforo = COALESCE(proveedores.aforo, NULL),
            costo_referencia = COALESCE(proveedores.costo_referencia, NULL),
            notas = COALESCE(proveedores.notas, 'Ciudad original en el Excel: "CDMX"'),
            updated_at = now()
        WHERE id = v_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_id, vv.t FROM (VALUES ('5545439822')) AS vv(t)
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_id AND telefono = vv.t);
END $blk$;

-- Proveedor: SUPER FIESTAS EVENTOS
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'SUPER FIESTAS EVENTOS' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'SUPER FIESTAS EVENTOS',
            (SELECT id FROM paises WHERE nombre = 'México'),
            NULL,
            NULL,
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Otro'),
            'Activo', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'México'),
            region_id = NULL,
            ciudad_id = NULL,
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Otro'),
            contacto = COALESCE(proveedores.contacto, NULL),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, NULL),
            email = COALESCE(proveedores.email, NULL),
            web = COALESCE(proveedores.web, NULL),
            direccion = COALESCE(proveedores.direccion, NULL),
            aforo = COALESCE(proveedores.aforo, NULL),
            costo_referencia = COALESCE(proveedores.costo_referencia, NULL),
            notas = COALESCE(proveedores.notas, NULL),
            updated_at = now()
        WHERE id = v_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_id, vv.t FROM (VALUES ('7222421010')) AS vv(t)
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_id AND telefono = vv.t);
END $blk$;

-- Proveedor: ULINE
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'ULINE' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'ULINE',
            (SELECT id FROM paises WHERE nombre = 'México'),
            (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Monterrey' LIMIT 1),
            (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Monterrey' LIMIT 1),
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Otro'),
            'Activo', 'ULINE', NULL, NULL, 'es.uline.mx', NULL, NULL, NULL, NULL, now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'México'),
            region_id = (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Monterrey' LIMIT 1),
            ciudad_id = (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Monterrey' LIMIT 1),
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Otro'),
            contacto = COALESCE(proveedores.contacto, 'ULINE'),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, NULL),
            email = COALESCE(proveedores.email, NULL),
            web = COALESCE(proveedores.web, 'es.uline.mx'),
            direccion = COALESCE(proveedores.direccion, NULL),
            aforo = COALESCE(proveedores.aforo, NULL),
            costo_referencia = COALESCE(proveedores.costo_referencia, NULL),
            notas = COALESCE(proveedores.notas, NULL),
            updated_at = now()
        WHERE id = v_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_id, vv.t FROM (VALUES ('6865517777')) AS vv(t)
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_id AND telefono = vv.t);
END $blk$;

-- Proveedor: JULIO PONZE
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'JULIO PONZE' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'JULIO PONZE',
            (SELECT id FROM paises WHERE nombre = 'México'),
            NULL,
            NULL,
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Entretenimiento y artistas'),
            'Activo', 'YURI FERNANDEZ', NULL, NULL, NULL, NULL, NULL, NULL, NULL, now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'México'),
            region_id = NULL,
            ciudad_id = NULL,
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Entretenimiento y artistas'),
            contacto = COALESCE(proveedores.contacto, 'YURI FERNANDEZ'),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, NULL),
            email = COALESCE(proveedores.email, NULL),
            web = COALESCE(proveedores.web, NULL),
            direccion = COALESCE(proveedores.direccion, NULL),
            aforo = COALESCE(proveedores.aforo, NULL),
            costo_referencia = COALESCE(proveedores.costo_referencia, NULL),
            notas = COALESCE(proveedores.notas, NULL),
            updated_at = now()
        WHERE id = v_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_id, vv.t FROM (VALUES ('5630076662')) AS vv(t)
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_id AND telefono = vv.t);
END $blk$;

-- Proveedor: KAPSULA
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'KAPSULA' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'KAPSULA',
            (SELECT id FROM paises WHERE nombre = 'México'),
            NULL,
            NULL,
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Entretenimiento y artistas'),
            'Activo', 'EUYIN MONREAL', NULL, NULL, NULL, NULL, NULL, NULL, NULL, now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'México'),
            region_id = NULL,
            ciudad_id = NULL,
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Entretenimiento y artistas'),
            contacto = COALESCE(proveedores.contacto, 'EUYIN MONREAL'),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, NULL),
            email = COALESCE(proveedores.email, NULL),
            web = COALESCE(proveedores.web, NULL),
            direccion = COALESCE(proveedores.direccion, NULL),
            aforo = COALESCE(proveedores.aforo, NULL),
            costo_referencia = COALESCE(proveedores.costo_referencia, NULL),
            notas = COALESCE(proveedores.notas, NULL),
            updated_at = now()
        WHERE id = v_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_id, vv.t FROM (VALUES ('5564306265')) AS vv(t)
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_id AND telefono = vv.t);
END $blk$;

-- Proveedor: MIX ON
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'MIX ON' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'MIX ON',
            (SELECT id FROM paises WHERE nombre = 'México'),
            NULL,
            NULL,
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Entretenimiento y artistas'),
            'Activo', 'PAOLO HARDEN COOPER', NULL, 'paolo@liveandhappy.com.mx', 'mixon.mx', NULL, NULL, NULL, NULL, now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'México'),
            region_id = NULL,
            ciudad_id = NULL,
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Entretenimiento y artistas'),
            contacto = COALESCE(proveedores.contacto, 'PAOLO HARDEN COOPER'),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, NULL),
            email = COALESCE(proveedores.email, 'paolo@liveandhappy.com.mx'),
            web = COALESCE(proveedores.web, 'mixon.mx'),
            direccion = COALESCE(proveedores.direccion, NULL),
            aforo = COALESCE(proveedores.aforo, NULL),
            costo_referencia = COALESCE(proveedores.costo_referencia, NULL),
            notas = COALESCE(proveedores.notas, NULL),
            updated_at = now()
        WHERE id = v_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_id, vv.t FROM (VALUES ('5535507806')) AS vv(t)
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_id AND telefono = vv.t);
END $blk$;

-- Proveedor: PMR
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'PMR' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'PMR',
            (SELECT id FROM paises WHERE nombre = 'México'),
            NULL,
            NULL,
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Entretenimiento y artistas'),
            'Activo', 'ALEC SAAVEDRA', NULL, 'contrataciones@pmrgroup.com.mx', 'pmrgroup.com.mx', NULL, NULL, NULL, NULL, now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'México'),
            region_id = NULL,
            ciudad_id = NULL,
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Entretenimiento y artistas'),
            contacto = COALESCE(proveedores.contacto, 'ALEC SAAVEDRA'),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, NULL),
            email = COALESCE(proveedores.email, 'contrataciones@pmrgroup.com.mx'),
            web = COALESCE(proveedores.web, 'pmrgroup.com.mx'),
            direccion = COALESCE(proveedores.direccion, NULL),
            aforo = COALESCE(proveedores.aforo, NULL),
            costo_referencia = COALESCE(proveedores.costo_referencia, NULL),
            notas = COALESCE(proveedores.notas, NULL),
            updated_at = now()
        WHERE id = v_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_id, vv.t FROM (VALUES ('5559677660')) AS vv(t)
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_id AND telefono = vv.t);
END $blk$;

-- Proveedor: ADIF PRORENTA
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'ADIF PRORENTA' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'ADIF PRORENTA',
            (SELECT id FROM paises WHERE nombre = 'México'),
            (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Iluminación y sonido'),
            'Activo', 'JAZMIN ROMERO', NULL, 'rentas@rentaequipo.com.mx', 'rentadelaptops.org', NULL, NULL, NULL, 'Ciudad original en el Excel: "CDMX"', now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'México'),
            region_id = (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            ciudad_id = (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Iluminación y sonido'),
            contacto = COALESCE(proveedores.contacto, 'JAZMIN ROMERO'),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, NULL),
            email = COALESCE(proveedores.email, 'rentas@rentaequipo.com.mx'),
            web = COALESCE(proveedores.web, 'rentadelaptops.org'),
            direccion = COALESCE(proveedores.direccion, NULL),
            aforo = COALESCE(proveedores.aforo, NULL),
            costo_referencia = COALESCE(proveedores.costo_referencia, NULL),
            notas = COALESCE(proveedores.notas, 'Ciudad original en el Excel: "CDMX"'),
            updated_at = now()
        WHERE id = v_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_id, vv.t FROM (VALUES ('5556119068')) AS vv(t)
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_id AND telefono = vv.t);
END $blk$;

-- Proveedor: JBL
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'JBL' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'JBL',
            (SELECT id FROM paises WHERE nombre = 'México'),
            (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Iluminación y sonido'),
            'Activo', 'CARLOS SANTANA', NULL, NULL, NULL, NULL, NULL, NULL, 'Ciudad original en el Excel: "CDMX"', now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'México'),
            region_id = (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            ciudad_id = (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Iluminación y sonido'),
            contacto = COALESCE(proveedores.contacto, 'CARLOS SANTANA'),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, NULL),
            email = COALESCE(proveedores.email, NULL),
            web = COALESCE(proveedores.web, NULL),
            direccion = COALESCE(proveedores.direccion, NULL),
            aforo = COALESCE(proveedores.aforo, NULL),
            costo_referencia = COALESCE(proveedores.costo_referencia, NULL),
            notas = COALESCE(proveedores.notas, 'Ciudad original en el Excel: "CDMX"'),
            updated_at = now()
        WHERE id = v_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_id, vv.t FROM (VALUES ('5553281021')) AS vv(t)
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_id AND telefono = vv.t);
END $blk$;

-- Proveedor: KC RENTA
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'KC RENTA' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'KC RENTA',
            (SELECT id FROM paises WHERE nombre = 'México'),
            (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Iluminación y sonido'),
            'Activo', 'DAVID REYES', NULL, NULL, 'rentaequipocomputo.mx', NULL, NULL, NULL, 'Ciudad original en el Excel: "CDMX"', now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'México'),
            region_id = (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            ciudad_id = (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Iluminación y sonido'),
            contacto = COALESCE(proveedores.contacto, 'DAVID REYES'),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, NULL),
            email = COALESCE(proveedores.email, NULL),
            web = COALESCE(proveedores.web, 'rentaequipocomputo.mx'),
            direccion = COALESCE(proveedores.direccion, NULL),
            aforo = COALESCE(proveedores.aforo, NULL),
            costo_referencia = COALESCE(proveedores.costo_referencia, NULL),
            notas = COALESCE(proveedores.notas, 'Ciudad original en el Excel: "CDMX"'),
            updated_at = now()
        WHERE id = v_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_id, vv.t FROM (VALUES ('5548730267'), ('EXT-415')) AS vv(t)
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_id AND telefono = vv.t);
END $blk$;

-- Proveedor: REYES RECORDING
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'REYES RECORDING' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'REYES RECORDING',
            (SELECT id FROM paises WHERE nombre = 'México'),
            (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Iluminación y sonido'),
            'Activo', 'FERNANDO REYES', 'DIRECTOR GENERAL', 'reyesrecording@gmail.com', NULL, NULL, NULL, NULL, 'Ciudad original en el Excel: "CDMX"', now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'México'),
            region_id = (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            ciudad_id = (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Iluminación y sonido'),
            contacto = COALESCE(proveedores.contacto, 'FERNANDO REYES'),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, 'DIRECTOR GENERAL'),
            email = COALESCE(proveedores.email, 'reyesrecording@gmail.com'),
            web = COALESCE(proveedores.web, NULL),
            direccion = COALESCE(proveedores.direccion, NULL),
            aforo = COALESCE(proveedores.aforo, NULL),
            costo_referencia = COALESCE(proveedores.costo_referencia, NULL),
            notas = COALESCE(proveedores.notas, 'Ciudad original en el Excel: "CDMX"'),
            updated_at = now()
        WHERE id = v_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_id, vv.t FROM (VALUES ('5549546830')) AS vv(t)
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_id AND telefono = vv.t);
END $blk$;

-- Proveedor: RUIDO BLANCO
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'RUIDO BLANCO' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'RUIDO BLANCO',
            (SELECT id FROM paises WHERE nombre = 'México'),
            (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Iluminación y sonido'),
            'Activo', 'RAUL VILLA', 'DIRECTOR GENERAL', 'raul.ruidoblanco@gmail.com', NULL, NULL, NULL, NULL, 'Ciudad original en el Excel: "CDMX"', now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'México'),
            region_id = (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            ciudad_id = (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Iluminación y sonido'),
            contacto = COALESCE(proveedores.contacto, 'RAUL VILLA'),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, 'DIRECTOR GENERAL'),
            email = COALESCE(proveedores.email, 'raul.ruidoblanco@gmail.com'),
            web = COALESCE(proveedores.web, NULL),
            direccion = COALESCE(proveedores.direccion, NULL),
            aforo = COALESCE(proveedores.aforo, NULL),
            costo_referencia = COALESCE(proveedores.costo_referencia, NULL),
            notas = COALESCE(proveedores.notas, 'Ciudad original en el Excel: "CDMX"'),
            updated_at = now()
        WHERE id = v_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_id, vv.t FROM (VALUES ('5519262587')) AS vv(t)
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_id AND telefono = vv.t);
END $blk$;

-- Proveedor: AUDIO HERTZ
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'AUDIO HERTZ' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'AUDIO HERTZ',
            (SELECT id FROM paises WHERE nombre = 'México'),
            (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Monterrey' LIMIT 1),
            (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Monterrey' LIMIT 1),
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Iluminación y sonido'),
            'Activo', 'RODOLFO BRAVO', NULL, 'audiohertzmty@gmail.com', 'audiohertzmty.com', NULL, NULL, NULL, NULL, now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'México'),
            region_id = (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Monterrey' LIMIT 1),
            ciudad_id = (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Monterrey' LIMIT 1),
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Iluminación y sonido'),
            contacto = COALESCE(proveedores.contacto, 'RODOLFO BRAVO'),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, NULL),
            email = COALESCE(proveedores.email, 'audiohertzmty@gmail.com'),
            web = COALESCE(proveedores.web, 'audiohertzmty.com'),
            direccion = COALESCE(proveedores.direccion, NULL),
            aforo = COALESCE(proveedores.aforo, NULL),
            costo_referencia = COALESCE(proveedores.costo_referencia, NULL),
            notas = COALESCE(proveedores.notas, NULL),
            updated_at = now()
        WHERE id = v_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_id, vv.t FROM (VALUES ('8180190035')) AS vv(t)
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_id AND telefono = vv.t);
END $blk$;

-- Proveedor: AUDIO PRODUCCIONES
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'AUDIO PRODUCCIONES' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'AUDIO PRODUCCIONES',
            (SELECT id FROM paises WHERE nombre = 'México'),
            (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Monterrey' LIMIT 1),
            (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Monterrey' LIMIT 1),
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Iluminación y sonido'),
            'Activo', 'OSCAR TREVIÑO', NULL, 'audioproducciones02@gmail.com', NULL, NULL, NULL, NULL, NULL, now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'México'),
            region_id = (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Monterrey' LIMIT 1),
            ciudad_id = (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Monterrey' LIMIT 1),
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Iluminación y sonido'),
            contacto = COALESCE(proveedores.contacto, 'OSCAR TREVIÑO'),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, NULL),
            email = COALESCE(proveedores.email, 'audioproducciones02@gmail.com'),
            web = COALESCE(proveedores.web, NULL),
            direccion = COALESCE(proveedores.direccion, NULL),
            aforo = COALESCE(proveedores.aforo, NULL),
            costo_referencia = COALESCE(proveedores.costo_referencia, NULL),
            notas = COALESCE(proveedores.notas, NULL),
            updated_at = now()
        WHERE id = v_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_id, vv.t FROM (VALUES ('8112690354')) AS vv(t)
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_id AND telefono = vv.t);
END $blk$;

-- Proveedor: GLAM CAM
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'GLAM CAM' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'GLAM CAM',
            (SELECT id FROM paises WHERE nombre = 'México'),
            (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Otro'),
            'Activo', NULL, NULL, NULL, 'glamcam.mx', NULL, NULL, NULL, 'Ciudad original en el Excel: "CDMX"', now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'México'),
            region_id = (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            ciudad_id = (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Otro'),
            contacto = COALESCE(proveedores.contacto, NULL),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, NULL),
            email = COALESCE(proveedores.email, NULL),
            web = COALESCE(proveedores.web, 'glamcam.mx'),
            direccion = COALESCE(proveedores.direccion, NULL),
            aforo = COALESCE(proveedores.aforo, NULL),
            costo_referencia = COALESCE(proveedores.costo_referencia, NULL),
            notas = COALESCE(proveedores.notas, 'Ciudad original en el Excel: "CDMX"'),
            updated_at = now()
        WHERE id = v_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_id, vv.t FROM (VALUES ('5578835147')) AS vv(t)
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_id AND telefono = vv.t);
END $blk$;

-- Proveedor: ISANISO
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'ISANISO' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'ISANISO',
            (SELECT id FROM paises WHERE nombre = 'México'),
            (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Otro'),
            'Activo', 'CASA DE CAMBIO', NULL, NULL, NULL, 'LIVERPOOL 140', NULL, NULL, 'C 19.60 V. 20.20 · Ciudad original en el Excel: "CDMX"', now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'México'),
            region_id = (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            ciudad_id = (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Otro'),
            contacto = COALESCE(proveedores.contacto, 'CASA DE CAMBIO'),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, NULL),
            email = COALESCE(proveedores.email, NULL),
            web = COALESCE(proveedores.web, NULL),
            direccion = COALESCE(proveedores.direccion, 'LIVERPOOL 140'),
            aforo = COALESCE(proveedores.aforo, NULL),
            costo_referencia = COALESCE(proveedores.costo_referencia, NULL),
            notas = COALESCE(proveedores.notas, 'C 19.60 V. 20.20 · Ciudad original en el Excel: "CDMX"'),
            updated_at = now()
        WHERE id = v_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_id, vv.t FROM (VALUES ('5555251901')) AS vv(t)
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_id AND telefono = vv.t);
END $blk$;

-- Proveedor: SIGMAAC ASESORES
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'SIGMAAC ASESORES' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'SIGMAAC ASESORES',
            (SELECT id FROM paises WHERE nombre = 'México'),
            (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Producción técnica (rigging, generadores, carpas)'),
            'Activo', 'ISABEL MURILLO', NULL, NULL, 'sigmaac.com.mx', NULL, NULL, NULL, 'Ciudad original en el Excel: "CDMX"', now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'México'),
            region_id = (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            ciudad_id = (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Producción técnica (rigging, generadores, carpas)'),
            contacto = COALESCE(proveedores.contacto, 'ISABEL MURILLO'),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, NULL),
            email = COALESCE(proveedores.email, NULL),
            web = COALESCE(proveedores.web, 'sigmaac.com.mx'),
            direccion = COALESCE(proveedores.direccion, NULL),
            aforo = COALESCE(proveedores.aforo, NULL),
            costo_referencia = COALESCE(proveedores.costo_referencia, NULL),
            notas = COALESCE(proveedores.notas, 'Ciudad original en el Excel: "CDMX"'),
            updated_at = now()
        WHERE id = v_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_id, vv.t FROM (VALUES ('5545628155'), ('3313477604')) AS vv(t)
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_id AND telefono = vv.t);
END $blk$;

-- Proveedor: NEXT COLOMBIA
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'NEXT COLOMBIA' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'NEXT COLOMBIA',
            (SELECT id FROM paises WHERE nombre = 'México'),
            NULL,
            NULL,
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Otro'),
            'Activo', 'ISABEL GARCÍA', 'CONTADOR', 'cp.igarciaspf@gmail.com', NULL, NULL, NULL, NULL, 'Otros correos en el Excel para este contacto (solo se guarda uno como Email): isgago82@hotmail.com', now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'México'),
            region_id = NULL,
            ciudad_id = NULL,
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Otro'),
            contacto = COALESCE(proveedores.contacto, 'ISABEL GARCÍA'),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, 'CONTADOR'),
            email = COALESCE(proveedores.email, 'cp.igarciaspf@gmail.com'),
            web = COALESCE(proveedores.web, NULL),
            direccion = COALESCE(proveedores.direccion, NULL),
            aforo = COALESCE(proveedores.aforo, NULL),
            costo_referencia = COALESCE(proveedores.costo_referencia, NULL),
            notas = COALESCE(proveedores.notas, 'Otros correos en el Excel para este contacto (solo se guarda uno como Email): isgago82@hotmail.com'),
            updated_at = now()
        WHERE id = v_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_id, vv.t FROM (VALUES ('5522963699')) AS vv(t)
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_id AND telefono = vv.t);
END $blk$;

-- Proveedor: GLOBOLÍN
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'GLOBOLÍN' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'GLOBOLÍN',
            (SELECT id FROM paises WHERE nombre = 'México'),
            NULL,
            NULL,
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Otro'),
            'Activo', 'ADRIANA BUSTANI', NULL, 'adriana.bustani@gmail.com', NULL, NULL, NULL, NULL, NULL, now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'México'),
            region_id = NULL,
            ciudad_id = NULL,
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Otro'),
            contacto = COALESCE(proveedores.contacto, 'ADRIANA BUSTANI'),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, NULL),
            email = COALESCE(proveedores.email, 'adriana.bustani@gmail.com'),
            web = COALESCE(proveedores.web, NULL),
            direccion = COALESCE(proveedores.direccion, NULL),
            aforo = COALESCE(proveedores.aforo, NULL),
            costo_referencia = COALESCE(proveedores.costo_referencia, NULL),
            notas = COALESCE(proveedores.notas, NULL),
            updated_at = now()
        WHERE id = v_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_id, vv.t FROM (VALUES ('5538673250')) AS vv(t)
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_id AND telefono = vv.t);
END $blk$;

-- Proveedor: MULTICONGRESS
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'MULTICONGRESS' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'MULTICONGRESS',
            (SELECT id FROM paises WHERE nombre = 'México'),
            (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Acapulco' LIMIT 1),
            (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Acapulco' LIMIT 1),
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Producción técnica (rigging, generadores, carpas)'),
            'Activo', 'LUZ BELLO', 'EJECUTIVA DE CUENTA', 'ventas.gpo@multicongress.com', 'multicongress.com', NULL, NULL, NULL, NULL, now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'México'),
            region_id = (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Acapulco' LIMIT 1),
            ciudad_id = (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Acapulco' LIMIT 1),
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Producción técnica (rigging, generadores, carpas)'),
            contacto = COALESCE(proveedores.contacto, 'LUZ BELLO'),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, 'EJECUTIVA DE CUENTA'),
            email = COALESCE(proveedores.email, 'ventas.gpo@multicongress.com'),
            web = COALESCE(proveedores.web, 'multicongress.com'),
            direccion = COALESCE(proveedores.direccion, NULL),
            aforo = COALESCE(proveedores.aforo, NULL),
            costo_referencia = COALESCE(proveedores.costo_referencia, NULL),
            notas = COALESCE(proveedores.notas, NULL),
            updated_at = now()
        WHERE id = v_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_id, vv.t FROM (VALUES ('7443819077')) AS vv(t)
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_id AND telefono = vv.t);
END $blk$;

-- Proveedor: DISPLAY MAYA
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'DISPLAY MAYA' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'DISPLAY MAYA',
            (SELECT id FROM paises WHERE nombre = 'México'),
            (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Cancún' LIMIT 1),
            (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Cancún' LIMIT 1),
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Producción técnica (rigging, generadores, carpas)'),
            'Activo', 'ISAAC GARCIA', NULL, 'displaymaya.ventas@gmail.com', 'displaymaya.com', NULL, NULL, NULL, NULL, now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'México'),
            region_id = (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Cancún' LIMIT 1),
            ciudad_id = (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Cancún' LIMIT 1),
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Producción técnica (rigging, generadores, carpas)'),
            contacto = COALESCE(proveedores.contacto, 'ISAAC GARCIA'),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, NULL),
            email = COALESCE(proveedores.email, 'displaymaya.ventas@gmail.com'),
            web = COALESCE(proveedores.web, 'displaymaya.com'),
            direccion = COALESCE(proveedores.direccion, NULL),
            aforo = COALESCE(proveedores.aforo, NULL),
            costo_referencia = COALESCE(proveedores.costo_referencia, NULL),
            notas = COALESCE(proveedores.notas, NULL),
            updated_at = now()
        WHERE id = v_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_id, vv.t FROM (VALUES ('9984628336')) AS vv(t)
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_id AND telefono = vv.t);
END $blk$;

-- Proveedor: ACTUAL PRODUCCIONES
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'ACTUAL PRODUCCIONES' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'ACTUAL PRODUCCIONES',
            (SELECT id FROM paises WHERE nombre = 'México'),
            (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Producción técnica (rigging, generadores, carpas)'),
            'Activo', 'MANUEL LAMA', 'DIRECTOR GENERAL', 'manuel.lama@actualproducciones.mx', 'actualproducciones.com.mx', NULL, NULL, NULL, 'Ciudad original en el Excel: "CDMX"', now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'México'),
            region_id = (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            ciudad_id = (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Producción técnica (rigging, generadores, carpas)'),
            contacto = COALESCE(proveedores.contacto, 'MANUEL LAMA'),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, 'DIRECTOR GENERAL'),
            email = COALESCE(proveedores.email, 'manuel.lama@actualproducciones.mx'),
            web = COALESCE(proveedores.web, 'actualproducciones.com.mx'),
            direccion = COALESCE(proveedores.direccion, NULL),
            aforo = COALESCE(proveedores.aforo, NULL),
            costo_referencia = COALESCE(proveedores.costo_referencia, NULL),
            notas = COALESCE(proveedores.notas, 'Ciudad original en el Excel: "CDMX"'),
            updated_at = now()
        WHERE id = v_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_id, vv.t FROM (VALUES ('5543703782')) AS vv(t)
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_id AND telefono = vv.t);
END $blk$;

-- Proveedor: EVOS
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'EVOS' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'EVOS',
            (SELECT id FROM paises WHERE nombre = 'México'),
            (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Producción técnica (rigging, generadores, carpas)'),
            'Activo', 'OMAR SOLARES', NULL, NULL, NULL, NULL, NULL, NULL, 'NO TRABAJAR CON ESE PROVEEDOR · Ciudad original en el Excel: "CDMX"', now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'México'),
            region_id = (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            ciudad_id = (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Producción técnica (rigging, generadores, carpas)'),
            contacto = COALESCE(proveedores.contacto, 'OMAR SOLARES'),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, NULL),
            email = COALESCE(proveedores.email, NULL),
            web = COALESCE(proveedores.web, NULL),
            direccion = COALESCE(proveedores.direccion, NULL),
            aforo = COALESCE(proveedores.aforo, NULL),
            costo_referencia = COALESCE(proveedores.costo_referencia, NULL),
            notas = COALESCE(proveedores.notas, 'NO TRABAJAR CON ESE PROVEEDOR · Ciudad original en el Excel: "CDMX"'),
            updated_at = now()
        WHERE id = v_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_id, vv.t FROM (VALUES ('5528788663')) AS vv(t)
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_id AND telefono = vv.t);
END $blk$;

-- Proveedor: GRADO DE ALTURA
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'GRADO DE ALTURA' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'GRADO DE ALTURA',
            (SELECT id FROM paises WHERE nombre = 'México'),
            (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Producción técnica (rigging, generadores, carpas)'),
            'Activo', 'DAVID', NULL, 'david@gradodealtura.com', 'gradodealtura.com', NULL, NULL, NULL, 'Ciudad original en el Excel: "CDMX"', now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'México'),
            region_id = (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            ciudad_id = (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Producción técnica (rigging, generadores, carpas)'),
            contacto = COALESCE(proveedores.contacto, 'DAVID'),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, NULL),
            email = COALESCE(proveedores.email, 'david@gradodealtura.com'),
            web = COALESCE(proveedores.web, 'gradodealtura.com'),
            direccion = COALESCE(proveedores.direccion, NULL),
            aforo = COALESCE(proveedores.aforo, NULL),
            costo_referencia = COALESCE(proveedores.costo_referencia, NULL),
            notas = COALESCE(proveedores.notas, 'Ciudad original en el Excel: "CDMX"'),
            updated_at = now()
        WHERE id = v_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_id, vv.t FROM (VALUES ('5540100311')) AS vv(t)
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_id AND telefono = vv.t);
END $blk$;

-- Proveedor: MOMA GROUP
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'MOMA GROUP' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'MOMA GROUP',
            (SELECT id FROM paises WHERE nombre = 'México'),
            (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Producción técnica (rigging, generadores, carpas)'),
            'Activo', 'MARCELINO SUAREZ', 'DIRECTOR GENERAL', 'marcelino@momagroup.com.mx', 'momagroup.com.mx', NULL, NULL, NULL, 'Ciudad original en el Excel: "CDMX" · Otros correos en el Excel para este contacto (solo se guarda uno como Email): marcelino.momagroup@gmail.com', now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'México'),
            region_id = (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            ciudad_id = (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Producción técnica (rigging, generadores, carpas)'),
            contacto = COALESCE(proveedores.contacto, 'MARCELINO SUAREZ'),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, 'DIRECTOR GENERAL'),
            email = COALESCE(proveedores.email, 'marcelino@momagroup.com.mx'),
            web = COALESCE(proveedores.web, 'momagroup.com.mx'),
            direccion = COALESCE(proveedores.direccion, NULL),
            aforo = COALESCE(proveedores.aforo, NULL),
            costo_referencia = COALESCE(proveedores.costo_referencia, NULL),
            notas = COALESCE(proveedores.notas, 'Ciudad original en el Excel: "CDMX" · Otros correos en el Excel para este contacto (solo se guarda uno como Email): marcelino.momagroup@gmail.com'),
            updated_at = now()
        WHERE id = v_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_id, vv.t FROM (VALUES ('5541454536')) AS vv(t)
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_id AND telefono = vv.t);
END $blk$;

-- Proveedor: SENZU PRODUCCION
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'SENZU PRODUCCION' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'SENZU PRODUCCION',
            (SELECT id FROM paises WHERE nombre = 'México'),
            (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Producción técnica (rigging, generadores, carpas)'),
            'Activo', 'EDUARDO ANZURES', 'DIRECTOR', 'eduardo.anzures@senzuproduccion.com.mx', 'senzuproduccion.com.mx', NULL, NULL, NULL, 'Ciudad original en el Excel: "CDMX"', now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'México'),
            region_id = (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            ciudad_id = (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Producción técnica (rigging, generadores, carpas)'),
            contacto = COALESCE(proveedores.contacto, 'EDUARDO ANZURES'),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, 'DIRECTOR'),
            email = COALESCE(proveedores.email, 'eduardo.anzures@senzuproduccion.com.mx'),
            web = COALESCE(proveedores.web, 'senzuproduccion.com.mx'),
            direccion = COALESCE(proveedores.direccion, NULL),
            aforo = COALESCE(proveedores.aforo, NULL),
            costo_referencia = COALESCE(proveedores.costo_referencia, NULL),
            notas = COALESCE(proveedores.notas, 'Ciudad original en el Excel: "CDMX"'),
            updated_at = now()
        WHERE id = v_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_id, vv.t FROM (VALUES ('2223231150')) AS vv(t)
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_id AND telefono = vv.t);
END $blk$;

-- Proveedor: SKYROCKET
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'SKYROCKET' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'SKYROCKET',
            (SELECT id FROM paises WHERE nombre = 'México'),
            (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Monterrey' LIMIT 1),
            (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Monterrey' LIMIT 1),
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Producción técnica (rigging, generadores, carpas)'),
            'Activo', 'VANESSA FLORES', 'DIRECTORA COMERCIAL', 'vflores@skyrocket.com.mx', 'skyrocket.com.mx', NULL, NULL, NULL, NULL, now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'México'),
            region_id = (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Monterrey' LIMIT 1),
            ciudad_id = (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Monterrey' LIMIT 1),
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Producción técnica (rigging, generadores, carpas)'),
            contacto = COALESCE(proveedores.contacto, 'VANESSA FLORES'),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, 'DIRECTORA COMERCIAL'),
            email = COALESCE(proveedores.email, 'vflores@skyrocket.com.mx'),
            web = COALESCE(proveedores.web, 'skyrocket.com.mx'),
            direccion = COALESCE(proveedores.direccion, NULL),
            aforo = COALESCE(proveedores.aforo, NULL),
            costo_referencia = COALESCE(proveedores.costo_referencia, NULL),
            notas = COALESCE(proveedores.notas, NULL),
            updated_at = now()
        WHERE id = v_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_id, vv.t FROM (VALUES ('8117713968'), ('8119765203')) AS vv(t)
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_id AND telefono = vv.t);
END $blk$;

-- Proveedor: SION PRODUCCIONES
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'SION PRODUCCIONES' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'SION PRODUCCIONES',
            (SELECT id FROM paises WHERE nombre = 'México'),
            (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Producción técnica (rigging, generadores, carpas)'),
            'Activo', 'BRENDA VAZQUEZ', NULL, 'ventas.sion@hotmail.com', 'sionproducciones.jimdo.com', NULL, NULL, NULL, 'Ciudad original en el Excel: "CDMX"', now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'México'),
            region_id = (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            ciudad_id = (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Producción técnica (rigging, generadores, carpas)'),
            contacto = COALESCE(proveedores.contacto, 'BRENDA VAZQUEZ'),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, NULL),
            email = COALESCE(proveedores.email, 'ventas.sion@hotmail.com'),
            web = COALESCE(proveedores.web, 'sionproducciones.jimdo.com'),
            direccion = COALESCE(proveedores.direccion, NULL),
            aforo = COALESCE(proveedores.aforo, NULL),
            costo_referencia = COALESCE(proveedores.costo_referencia, NULL),
            notas = COALESCE(proveedores.notas, 'Ciudad original en el Excel: "CDMX"'),
            updated_at = now()
        WHERE id = v_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_id, vv.t FROM (VALUES ('5553499200')) AS vv(t)
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_id AND telefono = vv.t);
END $blk$;

-- Proveedor: PELUCHES GT
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'PELUCHES GT' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'PELUCHES GT',
            (SELECT id FROM paises WHERE nombre = 'México'),
            (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Merchandising y regalos corporativos'),
            'Activo', 'FERNANDO GONZALEZ', NULL, 'fabricadepeluchesgt@gmail.com', 'fabricadepeluchesgt.com', 'Calle Av. Necaxa 10 Col. La Laguna, Tlanepantla', NULL, NULL, 'Ciudad original en el Excel: "CDMX" · Ciudad original en el Excel: "EDOMEX"', now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'México'),
            region_id = (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            ciudad_id = (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Merchandising y regalos corporativos'),
            contacto = COALESCE(proveedores.contacto, 'FERNANDO GONZALEZ'),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, NULL),
            email = COALESCE(proveedores.email, 'fabricadepeluchesgt@gmail.com'),
            web = COALESCE(proveedores.web, 'fabricadepeluchesgt.com'),
            direccion = COALESCE(proveedores.direccion, 'Calle Av. Necaxa 10 Col. La Laguna, Tlanepantla'),
            aforo = COALESCE(proveedores.aforo, NULL),
            costo_referencia = COALESCE(proveedores.costo_referencia, NULL),
            notas = COALESCE(proveedores.notas, 'Ciudad original en el Excel: "CDMX" · Ciudad original en el Excel: "EDOMEX"'),
            updated_at = now()
        WHERE id = v_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_id, vv.t FROM (VALUES ('5529796764')) AS vv(t)
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_id AND telefono = vv.t);
END $blk$;

-- Proveedor: FLORESTTA
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'FLORESTTA' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'FLORESTTA',
            (SELECT id FROM paises WHERE nombre = 'México'),
            (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Flores y ambientación'),
            'Activo', 'MARIA GOMEZ', NULL, 'maria@florestta.com', 'florestta.com', NULL, NULL, NULL, 'Ciudad original en el Excel: "CDMX"', now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'México'),
            region_id = (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            ciudad_id = (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Flores y ambientación'),
            contacto = COALESCE(proveedores.contacto, 'MARIA GOMEZ'),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, NULL),
            email = COALESCE(proveedores.email, 'maria@florestta.com'),
            web = COALESCE(proveedores.web, 'florestta.com'),
            direccion = COALESCE(proveedores.direccion, NULL),
            aforo = COALESCE(proveedores.aforo, NULL),
            costo_referencia = COALESCE(proveedores.costo_referencia, NULL),
            notas = COALESCE(proveedores.notas, 'Ciudad original en el Excel: "CDMX"'),
            updated_at = now()
        WHERE id = v_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_id, vv.t FROM (VALUES ('5533996373')) AS vv(t)
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_id AND telefono = vv.t);
END $blk$;

-- Proveedor: LOVE & CHARM
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'LOVE & CHARM' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'LOVE & CHARM',
            (SELECT id FROM paises WHERE nombre = 'México'),
            NULL,
            NULL,
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Flores y ambientación'),
            'Activo', 'LINDA DE ARRAYALES', NULL, 'loveandcharm@outlook.com', NULL, NULL, NULL, NULL, NULL, now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'México'),
            region_id = NULL,
            ciudad_id = NULL,
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Flores y ambientación'),
            contacto = COALESCE(proveedores.contacto, 'LINDA DE ARRAYALES'),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, NULL),
            email = COALESCE(proveedores.email, 'loveandcharm@outlook.com'),
            web = COALESCE(proveedores.web, NULL),
            direccion = COALESCE(proveedores.direccion, NULL),
            aforo = COALESCE(proveedores.aforo, NULL),
            costo_referencia = COALESCE(proveedores.costo_referencia, NULL),
            notas = COALESCE(proveedores.notas, NULL),
            updated_at = now()
        WHERE id = v_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_id, vv.t FROM (VALUES ('7773606694')) AS vv(t)
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_id AND telefono = vv.t);
END $blk$;

-- Proveedor: REFLECTUS
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'REFLECTUS' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'REFLECTUS',
            (SELECT id FROM paises WHERE nombre = 'México'),
            (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Otro'),
            'Activo', 'OMAR GUTIERREZ', NULL, 'reflectusfv@gmail.com', NULL, NULL, NULL, NULL, 'Ciudad original en el Excel: "CDMX"', now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'México'),
            region_id = (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            ciudad_id = (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Otro'),
            contacto = COALESCE(proveedores.contacto, 'OMAR GUTIERREZ'),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, NULL),
            email = COALESCE(proveedores.email, 'reflectusfv@gmail.com'),
            web = COALESCE(proveedores.web, NULL),
            direccion = COALESCE(proveedores.direccion, NULL),
            aforo = COALESCE(proveedores.aforo, NULL),
            costo_referencia = COALESCE(proveedores.costo_referencia, NULL),
            notas = COALESCE(proveedores.notas, 'Ciudad original en el Excel: "CDMX"'),
            updated_at = now()
        WHERE id = v_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_id, vv.t FROM (VALUES ('5538772030')) AS vv(t)
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_id AND telefono = vv.t);
END $blk$;

-- Proveedor: INKS
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'INKS' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'INKS',
            (SELECT id FROM paises WHERE nombre = 'México'),
            (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Impresos y señalética'),
            'Activo', 'MATIAS', NULL, 'ventas.2inks@gmail.com', NULL, NULL, NULL, NULL, 'Ciudad original en el Excel: "CDMX"', now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'México'),
            region_id = (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            ciudad_id = (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Impresos y señalética'),
            contacto = COALESCE(proveedores.contacto, 'MATIAS'),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, NULL),
            email = COALESCE(proveedores.email, 'ventas.2inks@gmail.com'),
            web = COALESCE(proveedores.web, NULL),
            direccion = COALESCE(proveedores.direccion, NULL),
            aforo = COALESCE(proveedores.aforo, NULL),
            costo_referencia = COALESCE(proveedores.costo_referencia, NULL),
            notas = COALESCE(proveedores.notas, 'Ciudad original en el Excel: "CDMX"'),
            updated_at = now()
        WHERE id = v_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_id, vv.t FROM (VALUES ('5526147001')) AS vv(t)
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_id AND telefono = vv.t);
END $blk$;

-- Proveedor: IMPRESAR
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'IMPRESAR' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'IMPRESAR',
            (SELECT id FROM paises WHERE nombre = 'México'),
            (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Toluca' LIMIT 1),
            (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Toluca' LIMIT 1),
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Impresos y señalética'),
            'Activo', 'ARTURO CARMONA', NULL, 'pedidos@impresar.com.mx', 'impresar.mx', NULL, NULL, NULL, 'Otros correos en el Excel para este contacto (solo se guarda uno como Email): impresar@hotmail.com', now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'México'),
            region_id = (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Toluca' LIMIT 1),
            ciudad_id = (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Toluca' LIMIT 1),
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Impresos y señalética'),
            contacto = COALESCE(proveedores.contacto, 'ARTURO CARMONA'),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, NULL),
            email = COALESCE(proveedores.email, 'pedidos@impresar.com.mx'),
            web = COALESCE(proveedores.web, 'impresar.mx'),
            direccion = COALESCE(proveedores.direccion, NULL),
            aforo = COALESCE(proveedores.aforo, NULL),
            costo_referencia = COALESCE(proveedores.costo_referencia, NULL),
            notas = COALESCE(proveedores.notas, 'Otros correos en el Excel para este contacto (solo se guarda uno como Email): impresar@hotmail.com'),
            updated_at = now()
        WHERE id = v_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_id, vv.t FROM (VALUES ('5545934970')) AS vv(t)
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_id AND telefono = vv.t);
END $blk$;

-- Proveedor: IMPRIMIMOS
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'IMPRIMIMOS' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'IMPRIMIMOS',
            (SELECT id FROM paises WHERE nombre = 'México'),
            (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Toluca' LIMIT 1),
            (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Toluca' LIMIT 1),
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Impresos y señalética'),
            'Activo', 'IVONNE ROGEL', NULL, 'imprexos@gmail.com', NULL, NULL, NULL, NULL, NULL, now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'México'),
            region_id = (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Toluca' LIMIT 1),
            ciudad_id = (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Toluca' LIMIT 1),
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Impresos y señalética'),
            contacto = COALESCE(proveedores.contacto, 'IVONNE ROGEL'),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, NULL),
            email = COALESCE(proveedores.email, 'imprexos@gmail.com'),
            web = COALESCE(proveedores.web, NULL),
            direccion = COALESCE(proveedores.direccion, NULL),
            aforo = COALESCE(proveedores.aforo, NULL),
            costo_referencia = COALESCE(proveedores.costo_referencia, NULL),
            notas = COALESCE(proveedores.notas, NULL),
            updated_at = now()
        WHERE id = v_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_id, vv.t FROM (VALUES ('7222121949')) AS vv(t)
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_id AND telefono = vv.t);
END $blk$;

-- Proveedor: STAMPA DIGITAL
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'STAMPA DIGITAL' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'STAMPA DIGITAL',
            (SELECT id FROM paises WHERE nombre = 'México'),
            (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Toluca' LIMIT 1),
            (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Toluca' LIMIT 1),
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Impresos y señalética'),
            'Activo', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'México'),
            region_id = (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Toluca' LIMIT 1),
            ciudad_id = (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Toluca' LIMIT 1),
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Impresos y señalética'),
            contacto = COALESCE(proveedores.contacto, NULL),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, NULL),
            email = COALESCE(proveedores.email, NULL),
            web = COALESCE(proveedores.web, NULL),
            direccion = COALESCE(proveedores.direccion, NULL),
            aforo = COALESCE(proveedores.aforo, NULL),
            costo_referencia = COALESCE(proveedores.costo_referencia, NULL),
            notas = COALESCE(proveedores.notas, NULL),
            updated_at = now()
        WHERE id = v_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_id, vv.t FROM (VALUES ('7223065271')) AS vv(t)
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_id AND telefono = vv.t);
END $blk$;

-- Proveedor: TU IMPRENTA
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'TU IMPRENTA' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'TU IMPRENTA',
            (SELECT id FROM paises WHERE nombre = 'México'),
            (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Toluca' LIMIT 1),
            (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Toluca' LIMIT 1),
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Impresos y señalética'),
            'Activo', 'GIOVANNI CAMPIRAN', NULL, 'hola@tuimprenta.com.mx', 'imprentaentoluca.com.mx', NULL, NULL, NULL, NULL, now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'México'),
            region_id = (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Toluca' LIMIT 1),
            ciudad_id = (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Toluca' LIMIT 1),
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Impresos y señalética'),
            contacto = COALESCE(proveedores.contacto, 'GIOVANNI CAMPIRAN'),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, NULL),
            email = COALESCE(proveedores.email, 'hola@tuimprenta.com.mx'),
            web = COALESCE(proveedores.web, 'imprentaentoluca.com.mx'),
            direccion = COALESCE(proveedores.direccion, NULL),
            aforo = COALESCE(proveedores.aforo, NULL),
            costo_referencia = COALESCE(proveedores.costo_referencia, NULL),
            notas = COALESCE(proveedores.notas, NULL),
            updated_at = now()
        WHERE id = v_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_id, vv.t FROM (VALUES ('7222145513')) AS vv(t)
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_id AND telefono = vv.t);
END $blk$;

-- Proveedor: GAMES WORLD
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'GAMES WORLD' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'GAMES WORLD',
            (SELECT id FROM paises WHERE nombre = 'México'),
            (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Entretenimiento y artistas'),
            'Activo', 'ALFREDO GAONA', 'DIRECTOR GENERAL', 'ventas@gamesworld.com.mx', 'gamesworld.com.mx', NULL, NULL, NULL, 'Ciudad original en el Excel: "CDMX"', now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'México'),
            region_id = (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            ciudad_id = (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Entretenimiento y artistas'),
            contacto = COALESCE(proveedores.contacto, 'ALFREDO GAONA'),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, 'DIRECTOR GENERAL'),
            email = COALESCE(proveedores.email, 'ventas@gamesworld.com.mx'),
            web = COALESCE(proveedores.web, 'gamesworld.com.mx'),
            direccion = COALESCE(proveedores.direccion, NULL),
            aforo = COALESCE(proveedores.aforo, NULL),
            costo_referencia = COALESCE(proveedores.costo_referencia, NULL),
            notas = COALESCE(proveedores.notas, 'Ciudad original en el Excel: "CDMX"'),
            updated_at = now()
        WHERE id = v_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_id, vv.t FROM (VALUES ('5513538908'), ('5597022643')) AS vv(t)
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_id AND telefono = vv.t);
END $blk$;

-- Proveedor: PELOTINAS
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'PELOTINAS' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'PELOTINAS',
            (SELECT id FROM paises WHERE nombre = 'México'),
            (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Entretenimiento y artistas'),
            'Activo', 'FERNANDA', NULL, NULL, NULL, NULL, NULL, NULL, 'Ciudad original en el Excel: "CDMX"', now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'México'),
            region_id = (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            ciudad_id = (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Entretenimiento y artistas'),
            contacto = COALESCE(proveedores.contacto, 'FERNANDA'),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, NULL),
            email = COALESCE(proveedores.email, NULL),
            web = COALESCE(proveedores.web, NULL),
            direccion = COALESCE(proveedores.direccion, NULL),
            aforo = COALESCE(proveedores.aforo, NULL),
            costo_referencia = COALESCE(proveedores.costo_referencia, NULL),
            notas = COALESCE(proveedores.notas, 'Ciudad original en el Excel: "CDMX"'),
            updated_at = now()
        WHERE id = v_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_id, vv.t FROM (VALUES ('5561265067')) AS vv(t)
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_id AND telefono = vv.t);
END $blk$;

-- Proveedor: MAQUILLISTA
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'MAQUILLISTA' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'MAQUILLISTA',
            (SELECT id FROM paises WHERE nombre = 'México'),
            NULL,
            NULL,
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Personal BTL (impulsadoras, edecanes, meseros)'),
            'Activo', 'ANILU', NULL, NULL, NULL, NULL, NULL, NULL, NULL, now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'México'),
            region_id = NULL,
            ciudad_id = NULL,
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Personal BTL (impulsadoras, edecanes, meseros)'),
            contacto = COALESCE(proveedores.contacto, 'ANILU'),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, NULL),
            email = COALESCE(proveedores.email, NULL),
            web = COALESCE(proveedores.web, NULL),
            direccion = COALESCE(proveedores.direccion, NULL),
            aforo = COALESCE(proveedores.aforo, NULL),
            costo_referencia = COALESCE(proveedores.costo_referencia, NULL),
            notas = COALESCE(proveedores.notas, NULL),
            updated_at = now()
        WHERE id = v_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_id, vv.t FROM (VALUES ('7227113851')) AS vv(t)
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_id AND telefono = vv.t);
END $blk$;

-- Proveedor: MARIACHI EL PALMERO
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'MARIACHI EL PALMERO' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'MARIACHI EL PALMERO',
            (SELECT id FROM paises WHERE nombre = 'México'),
            (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Toluca' LIMIT 1),
            (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Toluca' LIMIT 1),
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Entretenimiento y artistas'),
            'Activo', 'URIEL CALZADA BARRON', NULL, NULL, NULL, NULL, NULL, NULL, '2500 LA HORA', now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'México'),
            region_id = (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Toluca' LIMIT 1),
            ciudad_id = (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Toluca' LIMIT 1),
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Entretenimiento y artistas'),
            contacto = COALESCE(proveedores.contacto, 'URIEL CALZADA BARRON'),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, NULL),
            email = COALESCE(proveedores.email, NULL),
            web = COALESCE(proveedores.web, NULL),
            direccion = COALESCE(proveedores.direccion, NULL),
            aforo = COALESCE(proveedores.aforo, NULL),
            costo_referencia = COALESCE(proveedores.costo_referencia, NULL),
            notas = COALESCE(proveedores.notas, '2500 LA HORA'),
            updated_at = now()
        WHERE id = v_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_id, vv.t FROM (VALUES ('7224433678')) AS vv(t)
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_id AND telefono = vv.t);
END $blk$;

-- Proveedor: LE MODELS MARKETING
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'LE MODELS MARKETING' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'LE MODELS MARKETING',
            (SELECT id FROM paises WHERE nombre = 'México'),
            (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Toluca' LIMIT 1),
            (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Toluca' LIMIT 1),
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Impresos y señalética'),
            'Activo', 'EMANUEL SANTANA', 'COORDINADOR COMERCIAL', 'info@lemodels.com.mx', 'lemodels.com.mx', NULL, NULL, NULL, 'GRUPO NIXFE SAS DE CV', now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'México'),
            region_id = (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Toluca' LIMIT 1),
            ciudad_id = (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Toluca' LIMIT 1),
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Impresos y señalética'),
            contacto = COALESCE(proveedores.contacto, 'EMANUEL SANTANA'),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, 'COORDINADOR COMERCIAL'),
            email = COALESCE(proveedores.email, 'info@lemodels.com.mx'),
            web = COALESCE(proveedores.web, 'lemodels.com.mx'),
            direccion = COALESCE(proveedores.direccion, NULL),
            aforo = COALESCE(proveedores.aforo, NULL),
            costo_referencia = COALESCE(proveedores.costo_referencia, NULL),
            notas = COALESCE(proveedores.notas, 'GRUPO NIXFE SAS DE CV'),
            updated_at = now()
        WHERE id = v_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_id, vv.t FROM (VALUES ('7225413004')) AS vv(t)
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_id AND telefono = vv.t);
END $blk$;

-- Proveedor: ABAC
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'ABAC' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'ABAC',
            (SELECT id FROM paises WHERE nombre = 'México'),
            (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Otro'),
            'Activo', 'JORGE PINTO SALCIDO', NULL, 'contacto@abac.com.mx', 'abac.com.mx', NULL, NULL, NULL, 'Ciudad original en el Excel: "CDMX"', now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'México'),
            region_id = (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            ciudad_id = (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Otro'),
            contacto = COALESCE(proveedores.contacto, 'JORGE PINTO SALCIDO'),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, NULL),
            email = COALESCE(proveedores.email, 'contacto@abac.com.mx'),
            web = COALESCE(proveedores.web, 'abac.com.mx'),
            direccion = COALESCE(proveedores.direccion, NULL),
            aforo = COALESCE(proveedores.aforo, NULL),
            costo_referencia = COALESCE(proveedores.costo_referencia, NULL),
            notas = COALESCE(proveedores.notas, 'Ciudad original en el Excel: "CDMX"'),
            updated_at = now()
        WHERE id = v_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_id, vv.t FROM (VALUES ('5611097433')) AS vv(t)
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_id AND telefono = vv.t);
END $blk$;

-- Proveedor: BLACK STAGE
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'BLACK STAGE' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'BLACK STAGE',
            (SELECT id FROM paises WHERE nombre = 'México'),
            (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Mobiliario y decoración'),
            'Activo', 'EDGAR ANGELES', NULL, NULL, NULL, NULL, NULL, NULL, 'Ciudad original en el Excel: "CDMX"', now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'México'),
            region_id = (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            ciudad_id = (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Mobiliario y decoración'),
            contacto = COALESCE(proveedores.contacto, 'EDGAR ANGELES'),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, NULL),
            email = COALESCE(proveedores.email, NULL),
            web = COALESCE(proveedores.web, NULL),
            direccion = COALESCE(proveedores.direccion, NULL),
            aforo = COALESCE(proveedores.aforo, NULL),
            costo_referencia = COALESCE(proveedores.costo_referencia, NULL),
            notas = COALESCE(proveedores.notas, 'Ciudad original en el Excel: "CDMX"'),
            updated_at = now()
        WHERE id = v_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_id, vv.t FROM (VALUES ('5539726592')) AS vv(t)
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_id AND telefono = vv.t);
END $blk$;

-- Proveedor: SERVICIOS INTEGRALES AUDIOVISUALES
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'SERVICIOS INTEGRALES AUDIOVISUALES' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'SERVICIOS INTEGRALES AUDIOVISUALES',
            (SELECT id FROM paises WHERE nombre = 'México'),
            (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Monterrey' LIMIT 1),
            (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Monterrey' LIMIT 1),
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Mobiliario y decoración'),
            'Activo', 'PALOMA FLORES', 'VENTAS', 'admon@sia.com.mx', 'sia.com.mx', NULL, NULL, NULL, NULL, now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'México'),
            region_id = (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Monterrey' LIMIT 1),
            ciudad_id = (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Monterrey' LIMIT 1),
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Mobiliario y decoración'),
            contacto = COALESCE(proveedores.contacto, 'PALOMA FLORES'),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, 'VENTAS'),
            email = COALESCE(proveedores.email, 'admon@sia.com.mx'),
            web = COALESCE(proveedores.web, 'sia.com.mx'),
            direccion = COALESCE(proveedores.direccion, NULL),
            aforo = COALESCE(proveedores.aforo, NULL),
            costo_referencia = COALESCE(proveedores.costo_referencia, NULL),
            notas = COALESCE(proveedores.notas, NULL),
            updated_at = now()
        WHERE id = v_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_id, vv.t FROM (VALUES ('8183696464')) AS vv(t)
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_id AND telefono = vv.t);
END $blk$;

-- Proveedor: GRUPO STATUS
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'GRUPO STATUS' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'GRUPO STATUS',
            (SELECT id FROM paises WHERE nombre = 'México'),
            NULL,
            NULL,
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Mobiliario y decoración'),
            'Activo', 'ALEXANDRO', NULL, 'alexandro@grupo-status.com.mx', NULL, NULL, NULL, NULL, 'Ciudad original en el Excel: "MULTI LOCACIÓN"', now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'México'),
            region_id = NULL,
            ciudad_id = NULL,
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Mobiliario y decoración'),
            contacto = COALESCE(proveedores.contacto, 'ALEXANDRO'),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, NULL),
            email = COALESCE(proveedores.email, 'alexandro@grupo-status.com.mx'),
            web = COALESCE(proveedores.web, NULL),
            direccion = COALESCE(proveedores.direccion, NULL),
            aforo = COALESCE(proveedores.aforo, NULL),
            costo_referencia = COALESCE(proveedores.costo_referencia, NULL),
            notas = COALESCE(proveedores.notas, 'Ciudad original en el Excel: "MULTI LOCACIÓN"'),
            updated_at = now()
        WHERE id = v_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_id, vv.t FROM (VALUES ('5624333300')) AS vv(t)
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_id AND telefono = vv.t);
END $blk$;

-- Proveedor: OFIRENT
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'OFIRENT' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'OFIRENT',
            (SELECT id FROM paises WHERE nombre = 'México'),
            (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Logística y transporte'),
            'Activo', 'ALEJANDRA', 'EJECUTIVA DE CUENTA', 'contacto@ofirent.com.mx', 'ofirent.com.mx', NULL, NULL, NULL, 'Ciudad original en el Excel: "CDMX"', now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'México'),
            region_id = (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            ciudad_id = (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Logística y transporte'),
            contacto = COALESCE(proveedores.contacto, 'ALEJANDRA'),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, 'EJECUTIVA DE CUENTA'),
            email = COALESCE(proveedores.email, 'contacto@ofirent.com.mx'),
            web = COALESCE(proveedores.web, 'ofirent.com.mx'),
            direccion = COALESCE(proveedores.direccion, NULL),
            aforo = COALESCE(proveedores.aforo, NULL),
            costo_referencia = COALESCE(proveedores.costo_referencia, NULL),
            notas = COALESCE(proveedores.notas, 'Ciudad original en el Excel: "CDMX"'),
            updated_at = now()
        WHERE id = v_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_id, vv.t FROM (VALUES ('5532009907')) AS vv(t)
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_id AND telefono = vv.t);
END $blk$;

-- Proveedor: PLANNER
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'PLANNER' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'PLANNER',
            (SELECT id FROM paises WHERE nombre = 'México'),
            NULL,
            NULL,
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Diseño gráfico y creatividad'),
            'Activo', 'JESSICA ATALA', NULL, NULL, NULL, NULL, NULL, NULL, NULL, now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'México'),
            region_id = NULL,
            ciudad_id = NULL,
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Diseño gráfico y creatividad'),
            contacto = COALESCE(proveedores.contacto, 'JESSICA ATALA'),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, NULL),
            email = COALESCE(proveedores.email, NULL),
            web = COALESCE(proveedores.web, NULL),
            direccion = COALESCE(proveedores.direccion, NULL),
            aforo = COALESCE(proveedores.aforo, NULL),
            costo_referencia = COALESCE(proveedores.costo_referencia, NULL),
            notas = COALESCE(proveedores.notas, NULL),
            updated_at = now()
        WHERE id = v_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_id, vv.t FROM (VALUES ('5538263925')) AS vv(t)
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_id AND telefono = vv.t);
END $blk$;

-- Proveedor: OZNEER PUBLICIDAD E IMPRENTA
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'OZNEER PUBLICIDAD E IMPRENTA' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'OZNEER PUBLICIDAD E IMPRENTA',
            (SELECT id FROM paises WHERE nombre = 'México'),
            (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Monterrey' LIMIT 1),
            (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Monterrey' LIMIT 1),
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Impresos y señalética'),
            'Activo', NULL, NULL, 'info@ozneerpublicidad.com', NULL, NULL, NULL, NULL, NULL, now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'México'),
            region_id = (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Monterrey' LIMIT 1),
            ciudad_id = (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Monterrey' LIMIT 1),
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Impresos y señalética'),
            contacto = COALESCE(proveedores.contacto, NULL),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, NULL),
            email = COALESCE(proveedores.email, 'info@ozneerpublicidad.com'),
            web = COALESCE(proveedores.web, NULL),
            direccion = COALESCE(proveedores.direccion, NULL),
            aforo = COALESCE(proveedores.aforo, NULL),
            costo_referencia = COALESCE(proveedores.costo_referencia, NULL),
            notas = COALESCE(proveedores.notas, NULL),
            updated_at = now()
        WHERE id = v_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_id, vv.t FROM (VALUES ('8118144885')) AS vv(t)
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_id AND telefono = vv.t);
END $blk$;

-- Proveedor: FIDEICOMISO ARCHIVOS PLUTARCO
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'FIDEICOMISO ARCHIVOS PLUTARCO' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'FIDEICOMISO ARCHIVOS PLUTARCO',
            (SELECT id FROM paises WHERE nombre = 'México'),
            (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Salón de eventos'),
            'Activo', 'AMALIA TORREBLANCA', NULL, 'fapecft@fapecft.org.mx', NULL, NULL, NULL, NULL, 'Ciudad original en el Excel: "CDMX"', now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'México'),
            region_id = (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            ciudad_id = (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Salón de eventos'),
            contacto = COALESCE(proveedores.contacto, 'AMALIA TORREBLANCA'),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, NULL),
            email = COALESCE(proveedores.email, 'fapecft@fapecft.org.mx'),
            web = COALESCE(proveedores.web, NULL),
            direccion = COALESCE(proveedores.direccion, NULL),
            aforo = COALESCE(proveedores.aforo, NULL),
            costo_referencia = COALESCE(proveedores.costo_referencia, NULL),
            notas = COALESCE(proveedores.notas, 'Ciudad original en el Excel: "CDMX"'),
            updated_at = now()
        WHERE id = v_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_id, vv.t FROM (VALUES ('5552114999')) AS vv(t)
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_id AND telefono = vv.t);
END $blk$;

-- Proveedor: HOTEL BRICK
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'HOTEL BRICK' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'HOTEL BRICK',
            (SELECT id FROM paises WHERE nombre = 'México'),
            (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Salón de eventos'),
            'Activo', 'BRICK HOTEL', NULL, 'candy.santoyo@brickhotel.com.mx', NULL, NULL, NULL, NULL, 'Ciudad original en el Excel: "CDMX"', now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'México'),
            region_id = (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            ciudad_id = (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Salón de eventos'),
            contacto = COALESCE(proveedores.contacto, 'BRICK HOTEL'),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, NULL),
            email = COALESCE(proveedores.email, 'candy.santoyo@brickhotel.com.mx'),
            web = COALESCE(proveedores.web, NULL),
            direccion = COALESCE(proveedores.direccion, NULL),
            aforo = COALESCE(proveedores.aforo, NULL),
            costo_referencia = COALESCE(proveedores.costo_referencia, NULL),
            notas = COALESCE(proveedores.notas, 'Ciudad original en el Excel: "CDMX"'),
            updated_at = now()
        WHERE id = v_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_id, vv.t FROM (VALUES ('5591557610')) AS vv(t)
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_id AND telefono = vv.t);
END $blk$;

-- Proveedor: HOTEL CONDESA
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'HOTEL CONDESA' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'HOTEL CONDESA',
            (SELECT id FROM paises WHERE nombre = 'México'),
            (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Salón de eventos'),
            'Activo', 'SOFIA MAR', NULL, 'eventos@condesadf.com', NULL, NULL, NULL, NULL, 'Ciudad original en el Excel: "CDMX"', now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'México'),
            region_id = (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            ciudad_id = (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Salón de eventos'),
            contacto = COALESCE(proveedores.contacto, 'SOFIA MAR'),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, NULL),
            email = COALESCE(proveedores.email, 'eventos@condesadf.com'),
            web = COALESCE(proveedores.web, NULL),
            direccion = COALESCE(proveedores.direccion, NULL),
            aforo = COALESCE(proveedores.aforo, NULL),
            costo_referencia = COALESCE(proveedores.costo_referencia, NULL),
            notas = COALESCE(proveedores.notas, 'Ciudad original en el Excel: "CDMX"'),
            updated_at = now()
        WHERE id = v_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_id, vv.t FROM (VALUES ('5552412600')) AS vv(t)
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_id AND telefono = vv.t);
END $blk$;

-- Proveedor: TERRAZA BROCANTE
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'TERRAZA BROCANTE' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'TERRAZA BROCANTE',
            (SELECT id FROM paises WHERE nombre = 'México'),
            (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Salón de eventos'),
            'Activo', 'ERENDIRA TAMAYO', NULL, 'info@brocantebotanico.mx', NULL, NULL, 90, '35000.0', 'TERRAZA · Ciudad original en el Excel: "CDMX"', now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'México'),
            region_id = (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            ciudad_id = (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Salón de eventos'),
            contacto = COALESCE(proveedores.contacto, 'ERENDIRA TAMAYO'),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, NULL),
            email = COALESCE(proveedores.email, 'info@brocantebotanico.mx'),
            web = COALESCE(proveedores.web, NULL),
            direccion = COALESCE(proveedores.direccion, NULL),
            aforo = COALESCE(proveedores.aforo, 90),
            costo_referencia = COALESCE(proveedores.costo_referencia, '35000.0'),
            notas = COALESCE(proveedores.notas, 'TERRAZA · Ciudad original en el Excel: "CDMX"'),
            updated_at = now()
        WHERE id = v_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_id, vv.t FROM (VALUES ('5554759774')) AS vv(t)
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_id AND telefono = vv.t);
END $blk$;

-- Proveedor: TERRAZA EVEREST
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'TERRAZA EVEREST' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'TERRAZA EVEREST',
            (SELECT id FROM paises WHERE nombre = 'México'),
            (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Salón de eventos'),
            'Activo', 'FABIOLA MARTINEZ', NULL, 'info@terrazaeverest.com.mx', NULL, NULL, NULL, NULL, 'Ciudad original en el Excel: "CDMX"', now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'México'),
            region_id = (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            ciudad_id = (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Salón de eventos'),
            contacto = COALESCE(proveedores.contacto, 'FABIOLA MARTINEZ'),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, NULL),
            email = COALESCE(proveedores.email, 'info@terrazaeverest.com.mx'),
            web = COALESCE(proveedores.web, NULL),
            direccion = COALESCE(proveedores.direccion, NULL),
            aforo = COALESCE(proveedores.aforo, NULL),
            costo_referencia = COALESCE(proveedores.costo_referencia, NULL),
            notas = COALESCE(proveedores.notas, 'Ciudad original en el Excel: "CDMX"'),
            updated_at = now()
        WHERE id = v_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_id, vv.t FROM (VALUES ('5545159572')) AS vv(t)
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_id AND telefono = vv.t);
END $blk$;

-- Proveedor: EXPO SANTAFÉ
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'EXPO SANTAFÉ' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'EXPO SANTAFÉ',
            (SELECT id FROM paises WHERE nombre = 'México'),
            NULL,
            NULL,
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Salón de eventos'),
            'Activo', 'JULIETA GOMEZ', 'EJECUTIVA DE EVENTOS', 'jgomez@exposantafe.com.mx', 'exposantafe.com.mx', NULL, 70, '114000.0', 'TERRAZA', now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'México'),
            region_id = NULL,
            ciudad_id = NULL,
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Salón de eventos'),
            contacto = COALESCE(proveedores.contacto, 'JULIETA GOMEZ'),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, 'EJECUTIVA DE EVENTOS'),
            email = COALESCE(proveedores.email, 'jgomez@exposantafe.com.mx'),
            web = COALESCE(proveedores.web, 'exposantafe.com.mx'),
            direccion = COALESCE(proveedores.direccion, NULL),
            aforo = COALESCE(proveedores.aforo, 70),
            costo_referencia = COALESCE(proveedores.costo_referencia, '114000.0'),
            notas = COALESCE(proveedores.notas, 'TERRAZA'),
            updated_at = now()
        WHERE id = v_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_id, vv.t FROM (VALUES ('5552925350 ext.409')) AS vv(t)
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_id AND telefono = vv.t);
END $blk$;

-- Proveedor: FIDEICOMISO ARCHIVOS
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'FIDEICOMISO ARCHIVOS' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'FIDEICOMISO ARCHIVOS',
            (SELECT id FROM paises WHERE nombre = 'México'),
            NULL,
            NULL,
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Salón de eventos'),
            'Activo', 'AMALIA TORREBLANCA', NULL, 'amalia.torreblanca@fapecft.org.mx', 'fapecft.org.mx', NULL, 150, '50000.0', 'JARDÍN', now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'México'),
            region_id = NULL,
            ciudad_id = NULL,
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Salón de eventos'),
            contacto = COALESCE(proveedores.contacto, 'AMALIA TORREBLANCA'),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, NULL),
            email = COALESCE(proveedores.email, 'amalia.torreblanca@fapecft.org.mx'),
            web = COALESCE(proveedores.web, 'fapecft.org.mx'),
            direccion = COALESCE(proveedores.direccion, NULL),
            aforo = COALESCE(proveedores.aforo, 150),
            costo_referencia = COALESCE(proveedores.costo_referencia, '50000.0'),
            notas = COALESCE(proveedores.notas, 'JARDÍN'),
            updated_at = now()
        WHERE id = v_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_id, vv.t FROM (VALUES ('5552868339')) AS vv(t)
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_id AND telefono = vv.t);
END $blk$;

-- Proveedor: GENERAL PRIM
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'GENERAL PRIM' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'GENERAL PRIM',
            (SELECT id FROM paises WHERE nombre = 'México'),
            NULL,
            NULL,
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Salón de eventos'),
            'Activo', 'PAOLA', NULL, NULL, 'proyectopublicoprim.com', NULL, NULL, NULL, NULL, now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'México'),
            region_id = NULL,
            ciudad_id = NULL,
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Salón de eventos'),
            contacto = COALESCE(proveedores.contacto, 'PAOLA'),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, NULL),
            email = COALESCE(proveedores.email, NULL),
            web = COALESCE(proveedores.web, 'proyectopublicoprim.com'),
            direccion = COALESCE(proveedores.direccion, NULL),
            aforo = COALESCE(proveedores.aforo, NULL),
            costo_referencia = COALESCE(proveedores.costo_referencia, NULL),
            notas = COALESCE(proveedores.notas, NULL),
            updated_at = now()
        WHERE id = v_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_id, vv.t FROM (VALUES ('5544478580')) AS vv(t)
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_id AND telefono = vv.t);
END $blk$;

-- Proveedor: HOTEL NIMA
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'HOTEL NIMA' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'HOTEL NIMA',
            (SELECT id FROM paises WHERE nombre = 'México'),
            NULL,
            NULL,
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Salón de eventos'),
            'Activo', NULL, NULL, NULL, 'hotelnima.com', NULL, NULL, NULL, NULL, now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'México'),
            region_id = NULL,
            ciudad_id = NULL,
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Salón de eventos'),
            contacto = COALESCE(proveedores.contacto, NULL),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, NULL),
            email = COALESCE(proveedores.email, NULL),
            web = COALESCE(proveedores.web, 'hotelnima.com'),
            direccion = COALESCE(proveedores.direccion, NULL),
            aforo = COALESCE(proveedores.aforo, NULL),
            costo_referencia = COALESCE(proveedores.costo_referencia, NULL),
            notas = COALESCE(proveedores.notas, NULL),
            updated_at = now()
        WHERE id = v_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_id, vv.t FROM (VALUES ('5575917175')) AS vv(t)
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_id AND telefono = vv.t);
END $blk$;

-- Proveedor: MALAQUITA ROOFTOP
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'MALAQUITA ROOFTOP' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'MALAQUITA ROOFTOP',
            (SELECT id FROM paises WHERE nombre = 'México'),
            NULL,
            NULL,
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Salón de eventos'),
            'Activo', NULL, NULL, NULL, 'malaquitarooftop.com', NULL, NULL, NULL, NULL, now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'México'),
            region_id = NULL,
            ciudad_id = NULL,
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Salón de eventos'),
            contacto = COALESCE(proveedores.contacto, NULL),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, NULL),
            email = COALESCE(proveedores.email, NULL),
            web = COALESCE(proveedores.web, 'malaquitarooftop.com'),
            direccion = COALESCE(proveedores.direccion, NULL),
            aforo = COALESCE(proveedores.aforo, NULL),
            costo_referencia = COALESCE(proveedores.costo_referencia, NULL),
            notas = COALESCE(proveedores.notas, NULL),
            updated_at = now()
        WHERE id = v_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_id, vv.t FROM (VALUES ('5525369744')) AS vv(t)
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_id AND telefono = vv.t);
END $blk$;

-- Proveedor: SOBREMESA
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'SOBREMESA' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'SOBREMESA',
            (SELECT id FROM paises WHERE nombre = 'México'),
            NULL,
            NULL,
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Salón de eventos'),
            'Activo', NULL, NULL, NULL, 'sobremesa.mx', NULL, NULL, NULL, NULL, now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'México'),
            region_id = NULL,
            ciudad_id = NULL,
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Salón de eventos'),
            contacto = COALESCE(proveedores.contacto, NULL),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, NULL),
            email = COALESCE(proveedores.email, NULL),
            web = COALESCE(proveedores.web, 'sobremesa.mx'),
            direccion = COALESCE(proveedores.direccion, NULL),
            aforo = COALESCE(proveedores.aforo, NULL),
            costo_referencia = COALESCE(proveedores.costo_referencia, NULL),
            notas = COALESCE(proveedores.notas, NULL),
            updated_at = now()
        WHERE id = v_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_id, vv.t FROM (VALUES ('5571588660')) AS vv(t)
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_id AND telefono = vv.t);
END $blk$;

-- Proveedor: TERRAZA ALAMEDA
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'TERRAZA ALAMEDA' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'TERRAZA ALAMEDA',
            (SELECT id FROM paises WHERE nombre = 'México'),
            NULL,
            NULL,
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Salón de eventos'),
            'Activo', 'GENESIS MARTINEZ', NULL, NULL, NULL, NULL, 130, '70000.0', 'HOTEL HILTON TERRAZA', now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'México'),
            region_id = NULL,
            ciudad_id = NULL,
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Salón de eventos'),
            contacto = COALESCE(proveedores.contacto, 'GENESIS MARTINEZ'),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, NULL),
            email = COALESCE(proveedores.email, NULL),
            web = COALESCE(proveedores.web, NULL),
            direccion = COALESCE(proveedores.direccion, NULL),
            aforo = COALESCE(proveedores.aforo, 130),
            costo_referencia = COALESCE(proveedores.costo_referencia, '70000.0'),
            notas = COALESCE(proveedores.notas, 'HOTEL HILTON TERRAZA'),
            updated_at = now()
        WHERE id = v_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_id, vv.t FROM (VALUES ('5525693990')) AS vv(t)
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_id AND telefono = vv.t);
END $blk$;

-- Proveedor: TERRAZA i662
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'TERRAZA i662' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'TERRAZA i662',
            (SELECT id FROM paises WHERE nombre = 'México'),
            NULL,
            NULL,
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Salón de eventos'),
            'Activo', 'WENDY PINTO', NULL, 'terrazai662@gmail.com', 'facebook.com/terrazai662/?paipv=0&eav=AfZ2-RRWhUrO-prJ5lM-kBwOAGj3vi3gZnN4cV3_pI1r07EaXngXHpmIAqihWDwQeX0&_rdr', 'INSURGENTES SUR 662', 100, '60000.0', NULL, now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'México'),
            region_id = NULL,
            ciudad_id = NULL,
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Salón de eventos'),
            contacto = COALESCE(proveedores.contacto, 'WENDY PINTO'),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, NULL),
            email = COALESCE(proveedores.email, 'terrazai662@gmail.com'),
            web = COALESCE(proveedores.web, 'facebook.com/terrazai662/?paipv=0&eav=AfZ2-RRWhUrO-prJ5lM-kBwOAGj3vi3gZnN4cV3_pI1r07EaXngXHpmIAqihWDwQeX0&_rdr'),
            direccion = COALESCE(proveedores.direccion, 'INSURGENTES SUR 662'),
            aforo = COALESCE(proveedores.aforo, 100),
            costo_referencia = COALESCE(proveedores.costo_referencia, '60000.0'),
            notas = COALESCE(proveedores.notas, NULL),
            updated_at = now()
        WHERE id = v_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_id, vv.t FROM (VALUES ('5537270929')) AS vv(t)
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_id AND telefono = vv.t);
END $blk$;

-- Proveedor: TERRAZA INSURGENTES
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'TERRAZA INSURGENTES' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'TERRAZA INSURGENTES',
            (SELECT id FROM paises WHERE nombre = 'México'),
            NULL,
            NULL,
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Salón de eventos'),
            'Activo', NULL, NULL, NULL, 'terrazainsurgentes.com.mx', 'Insurgentes Sur 1248', NULL, NULL, NULL, now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'México'),
            region_id = NULL,
            ciudad_id = NULL,
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Salón de eventos'),
            contacto = COALESCE(proveedores.contacto, NULL),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, NULL),
            email = COALESCE(proveedores.email, NULL),
            web = COALESCE(proveedores.web, 'terrazainsurgentes.com.mx'),
            direccion = COALESCE(proveedores.direccion, 'Insurgentes Sur 1248'),
            aforo = COALESCE(proveedores.aforo, NULL),
            costo_referencia = COALESCE(proveedores.costo_referencia, NULL),
            notas = COALESCE(proveedores.notas, NULL),
            updated_at = now()
        WHERE id = v_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_id, vv.t FROM (VALUES ('5569789551')) AS vv(t)
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_id AND telefono = vv.t);
END $blk$;

-- Proveedor: TERRAZA TONALA
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'TERRAZA TONALA' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'TERRAZA TONALA',
            (SELECT id FROM paises WHERE nombre = 'México'),
            NULL,
            NULL,
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Salón de eventos'),
            'Activo', 'DIANA OVALLE', NULL, 'adriana.ovalleb@gmail.com', NULL, NULL, 50, NULL, NULL, now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'México'),
            region_id = NULL,
            ciudad_id = NULL,
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Salón de eventos'),
            contacto = COALESCE(proveedores.contacto, 'DIANA OVALLE'),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, NULL),
            email = COALESCE(proveedores.email, 'adriana.ovalleb@gmail.com'),
            web = COALESCE(proveedores.web, NULL),
            direccion = COALESCE(proveedores.direccion, NULL),
            aforo = COALESCE(proveedores.aforo, 50),
            costo_referencia = COALESCE(proveedores.costo_referencia, NULL),
            notas = COALESCE(proveedores.notas, NULL),
            updated_at = now()
        WHERE id = v_id;
    END IF;

END $blk$;

-- Proveedor: TERRAZAS AMBI
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'TERRAZAS AMBI' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'TERRAZAS AMBI',
            (SELECT id FROM paises WHERE nombre = 'México'),
            NULL,
            NULL,
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Salón de eventos'),
            'Activo', 'ABRIL QUEVEDO', NULL, NULL, 'e53terraza.com.mx', NULL, 50, '45000.0', 'TERRAZA', now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'México'),
            region_id = NULL,
            ciudad_id = NULL,
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Salón de eventos'),
            contacto = COALESCE(proveedores.contacto, 'ABRIL QUEVEDO'),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, NULL),
            email = COALESCE(proveedores.email, NULL),
            web = COALESCE(proveedores.web, 'e53terraza.com.mx'),
            direccion = COALESCE(proveedores.direccion, NULL),
            aforo = COALESCE(proveedores.aforo, 50),
            costo_referencia = COALESCE(proveedores.costo_referencia, '45000.0'),
            notas = COALESCE(proveedores.notas, 'TERRAZA'),
            updated_at = now()
        WHERE id = v_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_id, vv.t FROM (VALUES ('5620996213'), ('5624458405')) AS vv(t)
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_id AND telefono = vv.t);
END $blk$;

-- Proveedor: GRUPO MAX
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'GRUPO MAX' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'GRUPO MAX',
            (SELECT id FROM paises WHERE nombre = 'México'),
            (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Seguridad y protocolo'),
            'Activo', 'OSCAR BAUTIZTA', NULL, 'obautista@grupomaxmb.com', NULL, NULL, NULL, NULL, 'Ciudad original en el Excel: "CDMX"', now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'México'),
            region_id = (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            ciudad_id = (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Seguridad y protocolo'),
            contacto = COALESCE(proveedores.contacto, 'OSCAR BAUTIZTA'),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, NULL),
            email = COALESCE(proveedores.email, 'obautista@grupomaxmb.com'),
            web = COALESCE(proveedores.web, NULL),
            direccion = COALESCE(proveedores.direccion, NULL),
            aforo = COALESCE(proveedores.aforo, NULL),
            costo_referencia = COALESCE(proveedores.costo_referencia, NULL),
            notas = COALESCE(proveedores.notas, 'Ciudad original en el Excel: "CDMX"'),
            updated_at = now()
        WHERE id = v_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_id, vv.t FROM (VALUES ('5530269635')) AS vv(t)
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_id AND telefono = vv.t);
END $blk$;

-- Proveedor: RENT & COMPANY
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'RENT & COMPANY' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'RENT & COMPANY',
            (SELECT id FROM paises WHERE nombre = 'México'),
            (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Mobiliario y decoración'),
            'Activo', 'JOSE ALFREDO LUGO', NULL, 'josealfredo@rentandcompany.com', NULL, 'Prado norte # 460 piso 2, plaza del domo', NULL, NULL, 'Ciudad original en el Excel: "CDMX"', now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'México'),
            region_id = (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            ciudad_id = (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Mobiliario y decoración'),
            contacto = COALESCE(proveedores.contacto, 'JOSE ALFREDO LUGO'),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, NULL),
            email = COALESCE(proveedores.email, 'josealfredo@rentandcompany.com'),
            web = COALESCE(proveedores.web, NULL),
            direccion = COALESCE(proveedores.direccion, 'Prado norte # 460 piso 2, plaza del domo'),
            aforo = COALESCE(proveedores.aforo, NULL),
            costo_referencia = COALESCE(proveedores.costo_referencia, NULL),
            notas = COALESCE(proveedores.notas, 'Ciudad original en el Excel: "CDMX"'),
            updated_at = now()
        WHERE id = v_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_id, vv.t FROM (VALUES ('5565206355'), ('5555310620')) AS vv(t)
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_id AND telefono = vv.t);
END $blk$;

-- Proveedor: LOCAL TRENDY
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'LOCAL TRENDY' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'LOCAL TRENDY',
            (SELECT id FROM paises WHERE nombre = 'México'),
            (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Mobiliario y decoración'),
            'Activo', 'ASHANTY', NULL, 'contacto@localtrendy.com.mx', 'localtrendy.com.mx', NULL, NULL, NULL, 'Ciudad original en el Excel: "CDMX"', now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'México'),
            region_id = (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            ciudad_id = (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Mobiliario y decoración'),
            contacto = COALESCE(proveedores.contacto, 'ASHANTY'),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, NULL),
            email = COALESCE(proveedores.email, 'contacto@localtrendy.com.mx'),
            web = COALESCE(proveedores.web, 'localtrendy.com.mx'),
            direccion = COALESCE(proveedores.direccion, NULL),
            aforo = COALESCE(proveedores.aforo, NULL),
            costo_referencia = COALESCE(proveedores.costo_referencia, NULL),
            notas = COALESCE(proveedores.notas, 'Ciudad original en el Excel: "CDMX"'),
            updated_at = now()
        WHERE id = v_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_id, vv.t FROM (VALUES ('5535091018'), ('5552501615')) AS vv(t)
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_id AND telefono = vv.t);
END $blk$;

-- Proveedor: PROMOURIVEL
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'PROMOURIVEL' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'PROMOURIVEL',
            (SELECT id FROM paises WHERE nombre = 'México'),
            (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Impresos y señalética'),
            'Activo', 'RAFAEL VELASCO', NULL, 'ventas@promourivel.com', 'promourivel.com', NULL, NULL, NULL, 'Ciudad original en el Excel: "CDMX"', now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'México'),
            region_id = (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            ciudad_id = (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Impresos y señalética'),
            contacto = COALESCE(proveedores.contacto, 'RAFAEL VELASCO'),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, NULL),
            email = COALESCE(proveedores.email, 'ventas@promourivel.com'),
            web = COALESCE(proveedores.web, 'promourivel.com'),
            direccion = COALESCE(proveedores.direccion, NULL),
            aforo = COALESCE(proveedores.aforo, NULL),
            costo_referencia = COALESCE(proveedores.costo_referencia, NULL),
            notas = COALESCE(proveedores.notas, 'Ciudad original en el Excel: "CDMX"'),
            updated_at = now()
        WHERE id = v_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_id, vv.t FROM (VALUES ('5516342805')) AS vv(t)
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_id AND telefono = vv.t);
END $blk$;

-- Proveedor: 2DJS
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = '2DJS' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, '2DJS',
            (SELECT id FROM paises WHERE nombre = 'México'),
            (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Entretenimiento y artistas'),
            'Activo', '2 DJS', NULL, 'my2djs@mac.com', '2djs.com.mx', NULL, NULL, NULL, 'Ciudad original en el Excel: "CDMX"', now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'México'),
            region_id = (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            ciudad_id = (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Entretenimiento y artistas'),
            contacto = COALESCE(proveedores.contacto, '2 DJS'),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, NULL),
            email = COALESCE(proveedores.email, 'my2djs@mac.com'),
            web = COALESCE(proveedores.web, '2djs.com.mx'),
            direccion = COALESCE(proveedores.direccion, NULL),
            aforo = COALESCE(proveedores.aforo, NULL),
            costo_referencia = COALESCE(proveedores.costo_referencia, NULL),
            notas = COALESCE(proveedores.notas, 'Ciudad original en el Excel: "CDMX"'),
            updated_at = now()
        WHERE id = v_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_id, vv.t FROM (VALUES ('5564761091')) AS vv(t)
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_id AND telefono = vv.t);
END $blk$;

-- Proveedor: MAMBO / CUMBIA / SALSA
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'MAMBO / CUMBIA / SALSA' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'MAMBO / CUMBIA / SALSA',
            (SELECT id FROM paises WHERE nombre = 'México'),
            (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Entretenimiento y artistas'),
            'Activo', 'ORQUESTA PEREZ PRADO', NULL, 'orquestaperezprado@gmail.com', 'orquestaperezprado.com', NULL, NULL, NULL, 'Ciudad original en el Excel: "CDMX"', now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'México'),
            region_id = (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            ciudad_id = (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Entretenimiento y artistas'),
            contacto = COALESCE(proveedores.contacto, 'ORQUESTA PEREZ PRADO'),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, NULL),
            email = COALESCE(proveedores.email, 'orquestaperezprado@gmail.com'),
            web = COALESCE(proveedores.web, 'orquestaperezprado.com'),
            direccion = COALESCE(proveedores.direccion, NULL),
            aforo = COALESCE(proveedores.aforo, NULL),
            costo_referencia = COALESCE(proveedores.costo_referencia, NULL),
            notas = COALESCE(proveedores.notas, 'Ciudad original en el Excel: "CDMX"'),
            updated_at = now()
        WHERE id = v_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_id, vv.t FROM (VALUES ('5542354917')) AS vv(t)
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_id AND telefono = vv.t);
END $blk$;

-- Proveedor: CUMBIA / SALSA
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'CUMBIA / SALSA' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'CUMBIA / SALSA',
            (SELECT id FROM paises WHERE nombre = 'México'),
            (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Entretenimiento y artistas'),
            'Activo', 'ORQUESTA LA TIPICA', NULL, 'contacto@orquestalatipica.com', 'orquestalatipica.com', NULL, NULL, NULL, 'Ciudad original en el Excel: "CDMX" · Ciudad original en el Excel: "CDMX" · Correo en el Excel no parece válido, se guardó como nota: "MARITZA PAVON"', now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'México'),
            region_id = (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            ciudad_id = (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Entretenimiento y artistas'),
            contacto = COALESCE(proveedores.contacto, 'ORQUESTA LA TIPICA'),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, NULL),
            email = COALESCE(proveedores.email, 'contacto@orquestalatipica.com'),
            web = COALESCE(proveedores.web, 'orquestalatipica.com'),
            direccion = COALESCE(proveedores.direccion, NULL),
            aforo = COALESCE(proveedores.aforo, NULL),
            costo_referencia = COALESCE(proveedores.costo_referencia, NULL),
            notas = COALESCE(proveedores.notas, 'Ciudad original en el Excel: "CDMX" · Ciudad original en el Excel: "CDMX" · Correo en el Excel no parece válido, se guardó como nota: "MARITZA PAVON"'),
            updated_at = now()
        WHERE id = v_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_id, vv.t FROM (VALUES ('5556850859')) AS vv(t)
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_id AND telefono = vv.t);
END $blk$;

-- Proveedor: ZERO CERO
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'ZERO CERO' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'ZERO CERO',
            (SELECT id FROM paises WHERE nombre = 'México'),
            (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Impresos y señalética'),
            'Activo', 'JAIME SOSA', 'DIRECTOR GENERAL', 'ventas@zero-cero.net', 'facebook.com/ZeroCeroestilo', 'C. Alfonso Herrera 122, San Rafael, Cuauhtémoc, 06470 Ciudad de México, CDMX', NULL, NULL, 'Ciudad original en el Excel: "CDMX"', now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'México'),
            region_id = (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            ciudad_id = (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Impresos y señalética'),
            contacto = COALESCE(proveedores.contacto, 'JAIME SOSA'),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, 'DIRECTOR GENERAL'),
            email = COALESCE(proveedores.email, 'ventas@zero-cero.net'),
            web = COALESCE(proveedores.web, 'facebook.com/ZeroCeroestilo'),
            direccion = COALESCE(proveedores.direccion, 'C. Alfonso Herrera 122, San Rafael, Cuauhtémoc, 06470 Ciudad de México, CDMX'),
            aforo = COALESCE(proveedores.aforo, NULL),
            costo_referencia = COALESCE(proveedores.costo_referencia, NULL),
            notas = COALESCE(proveedores.notas, 'Ciudad original en el Excel: "CDMX"'),
            updated_at = now()
        WHERE id = v_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_id, vv.t FROM (VALUES ('5554370288')) AS vv(t)
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_id AND telefono = vv.t);
END $blk$;

-- Proveedor: SALSA
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'SALSA' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'SALSA',
            (SELECT id FROM paises WHERE nombre = 'México'),
            (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Entretenimiento y artistas'),
            'Activo', 'RODOLFO VALDEZ', NULL, NULL, NULL, NULL, NULL, NULL, 'Ciudad original en el Excel: "CDMX"', now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'México'),
            region_id = (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            ciudad_id = (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Entretenimiento y artistas'),
            contacto = COALESCE(proveedores.contacto, 'RODOLFO VALDEZ'),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, NULL),
            email = COALESCE(proveedores.email, NULL),
            web = COALESCE(proveedores.web, NULL),
            direccion = COALESCE(proveedores.direccion, NULL),
            aforo = COALESCE(proveedores.aforo, NULL),
            costo_referencia = COALESCE(proveedores.costo_referencia, NULL),
            notas = COALESCE(proveedores.notas, 'Ciudad original en el Excel: "CDMX"'),
            updated_at = now()
        WHERE id = v_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_id, vv.t FROM (VALUES ('5535641001')) AS vv(t)
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_id AND telefono = vv.t);
END $blk$;

-- Proveedor: BANDA VERSATIL
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'BANDA VERSATIL' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'BANDA VERSATIL',
            (SELECT id FROM paises WHERE nombre = 'México'),
            (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Entretenimiento y artistas'),
            'Activo', 'THE KOOL TOWN', NULL, NULL, 'thekooltown.com', NULL, NULL, NULL, 'Ciudad original en el Excel: "CDMX" · Correo en el Excel no parece válido, se guardó como nota: "MIGUEL GALI" · Ciudad original en el Excel: "CDMX" · Correo en el Excel no parece válido, se guardó como nota: "RUBEN ZEPEDA"', now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'México'),
            region_id = (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            ciudad_id = (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Entretenimiento y artistas'),
            contacto = COALESCE(proveedores.contacto, 'THE KOOL TOWN'),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, NULL),
            email = COALESCE(proveedores.email, NULL),
            web = COALESCE(proveedores.web, 'thekooltown.com'),
            direccion = COALESCE(proveedores.direccion, NULL),
            aforo = COALESCE(proveedores.aforo, NULL),
            costo_referencia = COALESCE(proveedores.costo_referencia, NULL),
            notas = COALESCE(proveedores.notas, 'Ciudad original en el Excel: "CDMX" · Correo en el Excel no parece válido, se guardó como nota: "MIGUEL GALI" · Ciudad original en el Excel: "CDMX" · Correo en el Excel no parece válido, se guardó como nota: "RUBEN ZEPEDA"'),
            updated_at = now()
        WHERE id = v_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_id, vv.t FROM (VALUES ('2221132041')) AS vv(t)
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_id AND telefono = vv.t);
END $blk$;

-- Proveedor: PERSONALIZALO
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'PERSONALIZALO' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'PERSONALIZALO',
            (SELECT id FROM paises WHERE nombre = 'México'),
            (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Toluca' LIMIT 1),
            (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Toluca' LIMIT 1),
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Impresos y señalética'),
            'Activo', 'MONICA ESPINOSA', 'DIRECTOR GENERAL', 'prsonalizalo@hotmail.com', 'prsonalizalo.mx', 'Ignacio Allende 414-int. 11, Francisco Murguia, 50130 Toluca de Lerdo, Méx.', NULL, NULL, 'CUIDAR COTIZACIONES PARA QUALA', now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'México'),
            region_id = (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Toluca' LIMIT 1),
            ciudad_id = (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Toluca' LIMIT 1),
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Impresos y señalética'),
            contacto = COALESCE(proveedores.contacto, 'MONICA ESPINOSA'),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, 'DIRECTOR GENERAL'),
            email = COALESCE(proveedores.email, 'prsonalizalo@hotmail.com'),
            web = COALESCE(proveedores.web, 'prsonalizalo.mx'),
            direccion = COALESCE(proveedores.direccion, 'Ignacio Allende 414-int. 11, Francisco Murguia, 50130 Toluca de Lerdo, Méx.'),
            aforo = COALESCE(proveedores.aforo, NULL),
            costo_referencia = COALESCE(proveedores.costo_referencia, NULL),
            notas = COALESCE(proveedores.notas, 'CUIDAR COTIZACIONES PARA QUALA'),
            updated_at = now()
        WHERE id = v_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_id, vv.t FROM (VALUES ('7221081100')) AS vv(t)
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_id AND telefono = vv.t);
END $blk$;

-- Proveedor: TRIBUTO JUAN GABRIEL
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'TRIBUTO JUAN GABRIEL' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'TRIBUTO JUAN GABRIEL',
            (SELECT id FROM paises WHERE nombre = 'México'),
            (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Entretenimiento y artistas'),
            'Activo', 'JULIO PONZE', NULL, NULL, NULL, NULL, NULL, NULL, 'Ciudad original en el Excel: "CDMX"', now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'México'),
            region_id = (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            ciudad_id = (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Entretenimiento y artistas'),
            contacto = COALESCE(proveedores.contacto, 'JULIO PONZE'),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, NULL),
            email = COALESCE(proveedores.email, NULL),
            web = COALESCE(proveedores.web, NULL),
            direccion = COALESCE(proveedores.direccion, NULL),
            aforo = COALESCE(proveedores.aforo, NULL),
            costo_referencia = COALESCE(proveedores.costo_referencia, NULL),
            notas = COALESCE(proveedores.notas, 'Ciudad original en el Excel: "CDMX"'),
            updated_at = now()
        WHERE id = v_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_id, vv.t FROM (VALUES ('5519536078')) AS vv(t)
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_id AND telefono = vv.t);
END $blk$;

-- Proveedor: LIONNE
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'LIONNE' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'LIONNE',
            (SELECT id FROM paises WHERE nombre = 'México'),
            (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Personal BTL (impulsadoras, edecanes, meseros)'),
            'Activo', 'JENNIPHER', NULL, 'jleon@lione.com.mx', 'edecanesvip.mx', NULL, NULL, NULL, 'Ciudad original en el Excel: "CDMX"', now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'México'),
            region_id = (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            ciudad_id = (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Personal BTL (impulsadoras, edecanes, meseros)'),
            contacto = COALESCE(proveedores.contacto, 'JENNIPHER'),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, NULL),
            email = COALESCE(proveedores.email, 'jleon@lione.com.mx'),
            web = COALESCE(proveedores.web, 'edecanesvip.mx'),
            direccion = COALESCE(proveedores.direccion, NULL),
            aforo = COALESCE(proveedores.aforo, NULL),
            costo_referencia = COALESCE(proveedores.costo_referencia, NULL),
            notas = COALESCE(proveedores.notas, 'Ciudad original en el Excel: "CDMX"'),
            updated_at = now()
        WHERE id = v_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_id, vv.t FROM (VALUES ('3329996478'), ('5568653799')) AS vv(t)
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_id AND telefono = vv.t);
END $blk$;

-- Proveedor: TOKA
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'TOKA' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'TOKA',
            (SELECT id FROM paises WHERE nombre = 'México'),
            (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Personal BTL (impulsadoras, edecanes, meseros)'),
            'Activo', NULL, NULL, 'contacto@tokamodelos.com', 'tokamodelos.com', NULL, NULL, NULL, 'Ciudad original en el Excel: "CDMX"', now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'México'),
            region_id = (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            ciudad_id = (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Personal BTL (impulsadoras, edecanes, meseros)'),
            contacto = COALESCE(proveedores.contacto, NULL),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, NULL),
            email = COALESCE(proveedores.email, 'contacto@tokamodelos.com'),
            web = COALESCE(proveedores.web, 'tokamodelos.com'),
            direccion = COALESCE(proveedores.direccion, NULL),
            aforo = COALESCE(proveedores.aforo, NULL),
            costo_referencia = COALESCE(proveedores.costo_referencia, NULL),
            notas = COALESCE(proveedores.notas, 'Ciudad original en el Excel: "CDMX"'),
            updated_at = now()
        WHERE id = v_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_id, vv.t FROM (VALUES ('5650358707'), ('5527068759')) AS vv(t)
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_id AND telefono = vv.t);
END $blk$;

-- Proveedor: VECTOR STANDS
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'VECTOR STANDS' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'VECTOR STANDS',
            (SELECT id FROM paises WHERE nombre = 'México'),
            NULL,
            NULL,
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Producción técnica (rigging, generadores, carpas)'),
            'Activo', 'ARMANDO', NULL, 'armando@vectorstands.com', 'vectorstands.com', NULL, NULL, NULL, 'Ciudad original en el Excel: "VALLARTA - CDMX"', now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'México'),
            region_id = NULL,
            ciudad_id = NULL,
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Producción técnica (rigging, generadores, carpas)'),
            contacto = COALESCE(proveedores.contacto, 'ARMANDO'),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, NULL),
            email = COALESCE(proveedores.email, 'armando@vectorstands.com'),
            web = COALESCE(proveedores.web, 'vectorstands.com'),
            direccion = COALESCE(proveedores.direccion, NULL),
            aforo = COALESCE(proveedores.aforo, NULL),
            costo_referencia = COALESCE(proveedores.costo_referencia, NULL),
            notas = COALESCE(proveedores.notas, 'Ciudad original en el Excel: "VALLARTA - CDMX"'),
            updated_at = now()
        WHERE id = v_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_id, vv.t FROM (VALUES ('3221239930')) AS vv(t)
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_id AND telefono = vv.t);
END $blk$;

-- Proveedor: STP LOGISTICA
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'STP LOGISTICA' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'STP LOGISTICA',
            (SELECT id FROM paises WHERE nombre = 'México'),
            (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Logística y transporte'),
            'Activo', 'SANTIAGO TENORIO', 'COORDINADOR COMERCIAL Y OPERACIONES', 'palomarestesantiago@gmail.com', 'linkedin.com/in/santiago-tenorio-a4765837a', NULL, NULL, NULL, 'Ciudad original en el Excel: "CDMX"', now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'México'),
            region_id = (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            ciudad_id = (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Logística y transporte'),
            contacto = COALESCE(proveedores.contacto, 'SANTIAGO TENORIO'),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, 'COORDINADOR COMERCIAL Y OPERACIONES'),
            email = COALESCE(proveedores.email, 'palomarestesantiago@gmail.com'),
            web = COALESCE(proveedores.web, 'linkedin.com/in/santiago-tenorio-a4765837a'),
            direccion = COALESCE(proveedores.direccion, NULL),
            aforo = COALESCE(proveedores.aforo, NULL),
            costo_referencia = COALESCE(proveedores.costo_referencia, NULL),
            notas = COALESCE(proveedores.notas, 'Ciudad original en el Excel: "CDMX"'),
            updated_at = now()
        WHERE id = v_id;
    END IF;

END $blk$;

-- Proveedor: COMEDYAR PRODUCTIONS
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'COMEDYAR PRODUCTIONS' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'COMEDYAR PRODUCTIONS',
            (SELECT id FROM paises WHERE nombre = 'México'),
            (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Entretenimiento y artistas'),
            'Activo', 'ALEJANDRO CURIEL', 'DIRECTOR', 'direccion@comedyarproductions.com', 'instagram.com/comedyarproductions?igsh=MXd4b250YXgycWg1Mw%3D%3D', NULL, NULL, NULL, 'Ciudad original en el Excel: "CDMX"', now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'México'),
            region_id = (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            ciudad_id = (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Entretenimiento y artistas'),
            contacto = COALESCE(proveedores.contacto, 'ALEJANDRO CURIEL'),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, 'DIRECTOR'),
            email = COALESCE(proveedores.email, 'direccion@comedyarproductions.com'),
            web = COALESCE(proveedores.web, 'instagram.com/comedyarproductions?igsh=MXd4b250YXgycWg1Mw%3D%3D'),
            direccion = COALESCE(proveedores.direccion, NULL),
            aforo = COALESCE(proveedores.aforo, NULL),
            costo_referencia = COALESCE(proveedores.costo_referencia, NULL),
            notas = COALESCE(proveedores.notas, 'Ciudad original en el Excel: "CDMX"'),
            updated_at = now()
        WHERE id = v_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_id, vv.t FROM (VALUES ('5528534036')) AS vv(t)
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_id AND telefono = vv.t);
END $blk$;

-- Proveedor: LOS CIPRES
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'LOS CIPRES' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'LOS CIPRES',
            (SELECT id FROM paises WHERE nombre = 'México'),
            (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Catering y F&B'),
            'Activo', 'EDUARDO LÓPEZ', 'DIRECTOR', 'pedidos@tacoselcipres.com.mx', 'Tacos El Cipres', NULL, NULL, NULL, 'Ciudad original en el Excel: "CDMX"', now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'México'),
            region_id = (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            ciudad_id = (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Catering y F&B'),
            contacto = COALESCE(proveedores.contacto, 'EDUARDO LÓPEZ'),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, 'DIRECTOR'),
            email = COALESCE(proveedores.email, 'pedidos@tacoselcipres.com.mx'),
            web = COALESCE(proveedores.web, 'Tacos El Cipres'),
            direccion = COALESCE(proveedores.direccion, NULL),
            aforo = COALESCE(proveedores.aforo, NULL),
            costo_referencia = COALESCE(proveedores.costo_referencia, NULL),
            notas = COALESCE(proveedores.notas, 'Ciudad original en el Excel: "CDMX"'),
            updated_at = now()
        WHERE id = v_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_id, vv.t FROM (VALUES ('5539398803'), ('5539226545')) AS vv(t)
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_id AND telefono = vv.t);
END $blk$;

-- Proveedor: LUARAN
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'LUARAN' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'LUARAN',
            (SELECT id FROM paises WHERE nombre = 'México'),
            (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Catering y F&B'),
            'Activo', NULL, 'GERENTE', 'luarancateringeventos@gmail.com', 'https://www.instagram.com/luaran.cateringmx/', NULL, NULL, NULL, 'Ciudad original en el Excel: "CDMX"', now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'México'),
            region_id = (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            ciudad_id = (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Catering y F&B'),
            contacto = COALESCE(proveedores.contacto, NULL),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, 'GERENTE'),
            email = COALESCE(proveedores.email, 'luarancateringeventos@gmail.com'),
            web = COALESCE(proveedores.web, 'https://www.instagram.com/luaran.cateringmx/'),
            direccion = COALESCE(proveedores.direccion, NULL),
            aforo = COALESCE(proveedores.aforo, NULL),
            costo_referencia = COALESCE(proveedores.costo_referencia, NULL),
            notas = COALESCE(proveedores.notas, 'Ciudad original en el Excel: "CDMX"'),
            updated_at = now()
        WHERE id = v_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_id, vv.t FROM (VALUES ('5588062475'), ('5627628598')) AS vv(t)
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_id AND telefono = vv.t);
END $blk$;

-- Proveedor: GENOMA CREATIVO
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'GENOMA CREATIVO' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'GENOMA CREATIVO',
            (SELECT id FROM paises WHERE nombre = 'México'),
            (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Impresos y señalética'),
            'Activo', 'LAURA VILLA', 'GERENTE', 'genomacreativo.mkt@gmail.com', 'https://sites.google.com/view/genomacreativoof/inicio', NULL, NULL, NULL, 'Ciudad original en el Excel: "CDMX"', now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'México'),
            region_id = (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            ciudad_id = (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Impresos y señalética'),
            contacto = COALESCE(proveedores.contacto, 'LAURA VILLA'),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, 'GERENTE'),
            email = COALESCE(proveedores.email, 'genomacreativo.mkt@gmail.com'),
            web = COALESCE(proveedores.web, 'https://sites.google.com/view/genomacreativoof/inicio'),
            direccion = COALESCE(proveedores.direccion, NULL),
            aforo = COALESCE(proveedores.aforo, NULL),
            costo_referencia = COALESCE(proveedores.costo_referencia, NULL),
            notas = COALESCE(proveedores.notas, 'Ciudad original en el Excel: "CDMX"'),
            updated_at = now()
        WHERE id = v_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_id, vv.t FROM (VALUES ('7228419440')) AS vv(t)
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_id AND telefono = vv.t);
END $blk$;

-- Proveedor: ORQUESTA PEREZ PRADO
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'ORQUESTA PEREZ PRADO' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'ORQUESTA PEREZ PRADO',
            (SELECT id FROM paises WHERE nombre = 'México'),
            (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Entretenimiento y artistas'),
            'Activo', 'ISRAEL GARNICA', 'DIRECTOR', NULL, NULL, NULL, NULL, NULL, 'Ciudad original en el Excel: "CDMX" · Correo "orquestaperezprado@gmail.com" ya usado por otro proveedor en este mismo archivo -- se dejó vacío aquí para que el import no lo rechace por duplicado; revisar si son la misma entidad.', now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'México'),
            region_id = (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            ciudad_id = (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Entretenimiento y artistas'),
            contacto = COALESCE(proveedores.contacto, 'ISRAEL GARNICA'),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, 'DIRECTOR'),
            email = COALESCE(proveedores.email, NULL),
            web = COALESCE(proveedores.web, NULL),
            direccion = COALESCE(proveedores.direccion, NULL),
            aforo = COALESCE(proveedores.aforo, NULL),
            costo_referencia = COALESCE(proveedores.costo_referencia, NULL),
            notas = COALESCE(proveedores.notas, 'Ciudad original en el Excel: "CDMX" · Correo "orquestaperezprado@gmail.com" ya usado por otro proveedor en este mismo archivo -- se dejó vacío aquí para que el import no lo rechace por duplicado; revisar si son la misma entidad.'),
            updated_at = now()
        WHERE id = v_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_id, vv.t FROM (VALUES ('5542354917')) AS vv(t)
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_id AND telefono = vv.t);
END $blk$;

-- Proveedor: KODOKAN MX
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'KODOKAN MX' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'KODOKAN MX',
            (SELECT id FROM paises WHERE nombre = 'México'),
            (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Entretenimiento y artistas'),
            'Activo', 'SANTIAGO MASAYOSHI', 'SHOWMAN', 'santiagomasayoshi@gmail.com', NULL, NULL, NULL, NULL, 'Ciudad original en el Excel: "CDMX"', now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'México'),
            region_id = (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            ciudad_id = (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Entretenimiento y artistas'),
            contacto = COALESCE(proveedores.contacto, 'SANTIAGO MASAYOSHI'),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, 'SHOWMAN'),
            email = COALESCE(proveedores.email, 'santiagomasayoshi@gmail.com'),
            web = COALESCE(proveedores.web, NULL),
            direccion = COALESCE(proveedores.direccion, NULL),
            aforo = COALESCE(proveedores.aforo, NULL),
            costo_referencia = COALESCE(proveedores.costo_referencia, NULL),
            notas = COALESCE(proveedores.notas, 'Ciudad original en el Excel: "CDMX"'),
            updated_at = now()
        WHERE id = v_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_id, vv.t FROM (VALUES ('5519566201')) AS vv(t)
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_id AND telefono = vv.t);
END $blk$;

-- Proveedor: CAMPUS WORTEV
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'CAMPUS WORTEV' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'CAMPUS WORTEV',
            (SELECT id FROM paises WHERE nombre = 'México'),
            (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Salón de eventos'),
            'Activo', 'PAULINA PÉREZ', 'EVENT PLANNER', 'soloriopau@gmail.com', 'https://wortev.com/', NULL, NULL, NULL, 'Ciudad original en el Excel: "CDMX"', now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'México'),
            region_id = (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            ciudad_id = (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Salón de eventos'),
            contacto = COALESCE(proveedores.contacto, 'PAULINA PÉREZ'),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, 'EVENT PLANNER'),
            email = COALESCE(proveedores.email, 'soloriopau@gmail.com'),
            web = COALESCE(proveedores.web, 'https://wortev.com/'),
            direccion = COALESCE(proveedores.direccion, NULL),
            aforo = COALESCE(proveedores.aforo, NULL),
            costo_referencia = COALESCE(proveedores.costo_referencia, NULL),
            notas = COALESCE(proveedores.notas, 'Ciudad original en el Excel: "CDMX"'),
            updated_at = now()
        WHERE id = v_id;
    END IF;

END $blk$;

-- Proveedor: INSULAR
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'INSULAR' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'INSULAR',
            (SELECT id FROM paises WHERE nombre = 'México'),
            (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Salón de eventos'),
            'Activo', 'EDUARDO SMEKE', 'DIRECTOR', 'smekee@gmail.com', 'https://www.linkedin.com/company/insularmx/about/', NULL, NULL, NULL, 'Ciudad original en el Excel: "CDMX"', now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'México'),
            region_id = (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            ciudad_id = (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Salón de eventos'),
            contacto = COALESCE(proveedores.contacto, 'EDUARDO SMEKE'),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, 'DIRECTOR'),
            email = COALESCE(proveedores.email, 'smekee@gmail.com'),
            web = COALESCE(proveedores.web, 'https://www.linkedin.com/company/insularmx/about/'),
            direccion = COALESCE(proveedores.direccion, NULL),
            aforo = COALESCE(proveedores.aforo, NULL),
            costo_referencia = COALESCE(proveedores.costo_referencia, NULL),
            notas = COALESCE(proveedores.notas, 'Ciudad original en el Excel: "CDMX"'),
            updated_at = now()
        WHERE id = v_id;
    END IF;

END $blk$;

-- Proveedor: LATICHO PROMOS
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'LATICHO PROMOS' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'LATICHO PROMOS',
            (SELECT id FROM paises WHERE nombre = 'México'),
            (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Impresos y señalética'),
            'Activo', 'EDGAR JUÁREZ', NULL, 'edgar.juabe@gmail.com', NULL, NULL, NULL, NULL, 'Ciudad original en el Excel: "CDMX"', now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'México'),
            region_id = (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            ciudad_id = (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Impresos y señalética'),
            contacto = COALESCE(proveedores.contacto, 'EDGAR JUÁREZ'),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, NULL),
            email = COALESCE(proveedores.email, 'edgar.juabe@gmail.com'),
            web = COALESCE(proveedores.web, NULL),
            direccion = COALESCE(proveedores.direccion, NULL),
            aforo = COALESCE(proveedores.aforo, NULL),
            costo_referencia = COALESCE(proveedores.costo_referencia, NULL),
            notas = COALESCE(proveedores.notas, 'Ciudad original en el Excel: "CDMX"'),
            updated_at = now()
        WHERE id = v_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_id, vv.t FROM (VALUES ('5579466150')) AS vv(t)
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_id AND telefono = vv.t);
END $blk$;

-- Proveedor: DESTINITY TRANSPORTACIÓN
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'DESTINITY TRANSPORTACIÓN' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'DESTINITY TRANSPORTACIÓN',
            (SELECT id FROM paises WHERE nombre = 'México'),
            (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Logística y transporte'),
            'Activo', 'MONTSERRAT GONZÁLEZ', NULL, 'destinity.rrss@gmail.com', NULL, NULL, NULL, NULL, 'Ciudad original en el Excel: "CDMX"', now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'México'),
            region_id = (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            ciudad_id = (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Logística y transporte'),
            contacto = COALESCE(proveedores.contacto, 'MONTSERRAT GONZÁLEZ'),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, NULL),
            email = COALESCE(proveedores.email, 'destinity.rrss@gmail.com'),
            web = COALESCE(proveedores.web, NULL),
            direccion = COALESCE(proveedores.direccion, NULL),
            aforo = COALESCE(proveedores.aforo, NULL),
            costo_referencia = COALESCE(proveedores.costo_referencia, NULL),
            notas = COALESCE(proveedores.notas, 'Ciudad original en el Excel: "CDMX"'),
            updated_at = now()
        WHERE id = v_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_id, vv.t FROM (VALUES ('556337018')) AS vv(t)
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_id AND telefono = vv.t);
END $blk$;

-- Proveedor: TEQUILA DON RAMÓN
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'TEQUILA DON RAMÓN' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'TEQUILA DON RAMÓN',
            (SELECT id FROM paises WHERE nombre = 'México'),
            (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Otro'),
            'Activo', 'HUGO SÁNCHEZ LÓPEZ', NULL, 'hugo.pmx05@gmail.com', NULL, NULL, NULL, NULL, 'Ciudad original en el Excel: "CDMX"', now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'México'),
            region_id = (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            ciudad_id = (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Otro'),
            contacto = COALESCE(proveedores.contacto, 'HUGO SÁNCHEZ LÓPEZ'),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, NULL),
            email = COALESCE(proveedores.email, 'hugo.pmx05@gmail.com'),
            web = COALESCE(proveedores.web, NULL),
            direccion = COALESCE(proveedores.direccion, NULL),
            aforo = COALESCE(proveedores.aforo, NULL),
            costo_referencia = COALESCE(proveedores.costo_referencia, NULL),
            notas = COALESCE(proveedores.notas, 'Ciudad original en el Excel: "CDMX"'),
            updated_at = now()
        WHERE id = v_id;
    END IF;

END $blk$;

-- Proveedor: JIMMY & THE BENJAMINS
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'JIMMY & THE BENJAMINS' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'JIMMY & THE BENJAMINS',
            (SELECT id FROM paises WHERE nombre = 'México'),
            (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Entretenimiento y artistas'),
            'Activo', 'JIMMY QUIJANO', NULL, 'quijanojimmy@gmail.com', NULL, NULL, NULL, NULL, 'Ciudad original en el Excel: "CDMX"', now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'México'),
            region_id = (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            ciudad_id = (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Entretenimiento y artistas'),
            contacto = COALESCE(proveedores.contacto, 'JIMMY QUIJANO'),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, NULL),
            email = COALESCE(proveedores.email, 'quijanojimmy@gmail.com'),
            web = COALESCE(proveedores.web, NULL),
            direccion = COALESCE(proveedores.direccion, NULL),
            aforo = COALESCE(proveedores.aforo, NULL),
            costo_referencia = COALESCE(proveedores.costo_referencia, NULL),
            notas = COALESCE(proveedores.notas, 'Ciudad original en el Excel: "CDMX"'),
            updated_at = now()
        WHERE id = v_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_id, vv.t FROM (VALUES ('5564076423')) AS vv(t)
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_id AND telefono = vv.t);
END $blk$;

-- Proveedor: MG SHOW AGENCY
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'MG SHOW AGENCY' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'MG SHOW AGENCY',
            (SELECT id FROM paises WHERE nombre = 'México'),
            (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Entretenimiento y artistas'),
            'Activo', 'EDUARDO VALLE', NULL, NULL, NULL, NULL, NULL, NULL, 'Ciudad original en el Excel: "CDMX"', now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'México'),
            region_id = (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            ciudad_id = (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Entretenimiento y artistas'),
            contacto = COALESCE(proveedores.contacto, 'EDUARDO VALLE'),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, NULL),
            email = COALESCE(proveedores.email, NULL),
            web = COALESCE(proveedores.web, NULL),
            direccion = COALESCE(proveedores.direccion, NULL),
            aforo = COALESCE(proveedores.aforo, NULL),
            costo_referencia = COALESCE(proveedores.costo_referencia, NULL),
            notas = COALESCE(proveedores.notas, 'Ciudad original en el Excel: "CDMX"'),
            updated_at = now()
        WHERE id = v_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_id, vv.t FROM (VALUES ('56 3787 9894')) AS vv(t)
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_id AND telefono = vv.t);
END $blk$;

-- Proveedor: AMATI BOX
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'AMATI BOX' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'AMATI BOX',
            (SELECT id FROM paises WHERE nombre = 'México'),
            (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Catering y F&B'),
            'Activo', 'ERICKA NAVARRETE', NULL, NULL, NULL, NULL, NULL, NULL, 'Ciudad original en el Excel: "CDMX"', now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'México'),
            region_id = (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            ciudad_id = (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Catering y F&B'),
            contacto = COALESCE(proveedores.contacto, 'ERICKA NAVARRETE'),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, NULL),
            email = COALESCE(proveedores.email, NULL),
            web = COALESCE(proveedores.web, NULL),
            direccion = COALESCE(proveedores.direccion, NULL),
            aforo = COALESCE(proveedores.aforo, NULL),
            costo_referencia = COALESCE(proveedores.costo_referencia, NULL),
            notas = COALESCE(proveedores.notas, 'Ciudad original en el Excel: "CDMX"'),
            updated_at = now()
        WHERE id = v_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_id, vv.t FROM (VALUES ('56 5116 0899')) AS vv(t)
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_id AND telefono = vv.t);
END $blk$;

-- Proveedor: CLM BANQUETES
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'CLM BANQUETES' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'CLM BANQUETES',
            (SELECT id FROM paises WHERE nombre = 'México'),
            (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Catering y F&B'),
            'Activo', 'KARINA ORTEGA', NULL, 'contacto@clmbanquetes.com', NULL, NULL, NULL, NULL, 'Ciudad original en el Excel: "CDMX"', now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'México'),
            region_id = (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            ciudad_id = (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Catering y F&B'),
            contacto = COALESCE(proveedores.contacto, 'KARINA ORTEGA'),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, NULL),
            email = COALESCE(proveedores.email, 'contacto@clmbanquetes.com'),
            web = COALESCE(proveedores.web, NULL),
            direccion = COALESCE(proveedores.direccion, NULL),
            aforo = COALESCE(proveedores.aforo, NULL),
            costo_referencia = COALESCE(proveedores.costo_referencia, NULL),
            notas = COALESCE(proveedores.notas, 'Ciudad original en el Excel: "CDMX"'),
            updated_at = now()
        WHERE id = v_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_id, vv.t FROM (VALUES ('55 3069 3277')) AS vv(t)
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_id AND telefono = vv.t);
END $blk$;

-- Proveedor: FIRST MOVING
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'FIRST MOVING' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'FIRST MOVING',
            (SELECT id FROM paises WHERE nombre = 'México'),
            (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Logística y transporte'),
            'Activo', 'SERVICIO AL CLIENTE', NULL, NULL, NULL, NULL, NULL, NULL, 'Ciudad original en el Excel: "CDMX"', now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'México'),
            region_id = (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            ciudad_id = (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Logística y transporte'),
            contacto = COALESCE(proveedores.contacto, 'SERVICIO AL CLIENTE'),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, NULL),
            email = COALESCE(proveedores.email, NULL),
            web = COALESCE(proveedores.web, NULL),
            direccion = COALESCE(proveedores.direccion, NULL),
            aforo = COALESCE(proveedores.aforo, NULL),
            costo_referencia = COALESCE(proveedores.costo_referencia, NULL),
            notas = COALESCE(proveedores.notas, 'Ciudad original en el Excel: "CDMX"'),
            updated_at = now()
        WHERE id = v_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_id, vv.t FROM (VALUES ('5535575102')) AS vv(t)
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_id AND telefono = vv.t);
END $blk$;

-- Proveedor: IMMMU
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'IMMMU' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'IMMMU',
            (SELECT id FROM paises WHERE nombre = 'México'),
            (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Salón de eventos'),
            'Activo', 'YESSICA HERNÁNDEZ', NULL, 'yessi.hdz030285@gmail.com', NULL, NULL, NULL, NULL, 'Ciudad original en el Excel: "CDMX"', now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'México'),
            region_id = (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            ciudad_id = (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Salón de eventos'),
            contacto = COALESCE(proveedores.contacto, 'YESSICA HERNÁNDEZ'),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, NULL),
            email = COALESCE(proveedores.email, 'yessi.hdz030285@gmail.com'),
            web = COALESCE(proveedores.web, NULL),
            direccion = COALESCE(proveedores.direccion, NULL),
            aforo = COALESCE(proveedores.aforo, NULL),
            costo_referencia = COALESCE(proveedores.costo_referencia, NULL),
            notas = COALESCE(proveedores.notas, 'Ciudad original en el Excel: "CDMX"'),
            updated_at = now()
        WHERE id = v_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_id, vv.t FROM (VALUES ('55 8784 8185')) AS vv(t)
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_id AND telefono = vv.t);
END $blk$;

-- Proveedor: PAPALOTE MUSEO DEL NIÑO
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'PAPALOTE MUSEO DEL NIÑO' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'PAPALOTE MUSEO DEL NIÑO',
            (SELECT id FROM paises WHERE nombre = 'México'),
            (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Salón de eventos'),
            'Activo', 'GUILLERMO BRITO', NULL, 'guillermo.brito@papalote.org.mx', NULL, NULL, NULL, NULL, 'Ciudad original en el Excel: "CDMX"', now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'México'),
            region_id = (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            ciudad_id = (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Salón de eventos'),
            contacto = COALESCE(proveedores.contacto, 'GUILLERMO BRITO'),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, NULL),
            email = COALESCE(proveedores.email, 'guillermo.brito@papalote.org.mx'),
            web = COALESCE(proveedores.web, NULL),
            direccion = COALESCE(proveedores.direccion, NULL),
            aforo = COALESCE(proveedores.aforo, NULL),
            costo_referencia = COALESCE(proveedores.costo_referencia, NULL),
            notas = COALESCE(proveedores.notas, 'Ciudad original en el Excel: "CDMX"'),
            updated_at = now()
        WHERE id = v_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_id, vv.t FROM (VALUES ('56 2443 3127')) AS vv(t)
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_id AND telefono = vv.t);
END $blk$;

-- Proveedor: EVENT PLANNER
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'EVENT PLANNER' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'EVENT PLANNER',
            (SELECT id FROM paises WHERE nombre = 'México'),
            (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Diseño gráfico y creatividad'),
            'Activo', 'ABNÉR CÉRDOBA', NULL, NULL, NULL, NULL, NULL, NULL, 'Ciudad original en el Excel: "CDMX" · Correo en el Excel no parece válido, se guardó como nota: "https://www.eventplannermexico.mx/"', now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'México'),
            region_id = (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            ciudad_id = (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Diseño gráfico y creatividad'),
            contacto = COALESCE(proveedores.contacto, 'ABNÉR CÉRDOBA'),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, NULL),
            email = COALESCE(proveedores.email, NULL),
            web = COALESCE(proveedores.web, NULL),
            direccion = COALESCE(proveedores.direccion, NULL),
            aforo = COALESCE(proveedores.aforo, NULL),
            costo_referencia = COALESCE(proveedores.costo_referencia, NULL),
            notas = COALESCE(proveedores.notas, 'Ciudad original en el Excel: "CDMX" · Correo en el Excel no parece válido, se guardó como nota: "https://www.eventplannermexico.mx/"'),
            updated_at = now()
        WHERE id = v_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_id, vv.t FROM (VALUES ('55 4473 0731')) AS vv(t)
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_id AND telefono = vv.t);
END $blk$;

-- Proveedor: RIDENSDL
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'RIDENSDL' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'RIDENSDL',
            (SELECT id FROM paises WHERE nombre = 'México'),
            (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Diseño gráfico y creatividad'),
            'Activo', 'GUSTABO RUÍZ', NULL, 'marketingridensdl@gmail.com', NULL, NULL, NULL, NULL, 'Ciudad original en el Excel: "CDMX"', now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'México'),
            region_id = (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            ciudad_id = (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Diseño gráfico y creatividad'),
            contacto = COALESCE(proveedores.contacto, 'GUSTABO RUÍZ'),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, NULL),
            email = COALESCE(proveedores.email, 'marketingridensdl@gmail.com'),
            web = COALESCE(proveedores.web, NULL),
            direccion = COALESCE(proveedores.direccion, NULL),
            aforo = COALESCE(proveedores.aforo, NULL),
            costo_referencia = COALESCE(proveedores.costo_referencia, NULL),
            notas = COALESCE(proveedores.notas, 'Ciudad original en el Excel: "CDMX"'),
            updated_at = now()
        WHERE id = v_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_id, vv.t FROM (VALUES ('55 3996 0523')) AS vv(t)
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_id AND telefono = vv.t);
END $blk$;

-- Proveedor: GRUPO GEORNAY
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'GRUPO GEORNAY' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'GRUPO GEORNAY',
            (SELECT id FROM paises WHERE nombre = 'México'),
            (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Logística y transporte'),
            'Activo', 'NAY FLORES', NULL, 'contacto@grupogeornay.com', NULL, NULL, NULL, NULL, 'Ciudad original en el Excel: "CDMX"', now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'México'),
            region_id = (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            ciudad_id = (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Logística y transporte'),
            contacto = COALESCE(proveedores.contacto, 'NAY FLORES'),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, NULL),
            email = COALESCE(proveedores.email, 'contacto@grupogeornay.com'),
            web = COALESCE(proveedores.web, NULL),
            direccion = COALESCE(proveedores.direccion, NULL),
            aforo = COALESCE(proveedores.aforo, NULL),
            costo_referencia = COALESCE(proveedores.costo_referencia, NULL),
            notas = COALESCE(proveedores.notas, 'Ciudad original en el Excel: "CDMX"'),
            updated_at = now()
        WHERE id = v_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_id, vv.t FROM (VALUES ('5514101920')) AS vv(t)
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_id AND telefono = vv.t);
END $blk$;

-- Proveedor: PHOTOSHOW
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'PHOTOSHOW' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'PHOTOSHOW',
            (SELECT id FROM paises WHERE nombre = 'México'),
            (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Otro'),
            'Activo', NULL, NULL, NULL, 'Photoshow', NULL, NULL, NULL, 'Ciudad original en el Excel: "CDMX"', now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'México'),
            region_id = (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            ciudad_id = (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Otro'),
            contacto = COALESCE(proveedores.contacto, NULL),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, NULL),
            email = COALESCE(proveedores.email, NULL),
            web = COALESCE(proveedores.web, 'Photoshow'),
            direccion = COALESCE(proveedores.direccion, NULL),
            aforo = COALESCE(proveedores.aforo, NULL),
            costo_referencia = COALESCE(proveedores.costo_referencia, NULL),
            notas = COALESCE(proveedores.notas, 'Ciudad original en el Excel: "CDMX"'),
            updated_at = now()
        WHERE id = v_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_id, vv.t FROM (VALUES ('5560500037')) AS vv(t)
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_id AND telefono = vv.t);
END $blk$;

-- Proveedor: AYUDA BARTENDER
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'AYUDA BARTENDER' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'AYUDA BARTENDER',
            (SELECT id FROM paises WHERE nombre = 'México'),
            (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Catering y F&B'),
            'Activo', 'Cristian Guerrero González', NULL, 'ayudabartender@gmail.com', NULL, NULL, NULL, NULL, 'Ciudad original en el Excel: "CDMX"', now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'México'),
            region_id = (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            ciudad_id = (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Catering y F&B'),
            contacto = COALESCE(proveedores.contacto, 'Cristian Guerrero González'),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, NULL),
            email = COALESCE(proveedores.email, 'ayudabartender@gmail.com'),
            web = COALESCE(proveedores.web, NULL),
            direccion = COALESCE(proveedores.direccion, NULL),
            aforo = COALESCE(proveedores.aforo, NULL),
            costo_referencia = COALESCE(proveedores.costo_referencia, NULL),
            notas = COALESCE(proveedores.notas, 'Ciudad original en el Excel: "CDMX"'),
            updated_at = now()
        WHERE id = v_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_id, vv.t FROM (VALUES ('55 3331 7816')) AS vv(t)
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_id AND telefono = vv.t);
END $blk$;

-- Proveedor: LIVISLOAD
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'LIVISLOAD' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'LIVISLOAD',
            (SELECT id FROM paises WHERE nombre = 'México'),
            (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Logística y transporte'),
            'Activo', 'LILIANA', NULL, 'gerencia1@livisload.com.mx', 'livisload.com.mx', NULL, NULL, NULL, 'Ciudad original en el Excel: "CDMX"', now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'México'),
            region_id = (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            ciudad_id = (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Logística y transporte'),
            contacto = COALESCE(proveedores.contacto, 'LILIANA'),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, NULL),
            email = COALESCE(proveedores.email, 'gerencia1@livisload.com.mx'),
            web = COALESCE(proveedores.web, 'livisload.com.mx'),
            direccion = COALESCE(proveedores.direccion, NULL),
            aforo = COALESCE(proveedores.aforo, NULL),
            costo_referencia = COALESCE(proveedores.costo_referencia, NULL),
            notas = COALESCE(proveedores.notas, 'Ciudad original en el Excel: "CDMX"'),
            updated_at = now()
        WHERE id = v_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_id, vv.t FROM (VALUES ('5534499880')) AS vv(t)
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_id AND telefono = vv.t);
END $blk$;

-- Proveedor: TRAGO X TRAGO
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'TRAGO X TRAGO' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'TRAGO X TRAGO',
            (SELECT id FROM paises WHERE nombre = 'México'),
            (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Catering y F&B'),
            'Activo', 'Lalo Bermúdez', NULL, 'admin.tragoxtrago@gmail.com', 'http://instagram.com/tragoxtrago_?igsh=MWo3bXNucGVjbm80Mg%3D%3D', NULL, NULL, NULL, 'Ciudad original en el Excel: "CDMX" · Otros correos en el Excel para este contacto (solo se guarda uno como Email): tragoxtrago2@gmail.com', now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'México'),
            region_id = (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            ciudad_id = (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Catering y F&B'),
            contacto = COALESCE(proveedores.contacto, 'Lalo Bermúdez'),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, NULL),
            email = COALESCE(proveedores.email, 'admin.tragoxtrago@gmail.com'),
            web = COALESCE(proveedores.web, 'http://instagram.com/tragoxtrago_?igsh=MWo3bXNucGVjbm80Mg%3D%3D'),
            direccion = COALESCE(proveedores.direccion, NULL),
            aforo = COALESCE(proveedores.aforo, NULL),
            costo_referencia = COALESCE(proveedores.costo_referencia, NULL),
            notas = COALESCE(proveedores.notas, 'Ciudad original en el Excel: "CDMX" · Otros correos en el Excel para este contacto (solo se guarda uno como Email): tragoxtrago2@gmail.com'),
            updated_at = now()
        WHERE id = v_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_id, vv.t FROM (VALUES ('55 2903 8020')) AS vv(t)
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_id AND telefono = vv.t);
END $blk$;

-- Proveedor: LAZZAR
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'LAZZAR' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'LAZZAR',
            (SELECT id FROM paises WHERE nombre = 'México'),
            (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Toluca' LIMIT 1),
            (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Toluca' LIMIT 1),
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Merchandising y regalos corporativos'),
            'Activo', 'LAZZAR', NULL, 'kflores@lazzarmexico.com', 'lazzarmexico.com', NULL, NULL, NULL, NULL, now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'México'),
            region_id = (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Toluca' LIMIT 1),
            ciudad_id = (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Toluca' LIMIT 1),
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Merchandising y regalos corporativos'),
            contacto = COALESCE(proveedores.contacto, 'LAZZAR'),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, NULL),
            email = COALESCE(proveedores.email, 'kflores@lazzarmexico.com'),
            web = COALESCE(proveedores.web, 'lazzarmexico.com'),
            direccion = COALESCE(proveedores.direccion, NULL),
            aforo = COALESCE(proveedores.aforo, NULL),
            costo_referencia = COALESCE(proveedores.costo_referencia, NULL),
            notas = COALESCE(proveedores.notas, NULL),
            updated_at = now()
        WHERE id = v_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_id, vv.t FROM (VALUES ('5545938657')) AS vv(t)
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_id AND telefono = vv.t);
END $blk$;

-- Proveedor: BUNKER
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'BUNKER' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'BUNKER',
            (SELECT id FROM paises WHERE nombre = 'México'),
            (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Impresos y señalética'),
            'Activo', 'GIO', 'VENTAS', 'angel@bunkeragency.mx', 'Bunker', NULL, NULL, NULL, 'Ciudad original en el Excel: "CDMX"', now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'México'),
            region_id = (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            ciudad_id = (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Impresos y señalética'),
            contacto = COALESCE(proveedores.contacto, 'GIO'),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, 'VENTAS'),
            email = COALESCE(proveedores.email, 'angel@bunkeragency.mx'),
            web = COALESCE(proveedores.web, 'Bunker'),
            direccion = COALESCE(proveedores.direccion, NULL),
            aforo = COALESCE(proveedores.aforo, NULL),
            costo_referencia = COALESCE(proveedores.costo_referencia, NULL),
            notas = COALESCE(proveedores.notas, 'Ciudad original en el Excel: "CDMX"'),
            updated_at = now()
        WHERE id = v_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_id, vv.t FROM (VALUES ('4421231493')) AS vv(t)
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_id AND telefono = vv.t);
END $blk$;

-- Proveedor: EBENEZER EVENTOS
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'EBENEZER EVENTOS' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'EBENEZER EVENTOS',
            (SELECT id FROM paises WHERE nombre = 'México'),
            (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Mobiliario y decoración'),
            'Activo', 'Miguel CR', NULL, 'contacto@ebenezereventos.com', 'Ebenezer Eventos', NULL, NULL, NULL, 'Ciudad original en el Excel: "CDMX"', now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'México'),
            region_id = (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            ciudad_id = (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Mobiliario y decoración'),
            contacto = COALESCE(proveedores.contacto, 'Miguel CR'),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, NULL),
            email = COALESCE(proveedores.email, 'contacto@ebenezereventos.com'),
            web = COALESCE(proveedores.web, 'Ebenezer Eventos'),
            direccion = COALESCE(proveedores.direccion, NULL),
            aforo = COALESCE(proveedores.aforo, NULL),
            costo_referencia = COALESCE(proveedores.costo_referencia, NULL),
            notas = COALESCE(proveedores.notas, 'Ciudad original en el Excel: "CDMX"'),
            updated_at = now()
        WHERE id = v_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_id, vv.t FROM (VALUES ('55 2910 9393')) AS vv(t)
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_id AND telefono = vv.t);
END $blk$;

-- Proveedor: RETORIKA
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'RETORIKA' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'RETORIKA',
            (SELECT id FROM paises WHERE nombre = 'México'),
            (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Impresos y señalética'),
            'Activo', 'TAMARA', NULL, NULL, 'https://www.retorika-promo.com/', NULL, NULL, NULL, 'Ciudad original en el Excel: "CDMX"', now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'México'),
            region_id = (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            ciudad_id = (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Impresos y señalética'),
            contacto = COALESCE(proveedores.contacto, 'TAMARA'),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, NULL),
            email = COALESCE(proveedores.email, NULL),
            web = COALESCE(proveedores.web, 'https://www.retorika-promo.com/'),
            direccion = COALESCE(proveedores.direccion, NULL),
            aforo = COALESCE(proveedores.aforo, NULL),
            costo_referencia = COALESCE(proveedores.costo_referencia, NULL),
            notas = COALESCE(proveedores.notas, 'Ciudad original en el Excel: "CDMX"'),
            updated_at = now()
        WHERE id = v_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_id, vv.t FROM (VALUES ('5626967595')) AS vv(t)
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_id AND telefono = vv.t);
END $blk$;

-- Proveedor: BIALAZI
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'BIALAZI' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'BIALAZI',
            (SELECT id FROM paises WHERE nombre = 'México'),
            (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Impresos y señalética'),
            'Activo', 'LUIS LUNA', 'DIRECTOR GENERAL', 'luis@bialazi.com', 'http://bialazi.com', NULL, NULL, NULL, 'Ciudad original en el Excel: "CDMX"', now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'México'),
            region_id = (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            ciudad_id = (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Impresos y señalética'),
            contacto = COALESCE(proveedores.contacto, 'LUIS LUNA'),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, 'DIRECTOR GENERAL'),
            email = COALESCE(proveedores.email, 'luis@bialazi.com'),
            web = COALESCE(proveedores.web, 'http://bialazi.com'),
            direccion = COALESCE(proveedores.direccion, NULL),
            aforo = COALESCE(proveedores.aforo, NULL),
            costo_referencia = COALESCE(proveedores.costo_referencia, NULL),
            notas = COALESCE(proveedores.notas, 'Ciudad original en el Excel: "CDMX"'),
            updated_at = now()
        WHERE id = v_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_id, vv.t FROM (VALUES ('5523205639')) AS vv(t)
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_id AND telefono = vv.t);
END $blk$;

-- Proveedor: RAÍZ STUDIO
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'RAÍZ STUDIO' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'RAÍZ STUDIO',
            (SELECT id FROM paises WHERE nombre = 'México'),
            (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Flores y ambientación'),
            'Activo', 'MÓNICA RODRÍGUEZZ', 'FUNDADORA', 'monicarr1990@gmail.com', 'https://www.instagram.com/raizestudio.mx', NULL, NULL, NULL, 'Ciudad original en el Excel: "CDMX"', now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'México'),
            region_id = (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            ciudad_id = (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Flores y ambientación'),
            contacto = COALESCE(proveedores.contacto, 'MÓNICA RODRÍGUEZZ'),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, 'FUNDADORA'),
            email = COALESCE(proveedores.email, 'monicarr1990@gmail.com'),
            web = COALESCE(proveedores.web, 'https://www.instagram.com/raizestudio.mx'),
            direccion = COALESCE(proveedores.direccion, NULL),
            aforo = COALESCE(proveedores.aforo, NULL),
            costo_referencia = COALESCE(proveedores.costo_referencia, NULL),
            notas = COALESCE(proveedores.notas, 'Ciudad original en el Excel: "CDMX"'),
            updated_at = now()
        WHERE id = v_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_id, vv.t FROM (VALUES ('3314736577')) AS vv(t)
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_id AND telefono = vv.t);
END $blk$;

-- Proveedor: ARTIK
DO $blk$
DECLARE v_id uuid;
BEGIN
    SELECT id INTO v_id FROM proveedores WHERE nombre = 'ARTIK' LIMIT 1;
    IF v_id IS NULL THEN
        v_id := gen_random_uuid();
        INSERT INTO proveedores (id, nombre, pais_id, region_id, ciudad_id, categoria_id, estado, contacto, cargo_contacto, email, web, direccion, aforo, costo_referencia, notas, created_at, updated_at)
        VALUES (v_id, 'ARTIK',
            (SELECT id FROM paises WHERE nombre = 'México'),
            (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            (SELECT id FROM categorias_proveedor WHERE nombre = 'Salón de eventos'),
            'Activo', 'JESÚS NAVA', NULL, 'kineziskidzania@gmail.com', 'go.artik.com.mx', NULL, NULL, NULL, 'Ciudad original en el Excel: "CDMX"', now(), now());
    ELSE
        UPDATE proveedores SET
            pais_id = (SELECT id FROM paises WHERE nombre = 'México'),
            region_id = (SELECT ci.region_id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            ciudad_id = (SELECT ci.id FROM ciudades ci JOIN regiones r ON ci.region_id = r.id JOIN paises p ON r.pais_id = p.id WHERE p.nombre = 'México' AND ci.nombre = 'Ciudad de México' LIMIT 1),
            categoria_id = (SELECT id FROM categorias_proveedor WHERE nombre = 'Salón de eventos'),
            contacto = COALESCE(proveedores.contacto, 'JESÚS NAVA'),
            cargo_contacto = COALESCE(proveedores.cargo_contacto, NULL),
            email = COALESCE(proveedores.email, 'kineziskidzania@gmail.com'),
            web = COALESCE(proveedores.web, 'go.artik.com.mx'),
            direccion = COALESCE(proveedores.direccion, NULL),
            aforo = COALESCE(proveedores.aforo, NULL),
            costo_referencia = COALESCE(proveedores.costo_referencia, NULL),
            notas = COALESCE(proveedores.notas, 'Ciudad original en el Excel: "CDMX"'),
            updated_at = now()
        WHERE id = v_id;
    END IF;

    INSERT INTO proveedor_telefonos (id, proveedor_id, telefono)
    SELECT gen_random_uuid(), v_id, vv.t FROM (VALUES ('5612893993')) AS vv(t)
    WHERE NOT EXISTS (SELECT 1 FROM proveedor_telefonos WHERE proveedor_id = v_id AND telefono = vv.t);
END $blk$;

