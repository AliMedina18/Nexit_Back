# 34 — Limpieza de los 3 Excel de la compañera para carga histórica

## Por qué

La compañera de Alicia le pasó 3 Excel con la operación real de Next: `BD_CLIENTES_Y_PROVEEDORES__NEXT_.xlsx`, `Base_de_Datos__Next_2026.xlsx` y `Seguimiento_de_proyectos_1.xlsx`. Se analizaron a fondo contra el esquema actual (ver el análisis compartido con Alicia por chat, no versionado aquí porque fue un documento de decisión puntual, no de referencia permanente) y, con sus decisiones confirmadas, se construyó `limpieza_excel_nexit.py` (vive fuera del repo, es un script de una sola vez) que genera los 3 Excel listos para el botón "Importar" (`docs/31`) más uno de revisión.

## Decisiones tomadas con Alicia

- **Etapa de cliente (E1-E6):** no se infiere del Excel -- ninguno de los 3 archivos trae un dato confiable de en qué etapa comercial está cada cliente. Queda vacía, se asigna después a mano desde la ficha del cliente (`docs/33`).
- **Categorías de proveedor:** se mapean a las 25 ya sembradas vía una tabla de equivalencias (`CATEGORIA_EQUIV` en el script); lo que no calza queda en "Otro" y se lista aparte para que Alicia/su compañera decidan si hace falta una categoría nueva.
- **Estado de proyecto:** se prioriza el vocabulario descriptivo de la hoja 2026 (más confiable); para 2023-2025 (solo "ETAPA 1".."ETAPA 9" genérico) se usa el % de avance como señal, con menor confianza -- esas filas quedan marcadas para revisar.
- **Organizaciones (mini-CRM del 3er Excel):** solo se usan las filas de Colombia como clientes adicionales (estado "Prospecto"); Interacciones/Oportunidades se dejan fuera, quedan como posible funcionalidad futura.
- **Clientes "NEXT COLOMBIA":** el Excel de contactos trae 9 filas distintas (una por persona del equipo de la sede Colombia) bajo el mismo nombre de empresa. Se fusionan en un solo cliente -- sin ciudad/dirección/web porque son la propia empresa, no un cliente externo -- y se importan tal cual, sin marca de revisión.

## Ciudades: dos reglas distintas, no una

Alicia pidió explícitamente arreglar bien las ciudades (encontró "BGTA" en vez de "Bogotá" en Proyectos). Investigar esto llevó a que **no hay una sola regla** -- `Cliente.Ciudad` y `Proyecto.Ciudad` son texto libre (`docs/31`), pero `Proveedor.Ciudad` es un catálogo real que se resuelve por nombre EXACTO (con tilde, no solo mayúsculas) contra `ciudades` (`docs/schema/seed_geografia...`) -- si no calza, **la fila completa del proveedor se rechaza al importar**, no solo el campo.

- **Clientes/Proyectos (texto libre):** se tradujeron las siglas encontradas (BGTA/BOGTA → Bogotá, CART → Cartagena, MEDELLIN → Medellín, CANCUN → Cancún, QUERETARO → Querétaro, CIUDAD DE MEXICO → Ciudad de México) y se limpiaron saltos de línea/espacios. Lo que no se reconoce se deja tal cual (no bloquea nada, es texto libre).
- **Proveedores (catálogo):** se mapeó cada valor real de `CIUDAD` en `BD_PROVEEDORES COL/MEX` contra el catálogo ya sembrado (`CIUDAD_PROVEEDOR_EQUIV` en el script). Lo que **no tiene equivalente exacto** se deja vacío (Ciudad es opcional) en vez de arriesgar que se rechace todo el proveedor, y el valor original queda guardado en Notas + en la hoja de revisión.

**Hallazgo importante:** de los 145 proveedores, **86 tienen "CDMX" como ciudad**, y el catálogo sembrado no tiene una ciudad "Ciudad de México" en sí -- solo sus alcaldías (Benito Juárez, Miguel Hidalgo, Coyoacán, etc.), que no son lo mismo. Por eso esos 86 quedan con Ciudad vacía en `proveedores.xlsx`. Se dejó preparado (sin aplicar) `docs/schema/17_agregar_ciudad_de_mexico.sql` -- una sola fila adicional en el catálogo -- para que, si Alicia lo aprueba y lo corre contra su base local, se pueda regenerar `proveedores.xlsx` con esas 86 filas completas. Sin equivalente exacto tampoco: FUNZA/COTA (Cundinamarca -- no están en el catálogo sembrado), "MULTI LOCACIÓN", "VALLARTA - CDMX" (multi-ciudad) y "EDOMEX" (no dice cuál ciudad del Estado de México).

## Resultado de la limpieza

| Archivo | Filas | Notas |
|---|---|---|
| `Clientes_importar.xlsx` | 63 | Deduplicado por nombre entre `BD_CLIENTES MEX` y `Organizaciones` (solo Colombia). 0 filas marcadas para revisar. |
| `Proveedores_importar.xlsx` | 145 | 13 con categoría sin equivalencia (quedan en "Otro"), 86 con ciudad "CDMX" sin equivalente en el catálogo todavía (ver arriba) -- el resto de ciudad-sin-equivalente son 7 casos puntuales (Funza, Cota, Multi locación, Vallarta-CDMX, Edomex). |
| `Proyectos_importar.xlsx` | 558 | 371 marcadas para revisar, casi todas por el estado inferido solo por % de avance en las hojas 2023-2025 (vocabulario ETAPA no confiable, ver arriba). |
| `Revisar_manualmente.xlsx` | -- | No se importa. Filas dudosas + resumen de categorías de proveedor sin mapear, para que Alicia/su compañera revisen antes de importar. No vive en `docs/datos` porque no es un archivo para el botón Importar. |

Los 3 primeros están en `docs/datos/` con el nombre exacto que ya usaba esa carpeta (`Clientes_importar.xlsx`, `Proyectos_importar.xlsx` existían de una carga anterior -- ver más abajo -- se sobrescriben con la versión nueva; `Proveedores_importar.xlsx` es nuevo, nunca se habían migrado proveedores).

## Importante: esto no es la primera carga histórica -- posible choque

Al ir a guardar estos archivos en `docs/datos/` se encontró que **ya existían** ahí `Clientes_importar.xlsx` (124 filas) y `Proyectos_importar.xlsx` (525 filas), de una carga anterior que -- según `docs/README.md`, punto 36 -- ya se importó de verdad ("Se migró el histórico real de proyectos 2023-2026, 521 proyectos, 126 clientes") usando el mismo botón Importar. Esos clientes eran registros "creados automáticamente desde el histórico de seguimiento de proyectos", con nota explícita de "sin teléfono/email/sector reales todavía -- completar con la base real de clientes cuando esté disponible" -- que es exactamente lo que estos 3 Excel nuevos, con datos reales de contacto, vienen a resolver.

Además existe `docs/schema/16_datos_reales_clientes_proveedores_proyectos.sql`, un tercer origen de datos (extraídos del prototipo HTML original), que sí es seguro correr más de una vez porque busca por nombre y actualiza en vez de duplicar.

El botón Importar (`docs/31`) **siempre crea, nunca actualiza** -- no tiene forma de detectar que "BAT" o "SEGUROS BOLIVAR" ya existen. Si la base de datos local (`nexit_dev`) todavía tiene esos ~525 proyectos y ~124 clientes de la carga anterior, importar estos archivos nuevos tal cual **duplicaría** casi todo. Antes de importar nada, hace falta decidir con Alicia cómo reconciliar las tres fuentes -- no se va a tocar ninguna base de datos sin eso.

## `docs/schema/18_proveedores_reales.sql` -- probado de verdad, no solo escrito

Proveedores es distinto a Clientes/Proyectos: nunca se había cargado antes (no hay `docs/datos/Proveedores_importar.xlsx` previo), así que no hay nada con qué reconciliar -- por eso se adelantó como script SQL independiente, generado directo desde `limpieza_excel_nexit.py` (misma fuente que `Proveedores_importar.xlsx`, para que ambos coincidan). Sigue el patrón idempotente de `schema/16` (busca por nombre, actualiza si existe, inserta si no) porque `proveedores.nombre` no tiene restricción UNIQUE en la base -- no se puede usar `ON CONFLICT`.

**Esto sí se probó de verdad**, no solo se escribió: se instaló Postgres 16 en el entorno donde se generó esto, se aplicó el esquema real (`01_esquema_completo.sql`, regenerado por EF Core -- no escrito a mano), el seed de catálogos, `schema/14` y `schema/17`, y se corrió `schema/18` completo contra esa base de prueba con `psql -v ON_ERROR_STOP=1`. Terminó sin errores (los 145 bloques). Verificado con consultas reales, no solo "no explotó":

- 138 proveedores insertados (no 145): 7 nombres se repetían en el Excel original (ej. "BANDA VERSATIL", "SKYROCKET" -- el mismo proveedor listado dos veces) y el script correctamente los fusionó en un solo registro en vez de duplicarlos.
- Con `schema/17` aplicado, 76 de los proveedores con ciudad "CDMX" quedan resueltos a "Ciudad de México" automáticamente (antes de esto, esos 76 se habrían quedado sin ciudad).
- 150 teléfonos cargados correctamente en `proveedor_telefonos`.
- 0 filas violan los `CHECK` constraints de `proveedores` (categoría/score/estado/presupuesto/cobertura).

**Bug de datos encontrado al verificar (no de este script, del Excel original):** dos proveedores ("BANDA VERSATIL", "EVENT PLANNER") traen un nombre de persona o una URL en la columna de correo, no un correo real -- confirmado contra el Excel original, fila por fila. Si eso se hubiera puesto tal cual en `Email`, habría rechazado la fila completa al importar (Cliente/Proveedor validan formato de correo, `docs/32`). Se corrigió: cualquier valor de "correo" sin "@" (5 casos en total, entre Proveedores y Clientes) se guarda en Notas en vez de en Email, con el texto original intacto.

**Pendiente antes de correr `schema/18` contra cualquier base real:** aplicar primero `schema/17` (si se quiere que las 76 filas con CDMX queden con ciudad) -- si no se aplica, esas filas simplemente cargan sin ciudad, no falla nada.

## Hallazgo aparte: dos migraciones de EF Core que nunca se generaron

Al armar el entorno de prueba se encontró que `docs/schema/01_esquema_completo.sql` (que se regenera automáticamente desde las migraciones reales de EF Core -- no se escribe a mano, por diseño) **no incluye ni las columnas de ubicación de Cliente ni la tabla `etapas_cliente`**, y su `ck_proyectos_propuesta` sigue siendo el viejo (`'No enviada', 'En proceso', 'Enviada'`), no el corregido por `schema/15` (`..., 'Aprobada', 'Rechazada'`). Revisando `src/Nexit.Infrastructure/Migrations/`:

- `AddClienteUbicacionCatalogoYEstado` (`schema/14`) **sí existe** como migración real -- solo falta que se le corra `dotnet ef database update` a `nexit_dev` (y aplicar el `.sql` a producción).
- `FixProyectoPropuestaEstadoValues` (`schema/15`) **NO existe** como migración -- se aplicó solo como SQL manual directo a la base (probablemente producción), y el código C# (`NexitDbContext.cs`) nunca se actualizó para que coincida. Si en algún momento se genera una migración nueva desde el código actual, EF Core va a intentar **revertir** ese constraint al valor viejo, lo que rompería cualquier proyecto con propuesta "Aprobada"/"Rechazada" en la base. Hay que corregir el `HasCheckConstraint` en `NexitDbContext.cs` antes de la próxima migración que toque `proyectos`.
- `AddEtapaClienteCatalogo` (el catálogo de etapas del documento 33) tampoco existe como migración -- exactamente como ya decía el documento 33, sigue pendiente que la usuaria corra `dotnet ef migrations add`.

No se tocó nada de esto en este paso -- se deja documentado para que, cuando la usuaria genere la próxima migración, no se lleve la sorpresa de que Proyectos pierde la corrección de `schema/15`.

## `docs/schema/19_borrar_clientes_proveedores_proyectos.sql` -- decisión de recargar limpio en vez de conciliar

En vez de conciliar los ~525 proyectos / ~124 clientes ya cargados (con nombres casi-duplicados sin resolver, ej. "DAVIbank" vs "DAVIVIENDA") contra los datos reales nuevos, Alicia decidió borrar todo lo de clientes/proveedores/proyectos y recargar limpio desde los 3 Excel depurados + `schema/18`. Confirmó explícitamente que no hace falta respaldo antes porque el sistema todavía no lo usan usuarios finales.

Se preparó `docs/schema/19_borrar_clientes_proveedores_proyectos.sql`: borra `proyectos`, `proveedores` y `clientes` (en ese orden, para que ningún proyecto quede transitoriamente sin cliente antes de borrarlo también) y limpia `solicitudes_eliminacion`/`notificaciones` que pudieran quedar apuntando a esos registros (no son llave foránea real, así que no truenan solas). No toca ningún catálogo (países, ciudades, categorías, estados, usuarios, servicios).

**Probado de verdad contra la base de prueba**, no solo escrito: se insertó un cliente + un proyecto (apuntando a ese cliente y a un proveedor real de los 138 ya cargados) + una fila en cada una de las 6 tablas que cuelgan de proyecto/proveedor (`proyecto_equipo`, `proyecto_proveedores`, `proyecto_seguimiento`, `proveedor_telefonos`, etc.), se corrió el script completo, y las 8 tablas verificadas quedaron en 0 -- incluyendo las que dependen por cascada, sin ningún error de restricción. Los catálogos (`paises`, `ciudades`, `categorias_proveedor`, `estados_proyecto`) se verificaron intactos después de correrlo.

Es un script directo (`DELETE`, no `TRUNCATE`) envuelto en una sola transacción (`BEGIN`/`COMMIT`) -- si algo falla a la mitad, no se aplica nada. Se puede correr más de una vez sin problema.

**Orden recomendado para recargar limpio, una vez corrido `schema/19`:** `schema/14` (ubicación/estado de cliente) → `schema/20` (etapas de cliente, ver abajo) → `schema/17` (catálogo CDMX) → `schema/18` (proveedores) → importar `Clientes_importar.xlsx` y `Proyectos_importar.xlsx` por el botón Importar (`docs/31`), en ese orden, ya que ahora sí van a ser los únicos clientes/proyectos en la base -- no hay nada con qué chocar.

## `docs/schema/20_etapas_cliente_catalogo.sql` -- el otro pendiente del hallazgo aparte, ya reventó en producción

Al intentar importar `Clientes_importar.xlsx` contra producción (2026-09-05, con `schema/19` y `schema/14` ya corridos ahí), la pantalla de Clientes dejó de cargar por completo, con "Ocurrió un error interno." Con el log real del backend (`dotnet run`) se confirmó la causa exacta -- no fue necesario adivinar más:

```
Npgsql.PostgresException: 42703: column c.etapa_id does not exist
Npgsql.PostgresException: 42P01: relation "etapas_cliente" does not exist
```

Esto es exactamente el segundo pendiente que ya había quedado documentado arriba (sección "Hallazgo aparte"): `AddEtapaClienteCatalogo` (el catálogo de etapas E1-E6 del documento 33) nunca se generó como migración real, así que la tabla `etapas_cliente` y la columna `clientes.etapa_id` -- que el código C# ya da por hechas en cada consulta de Clientes -- nunca se crearon en ninguna base real. No es un bug de la importación ni del Excel; cualquier pantalla que liste clientes iba a fallar igual, con o sin importar nada.

Se preparó `docs/schema/20_etapas_cliente_catalogo.sql`: crea `etapas_cliente` y `clientes.etapa_id` con exactamente los mismos nombres/tipos/constraints que ya espera `NexitDbContext.cs` (como si hubiera salido de `dotnet ef migrations add`), y siembra las 6 etapas reales de `docs/33` (Contacto inicial 10% → Facturación 100%) -- el mismo bloque que ya existía, sin poder correr nunca, en `seed_geografia_categorias_estados.sql`.

**Probado de verdad:** se corrió contra la base de prueba (que hasta ahora tenía este mismo parche puesto a mano, sin formalizar) partiendo de cero -- se le quitaron la tabla y la columna, se corrió el script una vez (crea todo y siembra las 6 filas), y se corrió una segunda vez para confirmar que no duplica ni falla (0 filas insertadas la segunda vez, sin errores). Después se repitieron exactamente las 2 consultas que habían fallado en el log real de producción -- ambas corren limpio.

Con `schema/14` y `schema/20` corridos, ya no debería haber ninguna columna o tabla pendiente para que Clientes cargue.

## Dos bugs reales encontrados al importar `Clientes_importar.xlsx` de verdad (2026-09-05/06)

Con la base ya al día, la importación corrió pero rechazó 29 de las 63 filas. Se investigó cada motivo hasta la causa real, no se dejó como "revisar manualmente":

**1. "Al menos un teléfono es requerido" (13 filas).** Esto no era un problema del Excel -- son clientes reales que de verdad no tienen teléfono capturado en la fuente (ej. EY MX, Lulo Bank, Red Bull). Revisando el validador (`CreateClienteValidator.cs`/`UpdateClienteValidator.cs`) apareció algo más serio: **esta regla ya se había decidido quitar antes**, está documentado en `docs/32-campos-obligatorios-vs-opcionales.md` ("teléfono ya no es obligatorio en Clientes", con la línea exacta a borrar) -- pero el código en el repositorio todavía la tenía en AMBOS validadores. Es decir, el fix se documentó como aplicado pero nunca llegó a guardarse en el código (o se perdió en algún punto). Se corrigió ahora, quitando `RuleFor(x => x.Telefonos).NotEmpty()...` de los dos archivos -- se mantiene la validación de que un teléfono individual, si se escribe, no puede quedar vacío ni pasar de 50 caracteres.

**2. "'Email' no es una dirección de correo electrónico válida" (22 filas).** El Excel original trae varios correos juntos en una sola celda para el mismo contacto (ej. Mapfre: `"gerenciafemap@mapfre.com.co; \nproveedoreservi@mapfre.com.co"`, Hocol con hasta 5 correos separados por saltos de línea y " y "). La función `email_o_nota()` solo revisaba que hubiera un "@" en la celda -- una celda con varios correos juntos SÍ tiene "@", pasaba la revisión, pero el validador de .NET la rechaza porque no es UN correo válido. Se corrigió: ahora la celda se separa por cualquier espacio/coma/punto y coma (un correo real nunca tiene espacios, así que es un separador seguro; conectores sueltos como "y"/"-"/"a" quedan solos y se descartan al no matchear el patrón de correo), se valida cada pedazo por separado, se usa el primero válido como Email, y el resto queda listado en Notas para no perder el dato. Se aplicó al regenerar los 3 archivos -- de paso corrigió el mismo problema en 5 proveedores de `docs/schema/18_proveedores_reales.sql` que tenían el mismo defecto (ej. "cp.igarciaspf@gmail.com - isgago82@hotmail.com" antes se guardaba entero y roto; ahora se queda con el primer correo real y el resto en Notas) -- se volvió a probar contra la base de prueba: 138 proveedores, 150 teléfonos, 0 correos con formato inválido en la tabla.

Con ambos fixes, las 63 filas de `Clientes_importar.xlsx` deberían importar sin ningún error de validación.

## `docs/schema/21_cliente_proveedor_emails.sql` -- Cliente/Proveedor ahora aceptan varios correos (2026-09-06)

Al revisar los dos bugs de arriba, Alicia hizo una observación de fondo: `Cliente.Email`/`Proveedor.Email` era un solo campo de texto, pero un contacto real puede tener más de un correo (de hecho eso fue exactamente lo que rompió las 22 filas del bug #2). Decidió con ella:

- **Aplica a ambas entidades** (Cliente y Proveedor), no solo Cliente.
- **Lista simple, sin "principal"** -- exactamente el mismo patrón que ya usa `Telefono` hoy (`cliente_telefonos`/`proveedor_telefonos`): sin flag de cuál es "el correo de verdad", el orden de la lista es el único orden que existe.

Implementación (mismo mirror que Telefono en cada capa):

- **Core:** `Cliente.Email`/`Proveedor.Email` (string) desaparecen; se agregan `Cliente.Emails`/`Proveedor.Emails` (`ICollection<ClienteEmail>`/`ICollection<ProveedorEmail>`), entidades nuevas idénticas en forma a `ClienteTelefono`/`ProveedorTelefono` (`Id`, `{Entidad}Id`, `Email`, `Etiqueta?`).
- **DTOs:** `CreateClienteDto`/`CreateProveedorDto` cambian `Email` por `List<ClienteEmailDto>`/`List<ProveedorEmailDto>` (`Update*Dto`/`*ResponseDto` lo heredan automático). El Excel de importación sigue trayendo una sola columna "Email" -- se guarda como el único elemento de la lista; si hace falta un segundo correo, se agrega después desde el formulario.
- **Validadores:** la regla de formato + "ya está registrado" ahora corre por cada correo de la lista (`RuleForEach`), no una sola vez.
- **Base de datos:** `docs/schema/21_cliente_proveedor_emails.sql` -- crea `cliente_emails`/`proveedor_emails`, migra lo que hubiera en las columnas viejas, y las borra. Asimetría a propósito, igual que ya pasaba antes: `cliente_emails.email` SÍ tiene índice único (como lo tenía `clientes.email`); `proveedor_emails.email` NO (como tampoco lo tenía `proveedores.email` -- de hecho ya hay un caso real en los datos, "orquestaperezprado@gmail.com", con dos proveedores distintos usando el mismo correo, que este script preserva sin tocar).
- **Frontend:** `ClienteFormModal`/`ProviderFormModal` reemplazan el campo único "Correo" por la misma UI de chips + "Agregar" que ya usa Teléfono; `ClienteDetail`/`ProviderDetail` listan cada correo como su propia fila (con el botón de `mailto:`); las búsquedas de las listas de Clientes/Proveedores ahora buscan en todos los correos, no solo el primero.

**Probado de verdad contra la base de prueba** (no se pudo compilar el C# -- este entorno no tiene `dotnet` instalado y el proxy bloquea `dotnetcli.azureedge.net`, así que cada cambio se hizo espejando con cuidado el patrón ya existente de Telefono en cada capa, campo por campo): se corrió `schema/21` contra una copia con datos reales (138 proveedores, 100 con correo, incluyendo el caso duplicado real de arriba) más 3 clientes de prueba (uno sin correo). Resultado: 2 correos de cliente migrados, 100 de proveedor migrados (el duplicado real se conservó tal cual, sin romper nada porque no hay índice único ahí), las columnas viejas quedaron borradas, el borrado en cascada de un cliente se llevó su fila de `cliente_emails` con él, y una segunda corrida del script no duplicó ni falló nada (idempotente). También se confirmó que el índice único de `cliente_emails.email` sí rechaza un correo repetido entre dos clientes distintos, como debía.

**Pendiente de Alicia:** correr `docs/schema/21_cliente_proveedor_emails.sql` contra su base (dev primero, luego producción cuando esté conforme) -- y, como con `schema/20`, lo ideal es que en algún momento genere la migración real de EF Core (`dotnet ef migrations add AddClienteProveedorEmails`) para que el modelo quede formalizado en el historial de migraciones y no solo como parche manual.

## `docs/schema/22_rls_cliente_proveedor_emails.sql` -- bug real en `schema/21`: faltaba activar seguridad de fila (2026-09-06)

Al intentar importar `Clientes_importar.xlsx` después de aplicar `schema/21` (correos múltiples), la importación volvió a fallar -- esta vez con un toast negro genérico: "La operación no pudo completarse por una restricción de datos." Con el log real del backend (`dotnet run`) se confirmó la causa exacta, otra vez sin adivinar:

```
Npgsql.PostgresException (0x80004005): 42501: new row violates row-level security policy for table "cliente_emails"
```

`schema/21` creó las tablas `cliente_emails`/`proveedor_emails` con `CREATE TABLE` directo, pero se le olvidó el paso que sí tienen todas las demás tablas de negocio (ver `docs/schema/04_extras_supabase_post_migraciones.sql`): activar `ROW LEVEL SECURITY` y agregar la política `"solo_nexit_app"` que le da permiso al rol de la aplicación (`nexit_app`, ver `docs/schema/02_rol_aplicacion_minimo_privilegio.sql`) para operar sobre la tabla. Sin esa política, Postgres activa RLS con cero reglas -- lo que bloquea TODO acceso para cualquier rol que no sea el dueño de la tabla, aunque ese rol ya tenga el `GRANT` a nivel de tabla (son dos capas independientes, las dos hacen falta -- ver el comentario 5 de `schema/02`). Por eso el error solo aparecía al intentar guardar un correo, nunca antes.

**Probado de verdad, reproduciendo el bug exacto antes de escribir el fix:** se montó el rol `nexit_app` con los mismos privilegios de `schema/02` en una base de prueba, se le activó RLS a `cliente_emails` sin ninguna política (exactamente el estado en el que quedó por el bug de `schema/21`), y se confirmó que un `INSERT` como `nexit_app` falla con el mismo `42501` de arriba -- carácter por carácter igual al log real. Después se aplicó `docs/schema/22_rls_cliente_proveedor_emails.sql` (activa RLS -- ya estaba activo, no hace nada -- y agrega la política `"solo_nexit_app"` a las dos tablas nuevas, igual que ya tienen `cliente_telefonos`/`proveedor_telefonos`) y el mismo `INSERT` pasó a funcionar, tanto para `cliente_emails` como para `proveedor_emails`. Se corrió el script dos veces para confirmar que no falla si ya se había aplicado (idempotente).

**Pendiente de Alicia:** correr `docs/schema/22_rls_cliente_proveedor_emails.sql` contra su base (dev primero, luego producción) antes de volver a intentar la importación de Clientes. No hace falta volver a correr `schema/21` -- las tablas y los datos migrados ya están bien, solo les faltaba esta política.

## Bug real al importar `Proyectos_importar.xlsx`: fechas sin zona horaria (2026-09-06)

Primer intento de importar Proyectos, y falló de inmediato en la primera fila con un error 500/409 genérico. El log real del backend mostró la causa exacta:

```
System.ArgumentException: Cannot write DateTime with Kind=Unspecified to PostgreSQL type
'timestamp with time zone', only UTC is supported.
```

Causa: `ProyectosImportExporter.FechaOpcional()` lee las fechas ("Fecha de solicitud", "Fecha del evento", "Fecha de pago") directo de la celda de Excel con ClosedXML. Excel no guarda zona horaria en sus fechas, así que ClosedXML las entrega como `DateTime` con `Kind=Unspecified` -- y Npgsql se niega a escribir eso en una columna `timestamp with time zone` porque no sabe si es UTC, hora de Bogotá, etc. Esto nunca se había topado antes porque Clientes/Proveedores no importan ninguna fecha, solo Proyectos.

**No es un bug nuevo del código -- ya existía este mismo problema en otro lado y ya estaba resuelto ahí:** `Nexit.Core.Utils.SedeTimeZoneResolver.cs` ya tiene exactamente este arreglo (`DateTime.SpecifyKind(fechaUtc, DateTimeKind.Utc)`) para el mismo síntoma. Se aplicó el mismo patrón en `FechaOpcional()`: se marca la fecha leída del Excel como UTC explícitamente (no se le suma ni resta nada a la hora, solo se etiqueta -- es lo correcto aquí porque estas fechas son fechas de calendario, no un instante con hora exacta que dependa de zona horaria).

**Pendiente de verificar:** este cambio es de una sola línea y sigue al pie de la letra un patrón que ya existe y funciona en el mismo repositorio, pero no se pudo compilar en este entorno (no tiene `dotnet` instalado). Hace falta que Alicia reconstruya el backend y reintente la importación de `Proyectos_importar.xlsx` para confirmar que ya no truena.

## Regeneración completa de Clientes_importar.xlsx y Proyectos_importar.xlsx: el bug de fondo era que nunca se cruzaban entre sí (2026-09-06)

Después de arreglar la RLS (schema/22), la importación de Proyectos avanzó pero rechazó 527 de 555 filas con "el cliente X no existe". Investigando la causa real (no adivinando): el script de limpieza original (`limpieza_excel_nexit.py`) generaba `Clientes_importar.xlsx` desde una fuente (BD_CLIENTES MEX + hoja "Organizaciones" de Colombia) y `Proyectos_importar.xlsx` desde una fuente completamente distinta (`Seguimiento_de_proyectos_1.xlsx`, texto libre escrito a mano por distintas personas entre 2023 y 2026) **sin cruzarlas nunca entre sí**. Resultado: 133 nombres de cliente usados en Proyectos (BAT con 162 filas, Falabella, Medtronic, GeoPark, etc.) nunca existieron en la lista de 63 clientes -- esa lista de "BD_CLIENTES/Organizaciones" es más un CRM de prospección que un registro completo de todos los clientes históricos que sí tuvieron proyectos reales.

**Fix, en `limpieza_excel_nexit_v2.py`:**

1. Antes de armar Proyectos, se escanean las 8 hojas de `Seguimiento_de_proyectos_1.xlsx` buscando cada valor distinto de la columna CLIENTE. El que no calza (case-insensitive, igual que el backend) con ningún cliente ya conocido se agrega como cliente nuevo "mínimo" (solo Nombre + Estado="Activo"), marcado en Notas como creado automáticamente.
2. Se detectó además que varias filas escriben el nombre de una CAMPAÑA en la columna CLIENTE en vez del cliente solo -- ejemplo real confirmado contra el Excel: columna CLIENTE = "BAT MURO PREMIADOS", columna DESCRIPCIÓN PROYECTO = "MURO TRASFORMADO EN OFICINAS". Confirmado con Alicia: cuando un nombre nuevo empieza con el nombre de un cliente ya conocido + espacio, se colapsa en ese cliente base (el texto de más queda en la Nota del proyecto, no se pierde). Redujo los clientes nuevos de 119 a 86 (33 variantes colapsadas: "BAT MURO PREMIADOS"/"BAT MX"/"BAT 2024"/etc. → "BAT"; "Quala MEX"/"QUALA MX"/"QUALA NOVA" → "Quala"; etc.).
3. Cada fila de Proyectos se reescribe con el nombre EXACTO que quedó en Clientes (mismo criterio para ambos archivos), así el emparejamiento al importar siempre encuentra la fila -- ya no depende de que coincidan mayúsculas o redacción.
4. A propósito NO se fusionan variantes que no comparten el prefijo completo (ej. "MEDTRONC", un typo real al que le falta la "I", queda como cliente aparte de "Medtronic") -- fusionar texto libre con errores de tipeo es una decisión de negocio que le toca a Alicia, no a un script. Quedan listadas en `revisar_manualmente.xlsx` (pestaña Clientes) para fusionarlas a mano si corresponde.

**Resultado, verificado programáticamente (no solo "se ve bien"):** `Clientes_importar.xlsx` pasó de 63 a 149 filas (63 originales + 86 nuevas). `Proyectos_importar.xlsx` sigue con 555 filas con cliente (+ 3 sin cliente, permitido). Se comprobó que el 100% de las 555 encuentran su cliente por nombre exacto (case-insensitive) en el nuevo `Clientes_importar.xlsx` -- cero huérfanas. Se confirmó también que no hay ningún nombre de cliente duplicado (case-insensitive) en el archivo final.

Proveedores no se tocó: la fuente de datos es la misma (mismo archivo, mismo hash) y la lógica de proveedores no cambió, así que `Proveedores_importar.xlsx` y `docs/schema/18_proveedores_reales.sql` quedaron idénticos byte a byte a los ya entregados -- no hace falta volver a aplicarlos si ya se corrieron, pero de todas formas el script `18` es idempotente.

**Plan de recarga completa acordado con Alicia (ella decidió borrar y recargar los tres desde cero, no reconciliar):**

1. Correr `docs/schema/19_borrar_clientes_proveedores_proyectos.sql` (ya entregado y probado) -- borra clientes, proveedores y proyectos, no toca catálogos.
2. Importar `docs/datos/Clientes_importar.xlsx` (nuevo, 149 filas) desde el botón Importar de la pantalla Clientes.
3. Correr `docs/schema/18_proveedores_reales.sql` (sin cambios, 145 proveedores).
4. Importar `docs/datos/Proyectos_importar.xlsx` (nuevo, 558 filas) desde el botón Importar de la pantalla Proyectos -- ahora todos los clientes referenciados ya existen.

Queda pendiente que Alicia revise `docs/datos/revisar_manualmente.xlsx` (pestaña Clientes: los 86 clientes nuevos, para fusionar a mano los que reconozca como duplicados de otro ya existente por typo; pestaña Proyectos: 371 filas con estado inferido solo por % de avance en las hojas 2023-2025, que no traen un vocabulario de etapa confiable) cuando tenga tiempo -- no bloquea la importación, es limpieza posterior.

## Segunda ronda: Alicia pidió revisar "pestaña por pestaña" -- aparecieron 2 bugs reales más + hojas completas sin usar (2026-09-06)

Después de entregar la regeneración de arriba (149 clientes / 558 proyectos), Alicia hizo una observación válida: que no se había buscado en Internet para completar datos, y que no se habían revisado todas las pestañas de los 3 Excel a fondo. Se revisó pestaña por pestaña de los 3 archivos (`wb.sheetnames` + conteo de filas/columnas de cada una) para confirmar qué tanto de eso era cierto -- y sí lo era: aparecieron 2 bugs reales más en cómo se usaba la hoja "Organizaciones" (la que ya se usaba), y 2 hojas completas de Proyectos que nunca se habían leído.

**Bug 3 -- Estado de Organizaciones hardcodeado.** `limpieza_excel_nexit_v2.py` le ponía `"Estado": "Prospecto"` a **todas** las filas que venían de la hoja "Organizaciones", sin mirar la columna "Estado" real de esa hoja -- que sí trae 4 valores reales y significativos: Prospecto (84), Cliente Actual (27), Cliente Antiguo (9), Cliente Nuevo (5). Se corrigió mapeando esos 4 valores a los únicos 3 que el validador de Nexit acepta (`CreateClienteValidator.cs`: Activo/Prospecto/Inactivo) -- Cliente Actual/Nuevo → Activo, Cliente Antiguo → Inactivo, Prospecto → Prospecto.

**Bug 4 -- el filtro de País descartaba clientes reales, no solo los de otro país.** El script original solo tomaba filas de "Organizaciones" donde la palabra "colombia" apareciera en la columna País -- pero **68 de las 125 filas de esa hoja simplemente tienen el País en blanco** (no es que sean de otro país, el dato no se llenó). Entre esas 68 estaban BAT, Falabella, Banco Falabella, Medtronic, Davivienda, INDRA, SURA y Seguros Bolívar (FFA/Bienestar/Cena y reunión de gerentes/Convención Comercial) -- **todos marcados "Cliente Actual" en el CRM, con Account Manager, sitio web, correo, teléfono, dirección, ciudad y notas reales** -- que el filtro estaba tirando a la basura. Esos mismos nombres SÍ aparecían en Proyectos (por eso ya existían como clientes "auto-creados" en la primera entrega), pero se estaban creando vacíos en vez de usar los datos reales que ya existían en el CRM. Se corrigió: ya no se filtra por País -- se toman las 125 filas con Nombre, y el texto de País se normaliza aparte (Colombia/México cuando se puede resolver limpio; en blanco si no, ya que `Cliente.País` es opcional y nunca bloquea el import a diferencia de `Proveedor.Ciudad`).

**Enriquecimiento con la hoja "Contactos" (983 filas, nunca se había usado).** Cada organización puede tener varios contactos reales -- ej. 6 contactos distintos de BAT, todos con correo `@bat.com`. Como `Cliente.Email`/`Teléfono`/`Contacto` son un solo valor cada uno (se confirmó que ni `ClientesImportExporter.cs` ni `ProveedoresImportExporter.cs` soportan varios valores por fila en el import de Excel), se eligió **un contacto representativo por organización** (el que tuviera más datos completos: correo, luego teléfono, luego cargo) para rellenar SOLO los campos que la fila de Organizaciones hubiera dejado en blanco -- nunca pisa un dato que ya viniera de la fuente original. Resultado: 54 de los 132 clientes reales (de BD_CLIENTES + Organizaciones) se completaron con contacto/correo/teléfono real.

**2 hojas de Proyectos que nunca se habían leído: "Trafico proyectos Next" y "PROYECTOS 2023".** Comparando conjunto contra las 8 hojas que sí se usaban, aportan 35 y 26 filas cliente+descripción realmente nuevas (no repetidas en ninguna de las 8) -- se agregaron a la lista de hojas que arma Proyectos. Se descubrió además que estas 2 hojas comparten 1 fila idéntica entre sí (mismo proyecto "WOM CHILE / Evento corporativo en la Bebeta" en ambas -- "Trafico" parece ser el primer semestre de 2023 y "PROYECTOS 2023" el archivo completo del año, con ese primer semestre repetido al inicio), así que se agregó una deduplicación específica entre esas 2 hojas para no duplicar esa única fila. También traen el % de avance como texto con signo ("100%") en vez de fracción numérica (0.3) -- se generalizó `parse_porcentaje` para aceptar ambos formatos.

**Completar datos por Internet (pedido explícito de Alicia).** De los 132 clientes reales (BD_CLIENTES + Organizaciones, ya con los 2 bugs de arriba corregidos), 99 seguían sin sitio web ni dirección. Se investigó cada nombre por separado con búsqueda web (usando 5 subagentes en paralelo, cada uno con el contexto de que estos son clientes de una agencia de eventos/marketing en Colombia/México, para poder distinguir p.ej. "Falabella" el retailer real de cualquier otro resultado ambiguo) confirmando que el sitio encontrado fuera realmente el de esa empresa. Se completaron **85 clientes** con sitio web + sector + ciudad confirmados (ej. BAT → batcolombia.com, Bavaria → bavaria.co, Davivienda → davivienda.com). Los nombres demasiado genéricos o ambiguos para identificar una empresa real con certeza (`PLANNER`, `Stork`, `Iso`, `COLCOMERCIO`, `One Soluciones`, `EL LIBERTADOR`, `DAVIbank`) se dejaron **intactos a propósito, sin adivinar** -- meter un sitio web incorrecto es peor que dejarlo vacío.

**Sobre "102 clientes negociados":** se revisó a fondo buscando de dónde podría salir ese número exacto -- se buscó la palabra "negoc" (sin importar mayúsculas) en los 3 archivos completos (solo aparece en la tabla de definición de etapas E1-E6, nada relacionado a un conteo), se contaron los clientes con Estado distinto de Prospecto en Organizaciones (39), los contactos con clasificación "tipo cliente" en la hoja Contactos (36 organizaciones distintas), la unión de ambos criterios (63), y la hoja "Oportunidades" (solo 7 filas reales, no un conteo). Ninguno de esos cálculos da 102 exacto. No se encontró ese número en los datos fuente -- puede ser una cifra que Alicia maneja de otra fuente (un reporte de ventas, un tablero comercial) que no está en estos 3 Excel. Vale la pena que ella confirme directamente contra qué número quiere comparar, en vez de seguir adivinando cálculos sobre los mismos 3 archivos.

**Resultado final, verificado programáticamente igual que la primera vez:** `Clientes_importar.xlsx` pasó de 149 a **218 filas** (132 reales de BD_CLIENTES/Organizaciones, ya con Estado/País/contacto/web reales donde se pudo -- + 86 agregados automáticamente desde Proyectos, igual que antes). `Proyectos_importar.xlsx` pasó de 558 a **634 filas** (624 con cliente + 10 sin cliente, permitido). Se volvió a comprobar: 100% de las 624 filas con cliente encuentran su nombre exacto (case-insensitive) en Clientes, y cero nombres de cliente duplicados en el archivo final. Proveedores no se tocó (misma fuente, misma lógica) -- sigue idéntico a lo ya entregado.

El script de limpieza pasó a `limpieza_excel_nexit_v3.py` (mismo directorio de trabajo que las versiones anteriores).

**Plan de recarga completa, actualizado:**

1. Correr `docs/schema/19_borrar_clientes_proveedores_proyectos.sql` (ya entregado y probado) -- borra clientes, proveedores y proyectos, no toca catálogos. Si ya se había corrido con la entrega anterior (149/558), no pasa nada por correrlo de nuevo o por usar `schema/23`+`24` (clientes y proyectos por separado) si Proveedores ya quedó cargado y no se quiere volver a tocar.
2. Importar `docs/datos/Clientes_importar.xlsx` (nuevo, **218 filas**) desde el botón Importar de la pantalla Clientes.
3. Correr `docs/schema/18_proveedores_reales.sql` (sin cambios, 145 proveedores) -- si no se corrió ya.
4. Importar `docs/datos/Proyectos_importar.xlsx` (nuevo, **634 filas**) desde el botón Importar de la pantalla Proyectos.

Queda pendiente que Alicia revise `docs/datos/revisar_manualmente.xlsx` cuando tenga tiempo (88 clientes: los agregados automáticamente desde Proyectos, para fusionar a mano los que reconozca como typos de otro ya existente; 447 proyectos con estado inferido solo por % de avance, mismo caso ya conocido de las hojas 2023-2025) -- no bloquea la importación.

## Verificación real del import completo (Clientes + Proveedores + Proyectos) contra una base de prueba (2026-09-06)

Alicia pidió, con razón, que se confirmara que el import por el botón Importar funciona de verdad para las 3 entidades -- no solo que los nombres crucen entre archivos, sino que cada fila pase de verdad por las mismas reglas que corre el backend. Como este entorno no tiene `dotnet` instalado (no se puede levantar la API real), se hizo lo más cercano posible: se leyó el código exacto de `ClientesImportExporter.cs`, `ProveedoresImportExporter.cs`, `ProyectosImportExporter.cs` y sus 3 validadores (`CreateClienteValidator`, `CreateProveedorValidator`, `CrearProyectoValidator`), se replicó esa misma lógica fila por fila en un script (resolviendo País/Categoría/Ciudad/Cliente contra los catálogos reales, aplicando las mismas reglas de formato/longitud/duplicados), y se insertó de verdad cada fila contra una base de prueba con el mismo esquema, catálogos y restricciones (constraints, FKs, índices únicos) que tiene el esquema real -- para que cualquier cosa que la réplica en Python se le pasara por alto, la quedara atrapando la base de datos misma.

**Resultado de la simulación completa, en orden (Proveedores → Clientes → Proyectos):**

- Proveedores: **138/138 filas importadas sin ningún error.**
- Clientes: **218/218 filas importadas sin ningún error.**
- Proyectos: **634/634 filas importadas sin ningún error** (624 con cliente asignado, 10 sin cliente -- permitido).

Al simular esto de verdad aparecieron 2 bugs reales más en Proveedores que no se habían visto antes (el pipeline de Proveedores nunca se había probado con una simulación de import real, solo se había comparado contra el SQL ya entregado):

**Bug 5 -- 7 proveedores duplicados por nombre exacto nunca se fusionaban.** A diferencia de Clientes (que sí fusiona duplicados desde el principio), Proveedores nunca tuvo esa lógica. `BD_PROVEEDORES MEX` trae 7 empresas con el nombre exactamente repetido dos veces (ej. "OFIRENT", con el mismo teléfono y correo en ambas filas -- parece una fila duplicada por error de captura). Se agregó la misma fusión que ya tenía Clientes: se conserva la primera fila y se completa con lo que la segunda tuviera de más.

**Bug 6 -- 2 proveedores con nombre DISTINTO comparten el mismo correo.** "MAMBO / CUMBIA / SALSA" y "ORQUESTA PEREZ PRADO" (parecen el mismo acto musical anotado dos veces con nombres distintos) tienen el mismo correo. Esto sí rompía el import real: `CreateProveedorValidator` rechaza un correo que ya esté usado por OTRO proveedor (a través de `ExistsByEmailAsync`), aunque la tabla `proveedor_emails` no tenga un índice único que lo impida a nivel de base de datos -- la simulación completa confirmó que la 2ª fila fallaría con "El email ya está registrado". Como fusionar por nombre sería adivinar (igual que con "MEDTRONC" vs "Medtronic" en Clientes, que a propósito no se fusiona), se dejó el correo en la primera fila y se quitó de la segunda (queda en Notas + marcado en `revisar_manualmente.xlsx` por si Alicia reconoce que sí son la misma entidad).

Con estos 2 fixes, `Proveedores_importar.xlsx` pasó de 145 a **138 filas**, y se regeneró también `docs/schema/18_proveedores_reales.sql` desde la misma fuente para que ambos coincidan exactamente (antes solo se había verificado que el SQL coincidiera con el Excel viejo -- ahora los dos ya reflejan la fusión y el correo liberado).

**Por qué ahora sí hay Excel para Proveedores y no solo SQL:** Alicia pidió específicamente usar el botón Importar (Excel) para Proveedores en vez de correr `schema/18` a mano -- `docs/datos/Proveedores_importar.xlsx` ya estaba en el repo desde antes de esta ronda (mismo archivo, misma lógica, no había dejado de generarse), pero quedó desactualizado un momento mientras se investigaban estos 2 bugs; ya está corregido y vuelto a dejar en el repo con las 138 filas finales, listo para el botón Importar.

**Números finales de los 3 archivos, ya con todo lo de esta sesión (Organizaciones completo, Contactos, búsqueda web, las 2 hojas de Proyectos nuevas, y estos 2 fixes de Proveedores):**

- `docs/datos/Clientes_importar.xlsx`: 218 filas.
- `docs/datos/Proveedores_importar.xlsx`: 138 filas.
- `docs/datos/Proyectos_importar.xlsx`: 634 filas.

Los 3 se probaron de verdad, en el mismo orden en que se importarían en producción (Proveedores y Clientes primero, sin depender uno del otro; Proyectos al final, ya que necesita que los clientes ya existan) y dieron 0 errores en cada uno.
