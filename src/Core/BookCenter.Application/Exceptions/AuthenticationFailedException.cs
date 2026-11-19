namespace BookCenter.Application.Exceptions
{
    /// <summary>Wrong credentials, locked or deactivated account. Mapped to HTTP 401.</summary>
    public class AuthenticationFailedException : Exception
    {
        public AuthenticationFailedException(string message) : base(message) { }
    }
}
