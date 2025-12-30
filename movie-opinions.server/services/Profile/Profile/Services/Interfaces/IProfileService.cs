using Profile.Models.Profile;
using Profile.Models.Responses;

namespace Profile.Services.Interfaces
{
    public interface IProfileService
    {
        Task<ProfileResult<UserProfileDTO>> GetUserByIdAsync(Guid userId);

        Task<ProfileResult<List<UserSearchDTO>>> SearchUsersByNameAsync(string name);

        Task<ProfileResult<Guid>> CreateProfileAsync(CreateUserProfileDTO model);

        Task<ProfileResult<UserProfileDTO>> UpdateProfileAsync(Guid userId, UpdateProfileDTO model);

        Task<ProfileResult<bool>> DeleteProfileAsync(Guid userId);
    }
}
