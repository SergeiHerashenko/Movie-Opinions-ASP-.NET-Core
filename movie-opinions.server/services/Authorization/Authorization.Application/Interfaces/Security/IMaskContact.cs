using Contracts.Integration;

namespace Authorization.Application.Interfaces.Security
{
    public interface IMaskContact
    {
        string MaskContactValue(string value, CommunicationChannel channel);
    }
}
