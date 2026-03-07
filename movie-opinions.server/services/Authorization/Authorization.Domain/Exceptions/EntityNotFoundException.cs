using static Contracts.Models.Status.StatusCode;

namespace Authorization.Domain.Exceptions
{
    public class EntityNotFoundException : BaseApplicationException
    {
        public EntityNotFoundException(string message)
            : base(message, General.NotFound) { }
    }
}
