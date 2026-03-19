using Profile.Domain.Common;

namespace Profile.Domain.Entities
{
    public class Profile : IBaseEntity
    {
        public Guid Id {  get; set; }

        public Guid UserId { get; set; }


    }
}
