using Authorization.Application.Interfaces.Identity;
using Authorization.Application.Interfaces.Repositories;
using Authorization.Application.Interfaces.Security;
using Authorization.Infrastructure.Identity;
using Authorization.Infrastructure.Persistence.Context.AdoNet;
using Authorization.Infrastructure.Persistence.Repositories.ADO;
using Authorization.Infrastructure.Persistence.Repositories.Dapper;
using Authorization.Infrastructure.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Authorization.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Провайдер підключення
            services.AddSingleton<IDbConnectionProvider, ConnectAuthorizationDb>();

            // Репозиторії ADO
            services.AddScoped<IUserRepository, AdoUserRepository>();
            services.AddScoped<IUserPendingRegistrationRepository, AdoUserPendingRegistrationRepository>();
            services.AddScoped<IUserDeletionRepository, AdoUserDeletionRepository>();
            services.AddScoped<IUserPendingAccountChangesRepository, AdoUserPendingAccountChangesRepository>();
            services.AddScoped<IUserRefreshTokenRepository, AdoUserTokenRepository>();
            services.AddScoped<IUserRestrictionRepository, AdoUserRestrictionRepository>();

            // Репозиторії Dapper
            //services.AddScoped<IUserRepository, DapperUserRepository>();
            //services.AddScoped<IUserPendingRegistrationRepository, DapperUserPendingRegistrationRepository>();
            //services.AddScoped<IUserRefreshTokenRepository, DapperUserTokenRepository>();
            //services.AddScoped<IUserDeletionRepository, DapperUserDeletionRepository>();
            //services.AddScoped<IUserPendingAccountChangesRepository, DapperUserPendingAccountChangesRepository>();
            //services.AddScoped<IUserRestrictionRepository, DapperUserRestrictionRepository>();

            // Реалізація 
            services.AddScoped<IHasher, Hasher>();

            services.AddScoped<IUserContext, UserContext>();

            services.AddProjectHttpClients(configuration);

            return services;
        }

        private static IServiceCollection AddProjectHttpClients(this IServiceCollection services, IConfiguration configuration)
        {
            return services;
        }
    }
}
