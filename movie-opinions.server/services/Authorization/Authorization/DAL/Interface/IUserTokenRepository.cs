using Authorization.Domain.Entities;
using MovieOpinions.Contracts.Models.Interface;
using MovieOpinions.Contracts.Models.RepositoryResponse;

namespace Authorization.DAL.Interface
{
    public interface IUserTokenRepository : IBaseRepository<UserToken, RepositoryResponse<UserToken>>
    {
        Task<RepositoryResponse<UserToken>> GetUserTokenAsync();
    }
}
