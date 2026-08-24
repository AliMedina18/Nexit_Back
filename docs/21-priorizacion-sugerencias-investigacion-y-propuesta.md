# Sistema de prioridad/sugerencias — investigación y propuesta

Esto responde a lo que pediste: investigar cómo lo resuelven otros sistemas (incluidos "microagentes de IA") antes de construir nada, para que el diseño quede bien pensado. Este documento es investigación + propuesta concreta — **todavía no hay código construido**, porque antes de escribirlo hace falta que confirmes un par de decisiones (sección 4).

## 1. Qué se investigó

Se revisaron tres familias de sistemas que resuelven el mismo problema de fondo — "de todos mis registros, ¿a cuál le dedico atención primero?" — en tres contextos distintos: ventas (CRM), compras (gestión de proveedores) y productividad personal con IA.

**Lead scoring en CRM** ([Monday.com](https://monday.com/blog/crm-and-sales/lead-scoring-rules/), [Capsule CRM](https://capsulecrm.com/blog/ai-lead-scoring-for-small-business/), [Digital Applied](https://www.digitalapplied.com/blog/build-ai-lead-scoring-agent-crm-2026)): el modelo más usado combina dos tipos de señal — señales de **encaje** (¿este prospecto se parece a mis mejores clientes?) y señales de **comportamiento** (¿está respondiendo, interactuando, mostrando urgencia?). El punto que más se repite en las tres fuentes, especialmente pensando en un equipo chico como el de Next: **no arrancar con machine learning**. Capsule CRM lo dice explícito — la predicción con ML "empieza a valer la pena con ~100 leads al mes y 6 meses de historial de decisiones" — por debajo de eso, un modelo de puntos simple y transparente (una "rúbrica" con pesos que cualquiera puede auditar) funciona mejor y es más fácil de confiar.

**RFM (Recencia, Frecuencia, Valor monetario)** ([Omniconvert](https://www.omniconvert.com/blog/rfm-score/)): el modelo clásico de priorización de clientes, usado desde antes de que existiera la IA. A cada cliente se le da un puntaje de 1 a 5 en tres ejes — hace cuánto fue la última interacción, cuántas veces ha interactuado, cuánto valor representa — y se combinan en un código (ej. "551" = interactuó hace poco, con frecuencia media, alto valor). Cada combinación cae en un segmento con una acción sugerida (ej. "en riesgo de perderse", "campeón", "hay que reactivarlo").

**Tiering de proveedores** ([ProQSmart](https://proqsmart.com/blog/strategic-supplier-tiering-prioritizing-vendors-for-procurement-success/)): en compras, el criterio no es solo "cuánto vale" sino cuatro cosas — volumen de negocio, qué tan crítico es para que la operación no se detenga, qué tan confiable ha sido cumpliendo, y si aporta algo que otros no. Con eso arman 3 niveles (estratégico / de apoyo / transaccional) y le dan más atención a los proveedores del nivel más alto.

**"Next best action" con IA** ([Inogic](https://www.inogic.com/blog/2026/05/how-ai-recommends-the-next-best-action-in-crm-with-real-examples/)): la capa de IA más nueva en CRMs grandes (Dynamics 365, Salesforce Einstein) no reemplaza el modelo de puntos — lo complementa. Lee el historial de actividad y, además del puntaje, sugiere **qué hacer** y **por qué** ("el cliente interactuó hace poco pero no ha respondido — mandar seguimiento para no perder el impulso"), con la razón siempre visible, no como una caja negra.

**Micro-agentes de IA (lo que preguntaste específicamente)** ([Digital Applied](https://www.digitalapplied.com/blog/build-ai-lead-scoring-agent-crm-2026), [Taskade](https://www.taskade.com/agents/crm/follow-up-reminder)): esto es lo más parecido a lo que imaginaste. El patrón que describe Digital Applied es exactamente un "micro-agente": un puntaje base se calcula con reglas simples (rápido, gratis, sin IA), y **solo cuando hay texto libre que un cálculo de reglas no puede leer** (notas, comentarios, bitácora), se manda ese texto puntual a un modelo de lenguaje (ellos usan Claude) con una pregunta muy acotada — "de este texto, ¿qué tan urgente es esto y por qué?" — y el modelo devuelve una etiqueta corta más la razón, nunca un número inventado de la nada. Es barato porque solo se usa IA para la parte que sí necesita entender lenguaje natural, no para todo. Un detalle importante que resaltan: **cambios grandes en el puntaje quedan pendientes de que una persona los confirme**, para que un error de la IA no reordene todo solo.

## 2. El patrón que se repite en las tres fuentes

Sin importar el sector (ventas, compras, productividad), el diseño que mejor funciona tiene siempre la misma forma:

1. Un puntaje o nivel calculado con **reglas simples y visibles** sobre datos que ya existen — no una caja negra.
2. Cada resultado viene con **la razón** de por qué quedó así, no solo el número.
3. La capa de IA (cuando la hay) se usa **solo** para leer texto libre que las reglas no pueden interpretar — no para todo el cálculo.
4. Una persona siempre puede revisar y ajustar — el sistema sugiere, no decide solo.

## 3. Propuesta concreta para Nexit

Lo bueno: **Nexit ya tiene casi todos los datos que estos modelos usan** — no hace falta pedirte que captures nada nuevo para arrancar con el nivel 1. Esto es justo el tipo de decisión que, según lo investigado, conviene construir por etapas.

### Nivel 1 — Rúbrica de puntos (sin IA, se puede construir ya)

Un puntaje calculado con los campos que ya existen en Proyecto/Cliente/Proveedor. Ejemplo de cómo se vería para **proyectos** (los pesos son solo un punto de partida — se ajustan en la sección 4):

| Señal | De dónde sale | Ejemplo de peso |
|---|---|---|
| Qué tan cerca está la fecha del evento | `Proyecto.FechaEvento` | Hasta 30 pts si es en los próximos 7 días |
| Prioridad marcada manualmente | `Proyecto.Prioridad` (ya existe, es texto libre) | Hasta 25 pts si dice "alta" |
| Estancado en la misma fase mucho tiempo | Fecha del último `HistorialCambio`/`ProyectoSeguimiento` de ese proyecto | Hasta 20 pts si no se toca hace +5 días |
| Propuesta sin enviar o brief pendiente | `Proyecto.PropuestaEstado` / `EstadoBrief` | 15 pts si sigue en "No enviada"/"Pendiente por enviar" |
| Sin pagar y con el evento ya cerca | `Proyecto.Pagado` + `FechaEvento` | 10 pts |

Para **proveedores**, ya existe incluso un campo `Score` (0-100 aprox., hoy sin usar en ninguna lógica) — se puede combinar con "hace cuánto no se le asigna a un proyecto" (`ProyectoProveedor`) y "cuántas personas lo tienen marcado como colaborador" (la funcionalidad que se acaba de construir, `ProveedorColaborador`) para saber cuáles proveedores buenos se están dejando de usar.

El resultado sería un endpoint (ej. `GET /api/proyectos/prioridad`) que devuelve la lista ordenada, con el puntaje y **la lista de razones** de cada uno (igual que hace el modelo de Digital Applied) — nunca solo un número sin explicación.

### Nivel 2 — El micro-agente de IA que preguntaste (encima del nivel 1, opcional)

Una vez que el Nivel 1 esté funcionando, se le puede sumar exactamente el patrón que investigamos: cuando un proyecto/cliente/proveedor tiene notas en `Notas` o entradas en la bitácora (`ProyectoSeguimiento`) que las reglas no pueden leer, se manda ese texto a un modelo de lenguaje con una pregunta acotada tipo *"con este texto, ¿este caso es urgente, y por qué en una frase?"* — y esa respuesta corta se agrega como una razón más, junto a las de la rúbrica. No reemplaza el puntaje de reglas, lo complementa. Esto es justo lo que llamaste "microagente": pequeño, con un solo trabajo, no un asistente general.

### Nivel 3 — Aprendizaje de casos pasados (a futuro, no ahora)

Con más historial acumulado (los 6+ meses que mencionan las fuentes), se podría mirar patrones en los proyectos que salieron bien vs. los que se cayeron o se atrasaron, para ajustar los pesos solos en vez de a mano. **No tiene sentido construir esto todavía** — hoy no hay suficiente historial acumulado (el historial de cambios recién se construyó, ver `docs/20`), y las tres fuentes coinciden en que sin datos suficientes esto no funciona mejor que la rúbrica simple.

## 4. Lo que necesito que definas antes de construir el Nivel 1

- **¿Empezamos por proyectos, por proveedores, por clientes, o por los tres a la vez?** La rúbrica de ejemplo de arriba es de proyectos porque tienen más señales ya capturadas (`FechaEvento`, `Prioridad`, `PropuestaEstado`); para clientes/proveedores habría que definir señales equivalentes.
- **Los pesos de la tabla de arriba son solo un punto de partida mío** — como hacen Capsule CRM y Digital Applied, lo ideal es mirar juntos 10-15 proyectos reales (algunos que salieron bien atendidos a tiempo, otros que se atrasaron) y ajustar los pesos según lo que de verdad importó en esos casos, no adivinar desde cero.
- **¿Quieres el Nivel 2 (el micro-agente que lee notas) desde ya, o primero probamos solo el Nivel 1 con reglas y vemos si es suficiente?** Es totalmente razonable arrancar solo con el Nivel 1 — es gratis, rápido, y ya cubre la mayoría del valor según lo investigado.

Cuando me confirmes eso, lo construyo con la misma disciplina del resto del sistema (pruebas, documentación, nada sin verificar).

---

**Fuentes consultadas:** [Monday.com — reglas de lead scoring](https://monday.com/blog/crm-and-sales/lead-scoring-rules/) · [Capsule CRM — lead scoring con IA para pequeñas empresas](https://capsulecrm.com/blog/ai-lead-scoring-for-small-business/) · [Digital Applied — arquitectura de un agente de IA para lead scoring](https://www.digitalapplied.com/blog/build-ai-lead-scoring-agent-crm-2026) · [Omniconvert — cálculo del puntaje RFM](https://www.omniconvert.com/blog/rfm-score/) · [ProQSmart — tiering estratégico de proveedores](https://proqsmart.com/blog/strategic-supplier-tiering-prioritizing-vendors-for-procurement-success/) · [Inogic — Next Best Action con IA en Dynamics 365](https://www.inogic.com/blog/2026/05/how-ai-recommends-the-next-best-action-in-crm-with-real-examples/) · [Taskade — agente de recordatorios de seguimiento](https://www.taskade.com/agents/crm/follow-up-reminder)
