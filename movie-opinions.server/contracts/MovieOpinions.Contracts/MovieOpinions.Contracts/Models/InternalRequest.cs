namespace MovieOpinions.Contracts.Models
{
    public class InternalRequest<TBody>
    {
        public string ClientName { get; set; }

        public string Endpoint { get; set; }

        public TBody Body { get; set; }

        public HttpMethod Method { get; set; }
    }
}
