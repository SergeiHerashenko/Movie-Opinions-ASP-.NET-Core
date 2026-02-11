using MovieOpinions.Messaging.Contracts.Models;

namespace Authorization.Domain.Request
{
    public class SendVerificationCodeRequest
    {
        public string ConfirmationToken { get; set; }

        public MessageChannels Channels { get; set; }
    }
}
