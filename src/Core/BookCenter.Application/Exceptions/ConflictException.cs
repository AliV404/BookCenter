namespace BookCenter.Application.Exceptions
{
    /// <summary>
    /// A request that is well-formed but violates a business rule
    /// (duplicate name, entity still in use, no copies left...). Mapped to HTTP 409.
    /// </summary>
    public class ConflictException : Exception
    {
        public ConflictException(string message) : base(message) { }
    }
}
