using Contracts.Models.Response;
using Profile.Application.DTO.Users;

namespace Profile.Application.Interfaces.Service
{
    public interface IProfileService
    {
        Task<Result> ProfileCreateAsync(CreateUserProfileDTO createUserProfileDTO);

        Task<Result> ProfileUpdatePublicDataAsync();

        Task<Result> ProfileUpdateTechnicalDataAsync();

        Task<Result> ProfileDeleteAsync();
    }
}
