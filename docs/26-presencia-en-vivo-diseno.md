# Presencia en vivo — quién está conectado ahora mismo (propuesta, sin construir)

> **Actualizado 2026-08-25: las preguntas de diseño abiertas de este documento ya se resolvieron y la historia ya se construyó — ver `docs/29-presencia-en-vivo-implementacion.md`.** Este documento queda como referencia histórica de cómo surgió el pedido y qué se evaluó, igual que `docs/19` frente a `docs/20`.

## Origen del pedido

Mensaje de WhatsApp del compañero de equipo (+57 324 3146290), 24/8/2026, al hablar de las cuentas de administrador "normales" (`administracion@agencianextmkt.com`, `andresacuna@agencianextmkt.com` — ver la nota de referencia en `docs/12-historias-de-usuario.md`): pidió que esos administradores puedan "ver los usuarios en el sistema y si están activos... como en Teams".

Aclarado con la usuaria el 25/8/2026: **no** se refiere a la lista de usuarios activo/inactivo que ya existe hoy (el campo `activo` de `usuarios` — dar de baja a alguien sin perder su historial, ver `docs/06` y `docs/17`). Se refiere a **presencia en vivo tipo Microsoft Teams**: saber quién tiene el sistema abierto en este momento, en tiempo real.

Este documento deja la propuesta por escrito para no perder el pedido, pero **no hay código nuevo construido todavía** — quedan preguntas de diseño abiertas que conviene resolver con la usuaria antes de escribir el modelo de datos, para no tener que rehacerlo después (mismo criterio que ya se siguió con `docs/19` antes de construir `docs/20`).

## Por qué está en 🔴 (falta backend), no es solo un factor externo

El login de Nexit usa JWT de Supabase, que es **stateless**: el backend valida el token en cada petición, pero no existe ningún mecanismo hoy que rastree si una persona sigue con el sistema abierto o ya lo cerró (no hay heartbeat, no hay tabla de sesiones, no hay WebSocket). Hay que construir esa pieza desde cero.

## Preguntas de diseño abiertas (a confirmar con la usuaria antes de programar)

1. **Mecanismo de detección de presencia:**
   - *Heartbeat por polling* (el frontend llama a un endpoint cada cierto tiempo, ej. cada 60-120 segundos, mientras la pestaña está abierta) — más simple, encaja con el resto del backend (REST puro, sin infraestructura nueva), pero la actualización no es instantánea (depende del intervalo).
   - *Conexión en tiempo real* (WebSocket o SignalR) — actualización instantánea, pero es infraestructura nueva para este backend (hoy 100% REST) y consume más recursos del servidor con 20-25 usuarios simultáneos.
   - Recomendación tentativa (a confirmar): heartbeat por polling, por ser la opción que menos infraestructura nueva agrega y el volumen de usuarios (20-25 personas) no lo justifica.
2. **Umbral de "desconectado":** ¿cuánto tiempo sin actividad se considera "ya no está conectado"? (ej. 2-3 minutos sin heartbeat).
3. **Quién puede ver la presencia de quién:** ¿solo `admin`/`super_admin` (como pidió el mensaje original), o también `manager`/`miembro` entre sí?
4. **Qué se muestra exactamente:** ¿solo un estado binario (conectado/desconectado), o también "visto por última vez hace X" cuando está desconectado?
5. **Dónde se ve:** ¿dentro de la pantalla de Usuarios (hoy exclusiva de `super_admin`, ver `docs/06`), o en un lugar nuevo visible para todos los admins?

## Próximo paso propuesto

Cuando la usuaria confirme las respuestas de la sección anterior, se diseña el esquema (probablemente un campo `ultima_actividad` en `usuarios`, o una tabla aparte si se necesita más detalle) y el endpoint de heartbeat (ej. `PUT /api/presencia`) más el de consulta (ej. `GET /api/usuarios/presencia`), y esta historia pasa de 🔴 a construirse — ver **HU-12** en `docs/12-historias-de-usuario.md`.
