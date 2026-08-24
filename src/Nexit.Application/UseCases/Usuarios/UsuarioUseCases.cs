using Microsoft.Extensions.Configuration;
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
        // Arranca/limpia el conteo de 30 días para la eliminación automática (docs/17) justo cuando
        // Activo cambia de verdad -- no en cada edición, para no reiniciar el plazo al corregir, por
        // ejemplo, solo el nombre de alguien que ya estaba desactivado.
        if (usuario.Activo && !input.Activo) usuario.FechaDesactivacion = DateTime.UtcNow;
        else if (!usuario.Activo && input.Activo) usuario.FechaDesactivacion = null;
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

/// <summary>
/// Eliminación manual e inmediata, exclusiva del super_admin (DELETE /api/usuarios/{id}) -- un
/// atajo para cuando no se quiere esperar los 30 días de la eliminación automática (ver
/// EliminarUsuariosInactivosUseCase). Igual que esa, deja respaldo en `usuarios_eliminados` antes
/// de borrar, y también intenta eliminar la cuenta de Supabase Auth -- ver docs/17.
/// </summary>
public class EliminarUsuarioUseCase(
    IUsuarioRepository repository,
    IUsuarioEliminadoRepository archivoRepository,
    ISupabaseAuthAdminService authAdmin,
    IUnitOfWork unitOfWork) : IEliminarUsuarioUseCase
{
    public async Task ExecuteAsync(Guid id, Guid callerId, CancellationToken cancellationToken = default)
    {
        if (id == callerId) throw new ForbiddenOperationException("No puedes eliminar tu propia cuenta.");
        var usuario = await repository.GetByIdAsync(id, cancellationToken) ?? throw new EntityNotFoundException("Usuario", id);

        await archivoRepository.AddAsync(UsuarioMapper.ToArchivo(usuario, eliminadoPorId: callerId), cancellationToken);
        await repository.DeleteAsync(id, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await authAdmin.EliminarCuentaAsync(id, cancellationToken);
    }
}

/// <summary>Ver IEliminarUsuariosInactivosUseCase. Lo dispara el background service, no un endpoint.</summary>
public class EliminarUsuariosInactivosUseCase(
    IUsuarioRepository repository,
    IUsuarioEliminadoRepository archivoRepository,
    ISupabaseAuthAdminService authAdmin,
    IUnitOfWork unitOfWork,
    IConfiguration configuration) : IEliminarUsuariosInactivosUseCase
{
    public async Task<int> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var diasInactividad = configuration.GetValue("EliminacionAutomatica:DiasInactividad", 30);
        var limite = DateTime.UtcNow.AddDays(-diasInactividad);
        var candidatos = await repository.GetInactivosDesdeAsync(limite, cancellationToken);

        foreach (var usuario in candidatos)
        {
            await archivoRepository.AddAsync(UsuarioMapper.ToArchivo(usuario, eliminadoPorId: null), cancellationToken);
            await repository.DeleteAsync(usuario.Id, cancellationToken);
        }
        if (candidatos.Count > 0) await unitOfWork.SaveChangesAsync(cancellationToken);

        foreach (var usuario in candidatos) await authAdmin.EliminarCuentaAsync(usuario.Id, cancellationToken);

        return candidatos.Count;
    }
}

internal static class UsuarioMapper
{
    public static UsuarioResponseDto ToResponse(Usuario usuario) => new()
    {
        Id = usuario.Id, Nombre = usuario.Nombre, Apellido = usuario.Apellido, Email = usuario.Email, Rol = usuario.Rol,
        Iniciales = usuario.Iniciales, Activo = usuario.Activo, FechaDesactivacion = usuario.FechaDesactivacion, CreatedAt = usuario.CreatedAt, UpdatedAt = usuario.UpdatedAt
    };

    public static UsuarioEliminado ToArchivo(Usuario usuario, Guid? eliminadoPorId) => new()
    {
        UsuarioIdOriginal = usuario.Id, Nombre = usuario.Nombre, Apellido = usuario.Apellido, Email = usuario.Email, Rol = usuario.Rol,
        Iniciales = usuario.Iniciales, FechaAltaOriginal = usuario.CreatedAt, FechaDesactivacion = usuario.FechaDesactivacion, EliminadoPorId = eliminadoPorId
    };
}
