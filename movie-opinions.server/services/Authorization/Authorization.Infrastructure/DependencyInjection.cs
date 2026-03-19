using Authorization.Application.Interfaces.ExternalServices;
using Authorization.Application.Interfaces.Http;
using Authorization.Application.Interfaces.Infrastructure;
using Authorization.Application.Interfaces.Integration;
using Authorization.Application.Interfaces.Repositories;
using Authorization.Application.Interfaces.Security;
using Authorization.Application.Interfaces.Security.JWT;
using Authorization.Infrastructure.ExternalServices;
using Authorization.Infrastructure.Http;
using Authorization.Infrastructure.Identity;
using Authorization.Infrastructure.Integration;
using Authorization.Infrastructure.Integration.Step;
using Authorization.Infrastructure.Persistence.Context.AdoNet;
using Authorization.Infrastructure.Persistence.Repositories.ADO;
using Authorization.Infrastructure.Security;
using Authorization.Infrastructure.Security.JWT;
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
            services.AddScoped<IUserDeletionRepository, AdoUserDeletionRepository>();
            services.AddScoped<IUserRestrictionRepository, AdoUserRestrictionRepository>();
            services.AddScoped<IUserTokenRepository, AdoUserTokenRepository>();
            services.AddScoped<IUserPendingAccountChangesRepository, AdoUserPendingAccountChangesRepository>();

            // Реалізація 
            services.AddScoped<IHasher, Hasher>();
            services.AddScoped<ISendInternalRequest, SendInternalRequest>();
            services.AddScoped<IContactTypeDetector, ContactTypeDetector>();

            services.AddScoped<IPostRegistrationStep, ProfileStep>();
            services.AddScoped<IPostRegistrationStep, ContactsStep>();
            services.AddScoped<IPostRegistrationStep, NotificationStep>();

            services.AddScoped<IProfileSender, ProfileIntegrationSender>();
            services.AddScoped<IContactsSender, ContactsIntegrationSender>();
            services.AddScoped<INotificationSender, NotificationIntegrationSender>();
            services.AddScoped<IVerificationSender, VerificationIntegrationSender>();

            services.AddScoped<IRegistrationOrchestrator, RegistrationOrchestrator>();

            services.AddScoped<IUserJwtProvider, UserJwtProvider>();
            services.AddScoped<IServiceJwtProvider, ServiceJwtProvider>();
            services.AddScoped<ICookieProvider, CookieProvider>();
            services.AddScoped<IUserContext, UserContext>();
            services.AddScoped<IMaskContact, MaskContact>();

            services.AddProjectHttpClients(configuration);

            return services;
        }

        private static IServiceCollection AddProjectHttpClients(this IServiceCollection services, IConfiguration configuration)
        {
            var profileServiceUrl = configuration["ServiceUrls:ProfileService"];
            var contactsServiceUrl = configuration["ServiceUrls:ContactsService"];
            var notificationServiceUrl = configuration["ServiceUrls:NotificationService"];
            var verificationServiceUrl = configuration["ServiceUrls:VerificationService"];

            if (string.IsNullOrEmpty(profileServiceUrl) || 
                string.IsNullOrEmpty(contactsServiceUrl) || 
                string.IsNullOrEmpty(notificationServiceUrl) ||
                string.IsNullOrEmpty(verificationServiceUrl))
            {
                throw new Exception("Помилка при отримані рядку підключення!");
            }

            services.AddHttpClient("ProfileClient", client =>
            {
                client.BaseAddress = new Uri(profileServiceUrl);
            });

            services.AddHttpClient("ContactsClient", client =>
            {
                client.BaseAddress = new Uri(contactsServiceUrl);
            });

            services.AddHttpClient("NotificationClient", client =>
            {
                client.BaseAddress = new Uri(notificationServiceUrl);
            });

            services.AddHttpClient("VerificationClient", client =>
            {
                client.BaseAddress = new Uri(verificationServiceUrl);
            });

            return services;
        }
    }
}
