using Authorization.Application.DTO.Context;
using Contracts.Model.Response;

namespace Authorization.Application.Interfaces.Integration
{
    public interface IRegistrationOrchestrator
    {
        Task<ServiceResponse> RunIntegrationsAsync(RegistrationContext context);
    }
}
