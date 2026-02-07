using Authorization.Domain.DTO;
using Authorization.Domain.Request;
using MovieOpinions.Contracts.Models.ServiceResponse;

namespace Authorization.Application.Interfaces.Services
{
    public interface IAuthorizationService
    {
        Task<ServiceResponse<UserResponseDTO>> LoginAsync(UserLoginModel loginMOdel);

        Task<ServiceResponse<UserResponseDTO>> RegisterAsync(UserRegisterModel registerModel);

        Task<ServiceResponse<bool>> LogoutAsync();
    }
}
