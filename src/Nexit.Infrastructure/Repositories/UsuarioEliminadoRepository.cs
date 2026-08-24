using Nexit.Core.Entities;
using Nexit.Core.Interfaces;
using Nexit.Infrastructure.Data;

namespace Nexit.Infrastructure.Repositories;

public class UsuarioEliminadoRepository(NexitDbContext context) : IUsuarioEliminadoRepository
{
    public Task AddAsync(UsuarioEliminado registro, CancellationToken cancellationToken = default) =>
        context.UsuariosEliminados.AddAsync(registro, cancellationToken).AsTask();
}
