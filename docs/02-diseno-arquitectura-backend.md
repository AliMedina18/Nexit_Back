# Backend Nexit - Clean Architecture Base (Opción 1)

**Proyecto:** Nexit — Sistema de gestión para Next (agencia experiencial)  
**Fase:** 1 — Construcción backend (base arquitectónica)  
**Fecha:** 2026-08-17  
**Autor:** Diseño colaborativo

---

## 1. Objetivo

Construir la base del backend en C# / ASP.NET Core 8 con Clean Architecture, que sirva como columna vertebral para las fases 2-3 (CRUD de catálogos, clientes, proveedores, proyectos). La base debe ser:
- **Testeable:** cada capa desacoplada, inyección de dependencias clara
- **Mantenible:** responsabilidades separadas, convenciones consistentes
- **Escalable:** agregar nuevos dominios (clientes, proveedores) sin tocar la infraestructura
- **Segura:** autenticación JWT, autorización, manejo global de errores

---

## 2. Decisiones Arquitectónicas Clave

### 2.1 Patrón de Arquitectura: Clean Architecture Modular

La solución consta de **4 proyectos .NET** organizados por capas horizontales (no por features):

| Proyecto | Responsabilidad | Dependencias |
|----------|-----------------|--------------|
| **Nexit.Core** | Entidades, enums, excepciones, interfaces de contrato | Ninguna (core puro) |
| **Nexit.Application** | DTOs, validators (FluentValidation), use cases, mappers (AutoMapper) | Core |
| **Nexit.Infrastructure** | EF Core DbContext, repositorios, UnitOfWork, Supabase client | Core, Application |
| **Nexit.API** | Controllers, middleware, configuración ASP.NET Core | Core, Application, Infrastructure |

**Data Flow (ejemplo: crear cliente):**
```
POST /api/clientes
  ↓ [CreateClienteDto entra]
  → Controller [Authorize]
  → FluentValidation (automático en pipeline)
  → UseCase: CrearClienteUseCase
    → IClienteRepository.AddAsync()
    → Implementation: ClienteRepository (Infrastructure)
      → EF Core DbContext
      → PostgreSQL/Supabase
  → AutoMapper: Entity → ClienteResponseDto
  → return 201 Created + LocationHeader
```

### 2.2 Stack Tecnológico

- **.NET 8 LTS** — framework base
- **ASP.NET Core Web API** — servidor REST
- **Entity Framework Core 8** — ORM
- **Npgsql.EntityFrameworkCore.PostgreSQL 8** — driver PostgreSQL
- **FluentValidation 11** — validadores declarativos
- **AutoMapper 13** — mapeo Entity ↔ DTO
- **Swashbuckle.AspNetCore 6** — Swagger/OpenAPI
- **Serilog 4** — logging estructurado
- **xUnit, Moq, FluentAssertions** — testing

### 2.3 Autenticación y Autorización

- **Token origen:** Supabase Auth (no construimos signup/login propios)
- **Validación:** JWT Bearer token en header `Authorization: Bearer <token>`
- **Middleware:** `JwtBearerDefaults.AuthenticationScheme` en `Program.cs`
- **Contexto de usuario:** Disponible via `HttpContext.User` (claims)
- **Atributo `[Authorize]`:** En controllers, valida presencia de token válido

**No incluido en esta fase:** Lógica de roles finogranulares (admin/manager/miembro). Se valida token, punto.

### 2.4 Gestión de Errores Global

Un **middleware centralizado** captura todas las excepciones y retorna un DTO consistente:

```json
{
  "statusCode": 400,
  "message": "Validación fallida",
  "errors": {
    "email": ["Email es requerido", "Formato inválido"],
    "nombre": ["Máximo 255 caracteres"]
  },
  "traceId": "0HN6SCCTG5QC3:00000001"
}
```

**Mapeo de excepciones:**
- `ValidationException` → 400 Bad Request
- `EntityNotFoundException` → 404 Not Found
- `BusinessRuleException` → 409 Conflict
- Cualquier otra → 500 Internal Server Error + TraceId (para debugging)

---

## 3. Estructura de Carpetas

```
Nexit_Back/
├── docs/
│   ├── 01-analisis-fase1.md
│   ├── schema/
│   │   ├── nexus_schema_v2.sql
│   │   └── seed_geografia_categorias_estados.sql
│   └── superpowers/
│       └── specs/
│           └── 2026-08-17-backend-clean-architecture-design.md (este archivo)
│
├── src/
│   ├── Nexit.Core/
│   │   ├── Entities/ → Usuario, Cliente, Proveedor, Proyecto, etc.
│   │   ├── Enums/ → ProvedorEstado, PrioridadProyecto, RolEquipo, etc.
│   │   ├── Exceptions/ → EntityNotFoundException, BusinessRuleException
│   │   └── Interfaces/ → IRepository, IClienteRepository, IUnitOfWork
│   │
│   ├── Nexit.Application/
│   │   ├── DTOs/ → (Clientes/, Proveedores/, etc.)
│   │   ├── Validators/ → CreateClienteValidator, UpdateClienteValidator, etc.
│   │   ├── MappingProfiles/ → ClienteProfile, ProveedorProfile (AutoMapper)
│   │   ├── UseCases/ → (Clientes/, Proveedores/, etc.)
│   │   ├── Exceptions/ → ValidationException, ApplicationException
│   │   └── DependencyInjection.cs → extensión para registrar servicios
│   │
│   ├── Nexit.Infrastructure/
│   │   ├── Data/
│   │   │   ├── NexitDbContext.cs
│   │   │   ├── DesignTimeDbContextFactory.cs
│   │   │   └── Migrations/ → auto-generadas por EF Core
│   │   ├── Repositories/ → Repository<T>, ClienteRepository, etc.
│   │   ├── UnitOfWork/ → UnitOfWork.cs
│   │   └── DependencyInjection.cs
│   │
│   └── Nexit.API/
│       ├── Program.cs → configuración startup, DI, middleware
│       ├── appsettings.json
│       ├── appsettings.Development.json
│       ├── appsettings.Production.json
│       ├── Controllers/ → BaseController, ClientesController, etc.
│       ├── Middleware/ → GlobalExceptionHandlerMiddleware, JwtMiddleware
│       └── Extensions/ → ServiceCollectionExtensions
│
├── tests/
│   └── Nexit.Tests/
│       ├── Fixtures/ → TestDataBuilder
│       ├── UseCases/ → CrearClienteUseCaseTests
│       ├── Repositories/ → ClienteRepositoryTests
│       └── Validators/ → CreateClienteValidatorTests
│
├── Nexit.sln
├── .gitignore
├── .env.example
└── README.md
```

---

## 4. Configuración y Startup

### 4.1 appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=db.supabase.co;Database=postgres;Username=postgres;Password=***;Port=5432;SSL Mode=Require;"
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
      "Microsoft.EntityFrameworkCore": "Warning"
    }
  }
}
```

### 4.2 Program.cs (pseudocódigo de estructura)

```csharp
var builder = WebApplicationBuilder.CreateBuilder(args);

// Registrar servicios
builder.Services.AddCore();                    // Custom extensions
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddControllers();

// Auth JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => {
        options.Authority = configuration["Jwt:Authority"];
        options.Audience = configuration["Jwt:Audience"];
        // ...
    });

builder.Services.AddSwaggerGen(options => {
    // Configurar Swagger con autenticación JWT
});

var app = builder.Build();

// Middleware
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

---

## 5. Validación y DTOs

### 5.1 Ejemplo: Cliente

**DTO (Application layer):**
```csharp
public class CreateClienteDto
{
    public string Nombre { get; set; }
    public string Email { get; set; }
    public string Contacto { get; set; }
    public string CargoContacto { get; set; }
    public List<ClienteTelefonoDto> Telefonos { get; set; }
    // ... más campos
}

public class ClienteTelefonoDto
{
    public string Telefono { get; set; }
    public string Etiqueta { get; set; }
}
```

**Validator (FluentValidation):**
```csharp
public class CreateClienteValidator : AbstractValidator<CreateClienteDto>
{
    private readonly IClienteRepository _repository;
    
    public CreateClienteValidator(IClienteRepository repository)
    {
        _repository = repository;
        
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre es requerido")
            .MaximumLength(255).WithMessage("Máximo 255 caracteres");
        
        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Email con formato inválido")
            .MustAsync(async (email, _) => !(await _repository.ExistsByEmailAsync(email)))
            .WithMessage("Email ya registrado");
        
        RuleFor(x => x.Telefonos)
            .NotEmpty().WithMessage("Al menos un teléfono es requerido")
            .ForEach(t => t.RuleFor(tel => tel.Telefono).NotEmpty());
    }
}
```

**AutoMapper Profile:**
```csharp
public class ClienteProfile : Profile
{
    public ClienteProfile()
    {
        // DTO → Entity (entrada)
        CreateMap<CreateClienteDto, Cliente>()
            .ForMember(dest => dest.Telefonos, opt => opt.MapFrom(src => src.Telefonos));
        
        // Entity → DTO (salida)
        CreateMap<Cliente, ClienteResponseDto>();
    }
}
```

---

## 6. Use Cases y Servicios

### 6.1 Interfaz y Implementación

```csharp
// Application layer: contrato
public interface ICrearClienteUseCase
{
    Task<ClienteResponseDto> ExecuteAsync(CreateClienteDto input, Guid usuarioId);
}

// Application layer: implementación
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
        // Mapear DTO → Entity
        var cliente = _mapper.Map<Cliente>(input);
        cliente.CreatedBy = usuarioId;
        cliente.CreatedAt = DateTime.UtcNow;
        
        // Guardar
        await _repository.AddAsync(cliente);
        await _unitOfWork.SaveChangesAsync();
        
        // Mapear Entity → DTO
        return _mapper.Map<ClienteResponseDto>(cliente);
    }
}
```

### 6.2 Inyección en DependencyInjection.cs

```csharp
public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // AutoMapper
        services.AddAutoMapper(typeof(ApplicationServiceCollectionExtensions));
        
        // FluentValidation
        services.AddFluentValidationAutoValidation();
        services.AddValidatorsFromAssemblyContaining(typeof(ApplicationServiceCollectionExtensions));
        
        // Use Cases
        services.AddScoped<ICrearClienteUseCase, CrearClienteUseCase>();
        services.AddScoped<IActualizarClienteUseCase, ActualizarClienteUseCase>();
        // ... más use cases
        
        return services;
    }
}
```

---

## 7. Controllers y Endpoints

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ClientesController : ControllerBase
{
    private readonly ICrearClienteUseCase _crearUseCase;
    private readonly ILogger<ClientesController> _logger;
    
    public ClientesController(ICrearClienteUseCase crearUseCase, ILogger<ClientesController> logger)
    {
        _crearUseCase = crearUseCase;
        _logger = logger;
    }
    
    /// <summary>Crear un nuevo cliente</summary>
    /// <param name="dto">Datos del cliente</param>
    /// <returns>Cliente creado con status 201</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ClienteResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateClienteDto dto)
    {
        var usuarioId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
        var resultado = await _crearUseCase.ExecuteAsync(dto, usuarioId);
        
        return CreatedAtAction(nameof(GetById), new { id = resultado.Id }, resultado);
    }
    
    // ... más métodos (Get, Update, Delete, etc.)
}
```

---

## 8. Testing

### 8.1 Unit Test: Validator

```csharp
public class CreateClienteValidatorTests
{
    [Fact]
    public async Task Validate_ConDatosValidos_DebeSerExitoso()
    {
        // Arrange
        var mockRepository = new Mock<IClienteRepository>();
        mockRepository.Setup(x => x.ExistsByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(false);
        
        var validator = new CreateClienteValidator(mockRepository.Object);
        var dto = new CreateClienteDto
        {
            Nombre = "SURA",
            Email = "contacto@sura.com",
            Telefonos = new List<ClienteTelefonoDto> 
            { 
                new() { Telefono = "555-1234" } 
            }
        };
        
        // Act
        var result = await validator.ValidateAsync(dto);
        
        // Assert
        result.IsValid.Should().BeTrue();
    }
    
    [Fact]
    public async Task Validate_ConEmailDuplicado_DebeRetornarError()
    {
        // Arrange
        var mockRepository = new Mock<IClienteRepository>();
        mockRepository.Setup(x => x.ExistsByEmailAsync("contacto@sura.com"))
            .ReturnsAsync(true);
        
        var validator = new CreateClienteValidator(mockRepository.Object);
        var dto = new CreateClienteDto { Email = "contacto@sura.com" };
        
        // Act
        var result = await validator.ValidateAsync(dto);
        
        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
    }
}
```

### 8.2 Unit Test: Use Case

```csharp
public class CrearClienteUseCaseTests
{
    [Fact]
    public async Task Execute_ConDatosValidos_DebeGuardarYRetornarDto()
    {
        // Arrange
        var mockRepository = new Mock<IClienteRepository>();
        var mockUnitOfWork = new Mock<IUnitOfWork>();
        var mockMapper = new Mock<IMapper>();
        
        var useCase = new CrearClienteUseCase(mockRepository.Object, mockUnitOfWork.Object, mockMapper.Object);
        var input = new CreateClienteDto { Nombre = "SURA", Email = "contacto@sura.com" };
        var clienteEntity = new Cliente { Id = Guid.NewGuid(), Nombre = "SURA" };
        var output = new ClienteResponseDto { Id = clienteEntity.Id, Nombre = "SURA" };
        
        mockMapper.Setup(m => m.Map<Cliente>(input)).Returns(clienteEntity);
        mockMapper.Setup(m => m.Map<ClienteResponseDto>(clienteEntity)).Returns(output);
        
        // Act
        var resultado = await useCase.ExecuteAsync(input, Guid.NewGuid());
        
        // Assert
        resultado.Should().NotBeNull();
        resultado.Id.Should().Be(clienteEntity.Id);
        mockRepository.Verify(x => x.AddAsync(It.IsAny<Cliente>()), Times.Once);
        mockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
    }
}
```

---

## 9. Migraciones EF Core

**Workflow:**
1. Definir cambios en Entities (Core)
2. Ejecutar: `dotnet ef migrations add NombreDelCambio --project src/Nexit.Infrastructure --startup-project src/Nexit.API`
3. Se genera archivo en `src/Nexit.Infrastructure/Migrations/`
4. Ejecutar: `dotnet ef database update --project src/Nexit.Infrastructure --startup-project src/Nexit.API`
5. Commit de la migración a Git

**DesignTimeDbContextFactory.cs:**
```csharp
public class NexitDbContextFactory : IDesignTimeDbContextFactory<NexitDbContext>
{
    public NexitDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();
        
        var optionsBuilder = new DbContextOptionsBuilder<NexitDbContext>();
        optionsBuilder.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
        
        return new NexitDbContext(optionsBuilder.Options);
    }
}
```

---

## 10. Logging y Monitoreo

**Serilog en Program.cs:**
```csharp
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("logs/nexit-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();
```

**En cualquier servicio:**
```csharp
_logger.LogInformation("Cliente {ClienteId} creado por usuario {UsuarioId}", cliente.Id, usuarioId);
_logger.LogError(ex, "Error al guardar cliente {ClienteId}", cliente.Id);
```

---

## 11. Qué NO está incluido en esta fase

❌ CRUD concreto de clientes, proveedores, proyectos, catálogos  
❌ Lógica de negocio de relaciones complejas (país→región→ciudad)  
❌ Validaciones de RLS (Row Level Security) de Supabase  
❌ Integración con Supabase Auth (solo JWT validation)  
❌ Tests de integración con BD real  
❌ Documentación de API en Postman  

Estas entran en **Opción 2 (catálogos)** y **Opción 3 (clientes, proveedores, proyectos)**.

---

## 12. Entregables al Final de esta Fase

✅ Solución compilable y runnable: `dotnet run` en Nexit.API  
✅ Swagger funcionando en `https://localhost:5001/swagger`  
✅ Todas las capas comunicándose (Controller → UseCase → Repository → DbContext)  
✅ Middleware global de errores funcionando  
✅ Al menos 5 tests unitarios (validators, use cases)  
✅ .env.example y README con pasos de setup  
✅ Git con commits atómicos y claros  

---

## 13. Siguientes Pasos (Opción 2 y 3)

**Opción 2 (CRUD Catálogos):**
- Entidades: Pais, Region, Ciudad, CategoriaProveedor, FaseProyecto, EstadoProyecto, Servicio
- Endpoints GET (listar, filtrar por pais), POST/PUT/DELETE (solo admin)
- Validadores: nombre único, FK coherentes

**Opción 3 (CRUD Clientes, Proveedores, Proyectos):**
- Entidades: Cliente, Proveedor, Proyecto + sus relaciones
- Endpoints CRUD completos
- Validadores cruzados: región ⊂ país, teléfono único por cliente, etc.
- Seguimiento y adjuntos

---

**Diseño validado:** 2026-08-17  
**Estado:** Listo para plan de implementación
