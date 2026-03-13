using Authorization.Domain.Exceptions;
using Authorization.Infrastructure.Persistence.Context.AdoNet;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Authorization.Infrastructure.Persistence.Repositories.Base
{
    public abstract class RepositoryBase
    {
        protected readonly ILogger _logger;

        public RepositoryBase(ILogger logger)
        {
            _logger = logger;
        }

        protected async Task<T> ExecuteAsync<T>(Func<Task<T>> action)
        {
            try
            {
                return await action();
            }
            catch (NpgsqlException ex)
            {
                _logger.LogCritical(ex, "Критична помилка Postgres: {State}", ex.SqlState);

                throw new DatabaseOperationException("Помилка бази даних", ex.SqlState ?? string.Empty);
            }
        }
    }
}
