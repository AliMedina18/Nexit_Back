using Nexit.Core.Entities;

namespace Nexit.Core.Interfaces;

/// <summary>Respaldo de usuarios eliminados -- ver UsuarioEliminado.</summary>
public interface IUsuarioEliminadoRepository
{
    Task AddAsync(UsuarioEliminado registro, CancellationToken cancellationToken = default);
}
