using BookCenter.Application.Common;

namespace BookCenter.Application.DTOs.Users
{
    /// <summary>A login account as seen by an administrator.</summary>
    /// <param name="ReaderId">The linked reader card; null for staff-only accounts or when the card was deleted.</param>
    /// <param name="LockedUntil">Set while the account is locked (by an admin or after too many failed logins).</param>
    public record GetUserDto(
        string Id,
        string Email,
        string FullName,
        IReadOnlyCollection<string> Roles,
        int? ReaderId,
        DateTimeOffset? LockedUntil);

    public class UserQuery : PaginationQuery
    {
        /// <summary>Only accounts in this role (Admin or Reader).</summary>
        public string? Role { get; set; }
    }

    public class SetAdminDto
    {
        public bool IsAdmin { get; set; }
    }

    public class ResetPasswordDto
    {
        public string NewPassword { get; set; } = null!;
    }
}
