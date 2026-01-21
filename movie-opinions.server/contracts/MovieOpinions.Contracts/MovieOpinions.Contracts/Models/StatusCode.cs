using System.Text.Json.Serialization;

namespace MovieOpinions.Contracts.Models
{
    [JsonConverter(typeof(StatusCodeJsonConverter))]
    public class StatusCode
    {
        private readonly int _value;
        public StatusCode(int value) => _value = value;

        public static implicit operator StatusCode(int value) => new StatusCode(value);

        public static implicit operator int(StatusCode code) => code._value;

        // Загальні статуси (читання, базові успіхи/помилки)
        public static class General
        {
            public const int Ok = 200;               // Успішно (GET, іноді PUT)
            public const int BadRequest = 400;       // Помилка валідації або логіки
            public const int NotFound = 404;         // Ресурс не знайдено
            public const int InternalError = 500;    // Щось пішло зовсім не так
        }

        // Створення ресурсів (POST)
        public static class Create
        {
            public const int Created = 201;          // Успішно створено
            public const int Conflict = 409;         // Вже існує (наприклад, такий Email)
        }

        // Оновлення ресурсів (PUT / PATCH)
        public static class Update
        {
            public const int Ok = 200;               // Оновлено + повернуто дані
            public const int NoContent = 204;        // Оновлено (відповідь порожня)
        }

        // Видалення ресурсів (DELETE)
        public static class Delete
        {
            public const int Ok = 200;               // Видалено + повідомлення
            public const int NoContent = 204;        // Видалено (відповідь порожня)
        }

        // Авторизація та Доступ
        public static class Auth
        {
            public const int Unauthorized = 401;     // Треба залогінитись
            public const int Forbidden = 403;        // Немає прав
            public const int Locked = 423;           // Аккаунт заблоковано (Locked)
        }

        // Верифікація (специфічні випадки)
        public static class Verification
        {
            public const int Expired = 410;          // Токен прострочений (Gone)
            public const int Invalid = 422;          // Токен невірний (Unprocessable Entity)
        }
    }
}
