namespace Nexit.Core.Entities;

public class Usuario : BaseEntity
{
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Rol { get; set; } = "miembro";
    public string? Iniciales { get; set; }
    public bool Activo { get; set; } = true;

    /// <summary>
    /// Momento en que Activo pasó de true a false. Null mientras está activa. Se usa para calcular
    /// cuándo se cumplen los 30 días de inactividad que disparan la eliminación automática (ver
    /// EliminarUsuariosInactivosUseCase y docs/17-eliminacion-automatica-usuarios.md). Se limpia
    /// (vuelve a null) si la cuenta se reactiva antes de cumplir el plazo.
    /// </summary>
    public DateTime? FechaDesactivacion { get; set; }

    /// <summary>
    /// Último momento en que este usuario le hizo "ping" al backend desde una sesión abierta del
    /// frontend (ver PresenciaController, HU-12/docs/26 y docs/29). Se usa solo para calcular si está
    /// "en línea ahora mismo" (dentro del umbral de `Presencia:UmbralMinutos`) -- no tiene relación con
    /// `Activo`/`FechaDesactivacion` (esos son sobre si la cuenta existe y tiene acceso, esto es sobre
    /// si alguien la está usando en este momento). Null si nunca ha hecho ping desde que se agregó esto.
    /// </summary>
    public DateTime? UltimaActividad { get; set; }

    /// <summary>
    /// True desde que esta persona termina de crear su contraseña por primera vez (o de
    /// restablecerla) en Nexit_Front (ver AuthController/docs/30). Se usa SOLO para que la pantalla
    /// de login sepa, antes de que escriba nada más que su correo, si debe pedirle directamente su
    /// contraseña ("Bienvenido de nuevo") o mandarla por el camino de código + crear contraseña (si
    /// es la primera vez). No es un espejo de si Supabase Auth tiene o no una contraseña guardada
    /// -- eso no es consultable desde este backend -- es una marca que pone el propio frontend de
    /// Nexit cuando completa ese paso. False por defecto: toda cuenta nueva (recién invitada) entra
    /// así, y el flujo de código sigue funcionando igual de bien si por algún motivo la marca no se
    /// puso (ver el fallback manual que se dejó en el login).
    /// </summary>
    public bool ContrasenaConfigurada { get; set; } = false;

    public ICollection<Cliente> ClientesCreados { get; set; } = new List<Cliente>();
    public ICollection<Proveedor> ProveedoresCreados { get; set; } = new List<Proveedor>();
    public ICollection<Proyecto> ProyectosCreados { get; set; } = new List<Proyecto>();
    public ICollection<ProyectoSeguimiento> SeguimientosEscritos { get; set; } = new List<ProyectoSeguimiento>();
}
