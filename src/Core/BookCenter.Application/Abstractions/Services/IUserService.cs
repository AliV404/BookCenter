using BookCenter.Application.Common;
using BookCenter.Application.DTOs.Users;

namespace BookCenter.Application.Abstractions.Services
{
    /// <summary>Account administration. Every change signs the affected user out of their other sessions.</summary>
    public interface IUserService
    {
        Task<PagedResult<GetUserDto>> GetAllAsync(UserQuery query);
        Task<GetUserDto> GetByIdAsync(string id);
        Task SetAdminAsync(string id, bool isAdmin);
        Task LockAsync(string id);
        Task UnlockAsync(string id);
        Task ResetPasswordAsync(string id, string newPassword);
    }
}
