using Contracts.Models.Status;

namespace Authorization.Application.DTO.Access
{
    public class CheckStepResult
    {
        public bool IsAllowed { get; set; }

        public required StatusCode StatusCode { get; set; }

        public required string Message { get; set; }
    }
}
