namespace Nexit.Core.Exceptions;

/// <summary>
/// Para reglas de autorización que dependen de datos en tiempo de ejecución (por ejemplo, "solo el
/// gerente responsable de este proyecto puede endosar esta solicitud") y por eso no se pueden expresar
/// con una política estática de [Authorize(Policy = "...")]. Se traduce a HTTP 403, a diferencia de
/// BusinessRuleException (409, para conflictos de datos/reglas de negocio que no son de permisos).
/// </summary>
public class ForbiddenOperationException : Exception
{
    public ForbiddenOperationException(string message) : base(message) { }
}
