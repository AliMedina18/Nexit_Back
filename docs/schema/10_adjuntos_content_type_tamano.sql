-- Corresponde a la migración de EF Core "AddAdjuntoContentTypeYTamano" (docs/28, HU-13 en docs/12),
-- ya aplicada a la base local `nexit_dev`. Agrega las dos columnas nuevas que necesita la subida real
-- de adjuntos (PDF/Excel) a Supabase Storage: el tipo de archivo y su tamaño en bytes -- ninguna de
-- las dos es obligatoria (un adjunto tipo "link" nunca las tiene).
--
-- Se ejecuta, una vez, sin orden fijo -- no depende de ningún otro script de esta carpeta.
--
-- Actualización 2026-09-02: se le agregó IF NOT EXISTS a las dos columnas (mismo patrón que
-- ya usan schema/06 y schema/07) -- así, si por error se corre dos veces contra el mismo
-- proyecto, la segunda vez no falla con "column already exists", solo no hace nada.

ALTER TABLE proveedor_adjuntos ADD COLUMN IF NOT EXISTS content_type character varying(255);
ALTER TABLE proveedor_adjuntos ADD COLUMN IF NOT EXISTS tamano_bytes bigint;
