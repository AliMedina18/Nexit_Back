# 33 — Catálogo de etapas de cliente (E1-E6)

## Por qué

Al analizar los 3 Excel que la usuaria recibió de su compañera (BD_CLIENTES_Y_PROVEEDORES, Base_de_Datos_Next_2026, Seguimiento_de_proyectos) contra el esquema actual, apareció una hoja — "Etapas clientes", dentro de Seguimiento_de_proyectos — que documenta el proceso comercial real de Next en 6 etapas (E1 a E6), cada una con qué hacer, cómo hacerlo, quién lo hace (Dep. Comercial / Dep. Creativo / Dep. Administrativo) y qué % del proceso representa (10%, 30%, 60%, 80%, 90%, 100%).

La usuaria marcó explícitamente esto como lo más valioso de todo el análisis, y confirmó agregarlo (opción A del análisis, no la B de no tocar nada).

**El vacío que llenaba:** `Cliente.Estado` ya existía (Activo/Prospecto/Inactivo) pero es demasiado grueso — no distingue "nunca contactado" de "reunión de reconocimiento ya hecha, esperando brief". Las etapas E3 en adelante (brief recibido, propuesta, cierre, facturación) ya estaban bien cubiertas por `Proyecto.PorcentajeAvance`/`EstadoId` y por las áreas Creativo/Comercial/Administrativo de `ProyectoSeguimiento` — que ya coincidían con esta misma hoja del Excel casi campo a campo, señal de que el diseño de Proyecto ya se había pensado con esto en mente. Lo que faltaba era E1 (Contacto inicial) y E2 (Reconocimiento), que pasan ANTES de que exista un Proyecto.

## Diseño

Mismo patrón que `fases_proyecto`/`estados_proyecto` (`Nexit.Core.Entities`), pero sin el nivel de "fase" — es una sola lista plana de 6 etapas, no una jerarquía de 2 niveles:

- `EtapaCliente` (tabla `etapas_cliente`): `Id`, `Nombre`, `Orden` (ambos únicos), `PorcentajeProceso` (0-100, con check constraint `ck_etapas_cliente_porcentaje`).
- `Cliente.EtapaId` (`Guid?`, FK `Restrict` a `etapas_cliente`) — **opcional a propósito**: los clientes que ya existen (y los que se importen del Excel) no tienen forma confiable de inferir su etapa comercial, así que quedan sin etapa hasta que alguien la asigne a mano desde la ficha del cliente. No se intentó adivinarla en la carga masiva.

Seed inicial (`docs/schema/seed_geografia_categorias_estados.sql`), tomado literal de la hoja "Etapas clientes":

| Orden | Nombre | % proceso |
|---|---|---|
| 1 | Contacto inicial | 10 |
| 2 | Reconocimiento | 30 |
| 3 | Oportunidad de negocio | 60 |
| 4 | Cierre y pre-producción | 80 |
| 5 | Finalización | 90 |
| 6 | Facturación | 100 |

## Wiring

Mismo patrón que cualquier otro catálogo simple (ver `docs/02`): `ICatalogosRepository`/`CatalogosRepository` ganan `GetEtapasClienteAsync`/`GetEtapaClienteAsync`; `ICatalogosService`/`CatalogosService` ganan `GetEtapasClienteAsync`/`CrearEtapaClienteAsync`/`ActualizarEtapaClienteAsync` (con su propia validación de `PorcentajeProceso` 0-100, `ValidarPorcentaje`) y el `case "etapas-cliente"` en `EliminarAsync`; `CatalogosController` gana `GET/POST/PUT /api/catalogos/etapas-cliente` (mismas políticas que `estados-proyecto`: lectura abierta, escritura `AdminOrAbove`). `CreateClienteDto`/`ClienteResponseDto`/`ClienteMapper` ganan `EtapaId` — sin validación de FK en `CreateClienteValidator`/`UpdateClienteValidator`, igual que `PaisId`/`RegionId`/`CiudadId` (ningún catálogo geográfico de Cliente se valida ahí tampoco).

El frontend (`Nexit_Front`) también quedó conectado en esta misma pasada: `types/api.ts` gana `EtapaCliente`/`EtapaClienteInput` y `"etapas-cliente"` en `CatalogoTipo`; `catalogos-service.ts`/`catalogos-store.ts` ganan `etapasCliente` (mismo patrón que `estadosProyecto`); `ClienteFormModal` gana un Dropdown de etapa (sección "Quién es", opcional, muestra `Nombre (Porcentaje%)`); `ClienteCard`/`ClienteDetail` muestran la etapa como badge junto al estado, cuando el cliente tiene una asignada.

**Pendiente, a propósito, hasta que se ejecute en la máquina de la usuaria (no hay entorno .NET disponible donde se escribió este cambio):** generar la migración con `dotnet ef migrations add AddEtapaClienteCatalogo --project src/Nexit.Infrastructure --startup-project src/Nexit.API`, `dotnet build`, `dotnet test`, y aplicar con `dotnet ef database update` contra la base de datos LOCAL de pruebas primero — nunca contra producción sin confirmación explícita de la usuaria (ver `docs/29` para el mismo procedimiento con un cambio de columna similar).

**No incluido en esta pasada:** las columnas de importación/exportación masiva (`docs/31`) — el Excel histórico no trae un dato confiable de en qué etapa está cada cliente, así que no tenía sentido forzar una columna "Etapa" en el importador todavía.
