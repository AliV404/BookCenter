using Microsoft.AspNetCore.Identity;

namespace BookCenter.Persistence.Identity
{
    /// <summary>
    /// Login account (ASP.NET Core Identity). Kept out of the Domain project so the domain
    /// doesn't depend on Identity; a Reader links to it through Reader.UserId.
    /// </summary>
    public class AppUser : IdentityUser
    {
        public string FullName { get; set; } = null!;
    }
}
