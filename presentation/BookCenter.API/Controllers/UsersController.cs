using BookCenter.Application.Abstractions.Services;
using BookCenter.Application.Common;
using BookCenter.Application.DTOs.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookCenter.API.Controllers
{
    /// <summary>Login accounts. Reader details (name, email, phone) are edited through /api/readers.</summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = Roles.Admin)]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _service;

        public UsersController(IUserService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResult<GetUserDto>>> GetAll([FromQuery] UserQuery query)
        {
            return Ok(await _service.GetAllAsync(query));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<GetUserDto>> GetById(string id)
        {
            return Ok(await _service.GetByIdAsync(id));
        }

        /// <summary>Grants or removes the Admin role. You can't remove your own.</summary>
        [HttpPut("{id}/admin")]
        public async Task<ActionResult<GetUserDto>> SetAdmin(string id, SetAdminDto dto)
        {
            await _service.SetAdminAsync(id, dto.IsAdmin);
            return Ok(await _service.GetByIdAsync(id));
        }

        /// <summary>Blocks sign-in until unlocked and ends the user's current sessions.</summary>
        [HttpPost("{id}/lock")]
        public async Task<ActionResult<GetUserDto>> Lock(string id)
        {
            await _service.LockAsync(id);
            return Ok(await _service.GetByIdAsync(id));
        }

        /// <summary>Lifts an admin lock or a failed-login lockout.</summary>
        [HttpPost("{id}/unlock")]
        public async Task<ActionResult<GetUserDto>> Unlock(string id)
        {
            await _service.UnlockAsync(id);
            return Ok(await _service.GetByIdAsync(id));
        }

        /// <summary>Sets a new password (e.g. a reader forgot theirs) and ends the user's current sessions.</summary>
        [HttpPost("{id}/reset-password")]
        public async Task<IActionResult> ResetPassword(string id, ResetPasswordDto dto)
        {
            await _service.ResetPasswordAsync(id, dto.NewPassword);
            return NoContent();
        }
    }
}
