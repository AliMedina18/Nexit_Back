namespace Nexit.Core.Exceptions;

public class EntityNotFoundException : Exception
{
    public EntityNotFoundException(string entityName, Guid id) : base($"Entidad '{entityName}' con ID '{id}' no encontrada.") { }
}
