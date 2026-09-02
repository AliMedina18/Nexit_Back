-- Corresponde a la migración de EF Core "AddContrasenaConfiguradaUsuarios" (docs/30).
-- Agrega la columna que usa GET /api/auth/estado-cuenta para saber, antes de que alguien inicie
-- sesión, si su correo ya tiene una contraseña configurada -- false por defecto, así que toda
-- cuenta existente hoy (y toda cuenta nueva recién invitada) arranca tratándose como "primera
-- vez" hasta que pase, dentro de Nexit, por crear o restablecer su contraseña al menos una vez.
--
-- Se ejecuta, una vez, sin orden fijo -- no depende de ningún otro script de esta carpeta.
--
-- Actualización 2026-09-02: se le agregó IF NOT EXISTS a la columna (mismo patrón que ya
-- usan schema/06 y schema/07) -- así, si por error se corre dos veces contra el mismo
-- proyecto, la segunda vez no falla con "column already exists", solo no hace nada.

ALTER TABLE usuarios ADD COLUMN IF NOT EXISTS contrasena_configurada boolean NOT NULL DEFAULT false;
