namespace Template.DAL.Connect_Database
{
    public interface IConnectTemplateDb
    {
        string Host { get; }

        string User { get; }

        string Password { get; }

        string Port { get; }

        string DatabaseName { get; }

        string GetConnectTemplateDataBase();
    }
}
