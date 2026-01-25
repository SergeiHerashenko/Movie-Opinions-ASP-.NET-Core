using Verification.Models.Enums;

namespace Verification.Models
{
    public class VerificationEntity
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public string Code { get; set; }

        public string CodeSalt { get; set; }

        public VerificationType Type { get; set; }

        public DateTime CreateAt { get; set; }

        public DateTime ExpiryDate { get; set; }
    }
}
