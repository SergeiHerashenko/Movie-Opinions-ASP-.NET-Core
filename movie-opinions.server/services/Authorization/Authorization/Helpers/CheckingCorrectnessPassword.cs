namespace Authorization.Helpers
{
    public class CheckingCorrectnessPassword
    {
        public async Task<bool> VerifyPasswordAsync(string enteredPassword, string passwordSalt, string storedHash)
        {
            // Шифруємо введений пароль з використанням ключа (солі)
            string enteredHash = await new HashPassword().GetHashedPasswordAsync(enteredPassword, passwordSalt);
            // Порівнюємо отриманий хеш зі збереженим хешем
            return storedHash.Equals(enteredHash);
        }
    }
}
