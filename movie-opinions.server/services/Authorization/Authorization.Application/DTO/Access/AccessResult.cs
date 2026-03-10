using Contracts.Models.Status;

namespace Authorization.Application.DTO.Access
{
    public class AccessResult
    {
        public bool IsAllowed { get; set; }

        public required StatusCode StatusCode { get; set; }

        public required string Message { get; set; }

        public List<string> PropertiesToReset { get; set; } = new();
    }
}
