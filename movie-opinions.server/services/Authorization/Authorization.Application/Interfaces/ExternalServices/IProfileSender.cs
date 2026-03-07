using Authorization.Application.DTO.Integration;
using Contracts.Model.Response;

namespace Authorization.Application.Interfaces.ExternalServices
{
    public interface IProfileSender
    {
        Task<ServiceResponse> SendCreateProfileRequestAsync(ProfileIntegrationDTO profileIntegrationDTO);

        Task<ServiceResponse> SendUpdateProfileRequestAsync(ProfileIntegrationDTO profileIntegrationDTO);

        Task<ServiceResponse> SendDeleteProfileRequestAsync(Guid userId);
    }
}
