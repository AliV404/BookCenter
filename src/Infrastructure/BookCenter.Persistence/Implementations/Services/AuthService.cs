using BookCenter.Application.Abstractions.Security;
using BookCenter.Application.Abstractions.Services;
using BookCenter.Application.Common;
using BookCenter.Application.DTOs.Auth;
using BookCenter.Application.Exceptions;
using BookCenter.Domain.Entities;
using BookCenter.Persistence.Data;
using BookCenter.Persistence.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BookCenter.Persistence.Implementations.Services
{
    public class AuthService : IAuthService
    {
        private const string InvalidCredentials = "Invalid email or password.";

        private readonly UserManager<AppUser> _userManager;
        private readonly AppDbContext _context;
        private readonly ITokenService _tokenService;

        public AuthService(UserManager<AppUser> userManager, AppDbContext context, ITokenService tokenService)
        {
            _userManager = userManager;
            _context = context;
            _tokenService = tokenService;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
        {
            var email = dto.Email.Trim().ToLowerInvariant();

            if (await _userManager.FindByEmailAsync(email) is not null)
                throw new ConflictException("An account with this email already exists. Try signing in.");

            // If staff already issued a reader card with this email at the desk, link to it
            // (keeping the details staff entered); otherwise create a new card.
            var reader = await _context.Readers.FirstOrDefaultAsync(r => r.Email == email);
            if (reader?.UserId is not null)
                throw new ConflictException("This reader card is already linked to another account.");
            reader ??= new Reader { Name = dto.Name, Surname = dto.Surname, Email = email, Phone = dto.Phone.Trim() };

            // Account, role and reader card are created together or not at all.
            await using var transaction = await _context.Database.BeginTransactionAsync();

            // The account's name always follows the card (also when staff later edit it).
            var user = new AppUser { UserName = email, Email = email, FullName = $"{reader.Name} {reader.Surname}" };
            var created = await _userManager.CreateAsync(user, dto.Password);
            if (!created.Succeeded)
                throw IdentityErrors.ToValidationException(created.Errors);

            (await _userManager.AddToRoleAsync(user, Roles.Reader)).ThrowIfFailed("assign the Reader role");

            if (reader.Id == 0)
                _context.Readers.Add(reader);
            reader.UserId = user.Id;
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return CreateResponse(user, [Roles.Reader], reader.Id);
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email.Trim())
                ?? throw new AuthenticationFailedException(InvalidCredentials);

            if (await _userManager.IsLockedOutAsync(user))
                throw new AuthenticationFailedException(user.IsLockedByAdmin()
                    ? "This account has been locked. Please contact the library."
                    : "Too many failed attempts. Please try again in a few minutes.");

            if (!await _userManager.CheckPasswordAsync(user, dto.Password))
            {
                // Counts towards lockout (see Lockout options in ServiceRegistration).
                await _userManager.AccessFailedAsync(user);
                throw new AuthenticationFailedException(InvalidCredentials);
            }
            await _userManager.ResetAccessFailedCountAsync(user);

            var roles = (await _userManager.GetRolesAsync(user)).ToList();
            var readerId = await FindReaderIdAsync(user.Id);

            // A reader whose card was deleted by staff can no longer sign in.
            if (!roles.Contains(Roles.Admin) && readerId is null)
                throw new AuthenticationFailedException("This account is no longer active. Please contact the library.");

            return CreateResponse(user, roles, readerId);
        }

        public async Task<CurrentUserDto> GetCurrentUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId)
                ?? throw new AuthenticationFailedException("Account not found.");

            var roles = (await _userManager.GetRolesAsync(user)).ToList();
            return new CurrentUserDto(user.Id, user.Email!, user.FullName, roles, await FindReaderIdAsync(user.Id));
        }

        public async Task<AuthResponseDto> ChangePasswordAsync(string userId, ChangePasswordDto dto)
        {
            var user = await _userManager.FindByIdAsync(userId)
                ?? throw new AuthenticationFailedException("Account not found.");

            // Also rotates the security stamp, so sessions on other devices are signed out.
            var changed = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
            if (!changed.Succeeded)
                throw IdentityErrors.ToValidationException(changed.Errors, passwordField: "newPassword");

            var roles = (await _userManager.GetRolesAsync(user)).ToList();
            return CreateResponse(user, roles, await FindReaderIdAsync(user.Id));
        }

        public async Task<bool> IsSessionValidAsync(string userId, string? securityStamp)
        {
            if (securityStamp is null)
                return false;

            // One small query per authenticated request. Every admin action that changes what a
            // user may do (roles, lock, password reset, deleting their reader card) rotates the stamp.
            var currentStamp = await _context.Users
                .Where(u => u.Id == userId)
                .Select(u => u.SecurityStamp)
                .FirstOrDefaultAsync();

            return currentStamp == securityStamp;
        }

        private Task<int?> FindReaderIdAsync(string userId) =>
            _context.Readers.Where(r => r.UserId == userId).Select(r => (int?)r.Id).FirstOrDefaultAsync();

        private AuthResponseDto CreateResponse(AppUser user, IReadOnlyCollection<string> roles, int? readerId)
        {
            var token = _tokenService.CreateToken(new TokenSubject(user.Id, user.Email!, user.FullName, roles, readerId, user.SecurityStamp!));
            return new AuthResponseDto(token.Token, token.ExpiresAt, new CurrentUserDto(user.Id, user.Email!, user.FullName, roles, readerId));
        }
    }
}
