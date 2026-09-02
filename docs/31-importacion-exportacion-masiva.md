# 31 — Importación y exportación masiva (Clientes, Proveedores, Proyectos)

## Por qué

La usuaria pidió, comparando con un sistema anterior (GAMO), un botón para exportar e importar datos de Clientes, Proveedores y Proyectos en Excel — el mismo tipo de operación masiva que ya usaba antes para cargar/descargar esos tres catálogos de golpe, en vez de registro por registro.

No existía nada parecido en el backend: la única exportación real que había era la de Informes (`docs/07`, `IInformeExcelExporter`), que arma un resumen ejecutivo, no un archivo pensado para volver a subirse. Tampoco existía ninguna exportación en PDF — se verificó con una búsqueda completa en ambos repositorios antes de construir nada; solo hay Excel (ClosedXML), en Informes y ahora aquí.

## Diseño (deliberado, léelo antes de usarlo)

**Exportar e importar usan el mismo formato de archivo.** El Excel que descargas con "Exportar" es exactamente el que puedes volver a subir con "Importar" — sirve como su propia plantilla. Primera fila = encabezados; cada fila siguiente = un registro.

**Importar SIEMPRE crea filas nuevas, nunca actualiza una existente.** No hay una llave natural confiable para decidir "esto ya existe" — ni el nombre ni el email son obligatorios o únicos en ninguna de las tres entidades. Volver a importar el mismo archivo dos veces crea duplicados a propósito, no los evita en silencio. Si necesitas corregir datos ya cargados, edítalos desde la pantalla normal (o bórralos y vuelve a importar).

**Una fila mala no detiene ni revierte el resto del archivo.** Cada fila se procesa de forma independiente: si la fila 15 tiene un dato inválido, las filas 1-14 y 16-200 se crean igual, y la 15 queda reportada con su número exacto y el motivo. El resultado de una importación siempre dice cuántas filas se crearon y, si hubo errores, la lista completa (fila + mensaje) para que puedas corregir solo esas y volver a intentar con un archivo más chico si quieres.

**Los países, ciudades, categorías y estados se resuelven por NOMBRE, no se inventan.** El Excel trae el nombre tal como lo escribirías a mano ("Colombia", "Bogotá", "Audiovisual", "Activo") porque es lo único que le sirve a quien abre el archivo — nadie tiene a mano el Id interno de un país. Si el nombre no coincide (sin importar mayúsculas/minúsculas) con ningún registro ya existente en Catálogos, la fila queda marcada como error con el nombre exacto que no se encontró. A propósito, **no se crea el catálogo solo ni se adivina el más parecido** — así nunca terminas con "Colombia" y "colombiaa" como dos países distintos por un error de tipeo. Si te falta un país/ciudad/categoría/estado, créalo primero en Catálogos y vuelve a importar.

**La importación requiere admin o super_admin.** Cualquier persona autenticada puede exportar (mismo permiso que ya tiene para ver la lista); solo un administrador puede importar, porque una importación masiva puede crear cientos de registros de golpe. El backend lo exige con la política `AdminOrAbove` sin importar lo que muestre el frontend; el botón "Importar" simplemente no aparece si tu rol no califica, para no mostrarte algo que de todos modos te va a rechazar con 403.

**Los proyectos NO importan equipo, proveedores asociados ni gerente explícito.** Esas tres cosas son relaciones, no datos planos de una fila, y forzarlas a columnas de Excel (nombres separados por comas, con el riesgo real de no encontrar a la persona exacta) habría complicado el archivo sin necesidad. Se completan después, proyecto por proyecto, desde la pantalla de edición — exactamente igual que si el proyecto se hubiera creado a mano sin llenarlas todavía. Si quien importa es gerente y no hay gerente explícito (no hay columna para eso), el proyecto queda autoasignado a quien importó — el mismo comportamiento que ya tenía crear un proyecto uno por uno desde el formulario.

## Endpoints

Los tres siguen exactamente el mismo patrón:

| Método | Ruta | Quién puede | Qué hace |
|---|---|---|---|
| `GET` | `/api/clientes/exportar` | Cualquier autenticado | Descarga todos los clientes como `.xlsx`. |
| `POST` | `/api/clientes/importar` | `admin`/`super_admin` | Sube un `.xlsx` (campo `archivo`, multipart), crea lo que se pueda, devuelve `{ creados, errores: [{ fila, mensaje }] }`. |
| `GET` | `/api/proveedores/exportar` | Cualquier autenticado | Igual, para proveedores. |
| `POST` | `/api/proveedores/importar` | `admin`/`super_admin` | Igual, para proveedores. |
| `GET` | `/api/proyectos/exportar` | Cualquier autenticado | Igual, para proyectos. |
| `POST` | `/api/proyectos/importar` | `admin`/`super_admin` | Igual, para proyectos. |

El archivo descargado se llama `{entidad}-{fecha}.xlsx` (por ejemplo `clientes-20260902.xlsx`). Si no adjuntas ningún archivo al importar, el backend responde con un error de negocio claro ("Debes adjuntar un archivo .xlsx.") en vez de un 500.

## Columnas de cada Excel (exportar e importar usan las mismas)

**Clientes:** Nombre, Sector, Ciudad, Dirección, Web, Contacto, Cargo del contacto, Email, Valor de referencia, Teléfono, Notas. Solo Nombre es obligatorio (misma validación que crear un cliente a mano); Ciudad es texto libre, no un catálogo — los clientes no tienen país/ciudad normalizados, a diferencia de los proveedores.

**Proveedores:** Nombre, País, Ciudad, Categoría, Estado, Contacto, Cargo del contacto, Email, Web, Dirección, Aforo, Costo de referencia, Score (1-5), Presupuesto, Cobertura, Teléfono, Notas. Nombre, País y Categoría son obligatorios (se resuelven por nombre contra Catálogos, ver arriba); Estado es texto libre con "Activo" por defecto si lo dejas vacío; Ciudad es opcional pero, si la pones, requiere que la fila también traiga el País (para saber en cuál país buscarla).

**Proyectos:** Nombre, Cliente, Contacto del proyecto, Tipo de proyecto, Prioridad, Ciudad, Sede Next, Fecha de solicitud, Fecha del evento, Estado, % de avance, Estado del brief, Estado de la propuesta, N.º de factura, Pagado (Sí/No), Fecha de pago, Notas. Nombre y Estado son obligatorios (Estado se resuelve por nombre contra Catálogos); Cliente es opcional, pero si lo escribes tiene que coincidir con un cliente que ya exista (créalo primero, o en una fila anterior del mismo archivo si también lo estás importando). "Pagado" acepta "Sí"/"No"/"true"/"false"/"1" sin distinguir mayúsculas; si pones "Sí" sin fecha de pago, se usa la fecha de hoy.

## Cómo está construido (para quien retome esto después)

Mismo patrón de capas que `IInformeExcelExporter` (`docs/07`): la interfaz (`IClientesImportExporter`/`IProveedoresImportExporter`/`IProyectosImportExporter`, en `Nexit.Application/Services/IImportExportServices.cs`) vive en Application, para que el controlador dependa de una abstracción y no de ClosedXML directamente; la implementación concreta (`Nexit.Infrastructure/Services/*ImportExporter.cs`) vive en Infrastructure. A diferencia del exportador de informes, estas SÍ dependen de los demás casos de uso de Application a propósito: cada fila importada se crea llamando el MISMO caso de uso (`ICrearClienteUseCase`/`ICrearProveedorUseCase`/`ICrearProyectoUseCase`) y el MISMO validador de FluentValidation que usa el formulario normal — así nunca hay dos formas distintas de decidir si un dato es válido, y cualquier regla de negocio nueva que se agregue a "crear un registro" aplica también a la importación sin tocar este código.

`IProyectosImportExporter.ExportarAsync` es async (a diferencia de los otros dos, síncronos) porque `ProyectoResponseDto` solo trae el Id de cliente/estado, no el nombre — la implementación resuelve esos nombres contra `IClienteRepository`/`ICatalogosRepository` antes de poder escribir las columnas del Excel.

Se agregaron 4 búsquedas por nombre nuevas a `ICatalogosRepository` (país, categoría, estado, ciudad-dentro-de-país) y una a `IClienteRepository` (cliente por nombre), todas case-insensitive (mismo criterio `.ToLower().Trim()` que ya usaba `NombreExisteAsync`) — nunca fuzzy match, coincidencia exacta salvo mayúsculas/espacios.

Pruebas: `ClientesImportExporterTests.cs` (5), `ProveedoresImportExporterTests.cs` (5), `ProyectosImportExporterTests.cs` (6) — cubren exportar con encabezados correctos, importar creando filas válidas, una fila inválida sin detener el resto, y los tres casos de catálogo-no-encontrado (país/categoría/ciudad para proveedores; cliente/estado para proyectos).

Frontend (`Nexit_Front`): un solo componente reutilizado (`ImportExportBar.tsx`) en las tres pantallas (`clientes/page.tsx`, `proveedores/page.tsx`, `proyectos/page.tsx`) en vez de triplicar la misma lógica de descarga + input de archivo + reporte de resultado — el botón "Exportar" siempre visible, "Importar" solo si el rol de quien está conectado es admin/super_admin (el backend lo exige igual, esto solo evita mostrar un botón que va a fallar con 403), y un modal con la lista de errores fila por fila después de importar. Cada store (`clientes-store.ts`/`providers-store.ts`/`projects-store.ts`) ganó un método `refresh()` para recargar la lista después de una importación exitosa, porque el `fetchAll()` normal no vuelve a pedir nada si ya se había cargado una vez.

La pantalla de Proyectos tenía antes un botón "Exportar" que generaba un CSV local (formato propio, no reimportable, calculado en el navegador a partir de lo ya cargado) — se reemplazó por este mismo `ImportExportBar` para que las tres pantallas se comporten igual y el archivo exportado sirva también para reimportar.
