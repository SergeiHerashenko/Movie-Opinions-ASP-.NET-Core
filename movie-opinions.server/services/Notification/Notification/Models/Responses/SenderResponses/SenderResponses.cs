using MovieOpinions.Contracts.Models;

namespace Notification.Models.Responses.SenderResponses
{
    public class SenderResponses
    {
        public bool IsSuccess { get; set; }

        public StatusCode StatusCode { get; set; }

        public string Messeage { get; set; }

        public Guid? ErrorId { get; set; }
    }
}
