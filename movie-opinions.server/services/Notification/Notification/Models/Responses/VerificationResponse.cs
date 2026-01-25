using MovieOpinions.Contracts.Models;

namespace Notification.Models.Responses
{
    public class VerificationResponse
    {
        public bool IsSuccess { get; set; }

        public string Data { get; set; } 

        public string Message { get; set; }

        public StatusCode StatusCode { get; set; }
    }
}
