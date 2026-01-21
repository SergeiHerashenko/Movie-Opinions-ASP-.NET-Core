using System.Text.Json;
using System.Text.Json.Serialization;

namespace MovieOpinions.Contracts.Models
{
    public class StatusCodeJsonConverter : JsonConverter<StatusCode>
    {
        // Читання: перетворюємо число з JSON назад у об'єкт StatusCode
        public override StatusCode Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // Якщо в JSON прийшло число
            if (reader.TokenType == JsonTokenType.Number)
            {
                int value = reader.GetInt32();
                return new StatusCode(value);
            }

            // Якщо раптом прийшов рядок (наприклад, "200" в лапках)
            if (reader.TokenType == JsonTokenType.String)
            {
                if (int.TryParse(reader.GetString(), out int value))
                {
                    return new StatusCode(value);
                }
            }

            // Якщо структура JSON складніша, потрібно пропустити цей вузол
            // щоб не зламати серіалізацію всього об'єкта
            reader.Skip();
            return new StatusCode(500);
        }

        // Запис: перетворюємо об'єкт StatusCode у просте число для JSON
        public override void Write(Utf8JsonWriter writer, StatusCode value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue((int)value); // Використовуємо implicit operator int
        }
    }
}
