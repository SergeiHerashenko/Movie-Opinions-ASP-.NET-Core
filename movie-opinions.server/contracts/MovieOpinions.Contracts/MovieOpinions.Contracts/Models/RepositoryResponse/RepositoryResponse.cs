namespace MovieOpinions.Contracts.Models.RepositoryResponse
{
    public class RepositoryResponse<T>
    {
        public bool IsSuccess { get; set; }

        public T? Data { get; set; }

        public string Message { get; set; }

        public StatusCode StatusCode { get; set; }
    }
}
