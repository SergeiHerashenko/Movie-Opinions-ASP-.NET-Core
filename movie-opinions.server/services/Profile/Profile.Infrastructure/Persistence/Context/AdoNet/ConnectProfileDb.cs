using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Profile.Infrastructure.Persistence.Context.AdoNet
{
    public class ConnectProfileDb : IDbConnectionProvider
    {
        private readonly IConfiguration _configuration;

        public ConnectProfileDb(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GetConnectionString()
        {
            return _configuration.GetConnectionString("DefaultConnection")
                   ?? throw new Exception("Connection string is missing in Secrets/Appsettings!");
        }

        public async Task<NpgsqlConnection> GetOpenConnectionAsync()
        {
            var connection = new NpgsqlConnection(GetConnectionString());

            await connection.OpenAsync();

            return connection;
        }
    }
}
