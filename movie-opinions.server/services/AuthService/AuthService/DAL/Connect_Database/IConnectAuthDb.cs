namespace AuthService.DAL.Connect_Database
{
    public interface IConnectAuthDb
    {
        string Host { get; }

        string User { get; }

        string Password { get; }

        string Port { get; }

        string DatabaseName { get; }

        string GetConnectAuthDataBase();
    }
}
