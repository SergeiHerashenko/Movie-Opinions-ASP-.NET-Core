using Authorization.Application.DTO.Access;
using Authorization.Application.DTO.Users;
using Authorization.Application.Interfaces.Access;
using Authorization.Application.Interfaces.Repositories;
using Contracts.Models.Status;

namespace Authorization.Application.AccessChecks
{
    public class DeletionCheck : IAccessCheck
    {
        private readonly IUserDeletionRepository _userDeletionRepository;

        public DeletionCheck(IUserDeletionRepository userDeletionRepository)
        {
            _userDeletionRepository = userDeletionRepository;
        }

        public string TargetProperty => nameof(UserAccessDTO.IsDeleted);

        public int Prioriti => 2;

        public async Task<CheckStepResult> ExecuteAsync(Guid userId)
        {
            var deletion = await _userDeletionRepository.GetUserDeletionsByIdAsync(userId);

            if (deletion == null)
            {
                return new CheckStepResult()
                {
                    IsAllowed = true,
                    StatusCode = StatusCode.General.Ok,
                    Message = "Користувач не видалений!"
                };
            }

            return new CheckStepResult()
            {
                IsAllowed = false,
                StatusCode = StatusCode.Auth.Deleted,
                Message = $"Користувач видалений! Причина видалення: {deletion.Reason}"
            };
        }
    }
}
