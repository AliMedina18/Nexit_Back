# Prioridad para proveedores y clientes — la misma lógica, en C#/.NET

Como pediste: nada de IA ni de gasto en API externa. Lo que se tradujo a Nexit no es código de los
dos repositorios (eso no se puede, ver `docs/23`) sino su **lógica** — reglas transparentes y con
razones explícitas — extendida ahora de proyectos (`docs/22`) a **proveedores** y **clientes**,
100% en C#/.NET, sin nada nuevo que dependa de internet ni de pagar por nada.

## Qué hay nuevo

### `GET /api/proveedores/prioridad`

Todos los proveedores que no estén "Bloqueado", puntuados y ordenados de mayor a menor, con las
razones de cada puntaje:

```json
[
  {
    "proveedorId": "...",
    "nombre": "Salón Los Cerezos",
    "puntaje": 60,
    "razones": [
      "Bien calificado (Score 5/5) pero sin proyectos en los últimos 120 días.",
      "3 personas del equipo lo tienen marcado como colaborador, pero no tiene ningún proyecto formal asociado todavía."
    ]
  }
]
```

**Señales y pesos** (punto de partida, igual que en `docs/22`):

| Señal | De dónde sale | Puntos |
|---|---|---|
| Calificado con Score bajo (1 o 2 de 5) | `Score` | 40 |
| Nunca se le puso Score, pero ya se ha usado en proyectos | `Score` + `ProyectoProveedor` | 20 |
| Bien calificado (Score 4 o 5) pero sin proyectos en los últimos 90 días, o nunca asignado | `Score` + `ProyectoProveedor` | 40 |
| Varias personas (2+) lo tienen marcado como colaborador sin que tenga ningún proyecto formal | `ProveedorColaborador` | 20 |

Por qué estas señales exactamente: `docs/21` ya había dejado anotado el campo `Score` (existe, no lo
usaba ninguna lógica) combinado con "hace cuánto no se le asigna un proyecto" y "cuánta gente lo
tiene marcado como colaborador". El hallazgo del proyecto de referencia sobre cadena de suministro
(`docs/23`) — que la confiabilidad de un proveedor predice mejor los problemas que su velocidad —
es lo que justifica seguirle dando peso real al `Score` en vez de ignorarlo.

Ojo: acá "prioridad" no significa "urgencia" como en proyectos, sino "vale la pena que alguien le
preste atención" — puede ser un buen proveedor que se está dejando de aprovechar, uno mal calificado
al que se le sigue asignando trabajo, o uno que nunca se calificó.

### `GET /api/clientes/prioridad`

Todos los clientes, puntuados y ordenados igual:

```json
[
  {
    "clienteId": "...",
    "nombre": "Café Central",
    "puntaje": 35,
    "razones": ["Sin proyectos nuevos hace 95 días."]
  }
]
```

**Señales y pesos:**

| Señal | De dónde sale | Puntos |
|---|---|---|
| Sin proyectos nuevos en los últimos 90 días | `CreatedAt` del proyecto más reciente | 35 |
| Cliente registrado sin ningún proyecto todavía | — | 20 |
| Cliente frecuente: 3 o más proyectos activos ahora mismo | `Proyecto.EstadoId` (excluyendo estados terminales) | 25 |

Esto es la parte de "recencia" y "frecuencia" del modelo RFM que ya se investigó en `docs/21`. Se
dejó fuera a propósito el eje de "valor monetario" del RFM clásico — `Cliente` no tiene todavía un
campo numérico confiable para eso (`ValorReferencia` es texto libre, no un número); es una
extensión posible más adelante si se decide capturar esa cifra de verdad.

## Por qué quedó así (decisiones de diseño)

- **Sigue sin haber IA en ningún punto**, tal como confirmaste. Es la misma aritmética por reglas
  que ya tenía `docs/22`, solo que ahora cubre tres entidades en vez de una.
- **Un solo endpoint de solo lectura por entidad, sin cambios de esquema.** Todo se calcula al
  vuelo con datos que ya existen — no se guarda ningún puntaje en ninguna tabla nueva, igual que en
  `docs/22`.
- **`ProveedorRepository` y `ClienteRepository` ahora traen sus proyectos asociados** (antes no
  hacía falta) — es lo único que cambió en la capa de datos, y solo para poder calcular "hace
  cuánto no se usa" y "cuántos proyectos activos tiene".
- **Se centralizó la lista de estados terminales** (Finalizado/Cancelado/No ejecutado/Facturado) en
  un solo lugar (`EstadosProyectoTerminales`) para que proyectos y clientes usen exactamente la
  misma definición de "proyecto activo", en vez de tenerla repetida en dos sitios.

## Verificación

213 pruebas en total (206 pasan, 7 dependen de Docker en este entorno — el mismo grupo de siempre),
22 nuevas para esta pieza: cada señal de proveedores y de clientes por separado, que se suman
correctamente cuando aplica más de una a la vez, que se excluyen los proveedores bloqueados, que se
usa el proyecto MÁS reciente (no cualquiera) para calcular hace cuánto no se usa un proveedor, y que
solo los proyectos en estado no-terminal cuentan como "activos" de un cliente.

## Próximos pasos posibles (sin construir todavía)

- Ajustar los pesos y umbrales (90 días, Score 4/2, etc.) con casos reales, como en `docs/22`.
- Si algún día se decide capturar un valor numérico real por cliente, sumar el eje "valor
  monetario" del RFM que quedó pendiente acá.
- La idea de `docs/23` de generar una notificación automática cuando algo cruce cierto puntaje
  (en vez de que alguien tenga que entrar a mirar el endpoint) sigue disponible para cuando quieras.
