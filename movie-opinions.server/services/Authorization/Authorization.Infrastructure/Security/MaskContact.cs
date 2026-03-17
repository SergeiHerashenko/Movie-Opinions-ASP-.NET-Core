using Authorization.Application.Interfaces.Security;
using Contracts.Integration;

namespace Authorization.Infrastructure.Security
{
    public class MaskContact : IMaskContact
    {
        public string MaskContactValue(string value, CommunicationChannel channel)
        {
            if (string.IsNullOrEmpty(value)) return value;

            if (channel == CommunicationChannel.Email)
            {
                var parts = value.Split('@');
                if (parts.Length < 2) return value;

                string name = parts[0];
                string domain = parts[1];

                if (name.Length <= 2) return $"{name[0]}***@{domain}";

                return $"{name.Substring(0, 2)}****{name.Substring(name.Length - 1)}@{domain}";
            }
            else if (channel == CommunicationChannel.Phone)
            {
                if (value.Length < 4) return value;
                return $"{value.Substring(0, value.Length - 4)}****{value.Substring(value.Length - 2)}";
            }

            return value;
        }
    }
}
