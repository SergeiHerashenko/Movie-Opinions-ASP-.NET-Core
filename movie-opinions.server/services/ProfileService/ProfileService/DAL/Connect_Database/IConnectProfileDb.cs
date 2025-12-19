namespace ProfileService.DAL.Connect_Database
{
    public interface IConnectProfileDb
    {
        string Host { get; }

        string User { get; }

        string Password { get; }

        string Port { get; }

        string DatabaseName { get; }

        string GetConnectProfileDataBase();
    }
}
