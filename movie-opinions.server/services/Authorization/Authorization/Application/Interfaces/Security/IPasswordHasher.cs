namespace Authorization.Application.Interfaces.Security
{
    public interface IPasswordHasher
    {
        Task<string> HashPasswordAsync(string password, string salt);

        Task<bool> VerifyPasswordAsync(string enteredPassword, string salt, string storedHash);
    }
}
