# 32 — Campos obligatorios vs. opcionales por formulario, y teléfono ya no obligatorio en Clientes

## Por qué

Al migrar el histórico real de proyectos (`docs/31`, ver también memoria de proyecto), quedó claro que el Excel de seguimiento nunca tuvo teléfono de los 126 clientes reales — y el validador de Clientes SÍ lo exigía. La usuaria pidió, primero, quitar esa obligatoriedad, y después, revisar qué es obligatorio y qué no en TODOS los formularios del sistema, no solo en Clientes.

## Cambio aplicado: teléfono ya no es obligatorio en Clientes

`CreateClienteValidator` y `UpdateClienteValidator` (`src/Nexit.Application/Validators/Clientes/`) tenían esta regla, que ya no existe:

```csharp
RuleFor(x => x.Telefonos).NotEmpty().WithMessage("Al menos un teléfono es requerido");
```

Se eliminó de ambos validadores. Se mantiene la regla que ya existía sobre cada teléfono individual (si escribes uno, no puede quedar vacío ni pasar de 50 caracteres) — solo cambia que la lista completa ahora puede quedar vacía. No hizo falta tocar la base de datos: los teléfonos siempre vivieron en una tabla aparte (`cliente_telefonos`), nunca fue una columna `NOT NULL` de `clientes`, así que esto era puramente una regla de la aplicación (FluentValidation), no del esquema.

Se revisaron las pruebas existentes (`ClientesTests.cs`, `ClientesFunctionalTests.cs`, `ClientesImportExporterTests.cs`, `SeguridadFunctionalTests.cs`): ninguna verificaba específicamente que crear un cliente sin teléfono fallara, así que el cambio no rompe ninguna prueba existente — de todos modos, **falta correr `dotnet build`/`dotnet test` en un entorno con el SDK de .NET instalado** (el puente a tu computador para esta sesión no tenía el SDK disponible) antes de dar esto por cerrado del todo.

Nota de diseño: Proveedores nunca exigió teléfono (siempre fue una lista libre, igual que ahora Clientes). Esta asimetría entre Clientes y Proveedores no estaba documentada en ningún lado como decisión deliberada — todo indica que fue un descuido al construir el validador de Clientes, no una decisión de negocio.

## Auditoría completa: qué es obligatorio hoy en cada formulario

"Obligatorio" = si lo omites o mandas vacío, el backend responde 400 y no crea/edita nada. Todo lo demás es opcional (puede quedar vacío/null).

### Clientes (crear / editar)

| Campo | ¿Obligatorio? | Detalle |
|---|---|---|
| Nombre | **Sí** | Máx. 255 caracteres |
| Email | No | Si lo escribes, debe ser un email válido y no puede repetirse con otro cliente |
| Teléfono(s) | No (cambiado hoy) | Si agregas uno, no puede quedar vacío ni pasar de 50 caracteres |
| Sector, Ciudad, Dirección, Web, Contacto, Cargo del contacto, Valor de referencia, Notas | No | Sin ninguna validación de formato |

### Proveedores (crear / editar)

| Campo | ¿Obligatorio? | Detalle |
|---|---|---|
| Nombre | **Sí** | Máx. 255 caracteres |
| País | **Sí** | Debe existir en Catálogos |
| Categoría | **Sí** | Debe existir en Catálogos |
| Estado | No | Texto libre, "Activo" por defecto si no lo mandas |
| Email | No | Si lo escribes, debe ser válido y no repetirse |
| Score | No | Si lo escribes, debe estar entre 1 y 5 |
| Teléfono(s) | No | Igual que Clientes: si agregas uno, no puede quedar vacío ni pasar de 50 caracteres |
| Región, Ciudad, Contacto, Cargo del contacto, Web, Dirección, Aforo, Costo de referencia, Presupuesto, Cobertura, Notas, Servicios | No | Sin validación de formato (Ciudad, si la mandas, debe pertenecer a la Región/País indicados — lo revisa un trigger en la base) |

### Proyectos (crear / editar)

| Campo | ¿Obligatorio? | Detalle |
|---|---|---|
| Nombre | **Sí** | Máx. 255 caracteres |
| Estado | **Sí** | Debe existir en Catálogos (`estados_proyecto`) |
| % de avance | **Sí, pero con valor por defecto 0** | Debe estar entre 0 y 100 si lo mandas |
| Estado del brief | **Sí, pero con valor por defecto "Pendiente por enviar"** | Debe ser uno de los 4 valores válidos |
| Estado de la propuesta | **Sí, pero con valor por defecto "No enviada"** | Debe ser uno de los 3 valores válidos |
| Fecha de pago | **Sí, solo si marcas Pagado = Sí** | Si no la mandas y Pagado = Sí, el backend usa la fecha de hoy |
| Cliente | No | Si lo mandas, debe existir |
| Tipo de proyecto | No | Si lo mandas, debe ser "Corporativo" o "Evento social" |
| Prioridad | No | Si la mandas, debe ser "Alta", "Media" o "Baja" |
| Cada integrante del Equipo (si agregas alguno) | Nombre **sí**, Rol **sí** (uno de los 5 roles válidos) | La lista de Equipo en sí es opcional (puede ir vacía) |
| Contacto del proyecto, Ciudad, Sede Next, Fecha de solicitud, Fecha del evento, N.º de factura, Notas, Proveedores asociados, Gerente | No | Sin validación de formato |

### Usuarios (crear el perfil de negocio / editar)

| Campo | ¿Obligatorio? | Detalle |
|---|---|---|
| Id (UUID de Supabase Auth) | **Sí, al crear** | Debe existir ya en Supabase Auth (se invita desde allá primero) |
| Nombre | **Sí** | Máx. 255 caracteres |
| Apellido | **Sí** | Máx. 255 caracteres |
| Email | **Sí, al crear** | Formato válido, no repetido, y de un dominio laboral permitido (`agencianextmkt.com`) |
| Rol | **Sí, pero con valor por defecto "miembro"** | Debe ser super_admin/admin/manager/miembro |
| Iniciales, Activo | No | Activo llega en `true` por defecto |

### Invitaciones de equipo

| Campo | ¿Obligatorio? | Detalle |
|---|---|---|
| Email | **Sí** | Formato válido, de dominio permitido, sin usuario existente ni invitación pendiente con ese correo |
| Rol | **Sí** | Debe ser uno de los 4 roles válidos |
| Mensaje | No | Máx. 500 caracteres si lo escribes |

### Solicitudes de eliminación

| Campo | ¿Obligatorio? | Detalle |
|---|---|---|
| Tipo de entidad | **Sí** | "cliente", "proveedor" o "proyecto" |
| Id de la entidad | **Sí** | Debe existir de verdad ese cliente/proveedor/proyecto |
| Motivo | No | Texto libre |

## Qué no cubre esta auditoría

No incluye reglas que viven solo en el frontend (`Nexit_Front`) si las hay — esto es exclusivamente lo que el backend exige, que es lo que manda de verdad (el frontend puede pedir más, nunca menos, sin que el backend lo rechace). Tampoco incluye Catálogos (país/ciudad/categoría/estado/servicio) porque son formularios de administración simple sin reglas particulares más allá de Nombre obligatorio y único.
