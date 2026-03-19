using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Profile.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddProjectHttpClients(configuration);

            return services;
        }

        private static IServiceCollection AddProjectHttpClients(this IServiceCollection services, IConfiguration configuration)
        {
            return services;
        }
    }
}
