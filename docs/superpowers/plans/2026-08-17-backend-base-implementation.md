# Backend Nexit - Clean Architecture Base Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Construir la base del backend ASP.NET Core 8 con Clean Architecture, incluyendo estructura de proyectos, DbContext, configuración, middleware, validación, mapeo y tests.

**Architecture:** Clean Architecture modular con 4 proyectos (Core, Application, Infrastructure, API). Core define contratos, Application DTOs/validators/use cases, Infrastructure EF Core y repositorios, API controllers y middleware. Data flow: Controller → Validator (FluentValidation) → UseCase → Repository → EF DbContext.

**Tech Stack:** .NET 8, ASP.NET Core, EF Core 8, Npgsql (PostgreSQL/Supabase), FluentValidation, AutoMapper, Serilog, Swashbuckle (Swagger), xUnit, Moq.

---

## Task 1: Crear solución y estructura de carpetas

**Files:**
- Create: `Nexit.sln`
- Create: `src/` (carpeta)
- Create: `tests/` (carpeta)
- Create: `.gitignore`
- Create: `README.md`
- Create: `.env.example`

- [ ] **Step 1: Crear carpeta src y tests**

```bash
cd c:\Users\USUARIO\Documents\Github\Nexit_Back
mkdir src
mkdir tests
```

- [ ] **Step 2: Crear proyecto Nexit.Core**

```bash
cd src
dotnet new classlib -n Nexit.Core
```

- [ ] **Step 3: Crear proyecto Nexit.Application**

```bash
dotnet new classlib -n Nexit.Application
```

- [ ] **Step 4: Crear proyecto Nexit.Infrastructure**

```bash
dotnet new classlib -n Nexit.Infrastructure
```

- [ ] **Step 5: Crear proyecto Nexit.API**

```bash
cd ..
dotnet new webapi -n Nexit.API
```

- [ ] **Step 6: Crear proyecto de tests**

```bash
cd ..\tests
dotnet new xunit -n Nexit.Tests
```

- [ ] **Step 7: Crear solución y agregar proyectos**

```bash
cd c:\Users\USUARIO\Documents\Github\Nexit_Back
dotnet new sln -n Nexit
dotnet sln add src/Nexit.Core/Nexit.Core.csproj
dotnet sln add src/Nexit.Application/Nexit.Application.csproj
dotnet sln add src/Nexit.Infrastructure/Nexit.Infrastructure.csproj
dotnet sln add src/Nexit.API/Nexit.API.csproj
dotnet sln add tests/Nexit.Tests/Nexit.Tests.csproj
```

- [ ] **Step 8: Crear referencias entre proyectos**

```bash
cd src/Nexit.Application
dotnet add reference ../Nexit.Core/Nexit.Core.csproj

cd ../Nexit.Infrastructure
dotnet add reference ../Nexit.Core/Nexit.Core.csproj
dotnet add reference ../Nexit.Application/Nexit.Application.csproj

cd ../Nexit.API
dotnet add reference ../Nexit.Core/Nexit.Core.csproj
dotnet add reference ../Nexit.Application/Nexit.Application.csproj
dotnet add reference ../Nexit.Infrastructure/Nexit.Infrastructure.csproj

cd ../../tests/Nexit.Tests
dotnet add reference ../../src/Nexit.Core/Nexit.Core.csproj
dotnet add reference ../../src/Nexit.Application/Nexit.Application.csproj
dotnet add reference ../../src/Nexit.Infrastructure/Nexit.Infrastructure.csproj
dotnet add reference ../../src/Nexit.API/Nexit.API.csproj
```

- [ ] **Step 9: Crear .gitignore**

```
# .gitignore
bin/
obj/
.vs/
.vscode/
*.user
*.suo
appsettings.Production.json
.env
.env.local
logs/
*.log
.DS_Store
.idea/
*.swp
```

- [ ] **Step 10: Crear .env.example**

```
DATABASE_URL=Host=db.supabase.co;Database=postgres;Username=postgres;Password=YOUR_PASSWORD;Port=5432;SSL Mode=Require;
SUPABASE_URL=https://YOUR_PROJECT.supabase.co
SUPABASE_ANON_KEY=YOUR_ANON_KEY
SUPABASE_SERVICE_ROLE_KEY=YOUR_SERVICE_ROLE_KEY
JWT_AUTHORITY=https://YOUR_PROJECT.supabase.co/auth/v1
JWT_AUDIENCE=authenticated
ASPNETCORE_ENVIRONMENT=Development
```

- [ ] **Step 11: Crear README.md**

```markdown
# Nexit Backend

Sistema de gestión de información para Next (agencia experiencial). Backend REST API en ASP.NET Core 8.

## Setup

### Requisitos
- .NET 8 SDK
- PostgreSQL/Supabase
- Git

### Instalación

1. Clonar repositorio
   \`\`\`bash
   git clone https://github.com/your-org/Nexit_Back.git
   cd Nexit_Back
   \`\`\`

2. Copiar variables de entorno
   \`\`\`bash
   cp .env.example .env
   # Editar .env con tus valores de Supabase
   \`\`\`

3. Restaurar dependencias
   \`\`\`bash
   dotnet restore
   \`\`\`

4. Aplicar migraciones
   \`\`\`bash
   cd src/Nexit.API
   dotnet ef database update --project ../Nexit.Infrastructure
   \`\`\`

5. Ejecutar API
   \`\`\`bash
   dotnet run
   \`\`\`

API estará en: https://localhost:5001

Swagger en: https://localhost:5001/swagger

### Desarrollo

Ejecutar tests:
\`\`\`bash
dotnet test
\`\`\`

Crear migración:
\`\`\`bash
dotnet ef migrations add MigrationName --project src/Nexit.Infrastructure --startup-project src/Nexit.API
\`\`\`

## Arquitectura

- **Nexit.Core**: Domain entities, enums, exceptions, interfaces
- **Nexit.Application**: DTOs, validators, use cases, mappers
- **Nexit.Infrastructure**: EF Core DbContext, repositories, UnitOfWork
- **Nexit.API**: Controllers, middleware, configuration

## Autores

Diseño colaborativo agosto 2026.
\`\`\`

- [ ] **Step 12: Crear estructura de carpetas en Core**

```bash
cd src/Nexit.Core
mkdir Entities
mkdir Enums
mkdir Exceptions
mkdir Interfaces
mkdir ValueObjects
```

- [ ] **Step 13: Crear estructura de carpetas en Application**

```bash
cd ../Nexit.Application
mkdir DTOs
mkdir Validators
mkdir MappingProfiles
mkdir UseCases
mkdir Exceptions
```

- [ ] **Step 14: Crear estructura de carpetas en Infrastructure**

```bash
cd ../Nexit.Infrastructure
mkdir Data
mkdir Repositories
mkdir UnitOfWork
mkdir Auth
```

- [ ] **Step 15: Crear estructura de carpetas en API**

```bash
cd ../Nexit.API
mkdir Controllers
mkdir Middleware
mkdir Extensions
```

- [ ] **Step 16: Commit inicial**

```bash
cd c:\Users\USUARIO\Documents\Github\Nexit_Back
git add .
git commit -m "chore: scaffold .NET solution structure with 4 projects"
```

---

## Task 2: Crear Entities en Nexit.Core

**Files:**
- Create: `src/Nexit.Core/Entities/BaseEntity.cs`
- Create: `src/Nexit.Core/Entities/Usuario.cs`
- Create: `src/Nexit.Core/Entities/Cliente.cs`
- Create: `src/Nexit.Core/Entities/ClienteTelefono.cs`
- Create: `src/Nexit.Core/Entities/Proveedor.cs`
- Create: `src/Nexit.Core/Entities/ProveedorTelefono.cs`
- Create: `src/Nexit.Core/Entities/ProveedorServicio.cs`
- Create: `src/Nexit.Core/Entities/Servicio.cs`
- Create: `src/Nexit.Core/Entities/Proyecto.cs`
- Create: `src/Nexit.Core/Entities/ProyectoEquipo.cs`
- Create: `src/Nexit.Core/Entities/ProyectoProveedor.cs`
- Create: `src/Nexit.Core/Entities/ProyectoSeguimiento.cs`

- [ ] **Step 1: Crear BaseEntity**

```csharp
// src/Nexit.Core/Entities/BaseEntity.cs
namespace Nexit.Core.Entities;

public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
}
```

- [ ] **Step 2: Crear Usuario**

```csharp
// src/Nexit.Core/Entities/Usuario.cs
namespace Nexit.Core.Entities;

public class Usuario : BaseEntity
{
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Rol { get; set; } = "miembro"; // admin, manager, miembro
    public string? Iniciales { get; set; }
    public bool Activo { get; set; } = true;
    
    // Navigation
    public ICollection<Cliente> ClientesCreados { get; set; } = new List<Cliente>();
    public ICollection<Proveedor> ProveedoresCreados { get; set; } = new List<Proveedor>();
    public ICollection<Proyecto> ProyectosCreados { get; set; } = new List<Proyecto>();
    public ICollection<ProyectoSeguimiento> SeguimientosEscritos { get; set; } = new List<ProyectoSeguimiento>();
}
```

- [ ] **Step 3: Crear Cliente**

```csharp
// src/Nexit.Core/Entities/Cliente.cs
namespace Nexit.Core.Entities;

public class Cliente : BaseEntity
{
    public string Nombre { get; set; } = string.Empty;
    public string? Sector { get; set; }
    public string? Ciudad { get; set; }
    public string? Direccion { get; set; }
    public string? Web { get; set; }
    public string? Contacto { get; set; }
    public string? CargoContacto { get; set; }
    public string? Email { get; set; }
    public string? ValorReferencia { get; set; }
    public string? Notas { get; set; }
    
    // Navigation
    public ICollection<ClienteTelefono> Telefonos { get; set; } = new List<ClienteTelefono>();
    public ICollection<Proyecto> Proyectos { get; set; } = new List<Proyecto>();
}
```

- [ ] **Step 4: Crear ClienteTelefono**

```csharp
// src/Nexit.Core/Entities/ClienteTelefono.cs
namespace Nexit.Core.Entities;

public class ClienteTelefono : BaseEntity
{
    public Guid ClienteId { get; set; }
    public string Telefono { get; set; } = string.Empty;
    public string? Etiqueta { get; set; } // Principal, WhatsApp, Oficina, etc.
    
    // Navigation
    public Cliente Cliente { get; set; } = null!;
}
```

- [ ] **Step 5: Crear Proveedor**

```csharp
// src/Nexit.Core/Entities/Proveedor.cs
namespace Nexit.Core.Entities;

public class Proveedor : BaseEntity
{
    public string Nombre { get; set; } = string.Empty;
    public Guid PaisId { get; set; }
    public Guid? RegionId { get; set; }
    public Guid? CiudadId { get; set; }
    public Guid CategoriaId { get; set; }
    public string Estado { get; set; } = "Activo"; // Activo, En evaluación, Pausado, Bloqueado
    public string? Contacto { get; set; }
    public string? CargoContacto { get; set; }
    public string? Email { get; set; }
    public string? Web { get; set; }
    public string? Direccion { get; set; }
    public int? Aforo { get; set; }
    public string? CostoReferencia { get; set; }
    public int? Score { get; set; } // 1-5
    public string? Presupuesto { get; set; } // $ Bajo, $$ Medio, etc.
    public string? Cobertura { get; set; } // Solo ciudad, Regional, Nacional, Internacional
    public string? Notas { get; set; }
    
    // Navigation
    public ICollection<ProveedorTelefono> Telefonos { get; set; } = new List<ProveedorTelefono>();
    public ICollection<ProveedorServicio> Servicios { get; set; } = new List<ProveedorServicio>();
    public ICollection<ProyectoProveedor> Proyectos { get; set; } = new List<ProyectoProveedor>();
}
```

- [ ] **Step 6: Crear ProveedorTelefono**

```csharp
// src/Nexit.Core/Entities/ProveedorTelefono.cs
namespace Nexit.Core.Entities;

public class ProveedorTelefono : BaseEntity
{
    public Guid ProveedorId { get; set; }
    public string Telefono { get; set; } = string.Empty;
    public string? Etiqueta { get; set; }
    
    // Navigation
    public Proveedor Proveedor { get; set; } = null!;
}
```

- [ ] **Step 7: Crear Servicio**

```csharp
// src/Nexit.Core/Entities/Servicio.cs
namespace Nexit.Core.Entities;

public class Servicio : BaseEntity
{
    public string Nombre { get; set; } = string.Empty;
    
    // Navigation
    public ICollection<ProveedorServicio> Proveedores { get; set; } = new List<ProveedorServicio>();
}
```

- [ ] **Step 8: Crear ProveedorServicio**

```csharp
// src/Nexit.Core/Entities/ProveedorServicio.cs
namespace Nexit.Core.Entities;

public class ProveedorServicio
{
    public Guid ProveedorId { get; set; }
    public Guid ServicioId { get; set; }
    
    // Navigation
    public Proveedor Proveedor { get; set; } = null!;
    public Servicio Servicio { get; set; } = null!;
}
```

- [ ] **Step 9: Crear Proyecto**

```csharp
// src/Nexit.Core/Entities/Proyecto.cs
namespace Nexit.Core.Entities;

public class Proyecto : BaseEntity
{
    public string Nombre { get; set; } = string.Empty;
    public Guid? ClienteId { get; set; }
    public string? ContactoProyecto { get; set; }
    public string? TipoProyecto { get; set; } // Corporativo, Evento social
    public string? Prioridad { get; set; } // Alta, Media, Baja
    public string? Ciudad { get; set; }
    public string? SedeNext { get; set; }
    public DateTime? FechaSolicitud { get; set; }
    public DateTime? FechaEvento { get; set; }
    public Guid EstadoId { get; set; }
    public int PorcentajeAvance { get; set; } = 0;
    public string EstadoBrief { get; set; } = "Pendiente por enviar";
    public string PropuestaEstado { get; set; } = "No enviada";
    public string? NumeroFactura { get; set; }
    public bool Pagado { get; set; } = false;
    public DateTime? FechaPago { get; set; }
    public string? Notas { get; set; }
    
    // Navigation
    public Cliente? Cliente { get; set; }
    public ICollection<ProyectoEquipo> Equipo { get; set; } = new List<ProyectoEquipo>();
    public ICollection<ProyectoProveedor> Proveedores { get; set; } = new List<ProyectoProveedor>();
    public ICollection<ProyectoSeguimiento> Seguimiento { get; set; } = new List<ProyectoSeguimiento>();
}
```

- [ ] **Step 10: Crear ProyectoEquipo**

```csharp
// src/Nexit.Core/Entities/ProyectoEquipo.cs
namespace Nexit.Core.Entities;

public class ProyectoEquipo : BaseEntity
{
    public Guid ProyectoId { get; set; }
    public string Rol { get; set; } = string.Empty; // Ejecutivo, Comercial, Administrativo, Diseñador 3D, etc.
    public string Nombre { get; set; } = string.Empty;
    
    // Navigation
    public Proyecto Proyecto { get; set; } = null!;
}
```

- [ ] **Step 11: Crear ProyectoProveedor**

```csharp
// src/Nexit.Core/Entities/ProyectoProveedor.cs
namespace Nexit.Core.Entities;

public class ProyectoProveedor
{
    public Guid ProyectoId { get; set; }
    public Guid ProveedorId { get; set; }
    
    // Navigation
    public Proyecto Proyecto { get; set; } = null!;
    public Proveedor Proveedor { get; set; } = null!;
}
```

- [ ] **Step 12: Crear ProyectoSeguimiento**

```csharp
// src/Nexit.Core/Entities/ProyectoSeguimiento.cs
namespace Nexit.Core.Entities;

public class ProyectoSeguimiento : BaseEntity
{
    public Guid ProyectoId { get; set; }
    public Guid? AutorId { get; set; }
    public string Area { get; set; } = "General"; // General, Creativo, Comercial, Administrativo
    public DateTime Fecha { get; set; } = DateTime.UtcNow;
    public string Nota { get; set; } = string.Empty;
    
    // Navigation
    public Proyecto Proyecto { get; set; } = null!;
    public Usuario? Autor { get; set; }
}
```

- [ ] **Step 13: Commit entities**

```bash
cd c:\Users\USUARIO\Documents\Github\Nexit_Back
git add src/Nexit.Core/Entities/
git commit -m "feat(core): add domain entities (Usuario, Cliente, Proveedor, Proyecto, etc.)"
```

---

## Task 3: Crear Enums y Exceptions en Nexit.Core

**Files:**
- Create: `src/Nexit.Core/Enums/ProvedorEstado.cs`
- Create: `src/Nexit.Core/Enums/PrioridadProyecto.cs`
- Create: `src/Nexit.Core/Enums/RolEquipo.cs`
- Create: `src/Nexit.Core/Enums/RolUsuario.cs`
- Create: `src/Nexit.Core/Enums/TipoProyecto.cs`
- Create: `src/Nexit.Core/Enums/EstadoBrief.cs`
- Create: `src/Nexit.Core/Enums/AreaSeguimiento.cs`
- Create: `src/Nexit.Core/Enums/PropuestaEstado.cs`
- Create: `src/Nexit.Core/Exceptions/EntityNotFoundException.cs`
- Create: `src/Nexit.Core/Exceptions/BusinessRuleException.cs`
- Create: `src/Nexit.Core/Exceptions/InvalidOperationException.cs`
- Create: `src/Nexit.Core/Interfaces/IRepository.cs`
- Create: `src/Nexit.Core/Interfaces/IClienteRepository.cs`
- Create: `src/Nexit.Core/Interfaces/IUnitOfWork.cs`

- [ ] **Step 1: Crear ProvedorEstado enum**

```csharp
// src/Nexit.Core/Enums/ProvedorEstado.cs
namespace Nexit.Core.Enums;

public enum ProvedorEstado
{
    Activo,
    EnEvaluacion,
    Pausado,
    Bloqueado
}
```

- [ ] **Step 2: Crear PrioridadProyecto enum**

```csharp
// src/Nexit.Core/Enums/PrioridadProyecto.cs
namespace Nexit.Core.Enums;

public enum PrioridadProyecto
{
    Alta,
    Media,
    Baja
}
```

- [ ] **Step 3: Crear RolEquipo enum**

```csharp
// src/Nexit.Core/Enums/RolEquipo.cs
namespace Nexit.Core.Enums;

public enum RolEquipo
{
    Ejecutivo,
    Comercial,
    Administrativo,
    Diseñador3D,
    DiseñadorGrafico
}
```

- [ ] **Step 4: Crear RolUsuario enum**

```csharp
// src/Nexit.Core/Enums/RolUsuario.cs
namespace Nexit.Core.Enums;

public enum RolUsuario
{
    Admin,
    Manager,
    Miembro
}
```

- [ ] **Step 5: Crear TipoProyecto enum**

```csharp
// src/Nexit.Core/Enums/TipoProyecto.cs
namespace Nexit.Core.Enums;

public enum TipoProyecto
{
    Corporativo,
    EventoSocial
}
```

- [ ] **Step 6: Crear EstadoBrief enum**

```csharp
// src/Nexit.Core/Enums/EstadoBrief.cs
namespace Nexit.Core.Enums;

public enum EstadoBrief
{
    PendientePorEnviar,
    EntregadoEsperandoRespuesta,
    RequiereAjustes,
    Aprobado
}
```

- [ ] **Step 7: Crear AreaSeguimiento enum**

```csharp
// src/Nexit.Core/Enums/AreaSeguimiento.cs
namespace Nexit.Core.Enums;

public enum AreaSeguimiento
{
    General,
    Creativo,
    Comercial,
    Administrativo
}
```

- [ ] **Step 8: Crear PropuestaEstado enum**

```csharp
// src/Nexit.Core/Enums/PropuestaEstado.cs
namespace Nexit.Core.Enums;

public enum PropuestaEstado
{
    NoEnviada,
    EnProceso,
    Enviada
}
```

- [ ] **Step 9: Crear EntityNotFoundException**

```csharp
// src/Nexit.Core/Exceptions/EntityNotFoundException.cs
namespace Nexit.Core.Exceptions;

public class EntityNotFoundException : Exception
{
    public EntityNotFoundException(string entityName, Guid id)
        : base($"Entidad '{entityName}' con ID '{id}' no encontrada.") { }
    
    public EntityNotFoundException(string message) : base(message) { }
}
```

- [ ] **Step 10: Crear BusinessRuleException**

```csharp
// src/Nexit.Core/Exceptions/BusinessRuleException.cs
namespace Nexit.Core.Exceptions;

public class BusinessRuleException : Exception
{
    public BusinessRuleException(string message) : base(message) { }
}
```

- [ ] **Step 11: Crear InvalidOperationException**

```csharp
// src/Nexit.Core/Exceptions/InvalidOperationException.cs
namespace Nexit.Core.Exceptions;

public class InvalidOperationException : Exception
{
    public InvalidOperationException(string message) : base(message) { }
}
```

- [ ] **Step 12: Crear IRepository<T>**

```csharp
// src/Nexit.Core/Interfaces/IRepository.cs
namespace Nexit.Core.Interfaces;

public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> FindAsync(Func<T, bool> predicate);
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(Guid id);
}
```

- [ ] **Step 13: Crear IClienteRepository**

```csharp
// src/Nexit.Core/Interfaces/IClienteRepository.cs
using Nexit.Core.Entities;

namespace Nexit.Core.Interfaces;

public interface IClienteRepository : IRepository<Cliente>
{
    Task<bool> ExistsByEmailAsync(string email);
    Task<bool> ExistsByNombreAsync(string nombre);
    Task<Cliente?> GetByEmailAsync(string email);
    Task<IEnumerable<Cliente>> GetByCiudadAsync(string ciudad);
}
```

- [ ] **Step 14: Crear IUnitOfWork**

```csharp
// src/Nexit.Core/Interfaces/IUnitOfWork.cs
using Nexit.Core.Entities;

namespace Nexit.Core.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IRepository<Usuario> Usuarios { get; }
    IClienteRepository Clientes { get; }
    IRepository<Proveedor> Proveedores { get; }
    IRepository<Proyecto> Proyectos { get; }
    
    Task<int> SaveChangesAsync();
    Task<bool> BeginTransactionAsync();
    Task<bool> CommitTransactionAsync();
    Task<bool> RollbackTransactionAsync();
}
```

- [ ] **Step 15: Commit core layer**

```bash
cd c:\Users\USUARIO\Documents\Github\Nexit_Back
git add src/Nexit.Core/Enums/ src/Nexit.Core/Exceptions/ src/Nexit.Core/Interfaces/
git commit -m "feat(core): add enums, exceptions, and repository interfaces"
```

---

## Task 4: Agregar NuGet packages a Nexit.Application

**Files:**
- Modify: `src/Nexit.Application/Nexit.Application.csproj`

- [ ] **Step 1: Agregar packages a Application**

```bash
cd src/Nexit.Application
dotnet add package AutoMapper --version 13.0.1
dotnet add package AutoMapper.Extensions.Microsoft.DependencyInjection --version 12.0.1
dotnet add package FluentValidation --version 11.9.2
dotnet add package FluentValidation.DependencyInjectionExtensions --version 11.9.2
```

---

## Task 5: Crear DTOs en Nexit.Application

**Files:**
- Create: `src/Nexit.Application/DTOs/Clientes/CreateClienteDto.cs`
- Create: `src/Nexit.Application/DTOs/Clientes/UpdateClienteDto.cs`
- Create: `src/Nexit.Application/DTOs/Clientes/ClienteResponseDto.cs`
- Create: `src/Nexit.Application/DTOs/Clientes/ClienteTelefonoDto.cs`

- [ ] **Step 1: Crear ClienteTelefonoDto**

```csharp
// src/Nexit.Application/DTOs/Clientes/ClienteTelefonoDto.cs
namespace Nexit.Application.DTOs.Clientes;

public class ClienteTelefonoDto
{
    public Guid? Id { get; set; }
    public string Telefono { get; set; } = string.Empty;
    public string? Etiqueta { get; set; }
}
```

- [ ] **Step 2: Crear CreateClienteDto**

```csharp
// src/Nexit.Application/DTOs/Clientes/CreateClienteDto.cs
namespace Nexit.Application.DTOs.Clientes;

public class CreateClienteDto
{
    public string Nombre { get; set; } = string.Empty;
    public string? Sector { get; set; }
    public string? Ciudad { get; set; }
    public string? Direccion { get; set; }
    public string? Web { get; set; }
    public string? Contacto { get; set; }
    public string? CargoContacto { get; set; }
    public string? Email { get; set; }
    public string? ValorReferencia { get; set; }
    public string? Notas { get; set; }
    public List<ClienteTelefonoDto> Telefonos { get; set; } = new();
}
```

- [ ] **Step 3: Crear UpdateClienteDto**

```csharp
// src/Nexit.Application/DTOs/Clientes/UpdateClienteDto.cs
namespace Nexit.Application.DTOs.Clientes;

public class UpdateClienteDto
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Sector { get; set; }
    public string? Ciudad { get; set; }
    public string? Direccion { get; set; }
    public string? Web { get; set; }
    public string? Contacto { get; set; }
    public string? CargoContacto { get; set; }
    public string? Email { get; set; }
    public string? ValorReferencia { get; set; }
    public string? Notas { get; set; }
    public List<ClienteTelefonoDto> Telefonos { get; set; } = new();
}
```

- [ ] **Step 4: Crear ClienteResponseDto**

```csharp
// src/Nexit.Application/DTOs/Clientes/ClienteResponseDto.cs
namespace Nexit.Application.DTOs.Clientes;

public class ClienteResponseDto
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Sector { get; set; }
    public string? Ciudad { get; set; }
    public string? Direccion { get; set; }
    public string? Web { get; set; }
    public string? Contacto { get; set; }
    public string? CargoContacto { get; set; }
    public string? Email { get; set; }
    public string? ValorReferencia { get; set; }
    public string? Notas { get; set; }
    public List<ClienteTelefonoDto> Telefonos { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
```

- [ ] **Step 5: Commit DTOs**

```bash
cd c:\Users\USUARIO\Documents\Github\Nexit_Back
git add src/Nexit.Application/DTOs/
git commit -m "feat(application): add Cliente DTOs (Create, Update, Response)"
```

---

## Task 6: Crear Validators en Nexit.Application

**Files:**
- Create: `src/Nexit.Application/Validators/Clientes/CreateClienteValidator.cs`
- Create: `src/Nexit.Application/Validators/Clientes/UpdateClienteValidator.cs`

- [ ] **Step 1: Crear CreateClienteValidator**

```csharp
// src/Nexit.Application/Validators/Clientes/CreateClienteValidator.cs
using FluentValidation;
using Nexit.Application.DTOs.Clientes;
using Nexit.Core.Interfaces;

namespace Nexit.Application.Validators.Clientes;

public class CreateClienteValidator : AbstractValidator<CreateClienteDto>
{
    private readonly IClienteRepository _repository;
    
    public CreateClienteValidator(IClienteRepository repository)
    {
        _repository = repository;
        
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre es requerido")
            .MaximumLength(255).WithMessage("El nombre no puede exceder 255 caracteres");
        
        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("El email debe tener un formato válido")
            .When(x => !string.IsNullOrEmpty(x.Email))
            .MustAsync(async (email, _) => !(await _repository.ExistsByEmailAsync(email)))
            .WithMessage("El email ya está registrado")
            .When(x => !string.IsNullOrEmpty(x.Email));
        
        RuleFor(x => x.Telefonos)
            .NotEmpty().WithMessage("Al menos un teléfono es requerido")
            .Must(x => x.All(t => !string.IsNullOrEmpty(t.Telefono)))
            .WithMessage("Todos los teléfonos deben tener un número");
    }
}
```

- [ ] **Step 2: Crear UpdateClienteValidator**

```csharp
// src/Nexit.Application/Validators/Clientes/UpdateClienteValidator.cs
using FluentValidation;
using Nexit.Application.DTOs.Clientes;
using Nexit.Core.Interfaces;

namespace Nexit.Application.Validators.Clientes;

public class UpdateClienteValidator : AbstractValidator<UpdateClienteDto>
{
    private readonly IClienteRepository _repository;
    
    public UpdateClienteValidator(IClienteRepository repository)
    {
        _repository = repository;
        
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("El ID del cliente es requerido");
        
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre es requerido")
            .MaximumLength(255).WithMessage("El nombre no puede exceder 255 caracteres");
        
        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("El email debe tener un formato válido")
            .When(x => !string.IsNullOrEmpty(x.Email))
            .MustAsync(async (dto, email, _) => 
            {
                var cliente = await _repository.GetByIdAsync(dto.Id);
                if (cliente?.Email == email) return true; // Email no cambió
                return !(await _repository.ExistsByEmailAsync(email));
            })
            .WithMessage("El email ya está registrado")
            .When(x => !string.IsNullOrEmpty(x.Email));
        
        RuleFor(x => x.Telefonos)
            .NotEmpty().WithMessage("Al menos un teléfono es requerido")
            .Must(x => x.All(t => !string.IsNullOrEmpty(t.Telefono)))
            .WithMessage("Todos los teléfonos deben tener un número");
    }
}
```

- [ ] **Step 3: Commit validators**

```bash
cd c:\Users\USUARIO\Documents\Github\Nexit_Back
git add src/Nexit.Application/Validators/
git commit -m "feat(application): add FluentValidation validators for Cliente"
```

---

## Task 7: Crear AutoMapper Profiles en Nexit.Application

**Files:**
- Create: `src/Nexit.Application/MappingProfiles/ClienteProfile.cs`

- [ ] **Step 1: Crear ClienteProfile**

```csharp
// src/Nexit.Application/MappingProfiles/ClienteProfile.cs
using AutoMapper;
using Nexit.Application.DTOs.Clientes;
using Nexit.Core.Entities;

namespace Nexit.Application.MappingProfiles;

public class ClienteProfile : Profile
{
    public ClienteProfile()
    {
        // CreateClienteDto → Cliente
        CreateMap<CreateClienteDto, Cliente>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(_ => Guid.NewGuid()))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(dest => dest.Telefonos, opt => opt.MapFrom(src => 
                src.Telefonos.Select(t => new ClienteTelefono 
                { 
                    Id = Guid.NewGuid(), 
                    Telefono = t.Telefono, 
                    Etiqueta = t.Etiqueta 
                }).ToList()));
        
        // UpdateClienteDto → Cliente
        CreateMap<UpdateClienteDto, Cliente>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(dest => dest.Telefonos, opt => opt.MapFrom(src => 
                src.Telefonos.Select(t => new ClienteTelefono 
                { 
                    Id = t.Id ?? Guid.NewGuid(), 
                    Telefono = t.Telefono, 
                    Etiqueta = t.Etiqueta 
                }).ToList()))
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore());
        
        // Cliente → ClienteResponseDto
        CreateMap<Cliente, ClienteResponseDto>()
            .ForMember(dest => dest.Telefonos, opt => opt.MapFrom(src => src.Telefonos));
        
        // ClienteTelefono → ClienteTelefonoDto
        CreateMap<ClienteTelefono, ClienteTelefonoDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id));
        
        // ClienteTelefonoDto → ClienteTelefono
        CreateMap<ClienteTelefonoDto, ClienteTelefono>();
    }
}
```

- [ ] **Step 2: Commit profiles**

```bash
cd c:\Users\USUARIO\Documents\Github\Nexit_Back
git add src/Nexit.Application/MappingProfiles/
git commit -m "feat(application): add AutoMapper profiles for Cliente entity"
```

---

## Task 8: Crear Use Cases en Nexit.Application

**Files:**
- Create: `src/Nexit.Application/UseCases/Clientes/ICrearClienteUseCase.cs`
- Create: `src/Nexit.Application/UseCases/Clientes/CrearClienteUseCase.cs`
- Create: `src/Nexit.Application/UseCases/Clientes/IActualizarClienteUseCase.cs`
- Create: `src/Nexit.Application/UseCases/Clientes/ActualizarClienteUseCase.cs`
- Create: `src/Nexit.Application/Exceptions/ApplicationException.cs`

- [ ] **Step 1: Crear ApplicationException**

```csharp
// src/Nexit.Application/Exceptions/ApplicationException.cs
namespace Nexit.Application.Exceptions;

public class ApplicationException : Exception
{
    public ApplicationException(string message) : base(message) { }
    public ApplicationException(string message, Exception innerException) 
        : base(message, innerException) { }
}
```

- [ ] **Step 2: Crear ICrearClienteUseCase**

```csharp
// src/Nexit.Application/UseCases/Clientes/ICrearClienteUseCase.cs
using Nexit.Application.DTOs.Clientes;

namespace Nexit.Application.UseCases.Clientes;

public interface ICrearClienteUseCase
{
    Task<ClienteResponseDto> ExecuteAsync(CreateClienteDto input, Guid usuarioId);
}
```

- [ ] **Step 3: Crear CrearClienteUseCase**

```csharp
// src/Nexit.Application/UseCases/Clientes/CrearClienteUseCase.cs
using AutoMapper;
using Nexit.Application.DTOs.Clientes;
using Nexit.Core.Entities;
using Nexit.Core.Interfaces;

namespace Nexit.Application.UseCases.Clientes;

public class CrearClienteUseCase : ICrearClienteUseCase
{
    private readonly IClienteRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    
    public CrearClienteUseCase(IClienteRepository repository, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }
    
    public async Task<ClienteResponseDto> ExecuteAsync(CreateClienteDto input, Guid usuarioId)
    {
        var cliente = _mapper.Map<Cliente>(input);
        cliente.CreatedBy = usuarioId;
        
        await _repository.AddAsync(cliente);
        await _unitOfWork.SaveChangesAsync();
        
        return _mapper.Map<ClienteResponseDto>(cliente);
    }
}
```

- [ ] **Step 4: Crear IActualizarClienteUseCase**

```csharp
// src/Nexit.Application/UseCases/Clientes/IActualizarClienteUseCase.cs
using Nexit.Application.DTOs.Clientes;

namespace Nexit.Application.UseCases.Clientes;

public interface IActualizarClienteUseCase
{
    Task<ClienteResponseDto> ExecuteAsync(UpdateClienteDto input, Guid usuarioId);
}
```

- [ ] **Step 5: Crear ActualizarClienteUseCase**

```csharp
// src/Nexit.Application/UseCases/Clientes/ActualizarClienteUseCase.cs
using AutoMapper;
using Nexit.Application.DTOs.Clientes;
using Nexit.Core.Exceptions;
using Nexit.Core.Interfaces;

namespace Nexit.Application.UseCases.Clientes;

public class ActualizarClienteUseCase : IActualizarClienteUseCase
{
    private readonly IClienteRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    
    public ActualizarClienteUseCase(IClienteRepository repository, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }
    
    public async Task<ClienteResponseDto> ExecuteAsync(UpdateClienteDto input, Guid usuarioId)
    {
        var cliente = await _repository.GetByIdAsync(input.Id) 
            ?? throw new EntityNotFoundException("Cliente", input.Id);
        
        _mapper.Map(input, cliente);
        cliente.UpdatedAt = DateTime.UtcNow;
        
        await _repository.UpdateAsync(cliente);
        await _unitOfWork.SaveChangesAsync();
        
        return _mapper.Map<ClienteResponseDto>(cliente);
    }
}
```

- [ ] **Step 6: Commit use cases**

```bash
cd c:\Users\USUARIO\Documents\Github\Nexit_Back
git add src/Nexit.Application/UseCases/ src/Nexit.Application/Exceptions/
git commit -m "feat(application): add use cases for Cliente (Create, Update)"
```

---

## Task 9: Crear DependencyInjection en Nexit.Application

**Files:**
- Create: `src/Nexit.Application/DependencyInjection.cs`

- [ ] **Step 1: Crear DependencyInjection.cs**

```csharp
// src/Nexit.Application/DependencyInjection.cs
using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Nexit.Application.DTOs.Clientes;
using Nexit.Application.MappingProfiles;
using Nexit.Application.UseCases.Clientes;
using Nexit.Application.Validators.Clientes;

namespace Nexit.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // AutoMapper
        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<ClienteProfile>();
            // Agregar más profiles aquí cuando existan
        });
        
        services.AddSingleton(mapperConfig.CreateMapper());
        services.AddAutoMapper(typeof(DependencyInjection));
        
        // FluentValidation
        services.AddFluentValidationAutoValidation();
        services.AddValidatorsFromAssemblyContaining(typeof(DependencyInjection));
        
        // Use Cases
        services.AddScoped<ICrearClienteUseCase, CrearClienteUseCase>();
        services.AddScoped<IActualizarClienteUseCase, ActualizarClienteUseCase>();
        
        return services;
    }
}
```

- [ ] **Step 2: Commit DI**

```bash
cd c:\Users\USUARIO\Documents\Github\Nexit_Back
git add src/Nexit.Application/DependencyInjection.cs
git commit -m "feat(application): add dependency injection configuration"
```

---

## Task 10: Agregar NuGet packages a Nexit.Infrastructure

**Files:**
- Modify: `src/Nexit.Infrastructure/Nexit.Infrastructure.csproj`

- [ ] **Step 1: Agregar packages**

```bash
cd src/Nexit.Infrastructure
dotnet add package Microsoft.EntityFrameworkCore --version 8.0.0
dotnet add package Microsoft.EntityFrameworkCore.Tools --version 8.0.0
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL --version 8.0.0
```

---

## Task 11: Crear DbContext en Nexit.Infrastructure

**Files:**
- Create: `src/Nexit.Infrastructure/Data/NexitDbContext.cs`
- Create: `src/Nexit.Infrastructure/Data/DesignTimeDbContextFactory.cs`

- [ ] **Step 1: Crear NexitDbContext**

```csharp
// src/Nexit.Infrastructure/Data/NexitDbContext.cs
using Microsoft.EntityFrameworkCore;
using Nexit.Core.Entities;

namespace Nexit.Infrastructure.Data;

public class NexitDbContext : DbContext
{
    public NexitDbContext(DbContextOptions<NexitDbContext> options) : base(options) { }
    
    public DbSet<Usuario> Usuarios { get; set; } = null!;
    public DbSet<Cliente> Clientes { get; set; } = null!;
    public DbSet<ClienteTelefono> ClienteTelefonos { get; set; } = null!;
    public DbSet<Proveedor> Proveedores { get; set; } = null!;
    public DbSet<ProveedorTelefono> ProveedorTelefonos { get; set; } = null!;
    public DbSet<Servicio> Servicios { get; set; } = null!;
    public DbSet<ProveedorServicio> ProveedorServicios { get; set; } = null!;
    public DbSet<Proyecto> Proyectos { get; set; } = null!;
    public DbSet<ProyectoEquipo> ProyectoEquipo { get; set; } = null!;
    public DbSet<ProyectoProveedor> ProyectoProveedores { get; set; } = null!;
    public DbSet<ProyectoSeguimiento> ProyectoSeguimientos { get; set; } = null!;
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Usuario
        modelBuilder.Entity<Usuario>()
            .HasKey(u => u.Id);
        modelBuilder.Entity<Usuario>()
            .HasIndex(u => u.Email)
            .IsUnique();
        
        // Cliente
        modelBuilder.Entity<Cliente>()
            .HasKey(c => c.Id);
        modelBuilder.Entity<Cliente>()
            .HasMany(c => c.Telefonos)
            .WithOne(t => t.Cliente)
            .HasForeignKey(t => t.ClienteId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Cliente>()
            .HasMany(c => c.Proyectos)
            .WithOne(p => p.Cliente);
        
        // ClienteTelefono
        modelBuilder.Entity<ClienteTelefono>()
            .HasKey(t => t.Id);
        
        // Proveedor
        modelBuilder.Entity<Proveedor>()
            .HasKey(p => p.Id);
        modelBuilder.Entity<Proveedor>()
            .HasMany(p => p.Telefonos)
            .WithOne(t => t.Proveedor)
            .HasForeignKey(t => t.ProveedorId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Proveedor>()
            .HasMany(p => p.Servicios)
            .WithOne(ps => ps.Proveedor)
            .HasForeignKey(ps => ps.ProveedorId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // ProveedorTelefono
        modelBuilder.Entity<ProveedorTelefono>()
            .HasKey(t => t.Id);
        
        // Servicio
        modelBuilder.Entity<Servicio>()
            .HasKey(s => s.Id);
        modelBuilder.Entity<Servicio>()
            .HasIndex(s => s.Nombre)
            .IsUnique();
        
        // ProveedorServicio
        modelBuilder.Entity<ProveedorServicio>()
            .HasKey(ps => new { ps.ProveedorId, ps.ServicioId });
        modelBuilder.Entity<ProveedorServicio>()
            .HasOne(ps => ps.Servicio)
            .WithMany(s => s.Proveedores)
            .HasForeignKey(ps => ps.ServicioId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Proyecto
        modelBuilder.Entity<Proyecto>()
            .HasKey(p => p.Id);
        modelBuilder.Entity<Proyecto>()
            .HasMany(p => p.Equipo)
            .WithOne(e => e.Proyecto)
            .HasForeignKey(e => e.ProyectoId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Proyecto>()
            .HasMany(p => p.Proveedores)
            .WithOne(pp => pp.Proyecto)
            .HasForeignKey(pp => pp.ProyectoId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Proyecto>()
            .HasMany(p => p.Seguimiento)
            .WithOne(s => s.Proyecto)
            .HasForeignKey(s => s.ProyectoId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // ProyectoEquipo
        modelBuilder.Entity<ProyectoEquipo>()
            .HasKey(pe => pe.Id);
        
        // ProyectoProveedor
        modelBuilder.Entity<ProyectoProveedor>()
            .HasKey(pp => new { pp.ProyectoId, pp.ProveedorId });
        modelBuilder.Entity<ProyectoProveedor>()
            .HasOne(pp => pp.Proveedor)
            .WithMany(p => p.Proyectos)
            .HasForeignKey(pp => pp.ProveedorId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // ProyectoSeguimiento
        modelBuilder.Entity<ProyectoSeguimiento>()
            .HasKey(ps => ps.Id);
        modelBuilder.Entity<ProyectoSeguimiento>()
            .HasOne(ps => ps.Autor)
            .WithMany(u => u.SeguimientosEscritos)
            .HasForeignKey(ps => ps.AutorId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
```

- [ ] **Step 2: Crear DesignTimeDbContextFactory**

```csharp
// src/Nexit.Infrastructure/Data/DesignTimeDbContextFactory.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Nexit.Infrastructure.Data;

public class NexitDbContextFactory : IDesignTimeDbContextFactory<NexitDbContext>
{
    public NexitDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .AddEnvironmentVariables()
            .Build();
        
        var optionsBuilder = new DbContextOptionsBuilder<NexitDbContext>();
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        
        optionsBuilder.UseNpgsql(connectionString);
        
        return new NexitDbContext(optionsBuilder.Options);
    }
}
```

- [ ] **Step 3: Commit DbContext**

```bash
cd c:\Users\USUARIO\Documents\Github\Nexit_Back
git add src/Nexit.Infrastructure/Data/
git commit -m "feat(infrastructure): add NexitDbContext and DesignTimeFactory"
```

---

## Task 12: Crear Repository genérico y ClienteRepository en Nexit.Infrastructure

**Files:**
- Create: `src/Nexit.Infrastructure/Repositories/Repository.cs`
- Create: `src/Nexit.Infrastructure/Repositories/ClienteRepository.cs`

- [ ] **Step 1: Crear Repository<T> genérico**

```csharp
// src/Nexit.Infrastructure/Repositories/Repository.cs
using Microsoft.EntityFrameworkCore;
using Nexit.Core.Interfaces;

namespace Nexit.Infrastructure.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    protected readonly NexitDbContext _context;
    protected readonly DbSet<T> _dbSet;
    
    public Repository(NexitDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }
    
    public virtual async Task<T?> GetByIdAsync(Guid id)
    {
        return await _dbSet.FindAsync(id);
    }
    
    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }
    
    public virtual async Task<IEnumerable<T>> FindAsync(Func<T, bool> predicate)
    {
        return await Task.FromResult(_dbSet.Where(predicate).ToList());
    }
    
    public virtual async Task AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
    }
    
    public virtual async Task UpdateAsync(T entity)
    {
        _dbSet.Update(entity);
        await Task.CompletedTask;
    }
    
    public virtual async Task DeleteAsync(Guid id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null)
        {
            _dbSet.Remove(entity);
        }
    }
}
```

- [ ] **Step 2: Crear ClienteRepository**

```csharp
// src/Nexit.Infrastructure/Repositories/ClienteRepository.cs
using Microsoft.EntityFrameworkCore;
using Nexit.Core.Entities;
using Nexit.Core.Interfaces;

namespace Nexit.Infrastructure.Repositories;

public class ClienteRepository : Repository<Cliente>, IClienteRepository
{
    public ClienteRepository(NexitDbContext context) : base(context) { }
    
    public async Task<bool> ExistsByEmailAsync(string email)
    {
        return await _dbSet.AnyAsync(c => c.Email == email);
    }
    
    public async Task<bool> ExistsByNombreAsync(string nombre)
    {
        return await _dbSet.AnyAsync(c => c.Nombre == nombre);
    }
    
    public async Task<Cliente?> GetByEmailAsync(string email)
    {
        return await _dbSet.FirstOrDefaultAsync(c => c.Email == email);
    }
    
    public async Task<IEnumerable<Cliente>> GetByCiudadAsync(string ciudad)
    {
        return await _dbSet.Where(c => c.Ciudad == ciudad).ToListAsync();
    }
    
    public override async Task<Cliente?> GetByIdAsync(Guid id)
    {
        return await _dbSet
            .Include(c => c.Telefonos)
            .FirstOrDefaultAsync(c => c.Id == id);
    }
    
    public override async Task<IEnumerable<Cliente>> GetAllAsync()
    {
        return await _dbSet
            .Include(c => c.Telefonos)
            .ToListAsync();
    }
}
```

- [ ] **Step 3: Commit repositories**

```bash
cd c:\Users\USUARIO\Documents\Github\Nexit_Back
git add src/Nexit.Infrastructure/Repositories/
git commit -m "feat(infrastructure): add generic Repository and ClienteRepository implementation"
```

---

## Task 13: Crear UnitOfWork en Nexit.Infrastructure

**Files:**
- Create: `src/Nexit.Infrastructure/UnitOfWork/UnitOfWork.cs`

- [ ] **Step 1: Crear UnitOfWork**

```csharp
// src/Nexit.Infrastructure/UnitOfWork/UnitOfWork.cs
using Microsoft.EntityFrameworkCore.Storage;
using Nexit.Core.Entities;
using Nexit.Core.Interfaces;
using Nexit.Infrastructure.Repositories;

namespace Nexit.Infrastructure.UnitOfWork;

public class UnitOfWork : IUnitOfWork
{
    private readonly NexitDbContext _context;
    private IDbContextTransaction? _transaction;
    
    private IRepository<Usuario>? _usuarios;
    private IClienteRepository? _clientes;
    private IRepository<Proveedor>? _proveedores;
    private IRepository<Proyecto>? _proyectos;
    
    public UnitOfWork(NexitDbContext context)
    {
        _context = context;
    }
    
    public IRepository<Usuario> Usuarios => _usuarios ??= new Repository<Usuario>(_context);
    public IClienteRepository Clientes => _clientes ??= new ClienteRepository(_context);
    public IRepository<Proveedor> Proveedores => _proveedores ??= new Repository<Proveedor>(_context);
    public IRepository<Proyecto> Proyectos => _proyectos ??= new Repository<Proyecto>(_context);
    
    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
    
    public async Task<bool> BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
        return true;
    }
    
    public async Task<bool> CommitTransactionAsync()
    {
        try
        {
            await _context.SaveChangesAsync();
            await _transaction?.CommitAsync()!;
            return true;
        }
        catch
        {
            await RollbackTransactionAsync();
            throw;
        }
        finally
        {
            _transaction?.Dispose();
        }
    }
    
    public async Task<bool> RollbackTransactionAsync()
    {
        try
        {
            await _transaction?.RollbackAsync()!;
            return true;
        }
        finally
        {
            _transaction?.Dispose();
        }
    }
    
    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}
```

- [ ] **Step 2: Commit UnitOfWork**

```bash
cd c:\Users\USUARIO\Documents\Github\Nexit_Back
git add src/Nexit.Infrastructure/UnitOfWork/
git commit -m "feat(infrastructure): add UnitOfWork implementation with transaction support"
```

---

## Task 14: Crear DependencyInjection en Nexit.Infrastructure

**Files:**
- Create: `src/Nexit.Infrastructure/DependencyInjection.cs`

- [ ] **Step 1: Crear DependencyInjection.cs**

```csharp
// src/Nexit.Infrastructure/DependencyInjection.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nexit.Core.Interfaces;
using Nexit.Infrastructure.Data;
using Nexit.Infrastructure.UnitOfWork;

namespace Nexit.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        
        // DbContext
        services.AddDbContext<NexitDbContext>(options =>
            options.UseNpgsql(connectionString, b => 
                b.MigrationsAssembly(typeof(NexitDbContext).Assembly.FullName)));
        
        // UnitOfWork y Repositories
        services.AddScoped<IUnitOfWork, UnitOfWork.UnitOfWork>();
        services.AddScoped<IClienteRepository, Repositories.ClienteRepository>();
        
        return services;
    }
}
```

- [ ] **Step 2: Commit Infrastructure DI**

```bash
cd c:\Users\USUARIO\Documents\Github\Nexit_Back
git add src/Nexit.Infrastructure/DependencyInjection.cs
git commit -m "feat(infrastructure): add dependency injection configuration"
```

---

## Task 15: Agregar NuGet packages a Nexit.API

**Files:**
- Modify: `src/Nexit.API/Nexit.API.csproj`

- [ ] **Step 1: Agregar packages**

```bash
cd src/Nexit.API
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer --version 8.0.0
dotnet add package Swashbuckle.AspNetCore --version 6.0.0
dotnet add package Serilog.AspNetCore --version 8.0.0
dotnet add package Serilog.Sinks.File --version 5.0.0
```

---

## Task 16: Crear appsettings en Nexit.API

**Files:**
- Create/Modify: `src/Nexit.API/appsettings.json`
- Create: `src/Nexit.API/appsettings.Development.json`

- [ ] **Step 1: Crear appsettings.json**

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=db.supabase.co;Database=postgres;Username=postgres;Password=YOUR_PASSWORD;Port=5432;SSL Mode=Require;"
  },
  "Supabase": {
    "Url": "https://YOUR_PROJECT.supabase.co",
    "AnonKey": "YOUR_ANON_KEY"
  },
  "Jwt": {
    "Authority": "https://YOUR_PROJECT.supabase.co/auth/v1",
    "Audience": "authenticated",
    "ValidateIssuer": true,
    "ValidateAudience": true
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.EntityFrameworkCore": "Warning",
      "Microsoft.AspNetCore": "Information"
    }
  },
  "AllowedHosts": "*"
}
```

- [ ] **Step 2: Crear appsettings.Development.json**

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=nexit_dev;Username=postgres;Password=postgres;Port=5432;SSL Mode=Disable;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.EntityFrameworkCore": "Information",
      "Microsoft.AspNetCore": "Debug"
    }
  }
}
```

- [ ] **Step 3: Commit config files**

```bash
cd c:\Users\USUARIO\Documents\Github\Nexit_Back
git add src/Nexit.API/appsettings*.json
git commit -m "chore(api): add appsettings configuration files"
```

---

## Task 17: Crear Middleware global en Nexit.API

**Files:**
- Create: `src/Nexit.API/Middleware/GlobalExceptionHandlerMiddleware.cs`
- Create: `src/Nexit.API/Models/ErrorResponse.cs`

- [ ] **Step 1: Crear ErrorResponse DTO**

```csharp
// src/Nexit.API/Models/ErrorResponse.cs
namespace Nexit.API.Models;

public class ErrorResponse
{
    public int StatusCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public Dictionary<string, string[]>? Errors { get; set; }
    public string? TraceId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
```

- [ ] **Step 2: Crear GlobalExceptionHandlerMiddleware**

```csharp
// src/Nexit.API/Middleware/GlobalExceptionHandlerMiddleware.cs
using System.Net;
using System.Text.Json;
using Nexit.API.Models;
using Nexit.Core.Exceptions;

namespace Nexit.API.Middleware;

public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;
    
    public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception has occurred: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }
    
    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        
        var response = new ErrorResponse
        {
            TraceId = context.TraceIdentifier,
            Timestamp = DateTime.UtcNow
        };
        
        switch (exception)
        {
            case EntityNotFoundException ex:
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                response.StatusCode = StatusCodes.Status404NotFound;
                response.Message = ex.Message;
                break;
                
            case BusinessRuleException ex:
                context.Response.StatusCode = StatusCodes.Status409Conflict;
                response.StatusCode = StatusCodes.Status409Conflict;
                response.Message = ex.Message;
                break;
                
            case InvalidOperationException ex:
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                response.StatusCode = StatusCodes.Status400BadRequest;
                response.Message = ex.Message;
                break;
                
            default:
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                response.StatusCode = StatusCodes.Status500InternalServerError;
                response.Message = "An internal server error occurred.";
                break;
        }
        
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        return context.Response.WriteAsJsonAsync(response, options);
    }
}
```

- [ ] **Step 3: Commit middleware**

```bash
cd c:\Users\USUARIO\Documents\Github\Nexit_Back
git add src/Nexit.API/Middleware/ src/Nexit.API/Models/
git commit -m "feat(api): add global exception handler middleware"
```

---

## Task 18: Crear BaseController en Nexit.API

**Files:**
- Create: `src/Nexit.API/Controllers/BaseController.cs`

- [ ] **Step 1: Crear BaseController**

```csharp
// src/Nexit.API/Controllers/BaseController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Nexit.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public abstract class BaseController : ControllerBase
{
    protected Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }
    
    protected string? GetUserEmail()
    {
        return User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
    }
    
    protected string? GetUserRole()
    {
        return User.FindFirst("role")?.Value;
    }
}
```

- [ ] **Step 2: Commit BaseController**

```bash
cd c:\Users\USUARIO\Documents\Github\Nexit_Back
git add src/Nexit.API/Controllers/BaseController.cs
git commit -m "feat(api): add BaseController with user context helpers"
```

---

## Task 19: Crear ClientesController en Nexit.API

**Files:**
- Create: `src/Nexit.API/Controllers/ClientesController.cs`

- [ ] **Step 1: Crear ClientesController**

```csharp
// src/Nexit.API/Controllers/ClientesController.cs
using Microsoft.AspNetCore.Mvc;
using Nexit.Application.DTOs.Clientes;
using Nexit.Application.UseCases.Clientes;
using Nexit.Core.Exceptions;

namespace Nexit.API.Controllers;

public class ClientesController : BaseController
{
    private readonly ICrearClienteUseCase _crearUseCase;
    private readonly IActualizarClienteUseCase _actualizarUseCase;
    private readonly ILogger<ClientesController> _logger;
    
    public ClientesController(
        ICrearClienteUseCase crearUseCase,
        IActualizarClienteUseCase actualizarUseCase,
        ILogger<ClientesController> logger)
    {
        _crearUseCase = crearUseCase;
        _actualizarUseCase = actualizarUseCase;
        _logger = logger;
    }
    
    /// <summary>
    /// Crear un nuevo cliente
    /// </summary>
    /// <param name="dto">Datos del cliente</param>
    /// <returns>Cliente creado</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ClienteResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create([FromBody] CreateClienteDto dto)
    {
        var usuarioId = GetUserId();
        if (usuarioId == Guid.Empty)
            return Unauthorized();
        
        try
        {
            var resultado = await _crearUseCase.ExecuteAsync(dto, usuarioId);
            _logger.LogInformation("Cliente {ClienteId} creado por usuario {UsuarioId}", resultado.Id, usuarioId);
            
            return CreatedAtAction(nameof(GetById), new { id = resultado.Id }, resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear cliente");
            throw;
        }
    }
    
    /// <summary>
    /// Actualizar un cliente existente
    /// </summary>
    /// <param name="id">ID del cliente</param>
    /// <param name="dto">Datos actualizados</param>
    /// <returns>Cliente actualizado</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ClienteResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateClienteDto dto)
    {
        var usuarioId = GetUserId();
        if (usuarioId == Guid.Empty)
            return Unauthorized();
        
        dto.Id = id;
        
        try
        {
            var resultado = await _actualizarUseCase.ExecuteAsync(dto, usuarioId);
            _logger.LogInformation("Cliente {ClienteId} actualizado por usuario {UsuarioId}", id, usuarioId);
            
            return Ok(resultado);
        }
        catch (EntityNotFoundException ex)
        {
            _logger.LogWarning(ex, "Cliente {ClienteId} no encontrado", id);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar cliente {ClienteId}", id);
            throw;
        }
    }
    
    /// <summary>
    /// Obtener un cliente por ID
    /// </summary>
    /// <param name="id">ID del cliente</param>
    /// <returns>Datos del cliente</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ClienteResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetById(Guid id)
    {
        if (id == Guid.Empty)
            return BadRequest("ID inválido");
        
        // Placeholder: se implementará en Opción 2 cuando tengamos GetByIdUseCase
        return NotFound();
    }
}
```

- [ ] **Step 2: Commit ClientesController**

```bash
cd c:\Users\USUARIO\Documents\Github\Nexit_Back
git add src/Nexit.API/Controllers/ClientesController.cs
git commit -m "feat(api): add ClientesController with Create and Update endpoints"
```

---

## Task 20: Actualizar Program.cs en Nexit.API

**Files:**
- Modify: `src/Nexit.API/Program.cs`

- [ ] **Step 1: Reescribir Program.cs**

```csharp
// src/Nexit.API/Program.cs
using Serilog;
using Nexit.API.Middleware;
using Nexit.Application;
using Nexit.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;

var builder = WebApplicationBuilder.CreateBuilder(args);

// Logging con Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("logs/nexit-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Servicios
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Autenticación JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["Jwt:Authority"];
        options.Audience = builder.Configuration["Jwt:Audience"];
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = bool.Parse(builder.Configuration["Jwt:ValidateIssuer"] ?? "true"),
            ValidateAudience = bool.Parse(builder.Configuration["Jwt:ValidateAudience"] ?? "true"),
            ValidateLifetime = true
        };
    });

builder.Services.AddAuthorization();

// Controllers
builder.Services.AddControllers();

// Swagger/OpenAPI
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Nexit API",
        Version = "v1",
        Description = "API REST para el sistema de gestión Nexit"
    });
    
    // Esquema de seguridad JWT para Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme (Example: 'Bearer 12345abcdef')",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] { }
        }
    });
    
    // XML comments para documentación de endpoints
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

var app = builder.Build();

// Middleware
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Nexit API v1");
    });
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

try
{
    Log.Information("Starting Nexit API");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
```

- [ ] **Step 2: Commit Program.cs**

```bash
cd c:\Users\USUARIO\Documents\Github\Nexit_Back
git add src/Nexit.API/Program.cs
git commit -m "feat(api): configure Program.cs with DI, auth, logging, and Swagger"
```

---

## Task 21: Crear tests unitarios en Nexit.Tests

**Files:**
- Create: `tests/Nexit.Tests/Fixtures/TestDataBuilder.cs`
- Create: `tests/Nexit.Tests/Validators/CreateClienteValidatorTests.cs`
- Create: `tests/Nexit.Tests/UseCases/CrearClienteUseCaseTests.cs`

- [ ] **Step 1: Agregar packages a tests**

```bash
cd tests/Nexit.Tests
dotnet add package Moq --version 4.20.0
dotnet add package FluentAssertions --version 6.12.0
```

- [ ] **Step 2: Crear TestDataBuilder**

```csharp
// tests/Nexit.Tests/Fixtures/TestDataBuilder.cs
using Nexit.Application.DTOs.Clientes;
using Nexit.Core.Entities;

namespace Nexit.Tests.Fixtures;

public class TestDataBuilder
{
    public static CreateClienteDto CreateValidClienteDto()
    {
        return new CreateClienteDto
        {
            Nombre = "SURA",
            Email = "contacto@sura.com",
            Sector = "Seguros",
            Ciudad = "Bogotá",
            Contacto = "Juan Pérez",
            CargoContacto = "Gerente de Eventos",
            Telefonos = new List<ClienteTelefonoDto>
            {
                new() { Telefono = "555-1234", Etiqueta = "Principal" }
            }
        };
    }
    
    public static UpdateClienteDto CreateValidUpdateClienteDto(Guid id)
    {
        return new UpdateClienteDto
        {
            Id = id,
            Nombre = "SURA Actualizado",
            Email = "nuevo@sura.com",
            Contacto = "Carlos Gómez",
            Telefonos = new List<ClienteTelefonoDto>
            {
                new() { Telefono = "555-5678", Etiqueta = "Principal" }
            }
        };
    }
    
    public static Cliente CreateValidCliente()
    {
        return new Cliente
        {
            Id = Guid.NewGuid(),
            Nombre = "SURA",
            Email = "contacto@sura.com",
            Sector = "Seguros",
            Ciudad = "Bogotá",
            Contacto = "Juan Pérez",
            CreatedAt = DateTime.UtcNow
        };
    }
}
```

- [ ] **Step 3: Crear CreateClienteValidatorTests**

```csharp
// tests/Nexit.Tests/Validators/CreateClienteValidatorTests.cs
using FluentAssertions;
using Moq;
using Nexit.Application.DTOs.Clientes;
using Nexit.Application.Validators.Clientes;
using Nexit.Core.Interfaces;
using Nexit.Tests.Fixtures;
using Xunit;

namespace Nexit.Tests.Validators;

public class CreateClienteValidatorTests
{
    [Fact]
    public async Task Validate_WithValidData_ShouldSucceed()
    {
        // Arrange
        var mockRepository = new Mock<IClienteRepository>();
        mockRepository.Setup(x => x.ExistsByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(false);
        
        var validator = new CreateClienteValidator(mockRepository.Object);
        var dto = TestDataBuilder.CreateValidClienteDto();
        
        // Act
        var result = await validator.ValidateAsync(dto);
        
        // Assert
        result.IsValid.Should().BeTrue();
    }
    
    [Fact]
    public async Task Validate_WithMissingNombre_ShouldFail()
    {
        // Arrange
        var mockRepository = new Mock<IClienteRepository>();
        var validator = new CreateClienteValidator(mockRepository.Object);
        var dto = new CreateClienteDto
        {
            Nombre = "",
            Email = "test@example.com",
            Telefonos = new List<ClienteTelefonoDto> { new() { Telefono = "555-1234" } }
        };
        
        // Act
        var result = await validator.ValidateAsync(dto);
        
        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Nombre");
    }
    
    [Fact]
    public async Task Validate_WithDuplicateEmail_ShouldFail()
    {
        // Arrange
        var mockRepository = new Mock<IClienteRepository>();
        mockRepository.Setup(x => x.ExistsByEmailAsync("contacto@sura.com"))
            .ReturnsAsync(true);
        
        var validator = new CreateClienteValidator(mockRepository.Object);
        var dto = TestDataBuilder.CreateValidClienteDto();
        
        // Act
        var result = await validator.ValidateAsync(dto);
        
        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email" && e.ErrorMessage.Contains("registrado"));
    }
    
    [Fact]
    public async Task Validate_WithMissingTelefonos_ShouldFail()
    {
        // Arrange
        var mockRepository = new Mock<IClienteRepository>();
        var validator = new CreateClienteValidator(mockRepository.Object);
        var dto = new CreateClienteDto
        {
            Nombre = "Test",
            Email = "test@example.com",
            Telefonos = new List<ClienteTelefonoDto>()
        };
        
        // Act
        var result = await validator.ValidateAsync(dto);
        
        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Telefonos");
    }
}
```

- [ ] **Step 4: Crear CrearClienteUseCaseTests**

```csharp
// tests/Nexit.Tests/UseCases/CrearClienteUseCaseTests.cs
using FluentAssertions;
using Moq;
using Nexit.Application.UseCases.Clientes;
using Nexit.Core.Interfaces;
using Nexit.Tests.Fixtures;
using Xunit;
using AutoMapper;
using Nexit.Application.MappingProfiles;
using Nexit.Core.Entities;

namespace Nexit.Tests.UseCases;

public class CrearClienteUseCaseTests
{
    private readonly IMapper _mapper;
    
    public CrearClienteUseCaseTests()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<ClienteProfile>();
        });
        _mapper = config.CreateMapper();
    }
    
    [Fact]
    public async Task Execute_WithValidData_ShouldCreateAndReturnCliente()
    {
        // Arrange
        var mockRepository = new Mock<IClienteRepository>();
        var mockUnitOfWork = new Mock<IUnitOfWork>();
        var usuarioId = Guid.NewGuid();
        
        mockRepository.Setup(x => x.AddAsync(It.IsAny<Cliente>()))
            .Returns(Task.CompletedTask);
        mockUnitOfWork.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);
        
        var useCase = new CrearClienteUseCase(mockRepository.Object, mockUnitOfWork.Object, _mapper);
        var input = TestDataBuilder.CreateValidClienteDto();
        
        // Act
        var resultado = await useCase.ExecuteAsync(input, usuarioId);
        
        // Assert
        resultado.Should().NotBeNull();
        resultado.Nombre.Should().Be(input.Nombre);
        resultado.Email.Should().Be(input.Email);
        resultado.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        
        mockRepository.Verify(x => x.AddAsync(It.IsAny<Cliente>()), Times.Once);
        mockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
    }
    
    [Fact]
    public async Task Execute_ShouldSetCreatedBy()
    {
        // Arrange
        var mockRepository = new Mock<IClienteRepository>();
        var mockUnitOfWork = new Mock<IUnitOfWork>();
        var usuarioId = Guid.NewGuid();
        Cliente? capturedCliente = null;
        
        mockRepository.Setup(x => x.AddAsync(It.IsAny<Cliente>()))
            .Callback<Cliente>(c => capturedCliente = c)
            .Returns(Task.CompletedTask);
        mockUnitOfWork.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);
        
        var useCase = new CrearClienteUseCase(mockRepository.Object, mockUnitOfWork.Object, _mapper);
        var input = TestDataBuilder.CreateValidClienteDto();
        
        // Act
        await useCase.ExecuteAsync(input, usuarioId);
        
        // Assert
        capturedCliente.Should().NotBeNull();
        capturedCliente!.CreatedBy.Should().Be(usuarioId);
    }
}
```

- [ ] **Step 5: Commit tests**

```bash
cd c:\Users\USUARIO\Documents\Github\Nexit_Back
git add tests/Nexit.Tests/
git commit -m "test: add unit tests for CreateClienteValidator and CrearClienteUseCase"
```

---

## Task 22: Ejecutar y validar solución

**Files:**
- No hay archivos nuevos

- [ ] **Step 1: Restaurar dependencias**

```bash
cd c:\Users\USUARIO\Documents\Github\Nexit_Back
dotnet restore
```

- [ ] **Step 2: Compilar solución**

```bash
dotnet build
```

Expected output: `Build succeeded.`

- [ ] **Step 3: Ejecutar tests**

```bash
dotnet test
```

Expected output: Todos los tests pasan.

- [ ] **Step 4: Crear migración inicial**

```bash
cd src/Nexit.API
dotnet ef migrations add InitialCreate --project ../Nexit.Infrastructure
```

- [ ] **Step 5: Verificar migración**

```bash
dir ..\Nexit.Infrastructure\Migrations\
```

Expected: Archivo `*_InitialCreate.cs` existe.

- [ ] **Step 6: Commit versión compilable**

```bash
cd c:\Users\USUARIO\Documents\Github\Nexit_Back
git add .
git commit -m "chore: add initial EF Core migration, all tests passing"
```

---

## Task 23: Documentación final - README actualizado

**Files:**
- Modify: `README.md` (ya existe del Task 1, actualizamos)

- [ ] **Step 1: Verificar README**

El archivo creado en Task 1 ya contiene los pasos de setup. Listo.

- [ ] **Step 2: Crear .gitkeep en carpetas de logs**

```bash
mkdir -p logs
echo "# Logs directory" > logs/.gitkeep
git add logs/.gitkeep
```

---

## Task 24: Commit final y verificación

**Files:**
- No hay archivos nuevos

- [ ] **Step 1: Verificar estado de git**

```bash
cd c:\Users\USUARIO\Documents\Github\Nexit_Back
git status
```

Expected: Working tree clean.

- [ ] **Step 2: Ver log de commits**

```bash
git log --oneline | head -20
```

Expected: 15+ commits con mensajes claros.

- [ ] **Step 3: Commit final**

```bash
git log --oneline | wc -l
```

Si todo está listo:

```bash
git tag -a v0.1.0-base -m "Backend base - Clean Architecture structure, EF Core, validators, use cases, tests"
git push origin main
git push origin v0.1.0-base
```

(Si no tienes remote configurado, omite los `push`).

---

## Summary

✅ **Nexit Backend Base completo**

- Solución .NET 8 con Clean Architecture (4 proyectos)
- DbContext EF Core mapeado a Supabase PostgreSQL
- JWT autenticación integrada
- FluentValidation para DTOs
- AutoMapper para Entity ↔ DTO
- UnitOfWork + Repository pattern
- Global middleware para manejo de errores
- Swagger/OpenAPI documentado
- xUnit tests con Moq
- Logging con Serilog
- Toda la estructura lista para agregar CRUD de catálogos (Opción 2)

**Duración estimada:** 3-4 horas  
**Commits:** 15+  
**Tests:** 5+ pasando  
**Status:** Compilable, runnable, testeable
