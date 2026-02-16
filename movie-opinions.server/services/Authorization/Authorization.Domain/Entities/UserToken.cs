using Authorization.Domain.Common;

namespace Authorization.Domain.Entities
{
    public class UserToken : IBaseEntity
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public required string RefreshToken { get; set; }

        public DateTime RefreshTokenExpiration { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}