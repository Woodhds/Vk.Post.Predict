using Npgsql;

namespace Vk.Post.Predict;

public interface IConnectionFactory
{
    NpgsqlConnection GetConnection();
}

public class ConnectionFactory : IConnectionFactory
{
    private readonly string _connectionString;

    public ConnectionFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    public NpgsqlConnection GetConnection()
    {
        return new NpgsqlConnection(_connectionString);
    }
}
