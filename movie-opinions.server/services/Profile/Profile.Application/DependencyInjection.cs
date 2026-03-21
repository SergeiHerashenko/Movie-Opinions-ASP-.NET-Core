using Microsoft.Extensions.DependencyInjection;
using Profile.Application.Interfaces.Service;
using Profile.Application.Services;

namespace Profile.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<IProfileService, ProfileService>();

            return services;
        }
    }
}
