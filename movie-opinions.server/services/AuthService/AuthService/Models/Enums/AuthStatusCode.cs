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
        UserLockedOut = 21,                // Користувач заблокований
        EmailConfirmationRequired = 22,    // Потрібно підтвердити пошту

        // --- Помилки, пов'язані з токенами ---
        TokenExpired = 30,                 // JWT/Refresh Token прострочено
        InvalidToken = 31,                 // Недійсний або підроблений токен
        InvalidConfirmationToken = 32,     // Недійсний токен підтвердження

        // --- Технічні помилки (те, що не має вийти за межі сервісу) ---
        InternalServerError = 90,          // Загальна помилка сервера
        DatabaseFailure = 91               // Проблеми з підключенням/запитом до БД
    }
}
