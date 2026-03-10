using Authorization.Application.DTO.Access;

namespace Authorization.Application.Interfaces.Access
{
    public interface IAccessCheck
    {
        string TargetProperty { get; }

        int Prioriti {  get; }

        Task<CheckStepResult> ExecuteAsync(Guid userId);
    }
}
