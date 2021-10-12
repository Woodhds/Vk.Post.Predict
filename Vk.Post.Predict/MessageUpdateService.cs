using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Npgsql;
using Vk.Post.Predict.Models;

namespace Vk.Post.Predict
{
    public interface IMessageUpdateService
    {
        /// <summary>
        /// Update Owner's names by id
        /// </summary>
        /// <param name="owners"></param>
        /// <returns>Count affected rows</returns>
        ValueTask<int> UpdateMessageOwners(IEnumerable<UpdateMessageOwner> owners);
    }

    public class MessageUpdateService : IMessageUpdateService
    {
        private readonly IConnectionFactory _connectionFactory;

        public MessageUpdateService(IConnectionFactory contextFactory)
        {
            _connectionFactory = contextFactory;
        }

        public async ValueTask<int> UpdateMessageOwners(IEnumerable<UpdateMessageOwner> owners)
        {
            if (owners == null || !owners.Any())
            {
                return 0;
            }

            var ownerDictionary = owners.ToDictionary(f => f.Id, f => f.Name);
            var ownerIds = ownerDictionary.Select(f => f.Key);

            await using var connection = _connectionFactory.GetConnection();
            await using var command = connection.CreateCommand();
            command.CommandText = @"Select ""Id"", ""OwnerId"" from ""Messages"" where ""OwnerId"" = any(@ownerIds)";
            command.Parameters.AddWithValue("ownerIds", ownerIds);

            await connection.OpenAsync();
            
            await using var reader = await command.ExecuteReaderAsync();
            await using var batch = connection.CreateBatch();
            while (await reader.ReadAsync())
            {
                var owner = reader.GetInt32(1);
                batch.BatchCommands.Add(new NpgsqlBatchCommand
                {
                    CommandText = @"update ""Messages"" set ""OwnerName"" = @owner where ""Id"" = @id",
                    Parameters =
                    {
                        new NpgsqlParameter("id", reader.GetInt32(0)),
                        new NpgsqlParameter("owner", ownerDictionary[owner])
                    }
                });
            }

            await batch.ExecuteNonQueryAsync();

            return reader.RecordsAffected;
        }
    }
}
