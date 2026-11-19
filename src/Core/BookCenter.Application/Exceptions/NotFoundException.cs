namespace BookCenter.Application.Exceptions
{
    /// <summary>Mapped to HTTP 404 by the API's global exception handler.</summary>
    public class NotFoundException : Exception
    {
        public NotFoundException(string entityName, object key)
            : base($"{entityName} with id '{key}' was not found.") { }
    }
}
