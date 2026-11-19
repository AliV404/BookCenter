using BookCenter.Application.Abstractions.Security;
using BookCenter.Application.Common;
using System.Security.Claims;

namespace BookCenter.API.Infrastructure
{
    /// <summary>Reads the current user from the validated JWT on the request.</summary>
    public sealed class HttpCurrentUser : ICurrentUser
    {
        private readonly IHttpContextAccessor _accessor;

        public HttpCurrentUser(IHttpContextAccessor accessor)
        {
            _accessor = accessor;
        }

        private ClaimsPrincipal? Principal => _accessor.HttpContext?.User;

        public string? UserId => Principal?.FindFirstValue(AppClaims.UserId);
        public string? Email => Principal?.FindFirstValue(AppClaims.Email);
        public bool IsAdmin => Principal?.IsInRole(Roles.Admin) == true;
        public int? ReaderId => int.TryParse(Principal?.FindFirstValue(AppClaims.ReaderId), out var id) ? id : null;
    }
}
