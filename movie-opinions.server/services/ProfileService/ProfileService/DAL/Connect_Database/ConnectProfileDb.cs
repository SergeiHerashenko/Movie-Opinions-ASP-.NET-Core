namespace ProfileService.DAL.Connect_Database
{
    public class ConnectProfileDb : IConnectProfileDb
    {
        // IP-адреса сервера бази даних /The IP address of the database server/
        public string Host { get; private set; } = "";

        // Ім'я користувача бази даних /Database username/
        public string User { get; private set; } = "";

        // Пароль для підключення до бази даних /Password to connect to the database/
        public string Password { get; private set; } = "";

        // Порт підключення до бази даних /Database connection port/
        public string Port { get; private set; } = "";
        // Назва бази даних /The name of the database/
        public string DatabaseName { get; private set; } = "";

        // Рядок підключення до бази /Connection string to the database/
        public string GetConnectProfileDataBase()
        {
            string connString =
            string.Format(
                "Server={0};Username={1};Database={2};Port={3};Password={4};SSLMode=Prefer",
                Host,
                User,
                DatabaseName,
                Port,
                Password);

            return connString;
        }
    }
}
