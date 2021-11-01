using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.ML;
using Npgsql;
using Vk.Post.Predict.Entities;
using Vk.Post.Predict.Models;

namespace Vk.Post.Predict.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PredictController : ControllerBase
    {
        private readonly PredictionEnginePool<VkMessageML, VkMessagePredict> _predictionEnginePool;
        private readonly IConnectionFactory _connectionFactory;

        public PredictController(PredictionEnginePool<VkMessageML, VkMessagePredict> predictionEnginePool,
            IConnectionFactory connectionFactory)
        {
            _predictionEnginePool = predictionEnginePool;
            _connectionFactory = connectionFactory;
        }

        [HttpPost]
        public async Task<IEnumerable<MessagePredictResponse>> GetPredictResponses(
            [FromBody] IReadOnlyCollection<MessagePredictRequest> messagePredictRequests)
        {
            var ids = messagePredictRequests.Select(f => $"{f.OwnerId}_{f.Id}").ToArray();
            await using var connection = _connectionFactory.GetConnection();
            await using var command = connection.CreateCommand();
            command.CommandText =
                @"select ""OwnerId"", ""Id"", ""Category"" from 
                        (select concat(""OwnerId"", '_', ""Id"") as k, ""OwnerId"", ""Id"", ""Category"" from ""Messages"") sub
                        where k = any(@keys)";
            command.Parameters.AddWithValue("keys", ids);
            await connection.OpenAsync();
            await command.PrepareAsync();
            await using var reader = await command.ExecuteReaderAsync();
            var messages = new List<(int OwnerId, int Id, string Category)>();
            while (await reader.ReadAsync())
            {
                var message = (reader.GetInt32(0), reader.GetInt32(1), reader.GetString(2));
                messages.Add(message);
            }

            return messagePredictRequests.Select(request =>
            {
                var message = messages.FirstOrDefault(a => a.Id == request.Id && a.OwnerId == request.OwnerId);
                return new MessagePredictResponse
                {
                    Id = request.Id,
                    OwnerId = request.OwnerId,
                    Category = string.IsNullOrWhiteSpace(message.Category)
                        ? _predictionEnginePool.Predict(new VkMessageML
                        {
                            Id = request.Id,
                            Text = request.Text,
                            OwnerId = request.OwnerId
                        })?.Category
                        : message.Category,
                    IsAccept = message != default
                };
            });
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            await using var connection = _connectionFactory.GetConnection();
            await using var command = connection.CreateCommand();
            await connection.OpenAsync();
            command.CommandText = @"select ""Id"", ""OwnerId"", ""Category"", ""Text"", ""OwnerName"" from ""Messages""";
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

            return File(JsonSerializer.SerializeToUtf8Bytes(messages, new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            }), MediaTypeNames.Text.Plain, "data.json");
        }


        [HttpPut]
        public async Task<IActionResult> Create([FromBody] Message message)
        {
            await using var connection = _connectionFactory.GetConnection();
            await using var command = connection.CreateCommand();
            command.CommandText = @"select exists(select 1 from ""Messages"" where ""Id"" = @id and ""OwnerId"" = @owner)";
            command.Parameters.AddRange(new[]
            {
                new NpgsqlParameter("id", message.Id),
                new NpgsqlParameter("owner", message.OwnerId)
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
                    new NpgsqlParameter("text", message.Text),
                    new NpgsqlParameter("category", message.Category),
                    new NpgsqlParameter("ownerName", message.OwnerName),
                });
            }
            else
            {
                command.CommandText = @"update ""Messages"" set ""Category"" = @category where ""Id"" = @id and ""OwnerId"" = @owner";
                command.Parameters.AddWithValue("category", message.Category);
            }

            await command.ExecuteNonQueryAsync();

            return Ok();
        }

        [HttpDelete]
        public async Task<IActionResult> Delete()
        {
            await using var connection = _connectionFactory.GetConnection();
            await using var command = connection.CreateCommand();
            command.CommandText = @"truncate ""Messages""";
            await command.ExecuteNonQueryAsync();
            return Ok();
        }
    }
}
