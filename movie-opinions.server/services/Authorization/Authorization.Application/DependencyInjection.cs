using Authorization.Application.Interfaces.Services;
using Authorization.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Authorization.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<IAuthorizationService, AuthorizationService>();

            return services;
        }
    }
}
