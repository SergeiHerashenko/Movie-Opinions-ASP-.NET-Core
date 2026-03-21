using Contracts.Enum;
using Contracts.Models.Response;
using Contracts.Models.Status;
using Profile.Application.DTO.Users;
using Profile.Application.Interfaces.Repositories;
using Profile.Application.Interfaces.Service;
using Profile.Domain.Entities;
using System.Security.Cryptography;

namespace Profile.Application.Services
{
    public class ProfileService : IProfileService
    {
        private readonly IUserProfileRepositories _userProfileRepositories;

        public ProfileService(IUserProfileRepositories userProfileRepositories)
        {
            _userProfileRepositories = userProfileRepositories;
        }

        public async Task<Result> ProfileCreateAsync(CreateUserProfileDTO createUserProfileDTO)
        {
            var existingProfile = await _userProfileRepositories.GetByUserIdAsync(createUserProfileDTO.UserId);

            if(existingProfile != null)
            {
                return new Result()
                {
                    IsSuccess = false,
                    Message = "Користувач з таким Id вже має профіль!",
                    StatusCode = StatusCode.Create.Conflict
                };
            }

            var crateNewProfile = CreateNewProfileEntity(createUserProfileDTO);

            await _userProfileRepositories.CreateAsync(crateNewProfile);

            return new Result()
            {
                IsSuccess = true,
                Message = "Пhофіль користувача успішно створений!",
                StatusCode = StatusCode.Create.Created
            };
        }

        public Task<Result> ProfileDeleteAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Result> ProfileUpdatePublicDataAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Result> ProfileUpdateTechnicalDataAsync()
        {
            throw new NotImplementedException();
        }

        private UserProfile CreateNewProfileEntity(CreateUserProfileDTO createUserProfileDTO)
        {
            var firtName = "New_User_#" + GetRandomString();

            return new UserProfile()
            {
                 Id = Guid.NewGuid(),
                 UserId = createUserProfileDTO.UserId,
                 Login = createUserProfileDTO.Login,
                 Role = createUserProfileDTO.Role,
                 FirstName = firtName,
                 LastName = null,
                 DateRegistration = DateTime.UtcNow,
                 LastActive = DateTime.UtcNow,
                 IsOnline = true,
                 PhotoUrl = "default"
            };
        }

        private string GetRandomString()
        {
            const string Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            const int length = 6;

            byte[] data = RandomNumberGenerator.GetBytes(length);
            char[] result = new char[length];

            for (int i = 0; i < length; i++)
            {
                result[i] = Chars[data[i] % Chars.Length];
            }

            return new string(result);
        }
    }
}
