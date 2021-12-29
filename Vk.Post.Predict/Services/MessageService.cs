using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;
using Vk.Post.Predict.Entities;

namespace Vk.Post.Predict.Services;

public interface IMessageService
{
    Task<IReadOnlyCollection<Message>> GetMessages();

    Task<IReadOnlyCollection<(int OwnerId, int Id, string Category)>> GetMessages(
        IReadOnlyCollection<MessageId> messageIds, CancellationToken ct);

    Task Create(Message message);
}

public class MessageService : IMessageService
{
    private readonly IConnectionFactory _connectionFactory;

    public MessageService(IConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyCollection<Message>> GetMessages()
    {
        await using var connection = _connectionFactory.GetConnection();
        await using var command = connection.CreateCommand();
        await connection.OpenAsync();
        command.CommandText =
            @"select ""Id"", ""OwnerId"", ""Category"", ""Text"", ""OwnerName"" from ""Messages""";
        await using var reader = await command.ExecuteReaderAsync();
        var messages = new List<Message>(256);
        while (await reader.ReadAsync())
        {
            messages.Add(new Message
            {
                Id = reader.GetInt32(0),
                OwnerId = reader.GetInt32(1),
                Category = reader.GetString(2),
                Text = reader.GetString(3),
                OwnerName = reader.GetString(4)
            });
        }

        return messages;
    }

    public async Task<IReadOnlyCollection<(int OwnerId, int Id, string Category)>> GetMessages(
        IReadOnlyCollection<MessageId> messageIds, 
        CancellationToken ct)
    {
        var ids = messageIds.Select(f => f.ToString()).ToArray();
        await using var connection = _connectionFactory.GetConnection();
        await using var command = connection.CreateCommand();
        command.CommandText =
            @"select ""OwnerId"", ""Id"", ""Category"" from 
                        (select concat(""OwnerId"", '_', ""Id"") as k, ""OwnerId"", ""Id"", ""Category"" from ""Messages"") sub
                        where k = any(@keys)";
        command.Parameters.AddWithValue("keys", ids);
        await connection.OpenAsync(ct);
        await command.PrepareAsync(ct);
        await using var reader = await command.ExecuteReaderAsync(ct);
        var messages = new List<(int OwnerId, int Id, string Category)>(messageIds.Count);
        while (await reader.ReadAsync(ct))
        {
            var message = (reader.GetInt32(0), reader.GetInt32(1), reader.GetString(2));
            messages.Add(message);
        }

        return messages;
    }

    public async Task Create(Message message)
    {
        await using var connection = _connectionFactory.GetConnection();
        await using var command = connection.CreateCommand();
        command.CommandText =
            @"select exists(select 1 from ""Messages"" where ""Id"" = @id and ""OwnerId"" = @owner)";
        command.Parameters.AddRange(new[]
        {
            new NpgsqlParameter("id", message.Id), new NpgsqlParameter("owner", message.OwnerId)
        });
        await connection.OpenAsync();
        await command.PrepareAsync();

        if (!(bool)await command.ExecuteScalarAsync())
        {
            command.CommandText =
                @"insert into ""Messages"" (""Id"", ""OwnerId"", ""Text"", ""Category"", ""OwnerName"") 
                        values (@id, @owner, @text, @category, @ownerName)";

            command.Parameters.AddRange(new[]
                {
                    new NpgsqlParameter("text", message.Text), new NpgsqlParameter("category", message.Category),
                    new NpgsqlParameter("ownerName", message.OwnerName),
                }
            );
        }
        else
        {
            command.CommandText =
                @"update ""Messages"" set ""Category"" = @category where ""Id"" = @id and ""OwnerId"" = @owner";
            command.Parameters.AddWithValue("category", message.Category);
        }

        await command.ExecuteNonQueryAsync();
    }
}
