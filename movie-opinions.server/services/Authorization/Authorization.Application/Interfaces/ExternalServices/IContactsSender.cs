using Authorization.Application.DTO.Integration;
using Authorization.Application.DTO.Integration.Responses;
using Contracts.Model.Response;

namespace Authorization.Application.Interfaces.ExternalServices
{
    public interface IContactsSender
    {
        Task<ServiceResponse> SendCreateContactRequestAsync(ContactIntegrationDTO contactCreateIntegrationDTO);

        Task<ServiceResponse> SendDeleteContactRequestAsync(Guid userId);

        Task<ServiceResponse> SendUpdateContactRequestAsync(ContactIntegrationDTO contactCreateIntegrationDTO);

        Task<ServiceResponse<IEnumerable<ContactResponseDTO>>> GetUserChannelsAsync(Guid userId);
    }
}
