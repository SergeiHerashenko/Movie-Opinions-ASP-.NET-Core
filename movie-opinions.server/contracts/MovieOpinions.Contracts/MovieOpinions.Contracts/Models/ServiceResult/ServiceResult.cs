namespace MovieOpinions.Contracts.Models.ServiceResult
{
    public class ServiceResult<T>
    {
        public bool IsSuccess { get; set; }

        public StatusCode StatusCode { get; set; }

        public string? Message { get; set; }

        public T? Data { get; set; }
    }
}
