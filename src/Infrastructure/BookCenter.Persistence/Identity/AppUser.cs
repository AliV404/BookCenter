using Microsoft.AspNetCore.Identity;

namespace BookCenter.Persistence.Identity
{
    /// <summary>
    /// Login account (ASP.NET Core Identity). Kept out of the Domain project so the domain
    /// doesn't depend on Identity; a Reader links to it through Reader.UserId.
    /// </summary>
    public class AppUser : IdentityUser
    {
        /// <summary>LockoutEnd used when an admin locks an account: it never unlocks by itself.</summary>
        public static readonly DateTimeOffset LockedByAdminUntil = DateTimeOffset.MaxValue;

        public string FullName { get; set; } = null!;

        // A method rather than a property so EF doesn't try to map it to a column.
        public bool IsLockedByAdmin() => LockoutEnd > DateTimeOffset.UtcNow.AddYears(100);
    }
}
