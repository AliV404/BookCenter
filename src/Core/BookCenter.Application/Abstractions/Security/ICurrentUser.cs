namespace BookCenter.Application.Abstractions.Security
{
    /// <summary>The user making the current request (all members null/false when anonymous).</summary>
    public interface ICurrentUser
    {
        string? UserId { get; }
        string? Email { get; }
        bool IsAdmin { get; }

        /// <summary>The reader card linked to this account, if any.</summary>
        int? ReaderId { get; }
    }
}
