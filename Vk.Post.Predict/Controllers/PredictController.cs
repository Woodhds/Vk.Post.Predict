using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.ML;
using Vk.Post.Predict.Entities;

namespace Vk.Post.Predict.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PredictController : ControllerBase
    {
        private readonly PredictionEnginePool<VkMessageML, VkMessagePredict> _predictionEnginePool;
        private readonly DataContext _dataContext;

        public PredictController(PredictionEnginePool<VkMessageML, VkMessagePredict> predictionEnginePool,
            IDbContextFactory<DataContext> contextFactory)
        {
            _predictionEnginePool = predictionEnginePool;
            _dataContext = contextFactory.CreateDbContext();
        }

        [HttpPost]
        public async Task<IEnumerable<MessagePredictResponse>> GetPredictResponses(
            [FromBody] IEnumerable<MessagePredictRequest> messagePredictRequests)
        {
            var ids = messagePredictRequests.Select(f => $"{f.OwnerId}_{f.Id}");

            var messages = await _dataContext.Messages
                .Select(s => new {s.OwnerId, s.Id, s.Category, key = s.OwnerId.ToString() + "_" + s.Id.ToString()})
                .Where(f => ids.Contains(f.key)).ToListAsync();

            return messagePredictRequests.Select(request =>
            {
                var message = messages.FirstOrDefault(a => a.Id == request.Id && a.OwnerId == request.OwnerId);
                return new MessagePredictResponse
                {
                    Id = request.Id,
                    OwnerId = request.OwnerId,
                    Category = message?.Category ?? _predictionEnginePool.Predict(new VkMessageML
                    {
                        Id = request.Id,
                        Text = request.Text,
                        OwnerId = request.OwnerId
                    })?.Category,
                    IsAccept = message != null
                };
            });
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var messages = await _dataContext.Messages.ToListAsync();
            return File(JsonSerializer.SerializeToUtf8Bytes(messages, new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            }), MediaTypeNames.Text.Plain, "data.json");
        }


        [HttpPut]
        public async Task<IActionResult> Create([FromBody] Message message)
        {
            var exist = await _dataContext.Messages.FirstOrDefaultAsync(a =>
                a.Id == message.Id && a.OwnerId == message.OwnerId);
            if (exist == null)
            {
                await _dataContext.Messages.AddAsync(message);
            }
            else
            {
                exist.Category = message.Category;
            }

            await _dataContext.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete]
        public async Task<IActionResult> Delete()
        {
            _dataContext.Messages.RemoveRange(_dataContext.Messages);
            await _dataContext.SaveChangesAsync();
            return Ok();
        }
    }
}
