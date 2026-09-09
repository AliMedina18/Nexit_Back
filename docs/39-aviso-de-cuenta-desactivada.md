# El primer correo que manda Nexit por su cuenta

*2026-09-08. Cierra el pendiente que quedó abierto en `docs/38`: avisarle a la persona cuando su
cuenta se desactiva, y decirle hasta cuándo tiene margen.*

## 1. Por qué hizo falta salirse de Supabase

Hasta hoy era literalmente cierto que **este backend nunca enviaba un correo** (`docs/10`): la
invitación, el código de acceso, restablecer contraseña y el aviso de contraseña cambiada los manda
Supabase Auth con sus plantillas.

Pero Supabase Auth solo sabe mandar los correos de su propio flujo de autenticación. No hay forma de
pedirle "envía este mensaje a esta persona", y *"tu cuenta fue desactivada"* es exactamente eso.
Tampoco servía una notificación dentro del sistema, que era la otra opción sobre la mesa: quien
acaba de perder el acceso **ya no puede entrar a verla**.

Se le presentaron a Alicia las dos vías reales — el Gmail que ya existe, o contratar el proveedor
transaccional que quedó recomendado en `docs/13` — y eligió **Gmail**: cero costo y funciona hoy, a
cambio de los límites de envío de Gmail y de que el remitente se vea como una cuenta de Gmail y no
como el dominio de la agencia. Si algún día eso molesta, se cambia una sola clase.

## 2. Qué se construyó

`IEmailService` (en Core) con un único método: destinatario, asunto, cuerpo HTML. Deliberadamente
mínimo, para que cambiar de proveedor sea cambiar la implementación y nada más.

`SmtpEmailService` (en Infrastructure) manda por SMTP con la misma cuenta de Gmail que ya usa
Supabase. **Ojo con la contraseña:** Gmail no acepta la contraseña normal de la cuenta para SMTP —
hay que generar una *contraseña de aplicación* (con verificación en dos pasos activada) y esa es la
que va en `Email:Contrasena`.

**El servicio nunca lanza**, por contrato de la interfaz. Sin configurar, deja un aviso en el log y
sigue; si el envío falla, lo registra con el destinatario y sigue. Desactivar a alguien tiene que
quedar guardado aunque el correo no salga — el aviso es una consecuencia del cambio, no un requisito
para hacerlo, y por eso se manda **después** de `SaveChangesAsync`, nunca antes.

`PlantillasCorreoUsuario` vive en **Application**, no en Infrastructure. El texto de un aviso es una
decisión de negocio ("qué se le dice a alguien a quien le quitaron el acceso"); Infrastructure solo
sabe empujar bytes por SMTP sin enterarse de qué dicen. Además las pruebas de arquitectura impiden
que Application dependa de Infrastructure, así que tampoco podría estar allá. El diseño es el mismo
de las plantillas que ya viven en Supabase (`docs/14`, `15`, `16`), y todo lo que viene de la base de
datos se escapa antes de entrar al HTML.

## 3. Cuándo se dispara

Solo cuando `Activo` **cambia de verdad**, que es la misma condición que ya arrancaba y limpiaba el
conteo de 30 días (`docs/17`):

| Qué pasó | Qué se manda |
|---|---|
| Estaba activa → se desactiva | *"Tu acceso a Nexit quedó suspendido"*, con **la fecha exacta** en que la cuenta se elimina sola |
| Estaba desactivada → se reactiva | *"Tu acceso a Nexit volvió"* |
| Se corrige el nombre, el rol, las iniciales… | nada |

Esa última fila tiene su propia prueba: nadie quiere recibir "tu acceso quedó suspendido" porque le
arreglaron una tilde al apellido.

La fecha del aviso sale de `FechaDesactivacion + EliminacionAutomatica:DiasInactividad`, o sea que si
algún día se cambian los 30 días por otro número, el correo lo dice solo.

## 4. Lo que hay que configurar (y qué pasa si no)

En `appsettings.Production.json` (ya está el bloque de ejemplo en el `.example`):

```json
"Email": {
  "SmtpHost": "smtp.gmail.com",
  "SmtpPuerto": 587,
  "Usuario": "analistacompras.nexit@gmail.com",
  "Contrasena": "<contraseña de aplicación de Google, NO la del correo>",
  "Remitente": "Nexit"
}
```

**Sin esto, no se rompe nada:** desactivar y reactivar funcionan igual, simplemente no sale el correo
y queda una línea en el log diciendo por qué. Es el mismo criterio de mejor esfuerzo que ya usa la
eliminación de cuentas de Supabase Auth (`docs/17`).

## 5. Verificado

301 pruebas de backend pasan (3 nuevas: avisa al desactivar, avisa al reactivar, y no manda nada
cuando solo se corrigen datos), más las funcionales que necesitan Docker. Las pruebas de
arquitectura confirman que Application sigue sin depender de Infrastructure.

## 6. Pendiente

- **Nada en base de datos.**
- Probarlo de verdad: hace falta que Alicia genere la contraseña de aplicación de Google y la ponga
  en `appsettings.Production.json`. Hasta entonces el aviso no sale de la máquina.
- El remitente se verá como la cuenta de Gmail, no como `@agencianextmkt.com`. Si eso incomoda al
  verlo en un correo real, la salida es el proveedor transaccional de `docs/13`.
