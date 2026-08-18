# Preguntas sobre permisos y flujo de trabajo por rol — Nexus

Antes de programar los permisos de cada rol (super administrador, administrador, gerente, miembro), necesito que confirmes cómo funciona el flujo real de trabajo del equipo. Estas respuestas van a definir exactamente qué puede hacer cada persona dentro del sistema, así que entre más preciso, mejor.

## 1. Equipo asignado a un proyecto

1. Cuando se crea un proyecto, ¿siempre tiene un gerente responsable/líder asignado, o puede quedar sin gerente asignado por un tiempo?
2. ¿Un proyecto puede tener más de un gerente a cargo al mismo tiempo, o siempre es uno solo?
3. ¿La persona que **crea** el proyecto en el sistema es automáticamente su gerente/líder, o eso se asigna aparte (por ejemplo, un miembro registra el proyecto pero el gerente se asigna después)?
4. Hoy el equipo de un proyecto se guarda como nombres en texto libre (para poder incluir colaboradores externos que no tienen cuenta en el sistema). ¿Todos los que aparecen en el equipo de un proyecto deberían tener cuenta de usuario en Nexus, o va a seguir habiendo gente en el equipo sin cuenta (freelancers, proveedores, etc.)?
5. Si un gerente sale del equipo de un proyecto o de la empresa, ¿quién puede reasignar el proyecto a otro gerente? ¿Solo el administrador/super administrador?

## 2. Permisos del gerente

6. Un gerente, ¿puede ver y gestionar **todos** los proyectos del sistema, o solo los que tiene asignados como responsable/líder?
7. Dentro de sus propios proyectos, ¿el gerente puede eliminarlos directamente, o eso siempre requiere que lo confirme un administrador?
8. Un gerente, ¿puede reasignar quién está en el equipo de su proyecto (agregar o quitar miembros) sin pedir permiso a nadie más?
9. Fuera de sus propios proyectos, ¿el gerente puede ver los proyectos de otros gerentes (aunque no los pueda editar), o esos quedan completamente ocultos para él?
10. Sobre clientes y proveedores (no proyectos): ¿el gerente puede crear y editar clientes/proveedores libremente? ¿Puede eliminarlos, o eso queda reservado para administrador/super administrador?
11. Si dos gerentes distintos quieren usar el mismo proveedor o cliente en sus proyectos, ¿ambos pueden editar esa ficha de proveedor/cliente, o solo quien la registró originalmente?

## 3. Permisos del miembro del equipo

12. Un miembro que **no** está asignado al equipo de un proyecto, ¿puede verlo (aunque sea en modo solo lectura), o directamente no debería aparecerle en su lista de proyectos?
13. Un miembro asignado a un proyecto, ¿puede editar cualquier campo de ese proyecto (fechas, estado, notas, etc.), o hay campos que deberían quedar reservados solo para el gerente/administrador (por ejemplo, si el proyecto ya está pagado, el número de factura, o cambiar quién es el cliente)?
14. Un miembro, ¿puede crear un proyecto nuevo por su cuenta (por ejemplo, para registrar algo que le acaban de asignar), o siempre lo crea primero el gerente o el administrador?
15. Si un miembro cree que un proyecto ya no sirve o hay que eliminarlo, ¿cuál es el proceso? ¿Se lo pide a su gerente, o hay alguna otra forma de "marcarlo" sin poder borrarlo directamente?
16. Sobre clientes y proveedores: ¿un miembro del equipo puede registrar clientes/proveedores nuevos y editarlos libremente, igual que hoy, o eso también debería depender de si está en algún proyecto relacionado?

## 4. Casos generales / de borde

17. Cuando el super administrador o el administrador desactivan a un usuario (por ejemplo, alguien que ya no trabaja en Next), ¿qué debería pasar con los proyectos donde esa persona era gerente o estaba en el equipo? ¿Se reasignan automáticamente a alguien, o quedan "huérfanos" hasta que alguien los reasigne a mano?
18. ¿Existe algún caso en el que un miembro del equipo sí deba poder eliminar algo (un proyecto, cliente o proveedor) sin pasar por un gerente o administrador? Por ejemplo, algo que él mismo creó por error hace un minuto.
19. ¿El administrador (no el super administrador) puede editar o eliminar **cualquier** proyecto/cliente/proveedor del sistema sin restricción, incluso los que no son suyos ni de su equipo?

---

*Estas preguntas son para definir el sistema de permisos por rol de Nexus (el backend que estamos construyendo). Cuando las tengas respondidas, se las paso directo a Claude para dejar los permisos programados.*
