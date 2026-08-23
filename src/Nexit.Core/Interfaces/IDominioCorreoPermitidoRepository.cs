namespace Nexit.Core.Interfaces;

/// <summary>
/// Valida el dominio de un correo contra el catálogo <c>dominios_correo_permitidos</c> -- la misma
/// tabla que respalda el trigger <c>check_usuario_dominio_correo</c> en Postgres (ver
/// docs/schema/nexus_schema_v2.sql). Esta interfaz existe para hacer esa misma validación TAMBIÉN
/// en la aplicación, antes de intentar crear la cuenta: el trigger de base de datos es el respaldo
/// final, pero como comentaba el propio script SQL, "la validación de verdad ... debe hacerse
/// también en la aplicación". Ver docs/06-modelo-permisos-roles.md y docs/09-crear-proyecto-supabase-paso-a-paso.md.
/// </summary>
public interface IDominioCorreoPermitidoRepository
{
    Task<bool> EsDominioPermitidoAsync(string email, CancellationToken cancellationToken = default);
}
