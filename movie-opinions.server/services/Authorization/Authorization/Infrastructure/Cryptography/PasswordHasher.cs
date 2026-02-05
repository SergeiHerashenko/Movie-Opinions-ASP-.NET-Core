using System.Security.Cryptography;
using System.Text;

namespace Authorization.Infrastructure.Cryptography
{
    public class PasswordHasher : IPasswordHasher
    {
        public async Task<string> HashPasswordAsync(string password, string salt)
        {
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password + salt);

            // Task.Run тут доречний, бо ComputeHash — це CPU-bound операція
            byte[] hashBytes = await Task.Run(() => SHA256.Create().ComputeHash(passwordBytes));

            return Convert.ToBase64String(hashBytes);
        }

        public async Task<bool> VerifyPasswordAsync(string enteredPassword, string salt, string storedHash)
        {
            string enteredHash = await HashPasswordAsync(enteredPassword, salt);

            return storedHash.Equals(enteredHash);
        }
    }
}
