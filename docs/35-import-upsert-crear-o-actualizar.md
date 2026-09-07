# 35 — Importar ahora actualiza en vez de duplicar (upsert)

## Por qué

Alicia probó el botón Importar (`docs/31`) a fondo y confirmó que funciona bien, pero pidió un cambio de lógica puntual: si vuelve a subir el mismo archivo (o una versión corregida de él), **no debe duplicar** los clientes/proveedores/proyectos que ya existen. Su idea, tal cual la planteó: "si uno de esos campos es diferente, nada más se actualizaría el campo... y si hay otro campo más nuevo, se agrega nada más esa fila -- ya se tiene que tener en cuenta que ya tenemos esa cantidad de proveedores. Hace chequeo: esto ya lo tenemos, esto ya lo tenemos, a esto no, esto se actualiza."

Hasta ahora, el importador **siempre creaba filas nuevas** -- era una decisión deliberada documentada en `IImportExportServices.cs` (el archivo tenía datos históricos de una sola vez, sin necesidad de reconciliar). Ese ya no es el caso: el flujo real de Alicia es reimportar el mismo Excel según lo va corrigiendo/completando, y necesita que eso sea seguro.

## Qué cambió: upsert (crear-o-actualizar) en los 3 importadores

`ClientesImportExporter`, `ProveedoresImportExporter` y `ProyectosImportExporter` ahora, fila por fila:

1. Buscan si el registro **ya existe** (ver "llave de coincidencia" abajo).
2. Si no existe: lo crea, exactamente como antes.
3. Si ya existe: lo **actualiza**, con estas reglas:
   - **Campo en blanco en el Excel → se conserva el valor que el registro ya tenía.** Nunca se borra nada por dejar una celda vacía.
   - **Campo con un valor → lo reemplaza**, sea igual o distinto al que ya había (si es igual, no hay cambio real).
   - **Teléfono/Email son la excepción:** el Excel solo trae uno de cada uno por fila, pero un cliente/proveedor puede tener varios. Reimportar nunca borra los que ya tenía -- si el de la fila es nuevo, se agrega a la lista; si ya estaba (comparación sin distinguir mayúsculas ni espacios), no se duplica.
   - **Las relaciones que este Excel no trae se conservan tal cual, nunca se vacían:**
     - Cliente: `RegionId`, `CiudadId`, `EtapaId` (se asignan a mano desde la ficha, `docs/33`).
     - Proveedor: los servicios asociados (`ServicioIds`).
     - Proyecto: el equipo asignado, los proveedores asociados y el gerente (`GerenteId`) -- son relaciones que se completan proyecto por proyecto desde la pantalla de edición, no datos planos de una fila de Excel.
   - **"Pagado" en blanco no revierte un proyecto ya marcado como pagado.** Antes, una celda vacía se leía como "No" y podía revertir accidentalmente un proyecto que ya se había marcado como pagado. Ahora una celda vacía significa "no cambiar" (se agregó `EsSiONoOpcional`, que distingue "vacío" de "No" explícito).

### Llave de coincidencia ("¿esto ya lo tenemos?")

- **Clientes y Proveedores:** por `Nombre` (sin distinguir mayúsculas ni espacios al inicio/final).
- **Proyectos:** por la pareja **(Cliente, Nombre)**, no el nombre solo -- el mismo nombre de proyecto puede repetirse legítimamente para clientes distintos (ej. "Lanzamiento de producto" para dos clientes diferentes no deben verse como el mismo proyecto).

Si una fila no resuelve un nombre de país/categoría/estado/cliente contra los catálogos, sigue quedando marcada como error igual que antes -- eso no cambió.

### Nuevo dato en el resultado de importar

`ImportarResultadoDto` ahora trae, además de `Creados` y `Errores`, un contador `Actualizados` -- para que el resumen que ve Alicia diga cuántas filas se crearon y cuántas se actualizaron, no solo "X creados".

## Archivos tocados

- `Nexit.Application/DTOs/Importacion/ImportacionDtos.cs` -- se agregó `Actualizados` a `ImportarResultadoDto`.
- `Nexit.Application/Services/IImportExportServices.cs` -- doc comment actualizado (ya no dice "siempre crea, nunca actualiza").
- `Nexit.Core/Interfaces/IProveedorRepository.cs` + `Nexit.Infrastructure/Repositories/ProveedorRepository.cs` -- nuevo `FindIdPorNombreAsync` (mismo patrón que ya existía en `IClienteRepository`).
- `Nexit.Core/Interfaces/IProyectoRepository.cs` + `Nexit.Infrastructure/Repositories/ProyectoRepository.cs` -- nuevo `FindIdPorClienteYNombreAsync`.
- `Nexit.Infrastructure/Services/ClientesImportExporter.cs`, `ProveedoresImportExporter.cs`, `ProyectosImportExporter.cs` -- reescritos con la rama de actualización descrita arriba. No se creó ningún caso de uso ni validador nuevo: se reutilizan `IActualizarClienteUseCase`/`IActualizarProveedorUseCase`/`IActualizarProyectoUseCase` y sus validadores, que ya existían para la edición manual desde el formulario.

## Verificado de verdad, no solo escrito

Como en este entorno no se puede compilar/correr el backend en .NET, se armó una simulación en Python (`upsert_sim.py`) que replica la lógica exacta de los 3 importadores (mismo COALESCE, mismo criterio de merge de teléfono/email, mismas relaciones preservadas) y la corre contra una base Postgres de prueba real (mismo esquema que produce EF Core), no contra datos inventados en memoria. 25 verificaciones, todas pasaron:

- **Reimportar el mismo archivo no duplica nada** -- mismo número de clientes/proveedores/proyectos/teléfonos/emails antes y después de reimportar.
- **Cambiar un solo campo en una sola fila solo actualiza ese campo, en esa fila** -- las demás filas y los demás campos de esa misma fila quedan bit a bit idénticos.
- **Dejar un campo en blanco al reimportar conserva el valor que ya había** (no lo vacía).
- *"Pagado" en blanco no revierte un proyecto ya marcado como pagado**, y `FechaPago` se conserva.
- **Las relaciones que el Excel no trae** (ubicación de cliente, servicios de proveedor, gerente/equipo/proveedores de proyecto -- asignadas a mano, simulando la pantalla de edición) **sobreviven intactas** al reimportar.
- **Un teléfono/email nuevo se agrega** a la lista (no reemplaza al que ya había); uno repetido (incluso con mayúsculas distintas) no se duplica.

## Pendiente

- Esto se escribió y se verificó la lógica contra Postgres real, pero **no se compiló** el backend en sí (no hay `dotnet` disponible en este entorno) -- hace falta que Alicia corra `dotnet build` en su máquina antes de dar esto por cerrado del todo.
- Parte 2 del pedido de Alicia (mejorar el diseño visual del modal "Resultado de la importación" en Clientes/Proveedores/Proyectos, y mostrar ahí el nuevo contador de `Actualizados`) se aborda por separado en el frontend.
