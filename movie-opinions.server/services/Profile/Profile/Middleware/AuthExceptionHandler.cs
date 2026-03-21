using Contracts.Models.Response;
using Microsoft.AspNetCore.Diagnostics;
using Profile.Domain.Exceptions;
using static Contracts.Models.Status.StatusCode;

namespace Profile.Middleware
{
    public class AuthExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<AuthExceptionHandler> _logger;

        public AuthExceptionHandler(ILogger<AuthExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(
           HttpContext httpContext,
           Exception exception,
           CancellationToken cancellationToken)
        {
            var (statusCode, message) = exception switch
            {
                BaseApplicationException appEx => (appEx.StatusCode, appEx.Message),

                _ => (General.InternalError, "Сталася внутрішня помилка сервера")
            };

            if (statusCode >= 500)
            {
                _logger.LogError(exception, "Критична помилка: {Message}", exception.Message);
            }
            else
            {
                _logger.LogWarning("Попередження бізнес-логіки: {Message}", exception.Message);
            }

            httpContext.Response.StatusCode = statusCode;
            httpContext.Response.ContentType = "application/json";

            var errorResponse = new Result()
            {
                IsSuccess = false,
                Message = message,
                StatusCode = statusCode
            };

            await httpContext.Response.WriteAsJsonAsync(errorResponse, cancellationToken);

            return true;
        }
    }
}
