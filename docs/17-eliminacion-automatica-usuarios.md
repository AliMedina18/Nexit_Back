# Eliminación automática de usuarios inactivos (30 días) + respaldo

Pediste completar el módulo de usuarios: crear, editar, activar/desactivar, y eliminar — con eliminación automática después de 30 días desactivado, y un respaldo antes de borrar de verdad, en vez de perder la información sin más. Este documento explica el diseño y qué investigué antes de construirlo.

## 1. Qué ya existía y qué faltaba

`Nexit_Back` ya tenía CRUD completo de usuarios (`UsuariosController`, exclusivo de `super_admin`): crear, editar (incluye activar/desactivar por el campo `Activo`), y eliminar. Lo que faltaba, revisando el código real:

- **"Desactivar" no bloqueaba nada de verdad.** El campo `Activo` se guardaba, pero ni el Auth Hook de Supabase (`docs/schema/03_auth_hook_custom_claims.sql`) ni las políticas de autorización del backend (`Nexit.API/Program.cs`) lo revisaban en ningún lado. Una cuenta desactivada, con una sesión de Supabase todavía válida, seguía teniendo acceso completo — desactivar era, hasta ahora, solo un dato guardado en la tabla, no una restricción real.
- **`DELETE /api/usuarios/{id}` era un borrado inmediato y definitivo**, sin ningún paso previo de 30 días ni respaldo — `repository.DeleteAsync` sin más.
- No existía ningún proceso automático que revisara cuentas inactivas.

## 2. Investigación: cómo hacen esto otros productos

Antes de diseñar, busqué cómo manejan este mismo patrón (desactivar → ventana de gracia → eliminar) productos con este problema resuelto desde hace años:

- **Microsoft Entra ID** (Azure AD) mueve las cuentas eliminadas a un estado "recientemente eliminada" por 30 días, recuperable, antes de purgarlas de verdad.
- **GitLab** extendió su período de "pending deletion" de 7 a 30 días exactamente por la misma razón que la tuya: dar margen suficiente para revertir un error.
- **Facebook** extendió su plazo de eliminación de cuenta de 14 a 30 días en 2018, por el mismo motivo.

Los 30 días que pediste no son arbitrarios — es el número que terminó adoptando la mayoría de estos productos después de ver que ventanas más cortas (7-14 días) generaban eliminaciones accidentales sin tiempo de reacción. Eso confirma que el diseño que pediste ya sigue la práctica estándar de la industria; no hizo falta inventar nada nuevo.

## 3. Cómo quedó el flujo

1. **Activar / desactivar** (`PUT /api/usuarios/{id}`, ya existía): al pasar `Activo` de `true` a `false`, el backend ahora también guarda `FechaDesactivacion = ahora`. Si se reactiva antes de que se cumpla el plazo, `FechaDesactivacion` se limpia (vuelve a `null`) y el conteo desaparece — no hay "medio plazo cumplido" que se retome después.
2. **Eliminación automática** (nueva, sin pantalla ni endpoint — corre sola): un proceso de fondo dentro de `Nexit_Back` revisa, una vez al día, qué cuentas llevan `FechaDesactivacion` de 30 días o más. A cada una: la copia completa a la tabla de respaldo `usuarios_eliminados`, la borra de `usuarios`, e intenta borrar también su cuenta de Supabase Auth (ver sección 5).
3. **Eliminación manual** (`DELETE /api/usuarios/{id}`, ya existía, cambió por dentro): sigue disponible como atajo para cuando no se quiere esperar los 30 días (por ejemplo, alguien despedido por una causa grave) — pero ahora también respalda antes de borrar, igual que la automática. La única diferencia entre las dos: en el respaldo queda registrado quién la ejecutó (`eliminado_por_id`) cuando fue manual, y queda en blanco cuando fue automática.

## 4. El respaldo: `usuarios_eliminados`

Se guarda en una tabla nueva dentro de la misma base de datos de Supabase (no una base de datos aparte) — nombre, apellido, correo, rol, cuándo se dio de alta originalmente, cuándo se desactivó, cuándo se eliminó, y quién la eliminó si fue manual. No la consulta la aplicación normal — es puramente un archivo de auditoría, para poder reconstruir quién era una cuenta si hace falta más adelante (por ejemplo, para saber qué proyectos creó alguien que ya no está, aunque su perfil ya no exista).

No se implementó como una base de datos físicamente separada porque no aporta nada aquí (mismo proyecto de Supabase, mismo control de acceso) y sí complica la operación — es el mismo patrón que ya usa el sistema para respaldos de este tipo.

## 5. Por qué también hay que borrar la cuenta de Supabase Auth (y qué falta para que esto quede 100% cerrado)

Este es el punto más importante que encontré al revisar el diseño completo, y que no estaba resuelto: borrar la fila de `usuarios` **no borra la cuenta de acceso** en Supabase Auth. Sin nada más, alguien eliminado seguiría pudiendo iniciar sesión — y como su fila en `usuarios` ya no existe, el Auth Hook le asignaría el rol por defecto `"miembro"` (pensado originalmente para gente a mitad del registro, no para gente eliminada). Es decir: sin este paso, "eliminar" a alguien no lo saca del sistema del todo.

Por eso, tanto la eliminación automática como la manual, además de borrar el perfil de negocio, ahora intentan borrar también la cuenta de Supabase Auth correspondiente, usando la Admin API de Supabase. Este paso necesita la **Service Role Key** del proyecto (Project Settings → API) — la clave de mayor privilegio que tiene Supabase, que no está guardada en ningún lado de este repositorio (nunca te la he pedido, y no deberías pegarla en el chat tampoco). Mientras no la configures en `appsettings.Production.json` (que ya está en `.gitignore`, igual que la conexión a la base de datos), este paso queda como un aviso en el log — el perfil de negocio sí se elimina y respalda igual, pero la cuenta de Supabase Auth queda huérfana hasta que la borres a mano desde el dashboard, o configures la clave para que se haga sola.

Cuando quieras cerrar esto del todo: ve a tu proyecto → **Project Settings → API**, copia la **`service_role` key** (no la publishable/anon que ya usamos para el resto), y agrégala a `appsettings.Production.json`:

```json
"Supabase": { "ProjectUrl": "https://zcxqcgzcsctallpdiutu.supabase.co", "ServiceRoleKey": "TU_SERVICE_ROLE_KEY" }
```

## 6. Bloquear el acceso de verdad al desactivar (no solo guardar el dato)

Esto es lo que hace que "desactivar" signifique algo, y no estaba antes: el Auth Hook (`docs/schema/03_auth_hook_custom_claims.sql`, ya actualizado) ahora agrega el claim `user_active=false` al JWT cuando `usuarios.activo` es `false`. El backend (`Nexit.API/Program.cs`) revisa ese claim en **todo** endpoint, no solo en los de super_admin/admin — antes esa revisión solo existía en un par de políticas puntuales.

**Limitación honesta, no un "ya quedó perfecto":** esto no es una revocación instantánea. El claim se agrega al token la próxima vez que la persona inicia sesión o su token se renueva (los tokens de Supabase duran hasta 1 hora). Una cuenta recién desactivada conserva acceso durante el resto de la vida de su token actual — hasta una hora, en el peor caso. Para una herramienta interna esto es un margen razonable; si en algún momento hace falta corte instantáneo (por ejemplo, para un despido con causa donde cada minuto cuenta), la única forma real es revocar la sesión directamente en Supabase Auth (`auth.admin.signOut`), que también necesita la Service Role Key de la sección anterior — no está implementado todavía, se puede agregar si lo necesitas.

Este mismo límite (token válido hasta que expira por su cuenta, aunque la sesión ya se haya cerrado o revocado) también aplica al cerrar sesión normal (`signOut`, ver `docs/12`, HU-07) — investigué esto al trabajar esa historia y confirmé, en la documentación oficial de Supabase, que no existe ningún mecanismo limpio para que el backend revoque al instante la sesión de una persona por su ID; solo se puede revocar una sesión con el JWT de esa misma persona, que el backend nunca tiene.

**Recomendación concreta para acortar esa ventana, la única palanca real que confirmé que existe:** bajar el tiempo de expiración del *access token* (JWT) en el dashboard de Supabase — **Authentication → Settings → JWT expiry limit** (Supabase lo permite entre unos 5 minutos y 1 hora; por defecto suele venir en 1 hora). Esto reduce, de forma pareja, la ventana de riesgo de los tres escenarios de esta sección: desactivar a alguien, cerrar sesión, y un token robado o filtrado — todos quedan expuestos como máximo el tiempo que dure el token, no más. Por ejemplo, bajarlo a 15 minutos reduce el peor caso de "hasta 1 hora" a "hasta 15 minutos", sin tocar ningún código de `Nexit_Back`. Contrapartida a tener en cuenta: un token más corto significa que el frontend renueva sesión (`refresh`) más seguido — con la librería de Supabase (`supabase-js`) esto ya pasa automáticamente en segundo plano, así que en la práctica la persona no lo nota; no es necesario cambiar nada en el frontend para poder bajar este valor.

## 7. Qué falta para que esto corra en producción

1. Ejecutar `docs/schema/06_eliminacion_automatica_usuarios.sql` contra tu base de datos de Supabase (agrega la columna `fecha_desactivacion` y la tabla `usuarios_eliminados`).
2. Volver a ejecutar `docs/schema/03_auth_hook_custom_claims.sql` completo (se actualizó — ya lo tenías corrido antes, pero con la versión vieja que no revisaba `activo`).
3. Opcional pero recomendado: configurar la Service Role Key (sección 5) para que la cuenta de Supabase Auth también se elimine sola.
4. Nada más — el resto (el proceso de fondo, el respaldo, el bloqueo de acceso) ya vive en el código, compilado y con pruebas (127 pruebas totales, 120 pasan sin necesitar Docker/base de datos real; las 7 restantes son las funcionales que sí necesitan Docker, mismas de siempre).

## 8. Configuración opcional

En `appsettings.Production.json`, sección `EliminacionAutomatica` (opcional, ya vienen estos valores por defecto si no la agregas):

```json
"EliminacionAutomatica": { "DiasInactividad": 30, "IntervaloHoras": 24 }
```

`DiasInactividad`: cuántos días de desactivación antes de eliminar. `IntervaloHoras`: cada cuánto revisa el proceso de fondo (por defecto, una vez al día).

## 9. Limitación conocida del proceso de fondo

El proceso que revisa cuentas vencidas vive dentro del mismo proceso de la API (`BackgroundService` de .NET) — solo corre mientras la API está corriendo. Si el servidor estuviera caído justo el día que le tocaba a alguien, se ejecuta en el siguiente arranque, no exactamente ese día. Para un panel interno esto es aceptable. Si más adelante hace falta que corra aunque la API esté apagada, la alternativa es moverlo a un cron de Postgres (`pg_cron`, ya disponible en tu proyecto de Supabase) — no se implementó así por ahora porque acopla lógica de negocio a SQL en vez de vivir en el backend con el resto del código, pero queda anotado como alternativa válida.

## Referencias

- [GitLab: Extend pending deletion period from 7 to 30 days](https://gitlab.com/groups/gitlab-org/-/epics/17375)
- [Microsoft Entra ID: Recover from deletions](https://learn.microsoft.com/en-us/entra/architecture/recover-from-deletions)
- [Facebook extends account deletion grace period from 14 to 30 days](https://alternativeto.net/news/2018/10/facebook-extends-account-deletion-grace-period-from-14-to-30-days)
