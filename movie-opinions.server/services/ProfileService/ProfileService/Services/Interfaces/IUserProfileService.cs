using ProfileService.Models.Profile;
using ProfileService.Models.Responses;

namespace ProfileService.Services.Interfaces
{
    public interface IUserProfileService
    {
        Task<ProfileResult<UserProfileDTO>> GetUserByIdAsync(Guid userId);

        Task<ProfileResult<List<UserSearchDTO>>> SearchUsersByNameAsync(string name);

        Task<ProfileResult<Guid>> CreateProfileAsync(CreateUserProfileDTO model);

        Task<ProfileResult<UserProfileDTO>> UpdateProfileAsync(Guid userId, UpdateProfileDTO model);

        Task<ProfileResult<bool>> DeleteProfileAsync(Guid userId);
    }
}
