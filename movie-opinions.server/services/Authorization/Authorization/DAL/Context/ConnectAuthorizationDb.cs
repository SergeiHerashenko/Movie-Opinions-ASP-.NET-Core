using Authorization.DAL.Context.Interface;

namespace Authorization.DAL.Context
{
    public class ConnectAuthorizationDb : IDbConnectionProvider
    {
        private readonly IConfiguration _configuration;

        public ConnectAuthorizationDb(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GetConnectionString()
        {
            return _configuration.GetConnectionString("DefaultConnection")
                   ?? throw new Exception("Connection string is missing in Secrets/Appsettings!");
        }
    }
}
