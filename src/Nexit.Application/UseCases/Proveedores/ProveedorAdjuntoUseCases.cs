using Nexit.Application.DTOs.Proveedores;
using Nexit.Core.Entities;
using Nexit.Core.Exceptions;
using Nexit.Core.Interfaces;

namespace Nexit.Application.UseCases.Proveedores;

public interface IProveedorAdjuntosUseCase
{
    Task<IReadOnlyList<ProveedorAdjuntoDto>> ListAsync(Guid proveedorId, CancellationToken cancellationToken = default);
    Task<ProveedorAdjuntoDto> CrearAsync(Guid proveedorId, CrearProveedorAdjuntoDto input, CancellationToken cancellationToken = default);

    /// <summary>Sube un archivo real (docs/28) -- a diferencia de CrearAsync (que espera un StoragePath ya conocido, o un link), esto recibe el contenido, lo valida (solo PDF/Excel, máximo 20 MB), lo sube a Supabase Storage y crea la fila con tipo "file".</summary>
    Task<ProveedorAdjuntoDto> SubirAsync(Guid proveedorId, string nombreArchivo, string contentType, long tamanoBytes, Stream contenido, CancellationToken cancellationToken = default);

    /// <summary>Devuelve la URL para descargar un adjunto: el link tal cual si es tipo "link", o una URL firmada temporal de Supabase Storage si es tipo "file".</summary>
    Task<string> ObtenerUrlDescargaAsync(Guid proveedorId, Guid id, CancellationToken cancellationToken = default);

    Task EliminarAsync(Guid proveedorId, Guid id, CancellationToken cancellationToken = default);
}

public class ProveedorAdjuntosUseCase(IProveedorRepository proveedores, IProveedorAdjuntoRepository adjuntos, ISupabaseStorageService storage, IUnitOfWork unitOfWork) : IProveedorAdjuntosUseCase
{
    /// <summary>Solo estos dos tipos, por regla del negocio (contratos/cotizaciones en PDF, y las planillas en Excel que ya usan a diario) -- ver docs/28.</summary>
    private static readonly Dictionary<string, string> ExtensionesPermitidas = new(StringComparer.OrdinalIgnoreCase)
    {
        [".pdf"] = "application/pdf",
        [".xlsx"] = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        [".xls"] = "application/vnd.ms-excel",
    };

    private const long TamanoMaximoBytes = 20 * 1024 * 1024; // 20 MB, igual que el límite configurado en el bucket de Supabase.

    public async Task<IReadOnlyList<ProveedorAdjuntoDto>> ListAsync(Guid proveedorId, CancellationToken ct = default)
    {
        await AsegurarProveedor(proveedorId, ct);
        return (await adjuntos.GetByProveedorIdAsync(proveedorId, ct)).Select(Map).ToList();
    }

    public async Task<ProveedorAdjuntoDto> CrearAsync(Guid proveedorId, CrearProveedorAdjuntoDto input, CancellationToken ct = default)
    {
        await AsegurarProveedor(proveedorId, ct); Validar(input);
        var adjunto = new ProveedorAdjunto { ProveedorId = proveedorId, Tipo = input.Tipo, Nombre = input.Nombre.Trim(), Url = input.Url?.Trim(), StoragePath = input.StoragePath?.Trim(), Meta = input.Meta?.Trim(), Fecha = input.Fecha ?? DateTime.UtcNow };
        await adjuntos.AddAsync(adjunto, ct); await unitOfWork.SaveChangesAsync(ct);
        return Map(adjunto);
    }

    public async Task<ProveedorAdjuntoDto> SubirAsync(Guid proveedorId, string nombreArchivo, string contentType, long tamanoBytes, Stream contenido, CancellationToken ct = default)
    {
        await AsegurarProveedor(proveedorId, ct);
        if (string.IsNullOrWhiteSpace(nombreArchivo)) throw new BusinessRuleException("El archivo no tiene nombre.");
        if (tamanoBytes <= 0) throw new BusinessRuleException("El archivo está vacío.");
        if (tamanoBytes > TamanoMaximoBytes) throw new BusinessRuleException($"El archivo supera el tamaño máximo permitido ({TamanoMaximoBytes / (1024 * 1024)} MB).");

        var extension = Path.GetExtension(nombreArchivo);
        if (!ExtensionesPermitidas.TryGetValue(extension, out var contentTypeEsperado))
            throw new BusinessRuleException("Solo se permiten archivos PDF (.pdf) o Excel (.xlsx, .xls).");

        // Se ignora el content-type que mande el cliente y se usa el que corresponde a la extensión --
        // así no depende de que el navegador lo haya mandado bien, y evita que alguien intente subir
        // un tipo de archivo distinto disfrazado con una extensión permitida y un content-type falso.
        var caracteresPermitidos = Path.GetFileNameWithoutExtension(nombreArchivo).Where(c => char.IsLetterOrDigit(c) || c is '-' or '_' or ' ').ToArray();
        var nombreSaneado = new string(caracteresPermitidos).Trim();
        if (string.IsNullOrWhiteSpace(nombreSaneado)) nombreSaneado = "archivo";
        var storagePath = $"proveedores/{proveedorId}/{Guid.NewGuid()}-{nombreSaneado}{extension.ToLowerInvariant()}";

        await storage.SubirAsync(storagePath, contenido, contentTypeEsperado, ct);

        var adjunto = new ProveedorAdjunto
        {
            ProveedorId = proveedorId,
            Tipo = "file",
            Nombre = nombreArchivo.Trim(),
            StoragePath = storagePath,
            ContentType = contentTypeEsperado,
            TamanoBytes = tamanoBytes,
            Fecha = DateTime.UtcNow,
        };
        await adjuntos.AddAsync(adjunto, ct); await unitOfWork.SaveChangesAsync(ct);
        return Map(adjunto);
    }

    public async Task<string> ObtenerUrlDescargaAsync(Guid proveedorId, Guid id, CancellationToken ct = default)
    {
        await AsegurarProveedor(proveedorId, ct);
        var adjunto = await adjuntos.GetByIdAsync(id, ct);
        if (adjunto is null || adjunto.ProveedorId != proveedorId) throw new EntityNotFoundException("ProveedorAdjunto", id);

        if (adjunto.Tipo == "link")
            return adjunto.Url ?? throw new BusinessRuleException("Este adjunto de tipo link no tiene una URL guardada.");

        if (string.IsNullOrWhiteSpace(adjunto.StoragePath))
            throw new BusinessRuleException("Este adjunto no tiene un archivo real en Storage para descargar.");
        return await storage.ObtenerUrlFirmadaAsync(adjunto.StoragePath, TimeSpan.FromMinutes(10), ct);
    }

    public async Task EliminarAsync(Guid proveedorId, Guid id, CancellationToken ct = default)
    {
        await AsegurarProveedor(proveedorId, ct);
        var adjunto = await adjuntos.GetByIdAsync(id, ct);
        if (adjunto is null || adjunto.ProveedorId != proveedorId) throw new EntityNotFoundException("ProveedorAdjunto", id);
        await adjuntos.DeleteAsync(id, ct); await unitOfWork.SaveChangesAsync(ct);
        // Si era un archivo real, también se borra de Storage -- de mejor esfuerzo (ver
        // ISupabaseStorageService.EliminarAsync), después de confirmar que la fila ya se borró, para
        // no dejar la fila si el borrado del archivo fallara primero.
        if (adjunto.Tipo == "file" && !string.IsNullOrWhiteSpace(adjunto.StoragePath))
            await storage.EliminarAsync(adjunto.StoragePath, ct);
    }

    private async Task AsegurarProveedor(Guid id, CancellationToken ct) { if (await proveedores.GetByIdAsync(id, ct) is null) throw new EntityNotFoundException("Proveedor", id); }
    private static void Validar(CrearProveedorAdjuntoDto input)
    {
        if (string.IsNullOrWhiteSpace(input.Nombre)) throw new BusinessRuleException("El nombre del adjunto es requerido.");
        if (input.Tipo == "link")
        {
            if (string.IsNullOrWhiteSpace(input.Url)) throw new BusinessRuleException("Un adjunto de tipo link requiere una URL.");
            // Solo http/https: evita que se guarden esquemas como javascript:/data: que, si el frontend
            // los renderiza como enlace clicable sin sanear, permitirían un XSS almacenado (hallazgo H9).
            var esUrlValida = Uri.TryCreate(input.Url, UriKind.Absolute, out var uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
            if (!esUrlValida) throw new BusinessRuleException("La URL del adjunto debe ser un enlace http:// o https:// válido.");
        }
        if (input.Tipo == "file" && string.IsNullOrWhiteSpace(input.StoragePath)) throw new BusinessRuleException("Un adjunto de tipo file requiere una ruta de almacenamiento.");
        if (input.Tipo is not ("link" or "file")) throw new BusinessRuleException("El tipo de adjunto debe ser link o file.");
    }
    private static ProveedorAdjuntoDto Map(ProveedorAdjunto x) => new() { Id = x.Id, ProveedorId = x.ProveedorId, Tipo = x.Tipo, Nombre = x.Nombre, Url = x.Url, StoragePath = x.StoragePath, Meta = x.Meta, ContentType = x.ContentType, TamanoBytes = x.TamanoBytes, Fecha = x.Fecha, CreatedAt = x.CreatedAt };
}
