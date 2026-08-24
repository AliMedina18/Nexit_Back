# Evaluación de los dos proyectos que trajiste

Revisé a fondo los dos proyectos que descargaste y me pediste mirar:

- `retail-supply-chain-analytics-main`
- `OmniSupply-AI-Multi-Agent-Supply-Chain-Intelligence-Platform-master`

Esto es lo que encontré, siendo directa sobre qué sirve y qué no — para no hacerte perder tiempo metiendo algo que en realidad no encaja.

## Resumen corto

Ningún código de ninguno de los dos se puede copiar dentro de Nexit_Back. Uno es Python (no C#), y el otro además tiene una licencia (GPLv3) que no lo permitiría aunque quisiéramos. Lo que sí vale la pena quedarnos son **dos ideas concretas**, explicadas abajo — y una validación importante de que el camino que ya elegimos (Nivel 1 con reglas, sin IA) es exactamente el mismo patrón que usa incluso el proyecto de IA "de verdad".

## 1. `retail-supply-chain-analytics-main`

Es un proyecto de análisis de datos (SQL + Python/pandas + Power BI) sobre una cadena de tiendas ficticia: por qué se quedan sin stock, qué proveedores son responsables, cuánto cuesta el desperdicio. No es un sistema de IA ni de agentes — es análisis estadístico clásico sobre datos sintéticos (inventados para el portafolio, no datos reales).

No tiene archivo `LICENSE`, así que no hay permiso explícito para copiar su código — pero eso da igual acá, porque tampoco aplicaría: analiza inventario físico de tiendas, y Nexit gestiona proyectos de agencia, no bodegas.

**Lo único rescatable son dos ideas, no código:**

- **La confiabilidad de un proveedor predice mejor los problemas que su velocidad.** El hallazgo central del proyecto es que la velocidad de entrega de un proveedor casi no se correlaciona con los problemas (-0.45, o sea entre más lento, menos problemas — porque ya se compensa), pero la *confiabilidad* sí (-0.44 en el sentido esperado). Esto refuerza algo que ya habíamos dejado anotado en el documento 21: el campo `Score` de `Proveedor` existe pero hoy no lo usa ninguna lógica — cuando decidamos extender la rúbrica de prioridad a proveedores, este hallazgo sugiere que ese campo (si se llena con confiabilidad real, no solo con "qué tan rápido responde") va a ser más útil que cualquier medida de velocidad.
- **Alertar *antes* de que algo se vuelva un problema, no solo cuando ya lo es.** Una de sus consultas (`Weeks below safety stock`) marca casos que todavía no son un stockout pero van camino a serlo. Es la misma lógica detrás de la señal "sin actividad reciente" que ya construimos en el documento 22 — confirma que ese tipo de señal de alerta temprana es un patrón real y útil, no algo que nos inventamos.

## 2. OmniSupply (el "multi-agente de IA")

Este sí es un sistema de agentes de IA de verdad — 5 agentes especializados (datos, riesgo, finanzas, reuniones, correo) coordinados por un supervisor, usando LangGraph, OpenAI (GPT-4o-mini) para generar texto, PostgreSQL para los datos y una base de datos vectorial (ChromaDB) para búsqueda semántica.

**Aviso importante sobre la licencia:** el README dice "MIT" en la insignia de arriba, pero el archivo `LICENSE` real del repositorio es **GPLv3**, no MIT — son inconsistentes entre sí, y legalmente manda el archivo `LICENSE`, no la insignia. GPLv3 es una licencia "copyleft" fuerte: si copiáramos o adaptáramos código de ahí dentro de Nexit_Back y lo distribuyéramos, estaríamos obligados a liberar el código combinado bajo la misma licencia GPL — es decir, tendríamos que hacer público el código fuente de Nexit. Eso es incompatible con un backend privado de agencia, así que de aquí no se copia nada de código, ni siquiera si algún día tuviéramos una versión en Python.

**Lo que sí vale la pena mirar es su arquitectura — y lo que confirma:**

Miré con detalle el `risk_agent.py` (el que más se parecía a lo que buscamos) y su clase base `base.py`. La sorpresa es que, aunque venden esto como "IA", **el puntaje de riesgo en sí no lo calcula ninguna IA** — es aritmética 100% determinística en Python:

```python
overall_score = (
    delivery_score * 0.4 +
    inventory_score * 0.3 +
    quality_score * 0.2 +
    financial_score * 0.1
)
```

Cuatro consultas SQL calculan cada "score" (tasa de entregas tardías, % de inventario crítico, tasa de devoluciones, % de pedidos con margen negativo), se suman con pesos fijos, y **solo después** ese número se le pasa a un modelo de lenguaje (GPT-4o-mini) — pero únicamente para que redacte el resumen en texto y decida el mensaje de alerta, nunca para que invente el puntaje. Es exactamente el mismo patrón que la investigación del documento 21 ya había identificado en Monday.com, Capsule CRM, y los demás sistemas: **reglas transparentes calculan el número, la IA (si se usa) solo lo traduce a palabras.**

Dicho de otra forma: un equipo que sí construyó la versión "con microagentes de IA" que mencionabas al principio, terminó con el mismo diseño de fondo que ya elegimos para Nexit — puntaje por reglas, razones explícitas, IA opcional y acotada. No es que nos quedamos cortos por no meter IA todavía; es que ya construimos la parte que de verdad importa.

**Una idea concreta que sí sirve, sin copiar código:** su patrón de "recomendación de alerta" — decidir si avisar, con qué severidad (INFO/WARNING/CRITICAL), y a quién — encaja bien con la tabla `Notificacion` que ya existe en Nexit (documento 20). Cuando quieras, se puede diseñar (no ahora, es solo una idea para más adelante) que un proyecto que cruce cierto puntaje en `GET /api/proyectos/prioridad` genere automáticamente una notificación en bandeja, en vez de que alguien tenga que ir a mirar el endpoint.

**Lo que NO tiene sentido traer:** toda la infraestructura pesada (LangGraph, ChromaDB, llamadas a la API de OpenAI con costo por uso, observabilidad con Opik) no encaja con el tamaño actual de Nexit ni con el stack (.NET/Postgres/Supabase, sin gasto recurrente de API externa). Meter eso ahora sería sobre-construir para el problema que tenemos.

## En resumen: ¿qué hacemos con esto?

Nada nuevo por construir todavía — esto es una validación, no un pendiente técnico. Cuando decidas seguir con el Nivel 2 (microagente de IA) o extender la rúbrica a proveedores, estas dos ideas quedan anotadas:

1. Usar confiabilidad real (no velocidad) si se activa el campo `Score` de proveedores.
2. Si algún día quieres automatizar el aviso, el puntaje de `GET /api/proyectos/prioridad` ya está listo para alimentar una notificación automática — es un paso pequeño desde donde estamos, no algo que requiera lo que vimos en OmniSupply.
