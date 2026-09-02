-- Corresponde a la migración de EF Core "AddPresenciaUsuarios" (docs/29, HU-12 en docs/12),
-- ya aplicada a la base local `nexit_dev`. Agrega la columna que necesita la presencia en vivo
-- (ver quién tiene el sistema abierto ahora mismo): la marca de tiempo del último "ping" que
-- mandó cada usuario -- nula si nunca ha hecho ping (por eso aparece como desconectado por defecto).
--
-- Se ejecuta, una vez, sin orden fijo -- no depende de ningún otro script de esta carpeta.
--
-- Actualización 2026-09-02: se le agregó IF NOT EXISTS a la columna (mismo patrón que ya
-- usan schema/06 y schema/07) -- así, si por error se corre dos veces contra el mismo
-- proyecto, la segunda vez no falla con "column already exists", solo no hace nada.

ALTER TABLE usuarios ADD COLUMN IF NOT EXISTS ultima_actividad timestamp with time zone;
