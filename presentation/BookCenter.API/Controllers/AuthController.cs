using BookCenter.Application.Abstractions.Security;
using BookCenter.Application.Abstractions.Services;
using BookCenter.Application.DTOs.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookCenter.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ICurrentUser _currentUser;

        public AuthController(IAuthService authService, ICurrentUser currentUser)
        {
            _authService = authService;
            _currentUser = currentUser;
        }

        /// <summary>Creates a reader account (and reader card) and signs it in.</summary>
        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDto>> Register(RegisterDto dto)
        {
            return Ok(await _authService.RegisterAsync(dto));
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login(LoginDto dto)
        {
            return Ok(await _authService.LoginAsync(dto));
        }

        /// <summary>The signed-in user. Useful for the frontend to re-check a stored token.</summary>
        [HttpGet("me")]
        public async Task<ActionResult<CurrentUserDto>> Me()
        {
            return Ok(await _authService.GetCurrentUserAsync(_currentUser.UserId!));
        }
    }
}
