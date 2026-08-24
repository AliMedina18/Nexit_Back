# Sistema de prioridad — Nivel 1 construido (proyectos)

Siguiendo la decisión de `docs/21` ("deberíamos probar primero con el algoritmo que nosotros vamos a crear"), esto es el **Nivel 1** de la propuesta ya construido y probado: una rúbrica de puntos transparente, sin IA, aplicada a proyectos. Todavía no incluye proveedores ni clientes, ni el microagente de IA (Nivel 2) — quedan para cuando se confirme avanzar con ellos.

## Cómo funciona

`GET /api/proyectos/prioridad` devuelve todos los proyectos que no estén en un estado terminal (Finalizado, Cancelado, No ejecutado, Facturado — no tiene sentido priorizar algo que ya se cerró), cada uno con un puntaje de 0 a 100 y **la lista de razones que lo explican**, ordenados de mayor a menor puntaje:

```json
[
  {
    "proyectoId": "...",
    "nombre": "Lanzamiento Q4",
    "puntaje": 75,
    "razones": [
      "El evento es en 3 día(s).",
      "Marcado con prioridad alta.",
      "La propuesta todavía no se ha enviado.",
      "Todavía no está pagado y el evento ya está cerca."
    ]
  }
]
```

## Las señales y sus pesos (punto de partida, ajustable)

| Señal | De dónde sale | Puntos |
|---|---|---|
| El evento es en los próximos 7 días | `FechaEvento` | 30 |
| El evento es dentro del próximo mes (8-30 días) | `FechaEvento` | 15 |
| Prioridad marcada como "alta" | `Prioridad` (texto libre) | 25 |
| Prioridad marcada como "media" | `Prioridad` | 10 |
| Sin actividad en la bitácora hace 5+ días | Entrada más reciente de `Seguimiento` (o la fecha de creación, si nunca tuvo ninguna) | 20 |
| Propuesta y/o brief todavía pendientes | `PropuestaEstado` / `EstadoBrief` | 15 (una sola vez, aunque falten los dos) |
| Sin pagar y el evento ya está a 14 días o menos | `Pagado` + `FechaEvento` | 10 |

Como se dejó anotado en `docs/21`: estos pesos son un punto de partida razonado, no el resultado de mirar datos reales de Next todavía. Todos están centralizados como constantes en `PrioridadProyectoCalculador` (una sola clase, fácil de encontrar y ajustar) — cuando quieras, miramos juntos unos cuantos proyectos reales y afinamos los números con lo que de verdad importó en esos casos, tal como recomienda la investigación de `docs/21`.

## Por qué quedó así (decisiones de diseño)

- **Sin IA todavía, tal como pediste.** Es 100% reglas — rápido, gratis, auditable. El Nivel 2 (el microagente que lee notas de texto libre) sigue disponible como siguiente paso si más adelante quieres sumarlo.
- **La "actividad reciente" se mide con la bitácora de seguimiento, no con el historial de cambios.** Ambos existen en el sistema, pero la bitácora (`ProyectoSeguimiento`) es la señal más directa de "alguien sigue trabajando en esto activamente" — una edición de un campo cualquiera (el historial de cambios de `docs/20`) no necesariamente significa lo mismo. Se puede sumar como señal adicional más adelante si hace falta.
- **Las fechas se comparan en UTC, no por sede como el calendario (`docs/18`).** Para decidir en qué mes cae un evento, un día de diferencia importa (cambia el mes). Para decidir "¿está a 5 o a 6 días?", un margen de un día no cambia el orden de prioridad de forma práctica — así que acá se mantuvo simple a propósito, sin la complejidad de zona horaria por sede.
- **Un solo endpoint de solo lectura**, sin cambios de esquema — el puntaje se calcula al vuelo con datos que ya existen, no se guarda en ninguna tabla nueva.

## Verificación

191 pruebas en total (184 pasan, 7 dependen de Docker en este entorno — el mismo grupo de siempre), 21 nuevas para esta pieza: cada señal de la rúbrica por separado, que las señales se suman correctamente cuando aplican varias a la vez, que un proyecto sin ninguna bitácora todavía puede puntuarse (usa su fecha de creación), que los proyectos en estado terminal quedan excluidos, y que el resultado sale ordenado de mayor a menor puntaje.

## Próximos pasos posibles (sin construir todavía, a la espera de tu decisión)

- Extender la misma rúbrica a proveedores y clientes (`docs/21` ya deja ejemplos de qué señales usar para proveedores: el campo `Score` que hoy no se usa en ninguna lógica, hace cuánto no se le asigna a un proyecto, cuánta gente lo tiene marcado como colaborador).
- Sumar el Nivel 2 (microagente de IA que lee `Notas`/bitácora) si el Nivel 1 solo no es suficiente.
- Ajustar los pesos con casos reales, como se explicó arriba.
