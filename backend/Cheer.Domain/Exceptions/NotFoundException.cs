namespace Cheer.Domain.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException(string entity, object id)
        : base($"{entity} '{id}' nao encontrado.")
    {
    }

    public NotFoundException(string message)
        : base(message)
    {
    }
}
