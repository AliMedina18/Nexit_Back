namespace Nexit.Core.Interfaces;

/// <summary>
/// Sube, firma la descarga y elimina archivos reales de adjuntos (docs/28) usando la API de
/// Supabase Storage -- el bucket privado "adjuntos-proveedores" (creado a mano, no vive en una
/// migración de EF Core porque Storage no es parte del esquema de Postgres). Igual que
/// <see cref="ISupabaseAuthAdminService"/>, requiere Supabase:ProjectUrl y Supabase:ServiceRoleKey
/// configurados -- si faltan, SubirAsync lanza (subir es la acción principal, no debe fallar en
/// silencio), pero EliminarAsync solo deja un aviso en el log (limpieza de mejor esfuerzo, no debe
/// bloquear que se borre la fila de la base si Supabase no está configurado).
/// </summary>
public interface ISupabaseStorageService
{
    /// <summary>Sube el contenido al bucket y devuelve la ruta de almacenamiento (StoragePath) que se guarda en la fila del adjunto.</summary>
    Task<string> SubirAsync(string rutaDestino, Stream contenido, string contentType, CancellationToken cancellationToken = default);

    /// <summary>Genera una URL firmada, temporal, para descargar un archivo privado del bucket sin exponerlo públicamente.</summary>
    Task<string> ObtenerUrlFirmadaAsync(string storagePath, TimeSpan vigencia, CancellationToken cancellationToken = default);

    /// <summary>Elimina el archivo real del bucket -- de mejor esfuerzo, no lanza si falla (ver el comentario de la clase).</summary>
    Task EliminarAsync(string storagePath, CancellationToken cancellationToken = default);
}
