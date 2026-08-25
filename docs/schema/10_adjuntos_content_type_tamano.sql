-- Corresponde a la migración de EF Core "AddAdjuntoContentTypeYTamano" (docs/28, HU-13 en docs/12),
-- ya aplicada a la base local `nexit_dev`. Agrega las dos columnas nuevas que necesita la subida real
-- de adjuntos (PDF/Excel) a Supabase Storage: el tipo de archivo y su tamaño en bytes -- ninguna de
-- las dos es obligatoria (un adjunto tipo "link" nunca las tiene).
--
-- Se ejecuta, una vez, sin orden fijo -- no depende de ningún otro script de esta carpeta.

ALTER TABLE proveedor_adjuntos ADD COLUMN content_type character varying(255);
ALTER TABLE proveedor_adjuntos ADD COLUMN tamano_bytes bigint;
