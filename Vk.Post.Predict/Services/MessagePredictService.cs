using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.ML;
using Microsoft.ML.Data;
using Vk.Post.Predict.Entities;
using Vk.Post.Predict.Models;
using Vk.Post.Predict.Services.Abstractions;

namespace Vk.Post.Predict.Services;

public class MessagePredictService : IMessagePredictService
{
    private readonly PredictionEnginePool<VkMessageML, VkMessagePredict> _predictionEnginePool;
    private readonly IMessageService _messageService;
    private IReadOnlyCollection<string> _categories;

    public MessagePredictService(PredictionEnginePool<VkMessageML, VkMessagePredict> predictionEnginePool,
        IMessageService messageService)
    {
        _predictionEnginePool = predictionEnginePool;
        _messageService = messageService;
        InitCategories();
    }

    public async Task<MessagePredictResponse> Predict(IReadOnlyCollection<MessagePredictRequest> request, CancellationToken ct)
    {
        var messages =
            await _messageService.GetMessages(
                request.Select(f => new MessageId(f.Id, f.OwnerId)).ToArray(), ct);

        return new MessagePredictResponse
        {
            Messages = request.GroupJoin(messages,
                a => new {a.Id, a.OwnerId},
                a => new {a.Id, a.OwnerId},
                (e, y) =>
                {
                    var category = y.Select(f => f.Category).FirstOrDefault();

                    if (!string.IsNullOrEmpty(category))
                    {
                        return new MessagePredictResponseItem(e.OwnerId, e.Id, category, true);
                    }

                    var predictResult = PredictInternal(e);

                    return new MessagePredictResponseItem(e.OwnerId, e.Id, predictResult.Category, false, predictResult.Scores);
                }).ToArray()
        };
    }

    public IReadOnlyDictionary<string, float> Predict(MessagePredictRequest request)
    {
        return PredictInternal(request).Scores;
    }

    private (string Category, IReadOnlyDictionary<string, float> Scores) PredictInternal(MessagePredictRequest request)
    {
        var response = _predictionEnginePool.Predict(new()
        {
            Id = request.Id, 
            OwnerId = request.OwnerId, 
            Text = request.Text
        });

        var top10Scores = _categories.Zip(response.Score)
            .OrderByDescending(f => f.Second)
            .Take(10)
            .ToDictionary(x => x.First, x => x.Second);

        return (response.Category, top10Scores);
    }

    private void InitCategories()
    {
        var labelBuffer = new VBuffer<ReadOnlyMemory<char>>();
        _predictionEnginePool.GetPredictionEngine().OutputSchema["Score"].Annotations
            .GetValue("SlotNames", ref labelBuffer);
        _categories = labelBuffer.DenseValues().Select(l => l.ToString()).ToArray();
    }
}
