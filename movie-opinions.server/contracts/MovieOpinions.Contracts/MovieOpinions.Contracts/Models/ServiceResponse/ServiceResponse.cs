namespace MovieOpinions.Contracts.Models.ServiceResponse
{
    public class ServiceResponse<T>
    {
        public bool IsSuccess { get; set; }

        public T? Data { get; set; }

        public string Message { get; set; }

        public StatusCode StatusCode { get; set; }
    }
}
