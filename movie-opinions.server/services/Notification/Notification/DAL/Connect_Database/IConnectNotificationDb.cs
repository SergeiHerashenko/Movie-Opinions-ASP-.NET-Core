namespace Notification.DAL.Connect_Database
{
    public interface IConnectNotificationDb
    {
        string Host { get; }

        string User { get; }

        string Password { get; }

        string Port { get; }

        string DatabaseName { get; }

        string GetConnectNotificationDataBase();
    }
}
