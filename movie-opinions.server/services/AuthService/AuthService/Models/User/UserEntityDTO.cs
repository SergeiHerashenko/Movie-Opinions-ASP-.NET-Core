namespace AuthService.Models.User
{
    public class UserEntityDTO
    {
        public Guid UserId { get; set; }

        public string Email { get; set; }

        public bool IsEmailConfirmed { get; set; }
    }
}
