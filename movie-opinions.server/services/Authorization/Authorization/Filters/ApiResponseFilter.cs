using Contracts.Model.Response;
using Contracts.Models.Status;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Reflection;

namespace Authorization.Filters
{
    public class ApiResponseFilter : IAsyncResultFilter
    {
        public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            if (context.Result is ObjectResult objectResult && objectResult.Value != null)
            {
                // 1. Встановлюємо дефолтне повідомлення
                string finalMessage = "Операція успішна";
                var data = objectResult.Value;

                // 2. Намагаємось знайти властивість "Message" в об'єкті
                PropertyInfo? messageProperty = data.GetType().GetProperty("Message");

                if (messageProperty != null)
                {
                    // Дістаємо текст повідомлення, який записаний у сервісі
                    var customMessage = messageProperty.GetValue(data)?.ToString();

                    if (!string.IsNullOrEmpty(customMessage))
                    {
                        finalMessage = customMessage;

                        // 3. Обнуляємо Message всередині об'єкта Data.
                        if (messageProperty.CanWrite)
                        {
                            messageProperty.SetValue(data, null);
                        }
                    }
                }

                // 4. Формуємо фінальну стандартну відповідь
                var response = new ServiceResponse<object>
                {
                    IsSuccess = true,
                    Message = finalMessage,
                    StatusCode = (StatusCode)context.HttpContext.Response.StatusCode,
                    Data = data
                };

                objectResult.Value = response;
            }
            else if (context.Result is StatusCodeResult || context.Result is EmptyResult)
            {
                var response = new ServiceResponse
                {
                    IsSuccess = true,
                    Message = "Виконано",
                    StatusCode = (StatusCode)context.HttpContext.Response.StatusCode
                };

                context.Result = new ObjectResult(response);
            }

            await next();
        }
    }
}