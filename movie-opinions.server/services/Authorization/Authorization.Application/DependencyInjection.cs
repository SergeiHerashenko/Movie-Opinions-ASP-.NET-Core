using Authorization.Application.AccessChecks;
using Authorization.Application.Interfaces.Access;
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
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IAccessService, AccessService>();
            services.AddScoped<IAccessCheck, BlockCheck>();
            services.AddScoped<IAccessCheck, DeletionCheck>();

            return services;
        }
    }
}
