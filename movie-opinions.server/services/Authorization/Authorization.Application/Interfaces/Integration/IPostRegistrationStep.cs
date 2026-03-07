using Authorization.Application.DTO.Context;
using Contracts.Model.Response;

namespace Authorization.Application.Interfaces.Integration
{
    public interface IPostRegistrationStep
    {
        int Order { get; }

        Task<ServiceResponse> ExecuteAsync(RegistrationContext context);

        Task RollbackAsync(Guid userId);
    }
}
