using MovieOpinions.Contracts.Models;
using MovieOpinions.Contracts.Models.ServiceResponse;
using System.Text;
using Verification.DAL.Interface;
using Verification.Models;
using Verification.Services.Interfaces;
using XSystem.Security.Cryptography;

namespace Verification.Services.Implementations
{
    public class VerificationService : IVerificationService
    {
        private readonly IVerificationRepositories _verificationRepositories;

        public VerificationService(IVerificationRepositories verificationRepositories)
        {
            _verificationRepositories = verificationRepositories;
        }

        public Task<ServiceResponse<Guid>> ConfirmVerification()
        {
            throw new NotImplementedException();
        }

        public Task<ServiceResponse<VerificationEntity>> GenerateVerificationCode()
        {
            throw new NotImplementedException();
        }

        public async Task<ServiceResponse<string>> GenerateVerificationToken(Guid userId)
        {
            string tokenGeneration = Guid.NewGuid().ToString();
            string tokenSalt = Guid.NewGuid().ToString();

            byte[] tokenBytes = Encoding.UTF8.GetBytes(tokenGeneration + tokenSalt);

            byte[] hashBytes = await Task.Run(() => new SHA256Managed().ComputeHash(tokenBytes));

            string hashedToken = Convert.ToBase64String(hashBytes);

            var tokenEntity = new VerificationEntity()
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Code = hashedToken,
                CreateAt = DateTime.Now,
                ExpiryDate = DateTime.Now.AddDays(1),
                Type = Models.Enums.VerificationType.URL
            };

            var createToken = await _verificationRepositories.CreateAsync(tokenEntity);
            
            if(createToken.StatusCode != StatusCode.Create.Created)
            {
                return new ServiceResponse<string>()
                {
                    IsSuccess = false,
                    StatusCode = createToken.StatusCode,
                    Message = createToken.Message
                };
            }

            string createURL = $"https://localhost:7089/confirm-page?token={hashedToken}";

            return new ServiceResponse<string>()
            {
                IsSuccess = true,
                StatusCode = StatusCode.General.Ok,
                Message = "Посилання створенно!",
                Data = createURL
            };
        }
    }
}
