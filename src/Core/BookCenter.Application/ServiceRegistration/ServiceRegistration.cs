using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace BookCenter.Application.ServiceRegistration
{
    public static class ServiceRegistration
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
            return services;
        }
    }
}
