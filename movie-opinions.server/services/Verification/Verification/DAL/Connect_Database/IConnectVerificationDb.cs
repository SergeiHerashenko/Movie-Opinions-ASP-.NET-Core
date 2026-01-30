namespace Verification.DAL.Connect_Database
{
    public interface IConnectVerificationDb
    {
        string Host { get; }

        string User { get; }

        string Password { get; }

        string Port { get; }

        string DatabaseName { get; }

        string GetConnectVerificatioDataBase();
    }
}
