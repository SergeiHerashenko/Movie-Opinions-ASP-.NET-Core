namespace ProfileService.Models.Profile
{
    public class UserSearchDTO
    {
        public Guid UserId { get; set; }

        public string UserName { get; set; } = string.Empty;

        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public string? AvatarUrl { get; set; }
    }
}
