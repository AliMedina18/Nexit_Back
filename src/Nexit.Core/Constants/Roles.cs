namespace Nexit.Core.Constants;

/// <summary>
/// Los 4 roles de negocio de Nexus, de mayor a menor privilegio. Deben coincidir exactamente con
/// el CHECK constraint ck_usuarios_rol (NexitDbContext) y con el ENUM rol_usuario de
/// docs/schema/nexus_schema_v2.sql — ver docs/06-modelo-permisos-roles.md para la matriz completa.
/// </summary>
public static class Roles
{
    public const string SuperAdmin = "super_admin";
    public const string Admin = "admin";
    public const string Manager = "manager";
    public const string Miembro = "miembro";

    public static readonly string[] Todos = [SuperAdmin, Admin, Manager, Miembro];

    /// <summary>
    /// Los que se pueden ASIGNAR desde la aplicación (2026-09-08, decisión de Alicia: "para poder
    /// colocar super admin como rol, jamás"). <see cref="SuperAdmin"/> queda fuera a propósito: es
    /// una sola persona, la dueña del sistema, y su cuenta se sembró directamente en la base. Nadie
    /// debe poder crear un segundo dueño invitando, registrando o editando a alguien -- si algún día
    /// hiciera falta, es un UPDATE consciente en la base, no un clic.
    ///
    /// La única excepción viva está en ActualizarUsuarioUseCase: la propia super administradora
    /// conserva su rol al editarse a sí misma (si no, no podría ni corregirse el nombre).
    /// </summary>
    public static readonly string[] Asignables = [Admin, Manager, Miembro];
}
