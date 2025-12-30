namespace Authorization.DAL.Connect_Database
{
    public interface IConnectAuthorizationDb
    {
        string Host { get; }

        string User { get; }

        string Password { get; }

        string Port { get; }

        string DatabaseName { get; }

        string GetConnectAuthorizationDatabase();
    }
}
