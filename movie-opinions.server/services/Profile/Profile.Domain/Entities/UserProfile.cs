using Contracts.Enum;
using Profile.Domain.Common;

namespace Profile.Domain.Entities
{
    public class UserProfile : IBaseEntity
    {
        public Guid Id {  get; set; }

        public Guid UserId { get; set; }

        public required string Login {  get; set; }

        public Role Role { get; set; } 

        public required string FirstName { get; set; }

        public string? LastName { get; set; }

        public DateTime DateRegistration { get; set; }

        public DateTime LastActive {  get; set; }

        public bool IsOnline { get; set; }

        public required string PhotoUrl { get; set; }
    }
}
