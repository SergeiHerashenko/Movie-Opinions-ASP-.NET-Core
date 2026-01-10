using Profile.DAL.Interface;
using Profile.Models.Profile;
using Profile.Models.Responses;
using Profile.Services.Interfaces;

namespace Profile.Services.Implementations
{
    public class ProfileService : IProfileService
    {
        private readonly IProfileRepository _profileRepository;

        public ProfileService(IProfileRepository profileRepository)
        {
            _profileRepository = profileRepository;
        }

        public async Task<ProfileResult<Guid>> CreateProfileAsync(CreateUserProfileDTO model)
        {
            try
            {
                var newUser = new UserProfile()
                {
                    UserId = model.UserId,
                    UserName = model.Email.Split('@')[0],
                    FirstName = null,
                    LastName = null,
                    PhoneNumber = null,
                    Bio = null,
                    AvatarUrl = null,
                    CreatedAt = DateTime.UtcNow,
                    LastUpdatedAt = null
                };

                var createUserProfile = await _profileRepository.CreateUserAsync(newUser);

                if (createUserProfile.StatusCode == Models.Enums.ProfileStatusCode.ProfileCreated)
                {
                    return new ProfileResult<Guid>
                    {
                        IsSuccess = true,
                        StatusCode = Models.Enums.ProfileStatusCode.ProfileCreated,
                        Message = "Користувача створенно!",
                        Data = model.UserId
                    };
                }

                return new ProfileResult<Guid>
                {
                    IsSuccess = false,
                    StatusCode = createUserProfile.StatusCode,
                    Message = createUserProfile.Message,
                    Data = model.UserId
                };
            }
            catch (Exception ex)
            {
                return new ProfileResult<Guid>
                {
                    IsSuccess = false,
                    StatusCode = Models.Enums.ProfileStatusCode.ProfileInternalError,
                    Message = ex.Message,
                    Data = model.UserId
                };
            }
        }

        public async Task<ProfileResult<bool>> DeleteProfileAsync(Guid userId)
        {
            throw new NotImplementedException();
        }

        public async Task<ProfileResult<UserProfileDTO>> GetUserByIdAsync(Guid userId)
        {
            throw new NotImplementedException();
        }

        public async Task<ProfileResult<List<UserSearchDTO>>> SearchUsersByNameAsync(string name)
        {
            throw new NotImplementedException();
        }

        public async Task<ProfileResult<UserProfileDTO>> UpdateProfileAsync(Guid userId, UpdateProfileDTO model)
        {
            throw new NotImplementedException();
        }
    }
}
