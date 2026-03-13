using Authorization.Application.DTO.Access;
using Authorization.Application.DTO.Users;
using Authorization.Application.Interfaces.Access;
using Authorization.Application.Interfaces.Repositories;
using Contracts.Models.Status;

namespace Authorization.Application.AccessChecks
{
    public class BlockCheck : IAccessCheck
    {
        private readonly IUserRestrictionRepository _userRestrictionRepository;

        public BlockCheck(IUserRestrictionRepository userRestrictionRepository)
        {
            _userRestrictionRepository = userRestrictionRepository;
        }

        public string TargetProperty => nameof(UserAccessDTO.IsBlocked);

        public int Prioriti => 1;

        public async Task<CheckStepResult> ExecuteAsync(Guid userId)
        {
            var block = await _userRestrictionRepository.GetActiveBanByUserIdAsync(userId);
            
            if(block == null)
            {
                return new CheckStepResult()
                {
                    IsAllowed = true,
                    StatusCode = StatusCode.General.Ok,
                    Message = "Користувач не має бану"
                };
            }

            return block.ExpiresAt switch
            {
                null => new CheckStepResult()
                {
                    IsAllowed = false,
                    StatusCode = StatusCode.Auth.Locked,
                    Message = "Ваш акаунт заблоковано назавжди!"
                },

                var expires when expires < DateTime.UtcNow => new CheckStepResult()
                {
                    IsAllowed = true,
                    StatusCode = StatusCode.General.Ok,
                    Message = "Дія блокування закінчилась!"
                },

                _ => new CheckStepResult()
                {
                    IsAllowed = false,
                    StatusCode = StatusCode.Auth.Locked,
                    Message = $"Користувач заблокований до: {block.ExpiresAt:dd.MM.yyyy HH:mm}. Причина блокування: {block.Reason}"
                }
            };
        }
    }
}
