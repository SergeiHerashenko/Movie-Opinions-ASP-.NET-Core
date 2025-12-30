namespace Profile.Models.Profile
{
    public class UserProfileDTO
    {
        public Guid UserId { get; set; }

        // Основна інформація
        public string UserName { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }

        // Контакти 
        public string? PhoneNumber { get; set; }

        // Деталі профілю
        public string? Bio { get; set; }
        public string? AvatarUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
