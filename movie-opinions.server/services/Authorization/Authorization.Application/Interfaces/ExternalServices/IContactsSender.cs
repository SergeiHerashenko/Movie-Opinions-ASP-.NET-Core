using Authorization.Application.DTO.Integration;
using Contracts.Model.Response;

namespace Authorization.Application.Interfaces.ExternalServices
{
    public interface IContactsSender
    {
        Task<ServiceResponse> SendCreateContactRequestAsync(ContactIntegrationDTO contactCreateIntegrationDTO);

        Task<ServiceResponse> SendDeleteContactRequestAsync(Guid userId);

        Task<ServiceResponse> SendUpdateContactRequestAsync(ContactIntegrationDTO contactCreateIntegrationDTO);
    }
}
