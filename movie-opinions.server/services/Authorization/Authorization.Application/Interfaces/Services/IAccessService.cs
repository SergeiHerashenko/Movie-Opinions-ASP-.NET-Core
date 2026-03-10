using Authorization.Application.DTO.Access;
using Authorization.Application.DTO.Users;

namespace Authorization.Application.Interfaces.Services
{
    public interface IAccessService
    {
        Task<AccessResult> CheckUserAccess(UserAccessDTO userAccessDTO);
    }
}
