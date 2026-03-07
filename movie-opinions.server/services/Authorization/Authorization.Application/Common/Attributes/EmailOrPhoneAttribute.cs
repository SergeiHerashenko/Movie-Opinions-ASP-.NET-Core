using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Authorization.Application.Common.Attributes
{
    public class EmailOrPhoneAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var input = value as string;

            if (string.IsNullOrWhiteSpace(input))
                return new ValidationResult("Поле не може бути порожнім");

            // Регулярка для Email
            bool isEmail = Regex.IsMatch(input, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");

            // Регулярка для телефону
            bool isPhone = Regex.IsMatch(input, @"^\+?[1-9]\d{1,14}$");

            if (isEmail || isPhone)
                return ValidationResult.Success;

            return new ValidationResult("Введіть коректний Email або номер телефону");
        }
    }
}
