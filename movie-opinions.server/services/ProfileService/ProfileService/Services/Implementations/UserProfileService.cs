using ProfileService.DAL.Interface;
using ProfileService.Models.Profile;
using ProfileService.Models.Responses;
using ProfileService.Services.Interfaces;

namespace ProfileService.Services.Implementations
{
    public class UserProfileService : IUserProfileService
    {
        private readonly IProfileRepository _profileRepository;

        public UserProfileService(IProfileRepository profileRepository)
        {
            _profileRepository = profileRepository;
        }

        public async Task<ProfileResult<bool>> CreateProfileAsync(CreateUserProfileDTO model)
        {
            try
            {
                var newUser = new UserProfile()
                {
                    UserId = model.UserId,
                    UserName = model.Email,
                    FirstName = null,
                    LastName = null,
                    PhoneNumber = null,
                    Bio = null,
                    AvatarUrl = null,
                    CreatedAt = DateTime.UtcNow,
                    LastUpdatedAt = null
                };

                var createUserProfile = await _profileRepository.CreateUserAsync(newUser);

                if(createUserProfile.StatusCode == Models.Enums.ProfileStatusCode.ProfileCreated)
                {
                    return new ProfileResult<bool>
                    {
                        IsSuccess = true,
                        StatusCode = Models.Enums.ProfileStatusCode.ProfileCreated,
                        Message = "Користувача створенно!"
                    };
                }

                return new ProfileResult<bool>
                {
                    IsSuccess = false,
                    StatusCode = createUserProfile.StatusCode,
                    Message = createUserProfile.ErrorMessage,
                };
            }
            catch (Exception ex)
            {
                return new ProfileResult<bool>
                {
                    IsSuccess = false,
                    StatusCode = Models.Enums.ProfileStatusCode.ProfileInternalError,
                    Message = ex.Message,
                };
            }
        }

        public Task<ProfileResult<bool>> DeleteProfileAsync(Guid userId)
        {
            throw new NotImplementedException();
        }

        public Task<ProfileResult<UserProfileDTO>> GetUserByIdAsync(Guid userId)
        {
            throw new NotImplementedException();
        }

        public Task<ProfileResult<List<UserSearchDTO>>> SearchUsersByNameAsync(string name)
        {
            throw new NotImplementedException();
        }

        public Task<ProfileResult<string>> UpdateAvatarAsync(Guid userId, string avatarUrl)
        {
            throw new NotImplementedException();
        }

        public Task<ProfileResult<UserProfileDTO>> UpdateProfileAsync(Guid userId, UpdateProfileDTO model)
        {
            throw new NotImplementedException();
        }
    }
}
