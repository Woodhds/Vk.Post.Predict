using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.ML;
using Vk.Post.Predict.Entities;
using Vk.Post.Predict.Models;
using Vk.Post.Predict.Services;

namespace Vk.Post.Predict.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PredictController : ControllerBase
    {
        private readonly PredictionEnginePool<VkMessageML, VkMessagePredict> _predictionEnginePool;
        private readonly IConnectionFactory _connectionFactory;
        private readonly IMessageService _messageService;

        public PredictController(PredictionEnginePool<VkMessageML, VkMessagePredict> predictionEnginePool,
            IConnectionFactory connectionFactory, IMessageService messageProvider)
        {
            _predictionEnginePool = predictionEnginePool;
            _connectionFactory = connectionFactory;
            _messageService = messageProvider;
        }

        [HttpPost]
        public async Task<IEnumerable<MessagePredictResponse>> GetPredictResponses(
            [FromBody] IReadOnlyCollection<MessagePredictRequest> messagePredictRequests)
        {
            var messages =
                await _messageService.GetMessages(
                    messagePredictRequests.Select(f => new MessageId(f.Id, f.OwnerId))
                    .ToArray());

            return messagePredictRequests.Select(request =>
            {
                var message = messages.FirstOrDefault(a => a.Id == request.Id && a.OwnerId == request.OwnerId);
                return new MessagePredictResponse(request.OwnerId, request.Id,
                    string.IsNullOrWhiteSpace(message.Category)
                        ? _predictionEnginePool.Predict(new VkMessageML
                        {
                            Id = request.Id,
                            Text = request.Text,
                            OwnerId = request.OwnerId
                        })?.Category
                        : message.Category, message != default);
            });
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var messages = await _messageService.GetMessages();

            return File(JsonSerializer.SerializeToUtf8Bytes(messages, new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            }), MediaTypeNames.Text.Plain, "data.json");
        }


        [HttpPut]
        public async Task<IActionResult> Create([FromBody] Message message)
        {
            await _messageService.Create(message);
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
