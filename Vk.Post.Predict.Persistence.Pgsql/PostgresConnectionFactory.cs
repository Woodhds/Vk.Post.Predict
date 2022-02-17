using System.Data.Common;
using Npgsql;
using Vk.Post.Predict.Persistence.Abstractions;

namespace Vk.Post.Predict.Persistence.Pgsql;

public class PostgresConnectionFactory : IConnectionFactory
{
    private readonly string _connectionString;

    public PostgresConnectionFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    public DbConnection GetConnection()
    {
        return new NpgsqlConnection(_connectionString);
    }
}
