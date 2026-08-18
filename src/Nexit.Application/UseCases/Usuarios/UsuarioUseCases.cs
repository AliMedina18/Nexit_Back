using Nexit.Application.DTOs.Usuarios;
using Nexit.Core.Constants;
using Nexit.Core.Entities;
using Nexit.Core.Exceptions;
using Nexit.Core.Interfaces;

namespace Nexit.Application.UseCases.Usuarios;

public class CrearUsuarioUseCase(IUsuarioRepository repository, IUnitOfWork unitOfWork) : ICrearUsuarioUseCase
{
    public async Task<UsuarioResponseDto> ExecuteAsync(CreateUsuarioDto input, CancellationToken cancellationToken = default)
    {
        var usuario = new Usuario { Id = input.Id, Nombre = input.Nombre, Apellido = input.Apellido, Email = input.Email, Rol = input.Rol, Iniciales = input.Iniciales, Activo = input.Activo };
        await repository.AddAsync(usuario, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return UsuarioMapper.ToResponse(usuario);
    }
}

public class ActualizarUsuarioUseCase(IUsuarioRepository repository, IUnitOfWork unitOfWork) : IActualizarUsuarioUseCase
{
    public async Task<UsuarioResponseDto> ExecuteAsync(Guid id, UpdateUsuarioDto input, Guid callerId, CancellationToken cancellationToken = default)
    {
        var usuario = await repository.GetByIdAsync(id, cancellationToken) ?? throw new EntityNotFoundException("Usuario", id);
        if (id == callerId)
        {
            // Protección contra que el super administrador se bloquee a sí mismo por accidente
            // (desactivarse o quitarse el rol super_admin, dejando el sistema sin nadie que pueda
            // administrar usuarios).
            if (!input.Activo) throw new ForbiddenOperationException("No puedes desactivar tu propia cuenta.");
            if (input.Rol != Roles.SuperAdmin) throw new ForbiddenOperationException("No puedes quitarte a ti mismo el rol de super administrador.");
        }
        usuario.Nombre = input.Nombre; usuario.Apellido = input.Apellido; usuario.Rol = input.Rol; usuario.Iniciales = input.Iniciales; usuario.Activo = input.Activo;
        usuario.UpdatedAt = DateTime.UtcNow;
        repository.Update(usuario);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return UsuarioMapper.ToResponse(usuario);
    }
}

public class ConsultarUsuariosUseCase(IUsuarioRepository repository) : IConsultarUsuariosUseCase
{
    public async Task<IReadOnlyList<UsuarioResponseDto>> ListAsync(CancellationToken cancellationToken = default) => (await repository.GetAllAsync(cancellationToken)).Select(UsuarioMapper.ToResponse).ToList();
    public async Task<UsuarioResponseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => UsuarioMapper.ToResponse(await repository.GetByIdAsync(id, cancellationToken) ?? throw new EntityNotFoundException("Usuario", id));
}

public class EliminarUsuarioUseCase(IUsuarioRepository repository, IUnitOfWork unitOfWork) : IEliminarUsuarioUseCase
{
    public async Task ExecuteAsync(Guid id, Guid callerId, CancellationToken cancellationToken = default)
    {
        if (id == callerId) throw new ForbiddenOperationException("No puedes eliminar tu propia cuenta.");
        if (await repository.GetByIdAsync(id, cancellationToken) is null) throw new EntityNotFoundException("Usuario", id);
        await repository.DeleteAsync(id, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

internal static class UsuarioMapper
{
    public static UsuarioResponseDto ToResponse(Usuario usuario) => new()
    {
        Id = usuario.Id, Nombre = usuario.Nombre, Apellido = usuario.Apellido, Email = usuario.Email, Rol = usuario.Rol,
        Iniciales = usuario.Iniciales, Activo = usuario.Activo, CreatedAt = usuario.CreatedAt, UpdatedAt = usuario.UpdatedAt
    };
}
