namespace Contracts.Models
{
    public class InternalRequest<TBody>
    {
        public required string ClientName { get; set; }

        public required string Endpoint { get; set; }

        public TBody? Body { get; set; }

        public required HttpMethod Method { get; set; }
    }
}
