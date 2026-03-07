using static Contracts.Models.Status.StatusCode;

namespace Authorization.Domain.Exceptions
{
    public class VerificationTokenExpiredException : BaseApplicationException
    {
        public VerificationTokenExpiredException()
            : base("Термін дії посилання вичерпано. Запитуйте нове.", Verification.Expired) { }
    }
}
