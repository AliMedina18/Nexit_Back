-- Hace reproducible por script la configuración del bucket "adjuntos-proveedores"
-- (docs/28, HU-13) -- hasta ahora existía solo porque se creó a mano, una vez,
-- desde el dashboard/API de Storage, sin quedar en ningún script (ver la nota
-- original en docs/28, sección "Bucket de Storage"). Los buckets de Supabase
-- Storage SÍ son filas reales de una tabla de Postgres (storage.buckets), así
-- que sí se pueden crear/configurar por SQL como cualquier otro objeto del
-- esquema -- la nota original de docs/28 estaba pensando en que no hace falta
-- una migración de EF Core (correcto, Storage no es parte del DbContext), no en
-- que no se pueda scriptear en absoluto.
--
-- Es un UPSERT (ON CONFLICT), así que es seguro correrlo tanto para crear el
-- bucket desde cero en un proyecto nuevo (staging, disaster recovery, otro
-- ambiente) como para simplemente re-confirmar/re-aplicar la configuración
-- exacta contra el bucket que ya existe hoy en producción -- no toca ni borra
-- ningún archivo ya subido, solo la configuración del bucket en sí (privacidad,
-- límite de tamaño, tipos MIME permitidos).
--
-- Se ejecuta, una vez por proyecto de Supabase, sin orden fijo.

insert into storage.buckets (id, name, public, file_size_limit, allowed_mime_types)
values (
  'adjuntos-proveedores',
  'adjuntos-proveedores',
  false, -- privado: nunca se sirve con una URL pública/permanente, solo con URL firmada temporal (ver SupabaseStorageService.ObtenerUrlFirmadaAsync)
  20971520, -- 20 MB en bytes, mismo límite que valida ProveedorAdjuntoUseCases.TamanoMaximoBytes en el backend (segunda capa)
  array[
    'application/pdf',
    'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet', -- .xlsx
    'application/vnd.ms-excel' -- .xls
  ]
)
on conflict (id) do update set
  public = excluded.public,
  file_size_limit = excluded.file_size_limit,
  allowed_mime_types = excluded.allowed_mime_types;

-- No hace falta ninguna política de RLS sobre storage.objects para este bucket:
-- el backend habla con la API de Storage usando la service_role key (ver
-- Supabase:ServiceRoleKey en appsettings.Production.json), que ignora RLS por
-- diseño -- el frontend nunca sube/descarga directo contra Supabase Storage con
-- la anon key, siempre pasa por ProveedorAdjuntosController (subir/descargar/
-- eliminar). Si en el futuro algo necesitara hablarle a Storage directo con la
-- anon key, ahí sí haría falta agregar políticas explícitas.
--
-- Verificación: en el dashboard de Supabase, Storage > adjuntos-proveedores >
-- Configuration, confirma "Private bucket" activado, "File size limit" en
-- 20 MB, y los tres tipos MIME de arriba en "Allowed MIME types".
