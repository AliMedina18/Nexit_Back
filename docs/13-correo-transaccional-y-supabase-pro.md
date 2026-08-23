# Correo transaccional (para los códigos OTP) y el proyecto de Supabase en plan Pro, bien hecho desde cero

Esto responde lo que pediste hoy: primero dejar listo el envío de correo (lo que va a mandar los códigos OTP de HU-01/HU-02), y después crear el proyecto de Supabase **bien hecho desde el principio**, en plan Pro, pensado para una empresa que va a operar con hasta 25 personas — no el plan gratuito que se documentó antes para probar y migrar después (`docs/09`, sección 1 y 11). Esta guía reemplaza esa ruta de "gratis ahora, Pro después": aquí se hace una sola vez, bien, directo en Pro.

**Nota de vocabulario, para que no se preste a confusión al buscar herramientas:** lo que necesitas no es un servicio de "correo masivo" en el sentido de campañas de marketing (tipo Mailchimp, para boletines a muchos destinatarios de una vez). Es un **servicio de correo transaccional** — manda correos automáticos, uno a la vez, disparados por una acción del sistema (a esa persona le llegó un código, a esa persona la invitaron). Es una categoría de herramienta distinta, con proveedores distintos — los que recomiendo abajo son de esa segunda categoría.

**Por qué este orden importa (correo primero, Supabase después):** el servidor de correo que Supabase trae *por defecto* (sin configurar nada) está limitado a **2 correos por hora**, pensado solo para pruebas — con eso ni siquiera alcanza para que tú sola inicies sesión unas cuantas veces seguidas, mucho menos para 25 personas trabajando. Si conectas primero un proveedor de correo propio, el proyecto de Supabase nace ya bien configurado — no hay que volver a tocarlo después.

## 1. Elegir y configurar el proveedor de correo transaccional

### 1.1. Cuál usar

Supabase lista oficialmente estos proveedores como compatibles (cualquier servicio con SMTP funciona, pero estos son los que ellos mismos prueban y documentan): **Resend, AWS SES, Postmark, Twilio SendGrid, ZeptoMail y Brevo**.

Para el tamaño de Next (hasta 25 personas, con logins ocasionales — no miles de correos al día), recomiendo **Resend**:

- Nivel gratis generoso para este volumen: 3.000 correos al mes, 100 al día — muy por encima de lo que 25 personas necesitan para códigos de acceso e invitaciones.
- Tiene tanto SMTP (lo que necesita Supabase para los correos de Auth) como una API HTTP propia — útil más adelante si `Nexit_Back` necesita mandar sus propios correos directamente (por ejemplo, para cerrar el hueco que ya quedó anotado en `docs/10`: notificar por correo las solicitudes de eliminación pendientes — hoy no existe, pero el día que se construya, esta misma cuenta ya estaría lista para usarse desde el backend, no solo desde Supabase).
- Configuración de dominio sencilla (verificación por DNS) y buena entregabilidad (que los correos no caigan en spam).

**Alternativas válidas, si prefieres otra:** Postmark (100 correos/mes gratis de prueba, luego USD 15/mes por 10.000 — más orientado a "solo transaccional, nunca marketing", lo cual es una garantía extra de que nunca te van a limitar la cuenta por parecer spam) o Brevo (nivel gratis más grande pero pensado también para marketing, lo cual puede ser una ventaja si más adelante Next quiere mandar boletines a clientes desde la misma cuenta). Cualquiera de las dos te sirve igual de bien para este caso — Resend es mi recomendación por simplicidad y por el balance entre nivel gratis y la posibilidad de usarlo también desde el backend más adelante.

**Actualización 2026-08-23:** me confirmaste que para arrancar ya vas a usar **Gmail SMTP** con la cuenta `analistacompras.nexit@gmail.com` (contraseña de aplicación), en lugar de Resend. Es una decisión válida para el volumen de 25 personas — ver el detalle, la configuración exacta y las diferencias frente a esta recomendación en la nueva sección 1.3, justo abajo.

*(Nota: no pude confirmar los precios exactos de Resend/Postmark contra sus propias páginas en esta sesión — la herramienta de navegación no pudo cargarlas directamente y tuve que apoyarme en fuentes secundarias que las resumen. Antes de decidirte, confirma el precio y los límites actuales directamente en `resend.com/pricing` o `postmarkapp.com/pricing` — para el volumen de 25 personas, cualquiera de los dos debería quedar en su nivel gratis de todas formas.)*

### 1.2. Pasos para configurarlo

1. Crea la cuenta en [resend.com](https://resend.com) (o el proveedor que elijas) con el correo corporativo que va a quedar como responsable de esto (puede ser tu propio correo cuando lo tengas, o el de `analistacompras@agencianextmkt.com` si esa cuenta ya existe — tú decides quién administra esto).
2. Agrega el dominio `agencianextmkt.com` en la sección de dominios del proveedor. Te va a pedir agregar unos registros DNS (típicamente un `TXT` y uno o más `CNAME`/`DKIM`) — estos se agregan donde sea que esté administrado el DNS de ese dominio (el panel del proveedor donde se compró/registró el dominio, o Google Workspace/Microsoft 365 si el correo corporativo vive ahí). Si no administras tú directamente el DNS, es el paso donde necesitas a quien sí lo administre.
3. Espera a que el proveedor confirme el dominio como verificado (puede tardar de minutos a un par de horas, según qué tan rápido se propaguen los registros DNS).
4. Genera las credenciales SMTP (usuario + contraseña/API key) — normalmente en una sección "SMTP" o "API Keys" del panel del proveedor. Guárdalas en un gestor de contraseñas, no en texto plano ni en el chat — las vas a necesitar en el paso 2.4 de abajo.

### 1.3. Lo que decidiste usar: Gmail SMTP con `analistacompras.nexit@gmail.com`

Confirmaste que el correo `analistacompras.nexit@gmail.com` es la cuenta administradora del envío de correos (distinta de `analistacompras@agencianextmkt.com`, que es la que va a **recibir** notificaciones del sistema — dos cosas separadas, ya quedó claro), y que vas a usar Gmail SMTP con una contraseña de aplicación en vez de Resend. Funciona técnicamente igual de bien para conectarlo a Supabase — lo documento aquí tal como lo vas a usar, con las diferencias que debes tener presentes frente a la recomendación de la sección 1.1:

- **Remitente no corporativo**: los correos van a salir "desde" `analistacompras.nexit@gmail.com`, no desde `@agencianextmkt.com`. La gente va a recibir sus códigos OTP desde una cuenta de Gmail, no desde el dominio de Next. No rompe nada, pero conviene que lo sepas de antemano.
- **Límite de envío de Gmail**: alrededor de 500 correos/día por cuenta (límite normal de Google, no de Workspace). Para 25 personas con logins ocasionales sobra por mucho — no es un problema real a este volumen.
- **Sin verificación de dominio propia (DKIM/SPF de `agencianextmkt.com`)**: Resend/Postmark dejan autenticar el dominio propio, lo que mejora que el correo no caiga en spam. Con Gmail SMTP la entregabilidad depende de la infraestructura de Google — funciona, pero sin ese control adicional.
- **Cuenta personal, no un servicio pensado para esto**: si en algún momento notas que los códigos OTP dejan de llegar o llegan tarde/en spam, esa sería la primera señal a revisar — Google en ocasiones es más estricto con cuentas normales que mandan correos automatizados repetitivos.

Nada de esto bloquea seguir — con 25 personas funciona bien. Si más adelante el volumen crece o hay problemas de entrega, cambiar a Resend es solo reemplazar las credenciales SMTP en el panel de Supabase (10 minutos), sin tocar el backend.

**Configuración exacta para el paso 3 de la sección 2.2 (Supabase → Project Settings → Auth → SMTP Settings):**

| Campo | Valor |
|---|---|
| Host | `smtp.gmail.com` |
| Puerto | `587` |
| Usuario | `analistacompras.nexit@gmail.com` |
| Contraseña | la contraseña de aplicación que ya generaste — se escribe directo en ese campo de Supabase cuando llegues ahí, no la voy a dejar escrita en ningún documento ni en la memoria del proyecto |
| Sender email | `analistacompras.nexit@gmail.com` |
| Sender name | el que quieras que vea la gente, ej. "Next – Sistema Nexit" |

⚠️ **Sobre la contraseña de aplicación que pegaste en el chat**: quedó registrada en el historial de esta conversación, así que, por buena práctica, cuando termines de configurarla en Supabase te recomiendo regenerarla (myaccount.google.com → Seguridad → Contraseñas de aplicaciones → eliminas esa y creas una nueva). Así la que quedó escrita en el chat deja de servir para nada. Es un paso de un minuto y cierra ese riesgo del todo — no es urgente, pero sí vale la pena hacerlo antes de dar el proyecto por cerrado.

## 2. Crear el proyecto de Supabase — plan Pro, bien hecho desde el principio, para hasta 25 personas

### 2.1. Por qué Pro y no Team

Supabase tiene tres planes: Free, Pro (USD 25/mes) y Team (USD 599/mes). Con 25 personas usando el sistema, **Pro es el plan correcto, no Team** — la diferencia de precio es enorme (25 vs. 599 al mes) y lo que agrega Team (cumplimiento SOC2, inicio de sesión corporativo SSO vía SAML, soporte prioritario con tiempos de respuesta garantizados) no aplica a una operación de este tamaño. La razón de fondo: el límite que de verdad importa para cuántas personas puede tener el sistema es el de "usuarios activos al mes" (*monthly active users*), y el plan Pro ya incluye **100.000** — muchísimo más de lo que 25 personas van a usar jamás. Pro también incluye: el proyecto nunca se pausa por inactividad (a diferencia del plan gratis, que se pausa a los 7 días sin uso), respaldos diarios, y dominios personalizados.

*(Mismo aviso que arriba: no pude confirmar estas cifras contra `supabase.com/pricing` directamente en esta sesión — confírmalas ahí antes de pagar, aunque por lo que encontré en varias fuentes independientes coinciden en estos mismos números.)*

### 2.2. Pasos, en orden — construidos sobre `docs/09`, con dos cambios importantes

Este es el mismo proceso de `docs/09-crear-proyecto-supabase-paso-a-paso.md`, con dos diferencias a propósito: (a) se crea directo en plan **Pro** con tu correo corporativo real, no en Free para migrar después — ya no hace falta repetir el proceso dos veces; (b) el correo transaccional (sección 1 de este documento) se conecta **antes** de invitar a nadie, no después.

1. **Crear la cuenta y el proyecto, en Pro desde ya.** Entra a [supabase.com](https://supabase.com), crea la organización con el correo corporativo (el que va a quedar como dueño de la facturación), y al crear el proyecto elige **Pro** como plan (te va a pedir un método de pago en este paso — a diferencia de la guía anterior, que empezaba en Free). Región: la más cercana a Colombia/México (`South America (São Paulo)` si está disponible). Guarda la contraseña maestra de base de datos que te pida, aparte, en un gestor de contraseñas.

2. **Guardar las credenciales del proyecto** (Project Settings → API): Project URL, `anon public key`, `service_role key` — igual que en `docs/09`, sección 2.

3. **Conectar el correo transaccional ANTES de invitar a nadie.** Ve a **Project Settings → Auth → SMTP Settings**, actívalo, y llena los datos con las credenciales de la tabla de la sección 1.3 (Gmail SMTP, que es lo que ya decidiste usar) — o, si en algún momento migras a Resend/otro proveedor, con las credenciales de la sección 1.2. Guarda. Con esto activo, el límite de 2 correos/hora del servidor de pruebas de Supabase deja de aplicar — el proyecto ya manda correos reales desde el día uno.

4. **Aplicar el esquema completo — orden corregido 2026-08-23** (ver el aviso en `docs/09`, sección 3): `01_esquema_completo.sql` → `02_rol_aplicacion_minimo_privilegio.sql` (cambia la contraseña de ejemplo por una generada) → `04_extras_supabase_post_migraciones.sql` → `03_auth_hook_custom_claims.sql` → `seed_geografia_categorias_estados.sql`, en ese orden, en el SQL Editor. (El orden anterior tenía 04 antes que 02 — falla, porque 04 crea políticas `TO nexit_app` que necesitan que ese rol ya exista.)

**Sobre el aviso de RLS de Supabase al correr `01_esquema_completo.sql`:** el editor va a advertir, tabla por tabla, que estás creando una tabla sin Row Level Security habilitado (es normal — RLS se habilita recién en el paso 3, `04_extras...`). Para `__EFMigrationsHistory` (una tabla interna de EF Core, sin datos de negocio, que la aplicación nunca lee ni escribe en producción) y para cualquier otra tabla que te muestre ese aviso durante este paso, elige **"Ejecuta y habilita RLS"** — no rompe nada (el paso 3 vuelve a habilitarlo de todas formas para las tablas de negocio, y hacerlo dos veces no da error) y cierra el hueco de que esa tabla quede expuesta por la API automática de Supabase mientras tanto.

5. **Activar el Auth Hook de roles** — igual que `docs/09`, sección 4 (Authentication → Hooks → Postgres → `custom_access_token_hook`).

6. **Ajustar las plantillas de correo.** Ve a **Authentication → Emails**. Con el diseño de login vigente (`docs/10`, sección 2.2 — todos los roles entran la primera vez por código OTP, no por un enlace de invitación con contraseña), la plantilla que de verdad importa es **"Magic Link"**: edítala para que muestre `{{ .Token }}` de forma visible (el código de 6 dígitos), en vez de solo el enlace por defecto. La plantilla "Invite user" ya no necesita pedir una contraseña en su propia página — con este diseño, la contraseña se crea dentro del sistema (frontend), no en la página de Supabase.

7. **Conectar el backend a las credenciales reales** — igual que `docs/09`, sección 6 (`ConnectionStrings:DefaultConnection` con el rol `nexit_app`, `Jwt:Authority`, `Jwt:Audience`) — esto no cambia por usar Pro ni por el correo transaccional, sigue siendo configuración de `appsettings.Production.json`/variables de entorno, nunca commiteada a Git.

8. **Invitar a las cuentas reales** — igual que `docs/09`, sección 7 (Authentication → Users → Invite user), una por una. Con el correo ya conectado, esa invitación llega de verdad, con buena entregabilidad, desde tu propio dominio.

9. **Bootstrap del primer super_admin y alta del resto por la API** — igual que `docs/09`, secciones 8 y 9 (inserción manual por SQL solo para la primera cuenta, porque `POST /api/usuarios` exige ya ser `super_admin`; el resto se registra por la API normal).

### 2.3. Lo que NO cambia — ningún código nuevo en `Nexit_Back`

Todo lo de este documento (elegir el proveedor de correo, verificar el dominio, conectar el SMTP, elegir el plan Pro) es configuración de Supabase — nada de esto toca código de `Nexit_Back`. Encaja exactamente con el veredicto que ya habíamos dejado en `docs/12-historias-de-usuario.md` para HU-01/HU-02: 🟡 backend listo en código, pendiente de un factor externo. Este documento **es** ese factor externo resolviéndose — cuando termines estos pasos, HU-01 y HU-02 pasan de 🟡 a ✅.

## 3. Qué necesito de ti cuando termines

Cuando ya tengas el correo transaccional configurado y el proyecto de Supabase Pro creado siguiendo la sección 2, para poder dejar `Nexit_Back` conectado de verdad contra el proyecto real, necesito que me pases (por un canal seguro, no pegado directo en texto plano si prefieres — dime cómo te queda cómodo):

- Project URL de Supabase (`https://<tu-proyecto>.supabase.co`).
- La contraseña que generaste para el rol `nexit_app` (paso 2.4 arriba).
- Confirmación de que el Auth Hook quedó activado (paso 5) y de que las 5 piezas del esquema (paso 4) corrieron sin error.

Con eso actualizo `appsettings.Production.json`/las variables de entorno del backend, y quedamos con el sistema conectado de punta a punta contra el proyecto real — listo para que sigamos con las siguientes historias de usuario.

## 4. Referencias

- `docs/09-crear-proyecto-supabase-paso-a-paso.md` — el detalle completo de cada paso de la sección 2.2 de arriba (este documento reordena y ajusta el plan, no repite cada instrucción letra por letra).
- `docs/10-correos-autenticacion-y-guia-frontend.md` — el diseño de login vigente (por qué la plantilla que importa es "Magic Link", no "Invite user").
- `docs/12-historias-de-usuario.md` — HU-01/HU-02, el veredicto de backend que este documento resuelve.
- [Supabase Auth: Send emails with custom SMTP](https://supabase.com/docs/guides/auth/auth-smtp) — proveedores compatibles y límites del servidor de pruebas.
