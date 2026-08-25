namespace Nexit.Application.DTOs.Presencia;

/// <summary>
/// Una fila del directorio de presencia (HU-12/docs/29): cada usuario activo del sistema, con si está
/// "en línea ahora mismo" o no. Solo la consultan admin/super_admin (ver PresenciaController).
/// </summary>
public class PresenciaResponseDto
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;

    /// <summary>
    /// True si `UltimaActividad` cayó dentro del umbral configurado (`Presencia:UmbralMinutos`,
    /// por defecto 2 minutos) contado desde ahora. False tanto si nunca ha hecho ping como si el
    /// último ping ya venció -- el frontend no necesita distinguir esos dos casos para mostrar el
    /// punto de "conectado"/"desconectado".
    /// </summary>
    public bool EnLinea { get; set; }

    /// <summary>Null si esta cuenta nunca ha hecho ping desde que existe este campo.</summary>
    public DateTime? UltimaActividad { get; set; }
}
