using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Profile.Application.Interfaces.Repositories;
using Profile.Infrastructure.Persistence.Context.AdoNet;
using Profile.Infrastructure.Persistence.Repositories.ADO;

namespace Profile.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Провайдер підключення
            services.AddSingleton<IDbConnectionProvider, ConnectProfileDb>();

            // Репозиторії ADO
            services.AddScoped<IUserProfileRepositories, AdoUserProfileRepositories>();

            services.AddProjectHttpClients(configuration);

            return services;
        }

        private static IServiceCollection AddProjectHttpClients(this IServiceCollection services, IConfiguration configuration)
        {
            return services;
        }
    }
}
