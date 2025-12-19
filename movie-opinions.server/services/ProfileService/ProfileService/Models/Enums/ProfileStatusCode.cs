namespace ProfileService.Models.Enums
{
    public enum ProfileStatusCode
    {
        None = 0,

        // 2xx – Позитивні статуси
        Success = 20000,             // Загальний успіх
        ProfileCreated = 20101,      // Профіль успішно створено (після реєстрації)
        ProfileUpdated = 20001,      // Дані оновлено
        ProfileDeleted = 20002,      // Профіль видалено

        // 4xx – клієнтські помилки
        ProfileNotFound = 40401,
        ProfileAccessDenied = 40301,
        ProfileValidationFailed = 40001,

        // 409 – конфлікти
        ProfileAlreadyExists = 40901,

        // 5xx – серверні
        ProfileUpdateFailed = 50001,
        ProfileInternalError = 50099
    }
}
