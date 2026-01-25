using MovieOpinions.Contracts.Models.ServiceResponse;
using MovieOpinions.Contracts.Models;
using Profile.DAL.Interface;
using Profile.Models.Profile;
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

        public async Task<ServiceResponse<Guid>> CreateProfileAsync(CreateUserProfileDTO model)
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

                var createUserProfile = await _profileRepository.CreateAsync(newUser);

                if (createUserProfile.StatusCode == StatusCode.Create.Created)
                {
                    return new ServiceResponse<Guid>
                    {
                        IsSuccess = true,
                        StatusCode = StatusCode.Create.Created,
                        Message = "Користувача створенно!",
                        Data = model.UserId
                    };
                }

                return new ServiceResponse<Guid>
                {
                    IsSuccess = false,
                    StatusCode = createUserProfile.StatusCode,
                    Message = createUserProfile.Message,
                    Data = model.UserId
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponse<Guid>
                {
                    IsSuccess = false,
                    StatusCode = StatusCode.General.InternalError,
                    Message = ex.Message,
                    Data = model.UserId
                };
            }
        }

        public async Task<ServiceResponse<bool>> DeleteProfileAsync(Guid userId)
        {
            throw new NotImplementedException();
        }

        public async Task<ServiceResponse<UserProfileDTO>> GetUserByIdAsync(Guid userId)
        {
            throw new NotImplementedException();
        }

        public async Task<ServiceResponse<List<UserSearchDTO>>> SearchUsersByNameAsync(string name)
        {
            throw new NotImplementedException();
        }

        public async Task<ServiceResponse<UserProfileDTO>> UpdateProfileAsync(Guid userId, UpdateProfileDTO model)
        {
            throw new NotImplementedException();
        }
    }
}
