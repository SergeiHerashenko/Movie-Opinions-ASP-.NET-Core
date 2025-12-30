using System.Text;
using XSystem.Security.Cryptography;

namespace Authorization.Helpers
{
    public class HashPassword
    {
        public async Task<string> GetHashedPasswordAsync(string passwordUser, string passwordSalt)
        {
            // Перетворюємо пароль та ключ в масив байтів
            byte[] passwordBytes = Encoding.UTF8.GetBytes(passwordUser + passwordSalt);
            // Обчислюємо хеш SHA-256 для об'єднаного масиву байтів паролю та ключа
            byte[] hashBytes = await Task.Run(() => new SHA256Managed().ComputeHash(passwordBytes));
            // Перетворюємо масив байтів хешу в рядок Base64
            string hashedPassword = Convert.ToBase64String(hashBytes);
            // Повертаємо хешований пароль у вигляді рядка
            return hashedPassword;
        }
    }
}
