using BookCenter.Application.DTOs.Auth;

namespace BookCenter.Application.Abstractions.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterDto dto);
        Task<AuthResponseDto> LoginAsync(LoginDto dto);
        Task<CurrentUserDto> GetCurrentUserAsync(string userId);

        /// <summary>Changes the caller's password and returns a fresh token (older tokens stop working).</summary>
        Task<AuthResponseDto> ChangePasswordAsync(string userId, ChangePasswordDto dto);

        /// <summary>
        /// False when the account was deleted, locked by an admin, had its roles or password changed,
        /// or lost its reader card after the token was issued. Checked on every authenticated request.
        /// </summary>
        Task<bool> IsSessionValidAsync(string userId, string? securityStamp);
    }
}
