using BookCenter.Application.Abstractions.Security;
using BookCenter.Application.Abstractions.Services;
using BookCenter.Application.Common;
using BookCenter.Application.DTOs.Users;
using BookCenter.Application.Exceptions;
using BookCenter.Persistence.Common;
using BookCenter.Persistence.Data;
using BookCenter.Persistence.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BookCenter.Persistence.Implementations.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly AppDbContext _context;
        private readonly ICurrentUser _currentUser;

        public UserService(UserManager<AppUser> userManager, AppDbContext context, ICurrentUser currentUser)
        {
            _userManager = userManager;
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<PagedResult<GetUserDto>> GetAllAsync(UserQuery query)
        {
            var users = _context.Users.AsNoTracking();

            if (query.Search is not null)
                users = users.Where(u => u.Email!.Contains(query.Search) || u.FullName.Contains(query.Search));

            if (query.Role is not null)
            {
                var role = Roles.All.First(r => r.Equals(query.Role, StringComparison.OrdinalIgnoreCase));
                users = users.Where(u => _context.UserRoles.Any(ur => ur.UserId == u.Id
                    && _context.Roles.Any(r => r.Id == ur.RoleId && r.Name == role)));
            }

            return await Project(users.OrderBy(u => u.Email).ThenBy(u => u.Id)).ToPagedResultAsync(query);
        }

        public async Task<GetUserDto> GetByIdAsync(string id)
        {
            return await Project(_context.Users.AsNoTracking().Where(u => u.Id == id)).FirstOrDefaultAsync()
                ?? throw new NotFoundException("User", id);
        }

        public async Task SetAdminAsync(string id, bool isAdmin)
        {
            var user = await FindAsync(id);
            if (await _userManager.IsInRoleAsync(user, Roles.Admin) == isAdmin)
                return;

            if (isAdmin)
            {
                (await _userManager.AddToRoleAsync(user, Roles.Admin)).ThrowIfFailed("grant admin rights");
            }
            else
            {
                // Since only an admin can call this, the "remove yourself" rule also guarantees
                // there is always at least one admin left.
                if (IsCurrentUser(id))
                    throw new ConflictException("You can't remove your own admin rights. Ask another admin to do it.");

                if (!await _context.Readers.AnyAsync(r => r.UserId == id))
                    throw new ConflictException("This account has no reader card, so without admin rights it couldn't sign in at all. Lock it instead.");

                (await _userManager.RemoveFromRoleAsync(user, Roles.Admin)).ThrowIfFailed("remove admin rights");
            }

            await SignOutEverywhereAsync(user);
        }

        public async Task LockAsync(string id)
        {
            if (IsCurrentUser(id))
                throw new ConflictException("You can't lock your own account.");

            var user = await FindAsync(id);
            user.LockoutEnabled = true;
            user.LockoutEnd = AppUser.LockedByAdminUntil;
            await SignOutEverywhereAsync(user); // saves the lockout too
        }

        public async Task UnlockAsync(string id)
        {
            var user = await FindAsync(id);
            // Clears both admin locks and the temporary lockout after too many failed logins.
            user.LockoutEnd = null;
            user.AccessFailedCount = 0;
            (await _userManager.UpdateAsync(user)).ThrowIfFailed("unlock the account");
        }

        public async Task ResetPasswordAsync(string id, string newPassword)
        {
            if (IsCurrentUser(id))
                throw new ConflictException("Use \"Change password\" for your own account.");

            var user = await FindAsync(id);

            // Same strength rules as registration. Validated before touching the account,
            // so a rejected password never leaves the user without one.
            var errors = new List<IdentityError>();
            foreach (var validator in _userManager.PasswordValidators)
            {
                var result = await validator.ValidateAsync(_userManager, user, newPassword);
                errors.AddRange(result.Errors);
            }
            if (errors.Count > 0)
                throw IdentityErrors.ToValidationException(errors, passwordField: "newPassword");

            user.PasswordHash = _userManager.PasswordHasher.HashPassword(user, newPassword);
            user.AccessFailedCount = 0;
            if (!user.IsLockedByAdmin())
                user.LockoutEnd = null; // lift a temporary failed-login lockout, but not an admin lock

            await SignOutEverywhereAsync(user);
        }

        private bool IsCurrentUser(string id) => id == _currentUser.UserId;

        private async Task<AppUser> FindAsync(string id) =>
            await _userManager.FindByIdAsync(id) ?? throw new NotFoundException("User", id);

        /// <summary>Saves the user and rotates their security stamp, which invalidates all their tokens.</summary>
        private async Task SignOutEverywhereAsync(AppUser user) =>
            (await _userManager.UpdateSecurityStampAsync(user)).ThrowIfFailed("update the account");

        private IQueryable<GetUserDto> Project(IQueryable<AppUser> users)
        {
            var now = DateTimeOffset.UtcNow;
            return users.Select(u => new GetUserDto(
                u.Id,
                u.Email!,
                u.FullName,
                _context.UserRoles
                    .Where(ur => ur.UserId == u.Id)
                    .Join(_context.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => r.Name!)
                    .OrderBy(name => name)
                    .ToList(),
                _context.Readers.Where(r => r.UserId == u.Id).Select(r => (int?)r.Id).FirstOrDefault(),
                u.LockoutEnd > now ? u.LockoutEnd : null));
        }
    }
}
