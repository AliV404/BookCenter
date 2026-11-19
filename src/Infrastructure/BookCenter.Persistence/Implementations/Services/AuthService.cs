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

            // Account, role and reader card are created together or not at all.
            await using var transaction = await _context.Database.BeginTransactionAsync();

            var user = new AppUser { UserName = email, Email = email, FullName = $"{dto.Name} {dto.Surname}" };
            var created = await _userManager.CreateAsync(user, dto.Password);
            if (!created.Succeeded)
                throw ToValidationException(created.Errors);

            var roleAdded = await _userManager.AddToRoleAsync(user, Roles.Reader);
            if (!roleAdded.Succeeded)
                throw new InvalidOperationException("Could not assign the Reader role: " + string.Join(" ", roleAdded.Errors.Select(e => e.Description)));

            // If staff already issued a reader card with this email at the desk, link to it
            // (keeping the details staff entered); otherwise create a new card.
            var reader = await _context.Readers.FirstOrDefaultAsync(r => r.Email == email);
            if (reader is null)
            {
                reader = new Reader { Name = dto.Name, Surname = dto.Surname, Email = email, Phone = dto.Phone.Trim() };
                _context.Readers.Add(reader);
            }
            else if (reader.UserId is not null)
            {
                throw new ConflictException("This reader card is already linked to another account.");
            }

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
                throw new AuthenticationFailedException("Too many failed attempts. Please try again in a few minutes.");

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

        private Task<int?> FindReaderIdAsync(string userId) =>
            _context.Readers.Where(r => r.UserId == userId).Select(r => (int?)r.Id).FirstOrDefaultAsync();

        private AuthResponseDto CreateResponse(AppUser user, IReadOnlyCollection<string> roles, int? readerId)
        {
            var token = _tokenService.CreateToken(new TokenSubject(user.Id, user.Email!, user.FullName, roles, readerId));
            return new AuthResponseDto(token.Token, token.ExpiresAt, new CurrentUserDto(user.Id, user.Email!, user.FullName, roles, readerId));
        }

        /// <summary>Maps Identity's error codes onto the request field they belong to.</summary>
        private static RequestValidationException ToValidationException(IEnumerable<IdentityError> errors)
        {
            var byField = errors
                .GroupBy(e => e.Code switch
                {
                    var c when c.StartsWith("Password") => "password",
                    var c when c.Contains("Email") || c.Contains("UserName") => "email",
                    _ => ""
                })
                .ToDictionary(g => g.Key, g => g.Select(e => e.Description).ToArray());

            return new RequestValidationException(byField);
        }
    }
}
