# 29 — Presencia en vivo: diseño resuelto e implementación (HU-12)

Este documento cierra las preguntas de diseño que había dejado abiertas `docs/26-presencia-en-vivo-diseno.md` y describe lo que se construyó. `docs/26` queda como referencia histórica de cómo surgió el pedido (WhatsApp) y qué se evaluó.

## Investigación: cómo lo resuelven otros sistemas

Antes de diseñar, se investigó cómo funciona la presencia "en línea ahora mismo" en sistemas reales:

- **Mecanismo general (Slack, chats en tiempo real):** el patrón estándar es un **heartbeat** — el cliente le avisa al servidor periódicamente que sigue ahí, y si el servidor deja de recibir avisos dentro de una ventana de tiempo, marca a esa persona como desconectada. Para un sistema pequeño (20-30 usuarios, el tamaño de Nexit), la recomendación concreta es: **heartbeat cada 30-60 segundos, umbral de desconexión de 1-2 minutos** — suficiente margen para no marcar a alguien desconectado por una pausa breve de red, sin hacer esperar demasiado para reflejar que alguien sí cerró la pestaña.
- **Supabase Realtime Presence** (ya que el proyecto ya usa Supabase): existe una función nativa pensada exactamente para esto — sincroniza el estado de "quién está conectado" entre clientes por WebSocket, sin necesitar heartbeats manuales ni una tabla en Postgres. Se investigó en detalle como alternativa.

Fuentes: [System Design: Real-Time Presence Platform](https://systemdesign.one/real-time-presence-platform-system-design/), [Supabase Docs: Presence](https://supabase.com/docs/guides/realtime/presence), [Supabase Docs: Realtime Authorization](https://supabase.com/docs/guides/realtime/authorization).

## Decisión: heartbeat propio en `Nexit_Back`, no Supabase Realtime Presence

Se evaluaron las dos opciones y se descartó Supabase Realtime Presence por tres razones concretas:

1. **Restringir quién puede ver la presencia de quién (solo admin/super_admin) no es gratis con Realtime Presence** — requiere activar "Realtime Authorization", que se configura con políticas de RLS sobre una tabla especial (`realtime.messages`), un mecanismo nuevo y separado de las políticas de autorización que ya existen y ya están probadas en todo el backend (`SuperAdminOnly`/`AdminOrAbove`). La propia documentación de Supabase advierte que más RLS ahí "puede impactar el rendimiento y aumentar la latencia de conexión".
2. **Consistencia con el resto del sistema:** en Nexit, todo lo que necesita control de acceso por rol pasa por el backend (`Nexit_Back`), nunca por reglas separadas en Supabase — es el mismo principio que ya se siguió al decidir que el frontend no debe hablar directo con Postgres (ver `nexit_proyecto.md`, decisión de arquitectura del 2026-08-23). Un heartbeat REST simple mantiene esa misma regla.
3. **Escala:** con 20-25 usuarios, la carga de un heartbeat cada 45-60 segundos es insignificante (en el peor caso, ~25-30 escrituras pequeñas por minuto) — no hay ninguna ventaja real de rendimiento en usar WebSockets aquí.

Por eso se construyó un mecanismo propio, simple, consistente con el resto del backend.

## Las tres preguntas de diseño de `docs/26`, resueltas

1. **Mecanismo de detección:** heartbeat por HTTP (polling), no WebSocket/tiempo real — ver la decisión de arriba.
2. **Umbral de "desconectado":** **2 minutos** sin heartbeat (configurable, `Presencia:UmbralMinutos` en `appsettings`) — dentro del rango recomendado (1-2 min) para este tamaño de equipo, con margen para heartbeats cada 45-60 segundos.
3. **Quién puede ver la presencia de quién:** solo `admin`/`super_admin` ven el directorio completo (`GET /api/presencia`), tal como pedía el mensaje original de WhatsApp. Cualquier persona autenticada puede (y debe) hacer ping — todos aparecen en el directorio, no solo los administradores.

## Qué se construyó

- **`Usuario.UltimaActividad`** (`DateTime?`, columna nueva): se actualiza cada vez que llega un ping. No tiene relación con `Activo`/`FechaDesactivacion` (esos son sobre si la cuenta existe y tiene acceso; esto es sobre si alguien la está usando en este momento).
- **`POST /api/presencia/ping`** — cualquier persona autenticada. El frontend lo llama cada 45-60 segundos mientras haya una sesión abierta con la pestaña activa. Si la cuenta no existe (o se borró justo en ese instante), no lanza error — es una vista informativa, no una acción crítica, así que un ping perdido no debe romper nada en el frontend.
- **`GET /api/presencia`** — exclusivo de `admin`/`super_admin` (política `AdminOrAbove`, ya usada en otras partes del backend). Devuelve el directorio completo de cuentas activas, cada una con `EnLinea` (true/false, calculado contra el umbral) y `UltimaActividad`, ordenado con los conectados primero.
- Las cuentas desactivadas (`Activo = false`) nunca aparecen en el directorio.

## Lo que decide el frontend (no es parte de esta historia)

- **Dónde se muestra:** queda abierto a diseño de UI — un punto verde/gris junto al nombre en la lista de Usuarios, un panel aparte, o lo que tenga más sentido visualmente. No hay una restricción técnica que lo obligue a un lugar específico.
- **Cada cuánto llamar al ping:** se recomienda 45-60 segundos, y solo mientras la pestaña esté visible/activa (para no seguir marcando a alguien "en línea" con la laptop cerrada pero la pestaña todavía abierta en segundo plano — el frontend puede usar la Page Visibility API del navegador para pausar el ping si la pestaña no está visible).
- **Cada cuánto refrescar el directorio en pantalla:** si un administrador deja la vista de presencia abierta, el frontend decide si vuelve a pedir `GET /api/presencia` cada cierto tiempo (por ejemplo, cada 30-60 segundos) para que se vea actualizado sin recargar la página.

## Migración de base de datos

`AddPresenciaUsuarios` — agrega la columna `ultima_actividad` (nullable) a `usuarios`. Falta generarla y aplicarla a `nexit_dev` (mismo procedimiento que `docs/28`): `dotnet ef migrations add`, `dotnet build`, `dotnet test`, `dotnet ef database update`.

## Pruebas

6 pruebas nuevas (`PresenciaTests.cs`): el ping marca `UltimaActividad` en la cuenta correcta, un ping a una cuenta inexistente no falla, alguien dentro del umbral aparece en línea, alguien fuera del umbral aparece desconectado, alguien que nunca ha hecho ping aparece desconectado (no null-por-defecto-en-línea), las cuentas desactivadas no aparecen en el directorio, y los conectados aparecen primero en el orden.
