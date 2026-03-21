using Npgsql;

namespace Profile.Infrastructure.Persistence.Context.AdoNet
{
    public interface IDbConnectionProvider
    {
        string GetConnectionString();

        Task<NpgsqlConnection> GetOpenConnectionAsync();
    }
}
