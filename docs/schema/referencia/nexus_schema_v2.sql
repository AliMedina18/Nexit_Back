-- ============================================================
-- ADVERTENCIA (agregada 2026-08-20): NO ejecutar este archivo completo para crear un
-- proyecto de Supabase nuevo. Quedó desactualizado frente a las migraciones reales de
-- EF Core (ej. falta la columna `updated_by`, agregada por la migración
-- `AddConcurrencyAndAuditTracking`) -- usarlo tal cual reproduce el mismo problema de
-- esquema desincronizado que se encontró y corrigió en la base local `nexit_dev`.
-- Para crear un proyecto nuevo, sigue en cambio docs/09-crear-proyecto-supabase-paso-a-paso.md:
-- las tablas se crean con `dotnet ef database update`, y solo lo que ese comando no
-- puede crear (la relación con auth.users y Row Level Security) se aplica aparte con
-- docs/schema/04_extras_supabase_post_migraciones.sql. Este archivo se conserva como
-- referencia legible del diseño completo, no como script para ejecutar de punta a punta.
-- ============================================================

-- ============================================================
-- NEXUS · Esquema de base de datos v2 (PostgreSQL / Supabase)
-- Fase 1 · Análisis y planificación — Proyecto "Nexit"
--
-- Parte de la base construida por el prototipo original
-- (nexus_schema.sql) y agrega lo acordado para el MVP:
--   1. Entidad CLIENTES propia (antes era texto libre)
--   2. Seguimiento operativo de proyectos (prioridad, ciudad,
--      fechas de solicitud, estado de propuestas, facturación)
--   3. Bitácora de seguimiento por proyecto (antes vivía como
--      texto libre acumulado en una sola celda de Excel)
--
-- Todo lo marcado "NUEVO" o "AMPLIADO" es lo que cambia frente
-- al schema original de la amiga. El resto se conserva igual.
--
-- Actualización 2026-08-17 (segunda revisión de campos, tras
-- releer las 13 hojas del Excel de seguimiento): se agregan
-- porcentaje_avance, clientes.valor_referencia, y una columna
-- "area" en la bitácora para no perder las notas por
-- departamento (creativo/comercial/administrativo) ni las
-- "OBSERVACIONES ADICIONALES" que en el Excel vivían separadas
-- de "SEGUIMIENTO". Ver notas 9 y 10 al final del archivo.
--
-- Actualización 2026-08-17 (tercera revisión, verificación
-- exhaustiva columna por columna + barrido de palabras clave
-- financieras/contractuales en las 13 hojas + columnas sin
-- encabezado con datos): se agrega el valor 'No ejecutado' a
-- proyecto_estado. No se encontró ningún otro campo faltante.
-- Ver nota 11 al final del archivo.
--
-- Actualización 2026-08-17 (cuarta revisión, a pedido explícito
-- de la usuaria): el HTML trae país→departamento/estado→ciudad
-- y la lista de categorías de proveedor como listas fijas
-- incrustadas en el JavaScript (constante GEO y el <select
-- id="f_cat">), y la lista de estados de proyecto como grupos
-- fijos en el <select id="pf_estado"> (Fase 1/2/3). Nada de eso
-- eran columnas de tabla en el prototipo — eran listas de
-- opciones para los formularios — pero SÍ es información que el
-- sistema nuevo necesita como catálogo real y consultable, no
-- como texto libre ni como un ENUM cerrado que solo se puede
-- ampliar cambiando el código. Se agregan 5 tablas de catálogo
-- (paises, regiones, ciudades, categorias_proveedor,
-- estados_proyecto), proveedores y proyectos pasan a referenciar
-- estas tablas por FK en vez de texto libre / ENUM, y el ENUM
-- proyecto_estado se elimina. Ver sección 3 y nota 12 al final.
-- Los datos de las 3 listas se cargan con el script separado
-- seed_geografia_categorias_estados.sql.
--
-- Actualización 2026-08-17 (quinta revisión: aplicación formal
-- de 1FN/2FN/3FN/4FN). Se encontraron y corrigieron 3 violaciones
-- reales:
--   a) 1FN — proveedores.servicios guardaba una lista separada
--      por comas en un solo campo (valor no atómico). Se separa
--      en catálogo `servicios` + tabla intermedia
--      `proveedor_servicios`.
--   b) 1FN — clientes/proveedores tenían "telefono" y "telefono2"
--      como 2 columnas para el mismo tipo de dato (grupo
--      repetitivo representado como columnas en vez de filas). Se
--      separan en `cliente_telefonos` y `proveedor_telefonos`.
--   c) 3FN — estados_proyecto.fase_nombre dependía de
--      estados_proyecto.fase (un atributo no-clave), no
--      directamente de estados_proyecto.id -> dependencia
--      transitiva. Se separa en catálogo `fases_proyecto` y
--      fase_nombre se elimina (se obtiene hociendo JOIN).
-- 2FN ya se cumplía en todo el modelo (ninguna tabla con clave
-- compuesta tiene atributos adicionales que pudieran depender
-- solo de una parte de la clave). No se encontraron violaciones
-- de 4FN adicionales una vez resueltas (a) y (b): cada hecho
-- multivaluado independiente (servicios, teléfonos, proveedores
-- asignados a un proyecto, equipo asignado a un proyecto) ya
-- vive en su propia tabla intermedia, sin mezclarse con otro
-- hecho multivaluado en la misma tabla. Ver el documento de
-- análisis (sección 10) para el detalle completo tabla por tabla,
-- y la nota 13 al final de este archivo.
-- ============================================================

-- Requiere pgcrypto para gen_random_uuid() (viene habilitado
-- por defecto en Supabase; en Postgres puro:
-- CREATE EXTENSION IF NOT EXISTS pgcrypto;

-- ============================================================
-- 1. TIPOS ENUMERADOS
-- ============================================================

CREATE TYPE proveedor_estado AS ENUM (
  'Activo', 'En evaluación', 'Pausado', 'Bloqueado'
);

CREATE TYPE presupuesto_tier AS ENUM (
  '$ Bajo (<20k)',
  '$$ Medio (20k–100k)',
  '$$$ Alto (100k–500k)',
  '$$$$ Premium (>500k)'
);

CREATE TYPE cobertura_tipo AS ENUM (
  'Solo ciudad', 'Regional', 'Nacional', 'Internacional'
);

-- El estado del proyecto (antes ENUM proyecto_estado) ahora es la
-- tabla estados_proyecto (sección 3): el HTML lo mostraba
-- agrupado en 3 fases con nombre propio ("Fase 1 · Planeación
-- interna", "Fase 2 · Con decisión del cliente", "Fase 3 · Cierre
-- y facturación"), y esa agrupación es información real que un
-- ENUM plano no puede guardar. 'No ejecutado' (agregado en la
-- tercera revisión) queda dentro de esa tabla también: en el
-- Excel hay una hoja "NO EJECUTADOS" separada todos los años
-- (2023-2026) para proyectos que se cotizaron pero el cliente
-- nunca confirmó — distinto de 'Cancelado', que implica que algo
-- sí se había confirmado y se frenó a mitad de camino.

CREATE TYPE brief_estado AS ENUM (
  'Pendiente por enviar',
  'Entregado, a espera de respuesta',
  'Requiere ajustes',
  'Aprobado'
);

CREATE TYPE adjunto_tipo AS ENUM ('link', 'file');

-- AMPLIADO: se agregan 'Comercial' y 'Administrativo'.
-- En el Excel de seguimiento (SEG.PROYECTOS CORP) cada proyecto
-- reparte responsabilidades en 3 frentes -> Creativo, Comercial
-- y Administrativo, además del/los Ejecutivo(s) a cargo. Se
-- unifica todo bajo la misma tabla proyecto_equipo en vez de
-- crear una tabla nueva por frente.
CREATE TYPE rol_equipo AS ENUM (
  'Ejecutivo', 'Comercial', 'Administrativo',
  'Diseñador 3D', 'Diseñador gráfico'
);

CREATE TYPE informe_periodo AS ENUM ('semanal', 'mensual');

-- NUEVO: tipo de proyecto. En el Excel histórico esto estaba
-- implícito en el nombre de la hoja (SEG.PROYECTOS CORP vs.
-- EVENTOS SOCIALES); aquí se vuelve un campo explícito para
-- poder filtrar/reportar sin depender de hojas separadas.
CREATE TYPE tipo_proyecto AS ENUM ('Corporativo', 'Evento social');

-- NUEVO: prioridad del proyecto (columna PRIORIDAD del Excel:
-- ALTA / MEDIA / BAJA).
CREATE TYPE prioridad_proyecto AS ENUM ('Alta', 'Media', 'Baja');

-- NUEVO: estado de la propuesta enviada al cliente para un
-- proyecto (columna PROPUESTA del seguimiento operativo, texto
-- libre "ENVIADA" / "N/A" en el Excel). Es UN solo campo: en un
-- primer borrador se había separado en 3 columnas (creativa /
-- gráfica / económica, copiando un formato antiguo y ya en desuso
-- de 2 hojas del Excel), pero eso repetía la misma columna 3
-- veces sin necesidad -- ver nota 14 más abajo.
CREATE TYPE propuesta_estado AS ENUM (
  'No enviada', 'En proceso', 'Enviada'
);

-- NUEVO: a qué área pertenece una nota de la bitácora de
-- seguimiento. El Excel traía notas de avance mezcladas con
-- notas específicas de creativo/comercial/administrativo en
-- columnas separadas; aquí quedan todas en la misma tabla,
-- diferenciadas por esta etiqueta.
CREATE TYPE area_seguimiento AS ENUM (
  'General', 'Creativo', 'Comercial', 'Administrativo'
);

-- NUEVO (octava revisión): rol del usuario dentro del sistema.
-- Antes era "rol text" con los valores permitidos solo escritos
-- en un comentario (nadie los obligaba a nivel de base de
-- datos) -- se vuelve ENUM para que quede validado de verdad,
-- igual que el resto de catálogos cerrados del sistema.
-- AMPLIADO (novena revisión, modelo de permisos de 4 niveles --
-- ver docs/06-modelo-permisos-roles.md): se agrega 'super_admin'
-- por encima de 'admin'. super_admin es exclusivo de quien
-- desarrolla/administra el sistema y es el ÚNICO rol que puede
-- ver o gestionar la tabla usuarios; admin puede administrar todo
-- lo demás (catálogos, y aprobar/rechazar solicitudes de
-- eliminación) pero no usuarios.
CREATE TYPE rol_usuario AS ENUM ('super_admin', 'admin', 'manager', 'miembro');

-- ============================================================
-- 2. USUARIOS (extiende auth.users de Supabase)
--    AMPLIADO (octava revisión): usuarios internos de Next.
--    - nombre/apellido separados (antes "nombre_completo" único):
--      permite ordenar/buscar por apellido y usarlo en plantillas
--      de correo ("Hola Alicia") sin tener que separar texto.
--    - activo: para dar de baja a alguien sin borrar su historial
--      (notas de seguimiento, proyectos creados, etc. quedan con
--      su autor intacto).
--    - rol ahora es el ENUM rol_usuario (antes texto libre con los
--      valores válidos solo en un comentario).
--    - updated_at: se agrega por consistencia con el resto de
--      tablas "vivas" del sistema (clientes, proveedores,
--      proyectos ya lo tenían).
--    - La CONTRASEÑA de cada usuario NO se guarda aquí: vive
--      cifrada en auth.users (Supabase Auth), que es quien la
--      administra. La tabla usuarios nunca debe tener una columna
--      de contraseña.
--    - La VERIFICACIÓN del correo al primer login (token +
--      expiración + envío del correo) tampoco se modela aquí:
--      se usa el flujo nativo de Supabase Auth (confirmation
--      token + email_confirmed_at en auth.users), en vez de
--      construir una tabla y un envío de correo propios -- ver
--      nota 15 al final del archivo.
-- ============================================================

CREATE TABLE usuarios (
  id          uuid PRIMARY KEY REFERENCES auth.users(id) ON DELETE CASCADE,
  nombre      text NOT NULL,
  apellido    text NOT NULL,
  email       text NOT NULL UNIQUE,  -- AMPLIADO: espejo de auth.users.email, validado contra dominios_correo_permitidos (ver trigger más abajo)
  rol         rol_usuario NOT NULL DEFAULT 'miembro',  -- AMPLIADO: antes texto libre
  iniciales   text,
  activo      boolean NOT NULL DEFAULT true,          -- NUEVO
  created_at  timestamptz NOT NULL DEFAULT now(),
  updated_at  timestamptz NOT NULL DEFAULT now()       -- NUEVO
);

-- NUEVO (octava revisión): dominios de correo laboral permitidos
-- para crear una cuenta (ej. "nextcolombia.com"). Se modela como
-- catálogo -- no como un valor fijo en código -- porque Next
-- opera en más de un país (Colombia y México, según los datos
-- de origen) y podría sumar otro dominio sin tocar el schema.
CREATE TABLE dominios_correo_permitidos (
  id       uuid PRIMARY KEY DEFAULT gen_random_uuid(),
  dominio  text NOT NULL UNIQUE  -- sin "@", ej. "nextcolombia.com"
);

-- Valida que usuarios.email termine en uno de los dominios
-- permitidos. Es un respaldo a nivel de base de datos: la
-- validación "de verdad" (evitar incluso intentar el signup en
-- Supabase Auth con un correo no laboral) debe hacerse también
-- en la aplicación, antes de llamar a Supabase Auth -- este
-- trigger existe para que, pase lo que pase en la app, la base
-- de datos nunca termine con un usuario de un dominio no permitido.
CREATE OR REPLACE FUNCTION check_usuario_dominio_correo()
RETURNS trigger AS $$
BEGIN
  IF NOT EXISTS (
    SELECT 1 FROM dominios_correo_permitidos d
    WHERE lower(NEW.email) LIKE '%@' || lower(d.dominio)
  ) THEN
    RAISE EXCEPTION 'El correo % no pertenece a un dominio laboral permitido', NEW.email;
  END IF;
  RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_usuarios_dominio_correo
  BEFORE INSERT OR UPDATE OF email ON usuarios
  FOR EACH ROW EXECUTE FUNCTION check_usuario_dominio_correo();

-- ============================================================
-- 3. CATÁLOGOS (NUEVO, cuarta revisión)
--    Vienen de listas que estaban incrustadas como fijas dentro
--    del HTML: la jerarquía país→departamento/estado→ciudad
--    (constante GEO) y la lista de categorías de proveedor
--    (<select id="f_cat">) y de estados de proyecto (<select
--    id="pf_estado">, agrupado en 3 fases). Se separan en tablas
--    propias en vez de dejarlas como texto libre o como ENUM
--    para que: a) la jerarquía país/departamento/ciudad quede
--    garantizada por FK (una ciudad no puede quedar "suelta" de
--    un departamento que no es de su país), b) se puedan agregar
--    países, departamentos, ciudades o categorías nuevas
--    insertando una fila (igual que el "Otro" del formulario)
--    sin tener que cambiar el schema, y c) el estado de proyecto
--    pueda llevar su fase (1/2/3) como dato consultable, no solo
--    como comentario. (fases_proyecto se agregó en la quinta
--    revisión para eliminar una dependencia transitiva, ver nota
--    13 al final del archivo.)
-- ============================================================

CREATE TABLE paises (
  id              uuid PRIMARY KEY DEFAULT gen_random_uuid(),
  nombre          text NOT NULL UNIQUE,
  etiqueta_region text NOT NULL DEFAULT 'Departamento'  -- cómo se le llama al 2do nivel en ese país: "Departamento" (Colombia) o "Estado" (México, EE.UU.)
);

CREATE TABLE regiones (
  id       uuid PRIMARY KEY DEFAULT gen_random_uuid(),
  pais_id  uuid NOT NULL REFERENCES paises(id) ON DELETE CASCADE,
  nombre   text NOT NULL,           -- departamento o estado
  UNIQUE (pais_id, nombre)
);

CREATE INDEX idx_regiones_pais ON regiones(pais_id);

CREATE TABLE ciudades (
  id         uuid PRIMARY KEY DEFAULT gen_random_uuid(),
  region_id  uuid NOT NULL REFERENCES regiones(id) ON DELETE CASCADE,
  nombre     text NOT NULL,
  UNIQUE (region_id, nombre)
);

CREATE INDEX idx_ciudades_region ON ciudades(region_id);

CREATE TABLE categorias_proveedor (
  id      uuid PRIMARY KEY DEFAULT gen_random_uuid(),
  nombre  text NOT NULL UNIQUE
);

-- NUEVO (quinta revisión, 3FN): antes estados_proyecto.fase_nombre
-- repetía el mismo texto en cada estado de la misma fase (una
-- dependencia transitiva: fase_nombre depende de fase, no
-- directamente de estados_proyecto.id). Se separa aquí.
CREATE TABLE fases_proyecto (
  fase    smallint PRIMARY KEY CHECK (fase BETWEEN 1 AND 3),
  nombre  text NOT NULL UNIQUE  -- "Planeación interna" / "Con decisión del cliente" / "Cierre y facturación"
);

CREATE TABLE estados_proyecto (
  id          uuid PRIMARY KEY DEFAULT gen_random_uuid(),
  nombre      text NOT NULL UNIQUE,
  fase        smallint NOT NULL REFERENCES fases_proyecto(fase),  -- AMPLIADO: antes smallint con CHECK; ahora FK (3FN)
  orden       smallint NOT NULL UNIQUE  -- para mostrar los estados siempre en el mismo orden
);

CREATE INDEX idx_estados_proyecto_fase ON estados_proyecto(fase);

-- ============================================================
-- 4. CLIENTES (NUEVO)
--    Antes "cliente" era un campo de texto libre dentro de
--    Proyectos. Se separa en su propia tabla porque:
--    a) BD_CLIENTES_Y_PROVEEDORES_NEXT_MX.xlsx ya trae datos
--       de cliente igual de estructurados que los de proveedor
--       (teléfono, correo, cargo, ciudad, etc.)
--    b) Permite reportes por cliente (recurrencia, facturación
--       acumulada) y evita clientes duplicados por errores de
--       tipeo (ej. "SURA" vs "Sura S.A.").
--    Se modela igual que "proveedores" para mantener el mismo
--    patrón en todo el sistema.
-- ============================================================

CREATE TABLE clientes (
  id             uuid PRIMARY KEY DEFAULT gen_random_uuid(),
  nombre         text NOT NULL,        -- nombre del cliente/empresa, ej. "SURA"
  sector         text,                 -- especialización / industria (ESPECIALIZACION en el Excel)
  ciudad         text,
  direccion      text,
  web            text,
  contacto       text,                 -- persona de contacto principal
  cargo_contacto text,                 -- cargo de esa persona (CARGO en el Excel)
  email          text,
  valor_referencia text,          -- NUEVO: tamaño/valor de referencia de los proyectos de este cliente (COSTO en BD_CLIENTES)
  notas          text,
  created_by     uuid REFERENCES usuarios(id),  -- auditoría: qué usuario de Next registró este cliente; NO implica que un cliente sea un usuario
  created_at     timestamptz NOT NULL DEFAULT now(),
  updated_at     timestamptz NOT NULL DEFAULT now()
);

CREATE INDEX idx_clientes_ciudad ON clientes(ciudad);

-- NUEVO (quinta revisión, 1FN): reemplaza "telefono"/"telefono2"
-- (un grupo repetitivo representado como 2 columnas). Un cliente
-- puede tener 1, 2 o más teléfonos sin agregar columnas nuevas.
CREATE TABLE cliente_telefonos (
  id          uuid PRIMARY KEY DEFAULT gen_random_uuid(),
  cliente_id  uuid NOT NULL REFERENCES clientes(id) ON DELETE CASCADE,
  telefono    text NOT NULL,
  etiqueta    text  -- ej. "Principal", "WhatsApp", "Oficina"
);

CREATE INDEX idx_cliente_telefonos_cliente ON cliente_telefonos(cliente_id);
CREATE INDEX idx_clientes_nombre ON clientes(nombre);

-- ============================================================
-- 5. PROVEEDORES
--    AMPLIADO (cuarta revisión): país/región/ciudad y categoría
--    dejan de ser texto libre y pasan a ser FK a los catálogos de
--    la sección 3. Si alguien escribe "Otro" en el formulario, la
--    app inserta la fila nueva en el catálogo correspondiente
--    (por nombre) y usa ese id — así el dato queda estructurado
--    igual, sin perder la posibilidad de agregar valores nuevos.
-- ============================================================

CREATE TABLE proveedores (
  id           uuid PRIMARY KEY DEFAULT gen_random_uuid(),
  nombre       text NOT NULL,
  pais_id      uuid NOT NULL REFERENCES paises(id),               -- AMPLIADO: antes "pais" (texto)
  region_id    uuid REFERENCES regiones(id),                      -- AMPLIADO: antes "region" (texto)
  ciudad_id    uuid REFERENCES ciudades(id),                      -- AMPLIADO: antes "ciudad" (texto)
  categoria_id uuid NOT NULL REFERENCES categorias_proveedor(id), -- AMPLIADO: antes "categoria" (texto)
  estado       proveedor_estado NOT NULL DEFAULT 'Activo',
  contacto     text,
  cargo_contacto text,            -- NUEVO: cargo de la persona de contacto (CARGO en BD_PROVEEDORES)
  email        text,
  web          text,              -- NUEVO
  direccion    text,              -- NUEVO
  aforo        int,               -- NUEVO: capacidad, relevante sobre todo para venues
  costo_referencia text,          -- NUEVO: costo de referencia visto en cotizaciones previas (texto libre, no siempre es un solo número)
  score        smallint CHECK (score BETWEEN 1 AND 5),
  presupuesto  presupuesto_tier,
  cobertura    cobertura_tipo,
  notas        text,
  created_by   uuid REFERENCES usuarios(id),  -- auditoría: qué usuario registró este proveedor; NO implica que un proveedor sea un usuario
  created_at   timestamptz NOT NULL DEFAULT now(),
  updated_at   timestamptz NOT NULL DEFAULT now()
);

CREATE INDEX idx_proveedores_pais       ON proveedores(pais_id);
CREATE INDEX idx_proveedores_region     ON proveedores(region_id);
CREATE INDEX idx_proveedores_ciudad     ON proveedores(ciudad_id);
CREATE INDEX idx_proveedores_categoria  ON proveedores(categoria_id);
CREATE INDEX idx_proveedores_estado     ON proveedores(estado);

-- NUEVO (quinta revisión, 1FN): reemplaza "telefono"/"telefono2".
CREATE TABLE proveedor_telefonos (
  id            uuid PRIMARY KEY DEFAULT gen_random_uuid(),
  proveedor_id  uuid NOT NULL REFERENCES proveedores(id) ON DELETE CASCADE,
  telefono      text NOT NULL,
  etiqueta      text  -- ej. "Principal", "WhatsApp", "Oficina"
);

CREATE INDEX idx_proveedor_telefonos_proveedor ON proveedor_telefonos(proveedor_id);

-- NUEVO (quinta revisión, 1FN): reemplaza "servicios" (antes una
-- lista separada por comas en un solo campo de texto, un valor no
-- atómico). Igual que categorias_proveedor, es un catálogo abierto:
-- si un proveedor ofrece un servicio que no existe todavía, la app
-- inserta la fila nueva y la reutiliza para los siguientes.
CREATE TABLE servicios (
  id      uuid PRIMARY KEY DEFAULT gen_random_uuid(),
  nombre  text NOT NULL UNIQUE
);

CREATE TABLE proveedor_servicios (
  proveedor_id  uuid NOT NULL REFERENCES proveedores(id) ON DELETE CASCADE,
  servicio_id   uuid NOT NULL REFERENCES servicios(id) ON DELETE CASCADE,
  PRIMARY KEY (proveedor_id, servicio_id)
);

CREATE INDEX idx_proveedor_servicios_servicio ON proveedor_servicios(servicio_id);

-- pais_id/region_id/ciudad_id son 3 FK independientes: por sí
-- solas no impiden guardar, por ejemplo, país="México" con
-- región="Antioquia" (que es de Colombia). Este trigger valida
-- que la cadena país→región→ciudad sea realmente consistente,
-- no solo que cada FK exista.
CREATE OR REPLACE FUNCTION check_proveedor_geografia()
RETURNS trigger AS $$
DECLARE
  region_pais_id uuid;
  ciudad_region_id uuid;
BEGIN
  IF NEW.region_id IS NOT NULL THEN
    SELECT pais_id INTO region_pais_id FROM regiones WHERE id = NEW.region_id;
    IF region_pais_id IS DISTINCT FROM NEW.pais_id THEN
      RAISE EXCEPTION 'region_id % no pertenece al pais_id %', NEW.region_id, NEW.pais_id;
    END IF;
  END IF;
  IF NEW.ciudad_id IS NOT NULL THEN
    SELECT region_id INTO ciudad_region_id FROM ciudades WHERE id = NEW.ciudad_id;
    IF NEW.region_id IS NULL OR ciudad_region_id IS DISTINCT FROM NEW.region_id THEN
      RAISE EXCEPTION 'ciudad_id % no pertenece al region_id %', NEW.ciudad_id, NEW.region_id;
    END IF;
  END IF;
  RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_proveedores_geografia
  BEFORE INSERT OR UPDATE ON proveedores
  FOR EACH ROW EXECUTE FUNCTION check_proveedor_geografia();

-- ============================================================
-- 6. ADJUNTOS DE PROVEEDOR
-- ============================================================

CREATE TABLE proveedor_adjuntos (
  id            uuid PRIMARY KEY DEFAULT gen_random_uuid(),
  proveedor_id  uuid NOT NULL REFERENCES proveedores(id) ON DELETE CASCADE,
  tipo          adjunto_tipo NOT NULL,
  nombre        text NOT NULL,
  url           text,          -- si tipo = 'link'
  storage_path  text,          -- si tipo = 'file' (ruta en Supabase Storage)
  meta          text,          -- dominio del link, o tamaño del archivo
  fecha         date NOT NULL DEFAULT current_date,
  created_at    timestamptz NOT NULL DEFAULT now()
);

CREATE INDEX idx_adjuntos_proveedor ON proveedor_adjuntos(proveedor_id);

-- ============================================================
-- 7. PROYECTOS
--    AMPLIADO con seguimiento operativo real (ver Excel
--    "Seguimiento de proyectos"): prioridad, ciudad/sede,
--    fecha de solicitud, estado de la propuesta y facturación
--    básica. cliente/contacto_cliente en texto libre se
--    reemplazan por cliente_id + contacto_proyecto opcional.
--    estado (cuarta revisión) pasa de ENUM a FK a estados_proyecto.
--    propuesta_estado (sexta revisión) pasa de 3 columnas a 1.
-- ============================================================

CREATE TABLE proyectos (
  id                uuid PRIMARY KEY DEFAULT gen_random_uuid(),
  nombre            text NOT NULL,
  cliente_id        uuid REFERENCES clientes(id),         -- AMPLIADO: antes "cliente" (texto)
  contacto_proyecto text,                                 -- AMPLIADO: override opcional si el contacto de este proyecto no es el contacto principal del cliente
  tipo_proyecto     tipo_proyecto,                        -- NUEVO
  prioridad         prioridad_proyecto,                   -- NUEVO
  ciudad            text,                                 -- NUEVO: ciudad del evento/proyecto
  sede_next         text,                                 -- NUEVO: sede de Next a cargo (ej. "Bogotá", "México")
  fecha_solicitud   date,                                 -- NUEVO: cuándo llegó la solicitud
  fecha_evento      date,
  estado_id         uuid NOT NULL REFERENCES estados_proyecto(id),  -- AMPLIADO: antes ENUM proyecto_estado; ver trigger trg_proyectos_estado_default más abajo para el valor por defecto ('Planeación interna')
  porcentaje_avance smallint NOT NULL DEFAULT 0 CHECK (porcentaje_avance BETWEEN 0 AND 100),  -- NUEVO: % de avance manual (PORCENTAJE PROCESO en el Excel)
  estado_brief      brief_estado NOT NULL DEFAULT 'Pendiente por enviar',
  propuesta_estado  propuesta_estado NOT NULL DEFAULT 'No enviada',  -- NUEVO (sexta revisión: un solo campo, no 3)
  numero_factura    text,                                 -- NUEVO
  pagado            boolean NOT NULL DEFAULT false,        -- NUEVO
  fecha_pago        date,                                 -- NUEVO
  notas             text,
  gerente_id        uuid REFERENCES usuarios(id) ON DELETE SET NULL,  -- NUEVO (novena revisión): gerente/manager dueño del proyecto -- ver docs/06-modelo-permisos-roles.md. Se asigna solo (al gerente que lo crea) o lo asigna un admin/super_admin; solo un admin/super_admin puede reasignarlo después. Un proyecto sin gerente asignado (NULL) no tiene "dueño" -- su eliminación va directo a un administrador.
  created_by        uuid REFERENCES usuarios(id),  -- auditoría: qué usuario registró este proyecto; NO implica que un proyecto sea un usuario
  created_at        timestamptz NOT NULL DEFAULT now(),
  updated_at        timestamptz NOT NULL DEFAULT now()
);

CREATE INDEX idx_proyectos_fecha     ON proyectos(fecha_evento);
CREATE INDEX idx_proyectos_estado    ON proyectos(estado_id);
CREATE INDEX idx_proyectos_brief     ON proyectos(estado_brief);
CREATE INDEX idx_proyectos_cliente   ON proyectos(cliente_id);  -- NUEVO
CREATE INDEX idx_proyectos_prioridad ON proyectos(prioridad);   -- NUEVO
CREATE INDEX idx_proyectos_gerente   ON proyectos(gerente_id);  -- NUEVO (novena revisión)

-- Como estado_id es una FK (no se le puede poner un valor
-- constante en DEFAULT), este trigger lo completa con el id de
-- 'Planeación interna' cuando se crea un proyecto sin indicar
-- estado, para no obligar a la aplicación a buscarlo cada vez.
CREATE OR REPLACE FUNCTION set_estado_proyecto_default()
RETURNS trigger AS $$
BEGIN
  IF NEW.estado_id IS NULL THEN
    SELECT id INTO NEW.estado_id FROM estados_proyecto WHERE nombre = 'Planeación interna';
  END IF;
  RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_proyectos_estado_default
  BEFORE INSERT ON proyectos
  FOR EACH ROW EXECUTE FUNCTION set_estado_proyecto_default();

-- ============================================================
-- 8. EQUIPO DEL PROYECTO
--    En el prototipo "Ejecutivo encargado" era un solo campo de
--    texto donde a veces se escribían varios nombres separados
--    por "*". Aquí queda modelado como filas independientes:
--    permite de verdad varios ejecutivos (o varios diseñadores,
--    comerciales, administrativos) por proyecto, sin depender
--    de un separador de texto.
-- ============================================================

CREATE TABLE proyecto_equipo (
  id           uuid PRIMARY KEY DEFAULT gen_random_uuid(),
  proyecto_id  uuid NOT NULL REFERENCES proyectos(id) ON DELETE CASCADE,
  rol          rol_equipo NOT NULL,
  nombre       text NOT NULL
);

CREATE INDEX idx_equipo_proyecto ON proyecto_equipo(proyecto_id);

-- ============================================================
-- 9. RELACIÓN PROYECTO ↔ PROVEEDORES (muchos a muchos)
-- ============================================================

CREATE TABLE proyecto_proveedores (
  proyecto_id   uuid NOT NULL REFERENCES proyectos(id) ON DELETE CASCADE,
  proveedor_id  uuid NOT NULL REFERENCES proveedores(id) ON DELETE CASCADE,
  PRIMARY KEY (proyecto_id, proveedor_id)
);

CREATE INDEX idx_pp_proveedor ON proyecto_proveedores(proveedor_id);

-- ============================================================
-- 10. BITÁCORA DE SEGUIMIENTO DEL PROYECTO (NUEVO)
--    Reemplaza TRES columnas de texto libre que en el Excel
--    vivían separadas: "SEGUIMIENTO (incluir fecha de
--    actualización)", "OBSERVACIONES ADICIONALES (incluir fecha
--    de actualización)", y las notas de pendientes por área
--    ("RESPONSABILIDADES - CREATIVO/COMERCIAL/ADMINISTRATIVO",
--    que en la práctica son comentarios de estado por
--    departamento, no asignación de personas). Todo el historial
--    de avances quedaba como texto libre acumulado en una sola
--    celda (ej. "29 jun - Confirmación de proveedores... \n
--    24 Junio - Se solicita..."). Ahora cada nota es una fila con
--    su propia fecha, autor y área: permite ordenar, filtrar y
--    saber quién escribió qué y sobre qué frente del proyecto.
-- ============================================================

CREATE TABLE proyecto_seguimiento (
  id           uuid PRIMARY KEY DEFAULT gen_random_uuid(),
  proyecto_id  uuid NOT NULL REFERENCES proyectos(id) ON DELETE CASCADE,
  autor_id     uuid REFERENCES usuarios(id),  -- qué usuario escribió esta nota (aquí sí es autoría de contenido, no solo auditoría)
  area         area_seguimiento NOT NULL DEFAULT 'General',  -- NUEVO
  fecha        date NOT NULL DEFAULT current_date,
  nota         text NOT NULL,
  created_at   timestamptz NOT NULL DEFAULT now()
);

CREATE INDEX idx_seguimiento_proyecto ON proyecto_seguimiento(proyecto_id);
CREATE INDEX idx_seguimiento_fecha    ON proyecto_seguimiento(fecha);
CREATE INDEX idx_seguimiento_area     ON proyecto_seguimiento(area);

-- ============================================================
-- 11. SNAPSHOTS DEL INFORME (semanal / mensual)
--     AMPLIADO con total_clientes para reflejar la nueva
--     entidad en los informes.
-- ============================================================

CREATE TABLE informes_snapshot (
  id                     uuid PRIMARY KEY DEFAULT gen_random_uuid(),
  tipo                   informe_periodo NOT NULL,
  periodo_key            text NOT NULL,   -- ej. '2026-W31' o '2026-07'
  total_proveedores      int NOT NULL,
  total_clientes         int NOT NULL DEFAULT 0,  -- NUEVO
  total_proyectos        int NOT NULL,
  proyectos_sin_proveedor int NOT NULL,
  por_estado             jsonb NOT NULL,  -- conteo por cada nombre de estados_proyecto
  por_brief              jsonb NOT NULL,  -- conteo por cada brief_estado
  created_by             uuid REFERENCES usuarios(id),  -- qué usuario generó este snapshot
  created_at             timestamptz NOT NULL DEFAULT now(),
  UNIQUE (tipo, periodo_key)
);

-- ============================================================
-- 11b. SOLICITUDES DE ELIMINACIÓN (NUEVO, novena revisión)
--     Modelo de permisos de 4 niveles -- ver docs/06-modelo-permisos-
--     roles.md. Un gerente o miembro no puede eliminar directamente
--     un cliente, proveedor o proyecto: en su lugar crea una fila
--     aquí. Si es un proyecto con gerente_id distinto de quien
--     solicita, la fila nace en estado 'pendiente_gerente' (debe
--     endosarla primero el gerente dueño del proyecto); en cualquier
--     otro caso (clientes, proveedores, proyecto sin gerente, o el
--     propio gerente pidiendo eliminar su proyecto) nace directo en
--     'pendiente_admin'. Solo cuando un admin/super_admin aprueba
--     (estado 'aprobada') el backend ejecuta el DELETE real sobre la
--     entidad -- esta tabla en sí misma nunca borra nada por sí sola,
--     es un flujo de aprobación, no una papelera.
-- ============================================================

CREATE TYPE solicitud_eliminacion_tipo AS ENUM ('cliente', 'proveedor', 'proyecto');
CREATE TYPE solicitud_eliminacion_estado AS ENUM ('pendiente_gerente', 'pendiente_admin', 'aprobada', 'rechazada');

CREATE TABLE solicitudes_eliminacion (
  id                       uuid PRIMARY KEY DEFAULT gen_random_uuid(),
  tipo_entidad             solicitud_eliminacion_tipo NOT NULL,
  entidad_id               uuid NOT NULL,  -- id de clientes/proveedores/proyectos según tipo_entidad -- sin FK propia porque apunta a una de 3 tablas distintas según el tipo
  solicitado_por_id        uuid NOT NULL REFERENCES usuarios(id),        -- quién pidió la eliminación
  motivo                   text,
  estado                   solicitud_eliminacion_estado NOT NULL DEFAULT 'pendiente_admin',
  gerente_responsable_id   uuid REFERENCES usuarios(id) ON DELETE SET NULL,  -- el gerente dueño del proyecto, cuando aplica (solo tipo_entidad = 'proyecto' con gerente distinto de quien solicita)
  aprobado_por_gerente_id  uuid REFERENCES usuarios(id) ON DELETE SET NULL,
  aprobado_por_gerente_en  timestamptz,
  revisado_por_id          uuid REFERENCES usuarios(id) ON DELETE SET NULL,  -- quién tomó la decisión final (aprobar/rechazar) como admin, o quién rechazó como gerente
  revisado_en              timestamptz,
  comentario_revision      text,
  created_at               timestamptz NOT NULL DEFAULT now()
);

CREATE INDEX idx_solicitudes_tipo_entidad ON solicitudes_eliminacion(tipo_entidad, entidad_id);
CREATE INDEX idx_solicitudes_estado       ON solicitudes_eliminacion(estado);
CREATE INDEX idx_solicitudes_gerente      ON solicitudes_eliminacion(gerente_responsable_id);

-- ============================================================
-- 12. updated_at AUTOMÁTICO
-- ============================================================

CREATE OR REPLACE FUNCTION set_updated_at()
RETURNS trigger AS $$
BEGIN
  NEW.updated_at = now();
  RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_clientes_updated_at
  BEFORE UPDATE ON clientes
  FOR EACH ROW EXECUTE FUNCTION set_updated_at();

CREATE TRIGGER trg_proveedores_updated_at
  BEFORE UPDATE ON proveedores
  FOR EACH ROW EXECUTE FUNCTION set_updated_at();

CREATE TRIGGER trg_proyectos_updated_at
  BEFORE UPDATE ON proyectos
  FOR EACH ROW EXECUTE FUNCTION set_updated_at();

CREATE TRIGGER trg_usuarios_updated_at  -- NUEVO (octava revisión)
  BEFORE UPDATE ON usuarios
  FOR EACH ROW EXECUTE FUNCTION set_updated_at();

-- ============================================================
-- 13. ROW LEVEL SECURITY
--     REVISADO en la auditoría de seguridad del 2026-08-17 (hallazgo H4 — ver
--     docs/05-plan-remediacion-seguridad.md). Diseño anterior: "cualquier autenticado
--     puede todo" (auth.role() = 'authenticated'). Ese diseño asume que el cliente
--     (frontend) habla directo con la API de datos de Supabase (PostgREST) usando el
--     JWT del usuario — pero en Nexit el único que habla con Postgres es el backend de
--     ASP.NET Core, con una conexión directa (Npgsql), no a través de PostgREST. La
--     función auth.role() depende de una variable de sesión que solo existe cuando
--     PostgREST arma la conexión; en una conexión directa no aplica. Además, si en el
--     futuro el backend se conectara con un rol distinto al superusuario `postgres`
--     (que ignora RLS) sin ajustar esto, esas políticas habrían bloqueado al propio
--     backend, no solo a usuarios no autorizados.
--
--     Diseño nuevo: RLS aquí es una segunda barrera, no la primera. La autorización
--     real (quién puede crear/editar/eliminar qué) vive en el backend (políticas
--     [Authorize] de ASP.NET Core). RLS solo garantiza que, aunque alguien obtenga la
--     clave pública "anon" de Supabase y la use para llamar a PostgREST directamente
--     (saltándose el backend por completo), no pueda leer ni escribir nada: con RLS
--     habilitado y sin ninguna política para los roles `anon`/`authenticated` de
--     Supabase, Postgres deniega todo por defecto. Solo el rol `nexit_app` (creado en
--     02_rol_aplicacion_minimo_privilegio.sql, el que usa el backend para conectarse)
--     tiene política de acceso.
-- ============================================================

ALTER TABLE usuarios              ENABLE ROW LEVEL SECURITY;
ALTER TABLE dominios_correo_permitidos ENABLE ROW LEVEL SECURITY;  -- NUEVO
ALTER TABLE paises                ENABLE ROW LEVEL SECURITY;
ALTER TABLE regiones              ENABLE ROW LEVEL SECURITY;
ALTER TABLE ciudades              ENABLE ROW LEVEL SECURITY;
ALTER TABLE categorias_proveedor  ENABLE ROW LEVEL SECURITY;
ALTER TABLE fases_proyecto        ENABLE ROW LEVEL SECURITY;
ALTER TABLE estados_proyecto      ENABLE ROW LEVEL SECURITY;
ALTER TABLE servicios             ENABLE ROW LEVEL SECURITY;
ALTER TABLE clientes              ENABLE ROW LEVEL SECURITY;
ALTER TABLE cliente_telefonos     ENABLE ROW LEVEL SECURITY;
ALTER TABLE proveedores           ENABLE ROW LEVEL SECURITY;
ALTER TABLE proveedor_telefonos   ENABLE ROW LEVEL SECURITY;
ALTER TABLE proveedor_servicios   ENABLE ROW LEVEL SECURITY;
ALTER TABLE proveedor_adjuntos    ENABLE ROW LEVEL SECURITY;
ALTER TABLE proyectos             ENABLE ROW LEVEL SECURITY;
ALTER TABLE proyecto_equipo       ENABLE ROW LEVEL SECURITY;
ALTER TABLE proyecto_proveedores  ENABLE ROW LEVEL SECURITY;
ALTER TABLE proyecto_seguimiento  ENABLE ROW LEVEL SECURITY;
ALTER TABLE informes_snapshot     ENABLE ROW LEVEL SECURITY;
ALTER TABLE solicitudes_eliminacion ENABLE ROW LEVEL SECURITY;  -- NUEVO (novena revisión)

-- Una sola política por tabla: solo nexit_app pasa. anon/authenticated (PostgREST)
-- quedan sin ninguna política, así que Postgres les deniega todo por defecto.
-- El control fino de "quién puede crear/editar/eliminar qué" (super_admin vs. admin
-- vs. manager vs. miembro, y el flujo de solicitudes de eliminación) lo hace el
-- backend, no esta capa — ver Nexit.API/Program.cs (políticas "SuperAdminOnly" y
-- "AdminOrAbove"), docs/06-modelo-permisos-roles.md y los controladores.
CREATE POLICY "solo_nexit_app" ON usuarios FOR ALL TO nexit_app USING (true) WITH CHECK (true);
CREATE POLICY "solo_nexit_app" ON dominios_correo_permitidos FOR ALL TO nexit_app USING (true) WITH CHECK (true);
CREATE POLICY "solo_nexit_app" ON paises FOR ALL TO nexit_app USING (true) WITH CHECK (true);
CREATE POLICY "solo_nexit_app" ON regiones FOR ALL TO nexit_app USING (true) WITH CHECK (true);
CREATE POLICY "solo_nexit_app" ON ciudades FOR ALL TO nexit_app USING (true) WITH CHECK (true);
CREATE POLICY "solo_nexit_app" ON categorias_proveedor FOR ALL TO nexit_app USING (true) WITH CHECK (true);
CREATE POLICY "solo_nexit_app" ON fases_proyecto FOR ALL TO nexit_app USING (true) WITH CHECK (true);
CREATE POLICY "solo_nexit_app" ON estados_proyecto FOR ALL TO nexit_app USING (true) WITH CHECK (true);
CREATE POLICY "solo_nexit_app" ON servicios FOR ALL TO nexit_app USING (true) WITH CHECK (true);
CREATE POLICY "solo_nexit_app" ON clientes FOR ALL TO nexit_app USING (true) WITH CHECK (true);
CREATE POLICY "solo_nexit_app" ON cliente_telefonos FOR ALL TO nexit_app USING (true) WITH CHECK (true);
CREATE POLICY "solo_nexit_app" ON proveedores FOR ALL TO nexit_app USING (true) WITH CHECK (true);
CREATE POLICY "solo_nexit_app" ON proveedor_telefonos FOR ALL TO nexit_app USING (true) WITH CHECK (true);
CREATE POLICY "solo_nexit_app" ON proveedor_servicios FOR ALL TO nexit_app USING (true) WITH CHECK (true);
CREATE POLICY "solo_nexit_app" ON proveedor_adjuntos FOR ALL TO nexit_app USING (true) WITH CHECK (true);
CREATE POLICY "solo_nexit_app" ON proyectos FOR ALL TO nexit_app USING (true) WITH CHECK (true);
CREATE POLICY "solo_nexit_app" ON proyecto_equipo FOR ALL TO nexit_app USING (true) WITH CHECK (true);
CREATE POLICY "solo_nexit_app" ON proyecto_proveedores FOR ALL TO nexit_app USING (true) WITH CHECK (true);
CREATE POLICY "solo_nexit_app" ON proyecto_seguimiento FOR ALL TO nexit_app USING (true) WITH CHECK (true);
CREATE POLICY "solo_nexit_app" ON informes_snapshot FOR ALL TO nexit_app USING (true) WITH CHECK (true);
CREATE POLICY "solo_nexit_app" ON solicitudes_eliminacion FOR ALL TO nexit_app USING (true) WITH CHECK (true);  -- NUEVO (novena revisión)

-- IMPORTANTE: ejecuta 02_rol_aplicacion_minimo_privilegio.sql (crea el rol nexit_app y
-- le da GRANT sobre estas tablas) ANTES o DESPUÉS de este archivo, da igual el orden,
-- pero ambos son necesarios: RLS por sí solo no basta, porque además de la política
-- hace falta el GRANT a nivel de tabla (RLS filtra FILAS, no reemplaza los permisos).

-- ============================================================
-- NOTAS DE DISEÑO (v2)
-- ============================================================
-- 1. UUID en vez de ids numéricos: evita choques si dos
--    personas crean un registro al mismo tiempo, y es el
--    estándar de Supabase.
-- 2. clientes se modela igual que proveedores (mismo patrón de
--    contacto/ciudad/notas) para que el sistema sea consistente
--    y fácil de aprender para quien lo use.
-- 3. proyecto_equipo separa cada rol en su propia fila -> ya no
--    depende de escribir varios nombres separados por "*" en un
--    solo campo de texto; se puede tener 1, 2 o 10 personas por
--    rol (incluyendo ahora Comercial y Administrativo).
-- 4. proyecto_proveedores es la tabla intermedia que reemplaza
--    el arreglo proveedorIds: una fila por cada proveedor
--    asignado a un proyecto.
-- 5. proyecto_seguimiento reemplaza el texto libre acumulado de
--    "SEGUIMIENTO" / "OBSERVACIONES ADICIONALES" del Excel: cada
--    actualización queda como una fila con fecha y autor propios.
-- 6. ON DELETE CASCADE en las FKs de detalle para que al borrar
--    un proyecto o proveedor no queden registros huérfanos.
-- 7. informes_snapshot tiene UNIQUE(tipo, periodo_key): no se
--    pueden guardar dos snapshots de la misma semana/mes por
--    error; un INSERT ... ON CONFLICT (tipo, periodo_key) DO
--    UPDATE permite "regrabar" el snapshot de la semana actual
--    si se guarda más de una vez.
-- 8. Lo que NO entró en este MVP (ver documento de análisis,
--    sección "Fuera de alcance"): el embudo comercial de
--    captación de clientes (hoja "Etapas clientes", Etapa 1-5)
--    y la migración de los datos históricos 2022-2026. Ambos
--    quedan propuestos como trabajo de una fase futura.
-- 9. La columna vieja "ETAPA" del Excel (valores inconsistentes
--    entre años: "ETAPA 1".."ETAPA 6", "Cierre - Facturado", "No
--    ejecutado") no se modela aparte: se asume que proyecto_estado
--    es el reemplazo limpio y ordenado de ese campo para el
--    sistema nuevo. Confirmar con Luisa que esa es la intención
--    antes de migrar el histórico.
-- 10. Se detectaron errores de captura en los Excel de origen que
--    NO se corrigen aquí (son de limpieza de datos, no de
--    modelo): valores de teléfono/texto en la columna AFORO de
--    BD_CLIENTES, y valores como "BAJA" en la columna CIUDAD
--    PROYECTO de NO EJECUTADOS 2026. Deben limpiarse al migrar.
-- 11. Riesgos para la futura migración (no son campos faltantes,
--    son de datos/constraints, quedan anotados para no
--    olvidarlos): BD_PROVEEDORES no tiene columna de país (solo
--    ciudad), y aquí proveedores.pais_id es NOT NULL -- habrá que
--    derivarlo de la ciudad al migrar o revisar el constraint.
--    clientes.costo/valor_referencia casi no se usa hoy en
--    BD_CLIENTES (0 de 30 filas lo traen lleno): se deja el
--    campo para uso futuro, no porque la operación actual lo
--    llene. También hay celdas sueltas sin encabezado con datos
--    en NO EJECUTADOS 2026 (parecen residuos de copiar/pegar
--    entre hojas) que no son un campo nuevo, solo ruido a limpiar.
-- 12. Catálogos (cuarta revisión): paises/regiones/ciudades y
--    categorias_proveedor reemplazan lo que en el HTML eran
--    listas fijas dentro del JavaScript (imposibles de consultar
--    o de ampliar sin tocar código); estados_proyecto reemplaza
--    el ENUM proyecto_estado por lo mismo, y de paso deja la
--    agrupación por fase (1/2/3) como dato real en vez de un
--    comentario en el código. Los tres niveles geográficos usan
--    FK en cascada (ciudad -> región -> país) para que la
--    jerarquía se garantice sola: no se puede guardar una ciudad
--    que no cuelgue de una región real. Los datos se cargan con
--    seed_geografia_categorias_estados.sql (3 países, 115
--    departamentos/estados, 411 ciudades, 26 categorías y 9
--    estados de proyecto, extraídos 1 a 1 del HTML).
--    Los demás ENUM (proveedor_estado, presupuesto_tier,
--    cobertura_tipo, brief_estado, rol_equipo, tipo_proyecto,
--    prioridad_proyecto, propuesta_estado, area_seguimiento)
--    se dejan como están: no tienen jerarquía ni necesitan
--    crecer libremente como sí pasa con países/ciudades/
--    categorías, así que un ENUM sigue siendo la opción más
--    simple para ellos. Si más adelante quieres el mismo
--    tratamiento para alguno de esos, se hace igual.
-- 13. Aplicación formal de 1FN/2FN/3FN/4FN (quinta revisión):
--    - 1FN (valores atómicos, sin grupos repetitivos): había 2
--      violaciones. (a) proveedores.servicios guardaba una lista
--      separada por comas en un solo campo -> se separó en
--      servicios + proveedor_servicios. (b) clientes/proveedores
--      tenían "telefono" y "telefono2" (grupo repetitivo
--      representado como columnas) -> se separaron en
--      cliente_telefonos y proveedor_telefonos. El resto de
--      columnas del modelo ya guardaban un solo valor indivisible
--      por celda, así que ya cumplían 1FN. informes_snapshot usa
--      jsonb para por_estado/por_brief: es una excepción a
--      propósito (es una foto/resumen calculado para informes,
--      no un dato operativo que se vaya a filtrar por campo
--      individual), no una violación a corregir.
--    - 2FN (todo atributo no-clave depende de la CLAVE COMPLETA):
--      solo aplica a tablas con clave compuesta. La única que hay
--      es proyecto_proveedores (proyecto_id, proveedor_id), que
--      no tiene ninguna columna adicional -> no hay nada que
--      pueda depender solo de una parte de la clave. El resto de
--      tablas usa una clave simple (id uuid), así que 2FN se
--      cumple automáticamente en todo el modelo.
--    - 3FN (ningún atributo no-clave depende de OTRO atributo
--      no-clave, solo de la clave primaria): 1 violación real.
--      estados_proyecto.fase_nombre dependía de estados_proyecto
--      .fase (todas las filas con fase=2 tenían el mismo
--      fase_nombre), no directamente del id de cada estado ->
--      dependencia transitiva. Se separó en fases_proyecto y
--      fase_nombre se eliminó (se obtiene con JOIN a fases_proyecto
--      por fase). El resto de columnas del modelo son hechos
--      directos de su propia fila (no derivados de otra columna
--      no-clave de la misma tabla).
--    - 4FN (ningún atributo no-clave depende de OTRO atributo
--      no-clave, solo de la clave primaria; sin dependencias
--      multivaluadas independientes mezcladas en una misma
--      tabla): no se encontraron violaciones adicionales una vez
--      resueltas las de 1FN. Cada hecho multivaluado independiente
--      de una entidad (los servicios de un proveedor, sus
--      teléfonos, los proveedores de un proyecto, el equipo de un
--      proyecto) ya vive en su propia tabla intermedia -- nunca se
--      mezclan dos hechos multivaluados distintos en la misma
--      tabla (ej. servicios y teléfonos de un proveedor NO
--      comparten tabla), que es justo lo que 4FN exige.
--    Fuentes usadas para las definiciones: "Normalización de
--    bases de datos" (Wikipedia en español) y el material de la
--    Facultad de Estadística e Informática de la Universidad
--    Veracruzana sobre formas normales.
-- 14. proyectos.propuesta_estado (sexta revisión): en la quinta
--    revisión el campo había quedado dividido en 3 columnas
--    (propuesta_creativa_estado, propuesta_grafica_estado,
--    propuesta_economica_estado), copiando el formato de 2 hojas
--    del Excel ("Trafico proyectos Next" y "PROYECTOS 2023") que
--    sí registran el envío de la propuesta creativa, gráfica y
--    económica por separado. Al revisar de nuevo el archivo
--    completo se confirmó que: (a) esas 2 hojas son un formato
--    antiguo, ya reemplazado por las hojas que Next usa hoy
--    (SEG.PROYECTOS CORP 2023-2026, PROYECTOS FINALIZADOS, NO
--    EJECUTADOS), ninguna de las cuales separa la propuesta por
--    tipo; y (b) repetir la misma columna 3 veces solo por un
--    calificador distinto (creativa/gráfica/económica) es el
--    mismo patrón que telefono/telefono2 (grupo repetitivo, nota
--    13). Se dejó un solo campo `propuesta_estado`: si en el
--    futuro Next vuelve a necesitar trackear varios tipos de
--    propuesta con estados independientes, el modelo correcto no
--    sería agregar más columnas sino una tabla relacionada
--    (tipos_propuesta + proyecto_propuestas), igual que se hizo
--    con servicios y con teléfonos.
-- 15. Usuarios del sistema (octava revisión): se amplió usuarios
--    con lo que hace falta para una plataforma normal:
--    - nombre/apellido separados (antes nombre_completo único).
--    - rol pasa de texto libre a ENUM rol_usuario (los valores
--      válidos ya quedan garantizados por la base de datos, no
--      solo por un comentario).
--    - activo (boolean): para dar de baja a alguien sin perder su
--      historial (proyectos creados, notas de seguimiento, etc.
--      siguen apuntando a su usuario).
--    - updated_at + trigger, igual que clientes/proveedores/
--      proyectos.
--    - Contraseña: NO se agregó ninguna columna. La contraseña de
--      cada usuario la administra Supabase Auth (auth.users.
--      encrypted_password, cifrada); la tabla usuarios nunca debe
--      duplicar ni guardar una contraseña, en texto plano ni
--      cifrada, por su cuenta.
--    - Verificación del correo al primer login (token con
--      expiración + envío del correo): se evaluaron 2 caminos y,
--      confirmado con la usuaria, se usa el flujo nativo de
--      Supabase Auth (confirmation token + expiración + envío de
--      correo, todo incluido) en vez de construir una tabla y un
--      envío de correo propios -- para un presupuesto de
--      ~$500.000 COP no tiene sentido reconstruir algo que
--      Supabase ya da gratis. Si más adelante hace falta más
--      control sobre ese flujo (mensajes personalizados,
--      reenvíos, etc.), ahí sí se justificaría una tabla propia.
--    - Dominio de correo laboral: esto SÍ es una regla propia del
--      negocio que Supabase Auth no valida solo, así que se
--      modela como catálogo (dominios_correo_permitidos) en vez
--      de un valor fijo en código -- el prototipo HTML usa
--      "nombre@nextexperiencial.com" como placeholder de login,
--      así que ese es el dominio confirmado en el seed. Se
--      agregó un trigger (check_usuario_dominio_correo) que
--      rechaza un usuario cuyo correo no termine en un dominio
--      permitido -- como respaldo de base de datos; la validación
--      principal (evitar siquiera intentar el signup) debe vivir
--      en la aplicación, antes de llamar a Supabase Auth.
-- 16. usuarios vs. clientes/proveedores/proyectos (aclaración):
--    son dos tipos de cosa distintos. usuarios son las personas
--    de Next que inician sesión (usuarios reales, con fila en
--    auth.users, contraseña administrada por Supabase Auth,
--    dominio de correo validado). clientes/proveedores/proyectos
--    son entidades de negocio: nunca inician sesión, no tienen
--    contraseña, no pasan por ninguna validación de usuarios.
--    Las columnas created_by/autor_id que los conectan con
--    usuarios(id) son campos de AUDITORÍA (created_by/autor_id
--    pattern, el mismo que usa cualquier CRM/ERP): registran qué
--    usuario creó o escribió esa fila, para poder responder "¿quién
--    registró este cliente?" -- no dicen ni implican que un
--    cliente, proveedor o proyecto sea también un usuario.
-- 17. Modelo de permisos de 4 niveles (novena revisión) -- ver
--    docs/06-modelo-permisos-roles.md para el detalle completo y
--    las decisiones de diseño. Resumen:
--    - rol_usuario gana 'super_admin' por encima de 'admin':
--      super_admin (la desarrolladora) es la única que ve/gestiona
--      la tabla usuarios; admin administra todo lo demás
--      (catálogos, clientes, proveedores, proyectos) igual que
--      antes, y además decide las solicitudes de eliminación.
--    - proyectos.gerente_id: el gerente/manager "dueño" de un
--      proyecto. Se asigna solo al crear (si el creador ya es
--      gerente) o lo asigna explícitamente un admin/super_admin;
--      reasignarlo después también es exclusivo de admin/super_admin.
--    - solicitudes_eliminacion: gerentes y miembros no pueden
--      eliminar directamente un cliente, proveedor o proyecto --
--      piden la eliminación aquí. Si es un proyecto con gerente_id
--      distinto de quien solicita, primero la debe endosar ese
--      gerente (pendiente_gerente); de ahí, o directo para
--      clientes/proveedores/proyectos sin gerente asignado o
--      solicitados por su propio gerente (pendiente_admin), un
--      admin/super_admin aprueba o rechaza -- solo al aprobar se
--      ejecuta el DELETE real.
--    - admin/super_admin conservan además una vía directa de
--      eliminación (sin pasar por esta tabla) para catálogos y
--      adjuntos, que no tienen el concepto de "dueño" ni justifican
--      el flujo de aprobación.
