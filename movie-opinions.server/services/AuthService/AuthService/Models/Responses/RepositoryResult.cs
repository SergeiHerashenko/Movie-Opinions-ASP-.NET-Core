using AuthService.Models.Enums;

namespace AuthService.Models.Responses
{
    public class RepositoryResult<T>
    {
        public bool IsSuccess { get; set; }

        public T? Data { get; set; }

        public string? ErrorMessage { get; set; }

        public AuthStatusCode StatusCode { get; set; }


        public static RepositoryResult<T> Success(T data) =>
        new RepositoryResult<T> { IsSuccess = true, Data = data, StatusCode = AuthStatusCode.Success };

        public static RepositoryResult<T> Failure(AuthStatusCode status, string message) =>
            new RepositoryResult<T> { IsSuccess = false, StatusCode = status, ErrorMessage = message };
    }
}
