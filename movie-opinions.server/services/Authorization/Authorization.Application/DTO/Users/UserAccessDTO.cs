namespace Authorization.Application.DTO.Users
{
    public class UserAccessDTO
    {
        public Guid UserId { get; set; }

        public bool IsBlocked { get; set; }

        public bool IsDeleted { get; set; }
    }
}
