-- ============================================================
-- Rol de aplicación de mínimo privilegio para el backend de Nexit
-- ============================================================
-- Por qué existe este archivo (hallazgo H2 de la auditoría de seguridad, 2026-08-17):
-- la cadena de conexión del backend NO debe usar el superusuario `postgres` de Supabase.
-- Un superusuario puede crear/borrar tablas, leer y escribir cualquier dato sin
-- restricción, e ignora Row Level Security por completo. Si algún día apareciera un bug
-- en el backend (o una dependencia con una vulnerabilidad), el radio de impacto con
-- `postgres` es "toda la base de datos"; con este rol dedicado, es solo "las filas de
-- las tablas de negocio de Nexit".
--
-- Cuándo ejecutar esto: una sola vez, como superusuario (`postgres`), DESPUÉS de haber
-- creado el esquema completo con nexus_schema_v2.sql (este script necesita que las
-- tablas ya existan para poder hacer GRANT sobre ellas).
--
-- Después de ejecutarlo, actualiza la cadena de conexión del backend
-- (ConnectionStrings:DefaultConnection / DATABASE_URL) para que use
-- Username=nexit_app en vez de Username=postgres, con la contraseña que pongas abajo.

-- 1) Crear el rol de login. CAMBIA la contraseña por una generada aleatoriamente
--    (ej. `openssl rand -base64 32`) y guárdala como secreto del backend, nunca en Git.
CREATE ROLE nexit_app WITH LOGIN PASSWORD 'CAMBIAR_ESTA_CONTRASENA' NOINHERIT;

-- 2) Acceso al esquema de la aplicación. Nada de acceso a `auth`, `storage` ni otros
--    esquemas internos de Supabase: la app no los necesita, y Supabase Auth ya
--    administra `auth.*` por su cuenta con sus propios roles internos.
GRANT USAGE ON SCHEMA public TO nexit_app;

-- 3) Privilegios sobre las tablas y secuencias que ya existen. Nótese que NO incluye
--    CREATE, DROP, ALTER, TRUNCATE ni REFERENCES: nexit_app puede leer y escribir
--    filas, pero no puede cambiar el esquema.
GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA public TO nexit_app;
GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA public TO nexit_app;

-- 4) Para que las tablas/secuencias que se creen en el futuro (nuevas migraciones o
--    catálogos) también queden accesibles automáticamente, sin tener que repetir este
--    script cada vez. Solo aplica a objetos creados por el mismo rol que ejecuta esto
--    (normalmente `postgres`, el dueño del esquema).
ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO nexit_app;
ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT USAGE, SELECT ON SEQUENCES TO nexit_app;

-- 5) Con Row Level Security habilitado (sección 13 de nexus_schema_v2.sql), nexit_app
--    es el único rol con una política que le permite pasar — así que estos GRANT de
--    tabla son necesarios pero no suficientes por sí solos (RLS filtra filas, GRANT
--    habilita la operación); hacen falta los dos a la vez. No se necesita ningún ajuste
--    adicional aquí: las políticas "solo_nexit_app" ya apuntan a este rol por nombre.

-- 6) Verificación rápida después de crear el rol y aplicar el resto del esquema
--    (ejecutar conectado como nexit_app, no como postgres):
--      SELECT current_user;                         -- debe decir nexit_app
--      SELECT count(*) FROM clientes;                -- debe funcionar (SELECT concedido)
--      -- Esto debe FALLAR (nexit_app no tiene permiso de DDL):
--      -- CREATE TABLE prueba_no_deberia_poder (id int);
