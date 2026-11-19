namespace BookCenter.Application.DTOs.Auth
{
    public class RegisterDto
    {
        public string Name { get; set; } = null!;
        public string Surname { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string Password { get; set; } = null!;
    }

    public class LoginDto
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
    }

    public class ChangePasswordDto
    {
        public string CurrentPassword { get; set; } = null!;
        public string NewPassword { get; set; } = null!;
    }

    public record CurrentUserDto(string Id, string Email, string FullName, IReadOnlyCollection<string> Roles, int? ReaderId);

    public record AuthResponseDto(string Token, DateTime ExpiresAt, CurrentUserDto User);
}
