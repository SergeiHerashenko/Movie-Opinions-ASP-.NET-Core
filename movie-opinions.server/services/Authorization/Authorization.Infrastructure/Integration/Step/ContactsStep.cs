using Authorization.Application.DTO.Context;
using Authorization.Application.DTO.Integration;
using Authorization.Application.Interfaces.ExternalServices;
using Authorization.Application.Interfaces.Integration;
using Contracts.Model.Response;

namespace Authorization.Infrastructure.Integration.Step
{
    public class ContactsStep(IContactsSender contactsSender) : IPostRegistrationStep
    {
        public int Order => 2;

        public async Task<ServiceResponse> ExecuteAsync(RegistrationContext context)
        {
            return await contactsSender.SendCreateContactRequestAsync(new ContactIntegrationDTO()
            {
                UserId = context.UserId,
                ContactValue = context.Login,
                CommunicationChannel = context.Channel
            });
        }

        public async Task RollbackAsync(Guid userId)
        {
            await contactsSender.SendDeleteContactRequestAsync(userId);
        }
    }
}
