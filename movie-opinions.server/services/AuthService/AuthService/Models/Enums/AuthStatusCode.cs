namespace AuthService.Models.Enums
{
    public enum AuthStatusCode
    {
        // --- Успіх ---
        Success = 1,

        // --- Помилки бізнес-логіки (те, що бачить користувач) ---
        InvalidCredentials = 10,           // Невірний логін/пароль
        UserAlreadyExists = 11,            // Реєстрація: користувач з таким email вже є

        // --- Помилки, пов'язані зі статусом користувача ---
        UserNotFound = 20,                 // Логін: користувача не знайдено
        UserFound = 21,                    // Логін: користувача знайдено
        UserLockedOut = 22,                // Користувач заблокований
        UserDeleted = 23,                  // Користувача видалено
        EmailConfirmationRequired = 24,    // Потрібно підтвердити пошту
        UserCreated = 25,                  // Створення користувача

        // --- Помилки, пов'язані з токенами ---
        TokenExpired = 30,                 // JWT/Refresh Token прострочено
        InvalidToken = 31,                 // Недійсний або підроблений токен
        InvalidConfirmationToken = 32,     // Недійсний токен підтвердження

        // --- Технічні помилки (те, що не має вийти за межі сервісу) ---
        InternalServerError = 90,          // Загальна помилка сервера
        DatabaseFailure = 91               // Проблеми з підключенням/запитом до БД
    }
}
