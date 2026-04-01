using Authorization.Application.Exceptions.BaseException;
using Authorization.Domain.Exceptions;
using Authorization.Domain.Exceptions.BaseException;
using Authorization.Response;
using Microsoft.AspNetCore.Diagnostics;

namespace Authorization.Middleware
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
            var (statusCode, errorCode) = exception switch
            {
                NotFoundException ex => (StatusCodes.Status404NotFound, ex.ErrorCode),
                ForbiddenException ex => (StatusCodes.Status403Forbidden, ex.ErrorCode),
                BadRequestException ex => (StatusCodes.Status400BadRequest, ex.ErrorCode),
                UnauthorizedException ex => (StatusCodes.Status401Unauthorized, ex.ErrorCode),

                BaseDomainException ex => (StatusCodes.Status400BadRequest, ex.ErrorCode),

                BaseApplicationException ex => (StatusCodes.Status400BadRequest, ex.ErrorCode),

                _ => (StatusCodes.Status500InternalServerError, "Server Error")
            };

            if(statusCode >= 500)
                _logger.LogError(exception, "Критична помилка: {Message}", exception.Message);
            else
                _logger.LogWarning("Помилка додатка [{Code}]: {Message}", errorCode, exception.Message);

            httpContext.Response.StatusCode = statusCode;
            httpContext.Response.ContentType = "application/json";

            var errorResponse = new ErrorResponse()
            {
                IsSuccess = false,
                Message = statusCode >= 500 ? "Виникла внутрішня помилка сервера." : exception.Message,
                StatusCode = statusCode,
                ErrorCode = errorCode
            };

            await httpContext.Response.WriteAsJsonAsync(errorResponse, cancellationToken);

            return true;
        }
    }
}