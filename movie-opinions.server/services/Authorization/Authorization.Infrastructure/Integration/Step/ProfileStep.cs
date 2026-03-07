using Authorization.Application.DTO.Context;
using Authorization.Application.DTO.Integration;
using Authorization.Application.Interfaces.ExternalServices;
using Authorization.Application.Interfaces.Integration;
using Contracts.Model.Response;

namespace Authorization.Infrastructure.Integration.Step
{
    public class ProfileStep(IProfileSender profileSender) : IPostRegistrationStep
    {
        public int Order => 1;

        public async Task<ServiceResponse> ExecuteAsync(RegistrationContext context)
        {
            return await profileSender.SendCreateProfileRequestAsync(new ProfileIntegrationDTO
            {
                UserId = context.UserId,
                Login = context.Login,
            });
        }

        public async Task RollbackAsync(Guid userId)
        {
            await profileSender.SendDeleteProfileRequestAsync(userId);
        }
    }
}
