-- ============================================================
-- Extras de Supabase que las migraciones de EF Core NO pueden crear
-- ============================================================
-- Por qué existe este archivo (aparece el 2026-08-20, al preparar la creación real del
-- proyecto de Supabase): las tablas, columnas, índices y triggers del sistema se crean
-- con las migraciones de EF Core (`dotnet ef database update`), NO con
-- `nexus_schema_v2.sql` -- ver docs/09-crear-proyecto-supabase-paso-a-paso.md para el
-- porqué (se encontró que ese script había quedado desactualizado frente a las
-- migraciones reales, por ejemplo le faltaba la columna `updated_by`).
--
-- Pero hay dos cosas que SÍ vienen únicamente de `nexus_schema_v2.sql` y que las
-- migraciones de EF Core deliberadamente NO incluyen, porque dependen del esquema
-- `auth` que solo existe en un proyecto de Supabase real (no existe en Postgres local
-- ni en el Postgres de las pruebas funcionales con Testcontainers, así que no podían
-- vivir en una migración sin romper esos otros entornos):
--
--   1) La relación entre `usuarios.id` y la cuenta de acceso real (`auth.users`).
--   2) Row Level Security (una segunda barrera, además de la autorización del backend
--      -- ver el comentario largo en nexus_schema_v2.sql sección 13 para el porqué).
--
-- Cuándo correr esto: DESPUÉS de `dotnet ef database update` (o de
-- 01_esquema_completo.sql contra Supabase) -- ver docs/09, paso 3.
-- **Corrección 2026-08-23:** este archivo SÍ tiene que correr DESPUÉS de
-- `02_rol_aplicacion_minimo_privilegio.sql`, no antes ni "en cualquier orden" como decía
-- esta nota antes -- las políticas de la sección 3 de abajo (`CREATE POLICY ... TO
-- nexit_app`) necesitan que el rol `nexit_app` ya exista, y ese rol lo crea el archivo
-- 02. Orden real: 01 -> 02 -> 04 (este archivo) -> 03 -> seed. Frente a
-- 03_auth_hook_custom_claims.sql sí da igual el orden entre ellos dos.

-- 1) usuarios.id referencia la cuenta real de Supabase Auth -- si se borra la cuenta de
--    auth.users, se borra también el perfil de negocio correspondiente.
ALTER TABLE usuarios
  ADD CONSTRAINT fk_usuarios_auth_users FOREIGN KEY (id) REFERENCES auth.users(id) ON DELETE CASCADE;

-- 2) Row Level Security -- copiado tal cual de la sección 13 de nexus_schema_v2.sql.
ALTER TABLE usuarios              ENABLE ROW LEVEL SECURITY;
ALTER TABLE dominios_correo_permitidos ENABLE ROW LEVEL SECURITY;
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
ALTER TABLE solicitudes_eliminacion ENABLE ROW LEVEL SECURITY;

-- Una sola política por tabla: solo nexit_app pasa. anon/authenticated (PostgREST)
-- quedan sin ninguna política, así que Postgres les deniega todo por defecto. El
-- control fino de "quién puede crear/editar/eliminar qué" lo hace el backend
-- (políticas "SuperAdminOnly"/"AdminOrAbove" de Nexit.API/Program.cs), no esta capa.
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
CREATE POLICY "solo_nexit_app" ON solicitudes_eliminacion FOR ALL TO nexit_app USING (true) WITH CHECK (true);

-- Verificación rápida después de correr esto y 02_rol_aplicacion_minimo_privilegio.sql:
--   SELECT tablename, rowsecurity FROM pg_tables WHERE schemaname = 'public';  -- rowsecurity debe ser 't' en todas
--   SELECT conname FROM pg_constraint WHERE conname = 'fk_usuarios_auth_users';  -- debe existir

-- 3) search_path fijo en las 4 funciones que crea 01_esquema_completo.sql (agregado
--    2026-08-23, tras el aviso "Function Search Path Mutable" del Security Advisor de
--    Supabase). Estas 4 funciones referencian tablas sin calificar (regiones, ciudades,
--    dominios_correo_permitidos, estados_proyecto) -- por eso se fija a "pg_catalog,
--    public" (no a '' vacío, que rompería esas referencias) en vez de reescribirlas.
--    01_esquema_completo.sql NO se edita a mano (se regenera desde las migraciones de
--    EF Core), así que este ajuste, específico de Supabase, vive aquí.
ALTER FUNCTION public.set_updated_at() SET search_path = pg_catalog, public;
ALTER FUNCTION public.check_proveedor_geografia() SET search_path = pg_catalog, public;
ALTER FUNCTION public.check_usuario_dominio_correo() SET search_path = pg_catalog, public;
ALTER FUNCTION public.set_estado_proyecto_default() SET search_path = pg_catalog, public;
