using System.Collections.Generic;
using System.Linq;
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

        public PredictController(PredictionEnginePool<VkMessageML, VkMessagePredict> predictionEnginePool)
        {
            _predictionEnginePool = predictionEnginePool;
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

        /*
        [HttpPut]
        public async Task<IActionResult> Create([FromBody] Message message)
        {
            await _dataContext.Messages.AddAsync(message);
            await _dataContext.SaveChangesAsync();
            return Ok();
        }*/
    }
}
