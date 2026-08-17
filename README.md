# Nexit Backend

API REST para la gestión de clientes, proveedores y proyectos de Next. Está construida con .NET 8, Clean Architecture, Entity Framework Core y PostgreSQL/Supabase.

## Requisitos

- .NET SDK 8
- PostgreSQL o un proyecto de Supabase

## Inicio rápido

1. Instala PostgreSQL local y crea una base vacía llamada `nexit_dev`.
2. Copia `src/Nexit.API/appsettings.Development.example.json` como `appsettings.Development.json` y configura tu contraseña local de PostgreSQL.
3. Ejecuta `dotnet restore`.
4. Aplica la migración local: `dotnet ef database update --project src/Nexit.Infrastructure --startup-project src/Nexit.Infrastructure`.
5. Ejecuta el seed de catálogos: `psql -h localhost -U postgres -d nexit_dev -f docs/schema/seed_geografia_categorias_estados.sql`.
6. Ejecuta `dotnet run --project src/Nexit.API`.

### Ambientes

`Development` usa PostgreSQL local mediante `appsettings.Development.json` y las migraciones de EF Core en `src/Nexit.Infrastructure/Migrations`.

`Production` apunta a Supabase: copia `src/Nexit.API/appsettings.Production.example.json` como `appsettings.Production.json` y configura las credenciales reales fuera de Git. No apliques la migración inicial sobre un proyecto Supabase que ya fue creado con `docs/schema/nexus_schema_v2.sql`; ese esquema incluye los ENUM, triggers, RLS y la referencia a `auth.users` específicos de Supabase.

Swagger queda disponible en `/swagger`. Los endpoints de clientes requieren un JWT emitido por Supabase. Los catálogos se consultan en `/api/catalogos/*`; crear, editar o eliminar requiere que el JWT incluya `app_role=admin`, `user_role=admin` o el rol `admin`.

## Catálogos

La API incluye catálogos de países, regiones, ciudades, categorías de proveedor, servicios, fases y estados de proyecto. Las rutas geográficas reciben filtros por query string: `GET /api/catalogos/regiones?paisId={id}` y `GET /api/catalogos/ciudades?regionId={id}`. Los estados pueden filtrarse con `GET /api/catalogos/estados-proyecto?fase={1|2|3}`.

## Estructura

- `src/Nexit.Core`: entidades y contratos de dominio.
- `src/Nexit.Application`: DTOs, validación, mapeo y casos de uso.
- `src/Nexit.Infrastructure`: EF Core, repositorios y persistencia.
- `src/Nexit.API`: API HTTP, JWT, Swagger y middleware.
- `tests/Nexit.Tests`: pruebas unitarias del flujo de clientes.
