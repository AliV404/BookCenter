using BookCenter.Application.Abstractions.Security;
using BookCenter.Infrastructure.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BookCenter.Infrastructure.ServiceRegistration
{
    public static class ServiceRegistration
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions<JwtOptions>()
                .Bind(configuration.GetSection(JwtOptions.SectionName))
                .Validate(o => o.Key.Length >= 32, "Jwt:Key must be set and at least 32 characters long (use user-secrets or an environment variable).")
                .Validate(o => o.ExpiryMinutes > 0, "Jwt:ExpiryMinutes must be positive.")
                .ValidateOnStart();

            services.AddSingleton<ITokenService, TokenService>();
            return services;
        }
    }
}
