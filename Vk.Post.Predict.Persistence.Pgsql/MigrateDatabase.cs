using System.Threading.Tasks;
using Vk.Post.Predict.Persistence.Abstractions;

namespace Vk.Post.Predict.Persistence.Pgsql;

public class MigrateDatabase : IMigrateDatabase
{
    private readonly IConnectionFactory _connectionFactory;

    public MigrateDatabase(IConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task MigrateAsync()
    {
        await using var connection = _connectionFactory.GetConnection();
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = @"
                CREATE TABLE IF NOT EXISTS ""Messages"" (
                    ""Id"" integer,
                    ""OwnerId"" integer,
                    ""OwnerName"" varchar,
                    ""Text"" varchar,
                    ""Category"" varchar
                );
                create unique index if not exists ""IX_Messages_Id_OwnerId"" on ""Messages""(""OwnerId"", ""Id"")
            ";
        await command.ExecuteNonQueryAsync();
    }
}
