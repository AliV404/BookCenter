using BookCenter.Application.Abstractions.Security;
using BookCenter.Application.Common;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

namespace BookCenter.Infrastructure.Security
{
    public sealed class TokenService : ITokenService
    {
        private readonly JwtOptions _options;
        private readonly JsonWebTokenHandler _handler = new();

        public TokenService(IOptions<JwtOptions> options)
        {
            _options = options.Value;
        }

        public IssuedToken CreateToken(TokenSubject subject)
        {
            var claims = new List<Claim>
            {
                new(AppClaims.UserId, subject.UserId),
                new(AppClaims.Email, subject.Email),
                new(AppClaims.Name, subject.FullName),
                new(AppClaims.SecurityStamp, subject.SecurityStamp),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };
            claims.AddRange(subject.Roles.Select(role => new Claim(AppClaims.Role, role)));
            if (subject.ReaderId is not null)
                claims.Add(new Claim(AppClaims.ReaderId, subject.ReaderId.Value.ToString()));

            var expiresAt = DateTime.UtcNow.AddMinutes(_options.ExpiryMinutes);

            var token = _handler.CreateToken(new SecurityTokenDescriptor
            {
                Issuer = _options.Issuer,
                Audience = _options.Audience,
                Subject = new ClaimsIdentity(claims),
                Expires = expiresAt,
                SigningCredentials = new SigningCredentials(CreateSigningKey(_options.Key), SecurityAlgorithms.HmacSha256),
            });

            return new IssuedToken(token, expiresAt);
        }

        public static SymmetricSecurityKey CreateSigningKey(string key) => new(Encoding.UTF8.GetBytes(key));
    }
}
