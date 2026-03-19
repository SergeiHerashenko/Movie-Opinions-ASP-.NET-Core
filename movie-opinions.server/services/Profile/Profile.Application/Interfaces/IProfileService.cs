using Contracts.Models.Response;

namespace Profile.Application.Interfaces
{
    public interface IProfileService
    {
        Task<Result> ProfileCreateAsync();
    }
}
