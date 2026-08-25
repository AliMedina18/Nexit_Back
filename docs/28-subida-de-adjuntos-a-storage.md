# 28 — Subida real de archivos adjuntos (PDF/Excel) a Supabase Storage

## De dónde salió esto

Hasta ahora, un "adjunto" de proveedor solo podía ser un **link** (una URL externa que alguien pegaba a mano — por ejemplo, a un archivo guardado en otro lado). No existía la opción de subir el archivo de verdad desde Nexit.

Al hablar de dónde guardar archivos reales, se evaluó primero Google Drive (la cuenta `analistacompras@agencianextmkt.com` ya está con el almacenamiento lleno) y se decidió usar **Supabase Storage** en su lugar, ya incluido en el plan Pro que ya se está pagando. Surgió entonces la preocupación de que subir archivos hiciera "la base de datos" más lenta — la aclaración importante, verificada directamente en el código, es que **Postgres nunca ha guardado los archivos en sí**: la tabla `proveedor_adjuntos` solo guarda una referencia (una URL o, ahora, una ruta de Storage), nunca los bytes del archivo. Subir 10, 100 o 10,000 archivos no hace más pesada ni más lenta la base de datos — el peso vive en Supabase Storage, un servicio de objetos separado, diseñado exactamente para eso.

Con esa duda resuelta, se confirmó la regla de negocio: **por ahora, solo se aceptan PDF y Excel** (`.pdf`, `.xlsx`, `.xls`) — son los dos formatos que de verdad usa el equipo.

## Qué se construyó

### Bucket de Storage
Se creó el bucket `adjuntos-proveedores` en el proyecto real de Supabase, directamente por la API de Storage (no requiere ningún script SQL nuevo — a diferencia del esquema de Postgres, la configuración de un bucket vive en la configuración del proyecto, no en una tabla):

- **Privado** (no público — cada archivo solo se puede descargar con una URL firmada temporal, nunca con un link permanente).
- **Límite de 20 MB por archivo.**
- **Whitelist de tipos MIME** a nivel del propio bucket, como segunda capa de protección además de la validación en el backend: `application/pdf`, `application/vnd.openxmlformats-officedocument.spreadsheetml.sheet` (`.xlsx`), `application/vnd.ms-excel` (`.xls`).

### Cambios en el backend

- **`ProveedorAdjunto`** (entidad): dos campos nuevos, `ContentType` (string, hasta 255 caracteres) y `TamanoBytes` (long) — ambos opcionales, porque un adjunto tipo `link` nunca los tiene.
- **`ISupabaseStorageService` / `SupabaseStorageService`** (nuevo): habla directo con la API REST de Supabase Storage —
  - `SubirAsync`: sube el archivo al bucket.
  - `ObtenerUrlFirmadaAsync`: genera una URL de descarga temporal (el archivo no es público).
  - `EliminarAsync`: borra el archivo del bucket (best-effort — si falla, se registra en el log pero no bloquea el borrado del registro en la base de datos, mismo patrón que ya se usaba para borrar cuentas de Supabase Auth, ver `docs/17`).
- **`ProveedorAdjuntoUseCases`**: valida la extensión (whitelist explícita — cualquier otra extensión se rechaza de inmediato, incluso si el cliente dice que el `Content-Type` es válido: el tipo real lo decide el backend según la extensión, nunca lo que mande el navegador), valida el tamaño (20 MB, mismo límite que el bucket), sanea el nombre del archivo (solo letras, números, espacios, guiones y guiones bajos — cualquier otro carácter se descarta) y arma la ruta de guardado: `proveedores/{proveedorId}/{guid}-{nombreSaneado}{extensión}` — el GUID evita choques de nombre entre archivos distintos con el mismo nombre.
- **`ProveedorAdjuntosController`**: dos endpoints nuevos —
  - `POST /api/proveedores/adjuntos/subir` — multipart, recibe el archivo real, lo valida y lo sube.
  - `GET /api/proveedores/adjuntos/{id}/descargar` — devuelve la URL de descarga (firmada y temporal si es tipo `file`; la URL guardada tal cual si es tipo `link`).
- **Eliminar un adjunto** (`DELETE`, ya existente): ahora, si el adjunto es tipo `file`, también borra el archivo del bucket — ya no queda huérfano en Storage. Si es tipo `link`, no toca Storage (nunca tuvo un archivo ahí).
- **Convivencia con los adjuntos tipo `link`:** esa opción sigue existiendo tal cual estaba — para lo que sí siga teniendo sentido como link externo, o para cualquier otro tipo de archivo que no sea PDF/Excel, se puede seguir pegando la URL a mano. Lo nuevo es una opción adicional, no un reemplazo.

### Migración de base de datos

`AddAdjuntoContentTypeYTamano` — agrega las columnas `content_type` y `tamano_bytes` a `proveedor_adjuntos`. Aplicada a `nexit_dev` (base local) el 2026-08-25. Para producción, se aplica corriendo `dotnet ef migrations script` (igual que las demás migraciones — ver la nota de `schema/01_esquema_completo.sql` en el índice) contra el Supabase real, cuando se despliegue.

### Pruebas

8 pruebas nuevas (`ProveedorAdjuntosUploadTests.cs`): rechaza extensión no permitida, rechaza archivo demasiado grande, sube un PDF válido y crea el registro con la ruta correcta, sube un `.xlsx` ignorando el `Content-Type` que mande el cliente (usa el derivado de la extensión), `EliminarAsync` sí borra del Storage para tipo `file` pero no para tipo `link`, `ObtenerUrlDescargaAsync` devuelve la URL tal cual para `link` (sin tocar Storage) y la URL firmada para `file`.

**234 pruebas en total (227 pasan sin necesitar Docker, 7 dependen de Docker en este entorno — mismas de siempre, no relacionadas con esto), 8 nuevas para esta historia, cero regresiones** (confirmado por la usuaria: `dotnet build` y `dotnet test` corrieron sin errores en su máquina).

## Sobre la capacidad y el costo (la cuenta que hizo la usuaria)

La estimación fue: hasta 24 personas, unos 60 proyectos al mes, un ejemplo de 10 archivos por proyecto — es decir, del orden de 600 archivos al mes en el peor caso. Como los archivos son PDF y Excel (típicamente unos pocos MB cada uno, no videos ni imágenes pesadas) y **la base de datos de Postgres no crece con esto** (solo guarda la ruta, nunca el archivo), el único límite real es el espacio del bucket de Storage dentro del plan Pro de Supabase ya contratado — con ese volumen mensual, queda cómodo dentro de lo incluido. Si en algún punto el equipo empieza a subir archivos mucho más pesados (por ejemplo, videos), ahí sí valdría la pena revisar el consumo real contra el límite del plan — pero para PDF/Excel a este volumen, no hay ninguna señal de que vaya a ser un problema.

## Qué no se construyó (fuera de alcance, a propósito)

- No hay control de versiones de archivos (subir un archivo nuevo no "reemplaza" uno viejo — quedan como adjuntos separados, igual que ya pasaba con los links).
- No hay previsualización de archivos dentro de Nexit — la descarga entrega la URL firmada, y el navegador/sistema operativo de quien la abre decide cómo mostrarla.
- No se agregaron más tipos de archivo que PDF/Excel — si más adelante se necesita otro tipo (Word, imágenes), es un cambio pequeño (agregar la extensión a la whitelist del backend y al bucket), no un rediseño.
