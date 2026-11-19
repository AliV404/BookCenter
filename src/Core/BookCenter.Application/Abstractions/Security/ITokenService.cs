namespace BookCenter.Application.Abstractions.Security
{
    public record TokenSubject(string UserId, string Email, string FullName, IReadOnlyCollection<string> Roles, int? ReaderId);

    public record IssuedToken(string Token, DateTime ExpiresAt);

    public interface ITokenService
    {
        IssuedToken CreateToken(TokenSubject subject);
    }
}
