using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text.Json;
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
        public IEnumerable<MessagePredictResponse> GetPredictResponses(
            [FromBody] IEnumerable<MessagePredictRequest> messagePredictRequests)
        {
            return messagePredictRequests.Select(request => new MessagePredictResponse
            {
                Id = request.Id,
                OwnerId = request.OwnerId,
                Category = _predictionEnginePool.Predict(new VkMessageML
                {
                    Id = request.Id,
                    Text = request.Text,
                    OwnerId = request.OwnerId
                })?.Category
            });
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var messages = await _dataContext.Messages.ToListAsync();
            return File(JsonSerializer.SerializeToUtf8Bytes(messages), MediaTypeNames.Text.Plain, "data.json");
        }

        [HttpPut]
        public async Task<IActionResult> Create([FromBody] Message message)
        {
            await _dataContext.Messages.AddAsync(message);
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
