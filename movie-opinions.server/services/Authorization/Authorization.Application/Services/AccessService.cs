using Authorization.Application.DTO.Access;
using Authorization.Application.DTO.Users;
using Authorization.Application.Interfaces.Access;
using Authorization.Application.Interfaces.Services;
using Contracts.Models.Status;

namespace Authorization.Application.Services
{
    public class AccessService : IAccessService
    {
        private readonly IEnumerable<IAccessCheck> _accessCheck;

        public AccessService(IEnumerable<IAccessCheck> accessCheck)
        {
            _accessCheck = accessCheck.OrderBy(c => c.Prioriti);
        }

        public async Task<AccessResult> CheckUserAccess(UserAccessDTO userAccessDTO)
        {
            var propertiesToReset = new List<string>();

            foreach (var accessCheck in _accessCheck)
            {
                var property = typeof(UserAccessDTO).GetProperty(accessCheck.TargetProperty);
                if (property == null) continue;

                if (property.GetValue(userAccessDTO) is bool isFlagSet && isFlagSet)
                {
                    var result = await accessCheck.ExecuteAsync(userAccessDTO.UserId);

                    if (result.IsAllowed)
                    {
                        propertiesToReset.Add(accessCheck.TargetProperty);
                    }
                    else
                    {
                        return new AccessResult()
                        {
                            IsAllowed = false,
                            StatusCode = result.StatusCode,
                            PropertiesToReset = propertiesToReset,
                            Message = result.Message
                        };
                    }
                }
            }

            return new AccessResult()
            {
                IsAllowed = true,
                StatusCode = StatusCode.General.Ok,
                PropertiesToReset = propertiesToReset,
                Message = "Вхід дозволено!"
            };
        }
    }
}
