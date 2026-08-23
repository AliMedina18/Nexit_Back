using Microsoft.EntityFrameworkCore;
using Nexit.Core.Interfaces;
using Nexit.Infrastructure.Data;

namespace Nexit.Infrastructure.Repositories;

public class DominioCorreoPermitidoRepository(NexitDbContext context) : IDominioCorreoPermitidoRepository
{
    public async Task<bool> EsDominioPermitidoAsync(string email, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@')) return false;
        var dominioCorreo = email[(email.LastIndexOf('@') + 1)..].Trim().ToLowerInvariant();
        if (dominioCorreo.Length == 0) return false;

        // Mismo criterio que el trigger de Postgres check_usuario_dominio_correo: coincide si el
        // dominio del correo termina exactamente en uno de los dominios permitidos (comparación
        // case-insensitive), no una simple coincidencia parcial.
        var dominiosPermitidos = await context.DominiosCorreoPermitidos.AsNoTracking().Select(d => d.Dominio).ToListAsync(cancellationToken);
        return dominiosPermitidos.Any(dominio => dominioCorreo == dominio.Trim().ToLowerInvariant());
    }
}
