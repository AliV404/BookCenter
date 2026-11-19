using BookCenter.Application.DTOs.Auth;

namespace BookCenter.Application.Abstractions.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterDto dto);
        Task<AuthResponseDto> LoginAsync(LoginDto dto);
        Task<CurrentUserDto> GetCurrentUserAsync(string userId);
    }
}
