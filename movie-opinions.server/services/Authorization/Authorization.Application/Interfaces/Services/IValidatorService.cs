using Authorization.Application.DTO.Validator;
using Authorization.Domain.Entities;
using Contracts.Models.Response;

namespace Authorization.Application.Interfaces.Services
{
    public interface IValidatorService
    {
        Task<ValidationResult<Guid>> ValidateForUser(string oldPassword, string newPassword);

        Task<ValidationResult<Guid>> ValidateForSend(Guid requestId, string confirmationToken);

        Task<ValidationResult<UserPendingChange>> ValidateForConfirm(Guid requestId, string confirmationToken);
    }
}
