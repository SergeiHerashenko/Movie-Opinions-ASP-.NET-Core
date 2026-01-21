using MovieOpinions.Contracts.Models.ServiceResponse;
using Profile.Models.Profile;

namespace Profile.Services.Interfaces
{
    public interface IProfileService
    {
        Task<ServiceResponse<UserProfileDTO>> GetUserByIdAsync(Guid userId);

        Task<ServiceResponse<List<UserSearchDTO>>> SearchUsersByNameAsync(string name);

        Task<ServiceResponse<Guid>> CreateProfileAsync(CreateUserProfileDTO model);

        Task<ServiceResponse<UserProfileDTO>> UpdateProfileAsync(Guid userId, UpdateProfileDTO model);

        Task<ServiceResponse<bool>> DeleteProfileAsync(Guid userId);
    }
}
