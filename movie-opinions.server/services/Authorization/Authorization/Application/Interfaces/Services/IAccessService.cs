using Authorization.Domain.DTO;
using Authorization.Domain.Entities;
using MovieOpinions.Contracts.Models.ServiceResponse;

namespace Authorization.Application.Interfaces.Services
{
    public interface IAccessService
    {
        Task<ServiceResponse<UserResponseDTO>> CheckUserAccess(User entity);
    }
}
