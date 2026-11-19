namespace BookCenter.Application.Common
{
    public static class Roles
    {
        /// <summary>Library staff: full access.</summary>
        public const string Admin = "Admin";

        /// <summary>Self-registered member: browse the catalog and see own loans.</summary>
        public const string Reader = "Reader";

        public static readonly string[] All = [Admin, Reader];
    }

    /// <summary>Claim names used in issued JWTs.</summary>
    public static class AppClaims
    {
        public const string UserId = "sub";
        public const string Email = "email";
        public const string Name = "name";
        public const string Role = "role";
        public const string ReaderId = "reader_id";

        /// <summary>
        /// Identity's security stamp at sign-in. It changes whenever the account's access changes,
        /// which invalidates every token issued before (see IAuthService.IsSessionValidAsync).
        /// </summary>
        public const string SecurityStamp = "stamp";
    }
}
