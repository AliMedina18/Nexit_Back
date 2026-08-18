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
}
