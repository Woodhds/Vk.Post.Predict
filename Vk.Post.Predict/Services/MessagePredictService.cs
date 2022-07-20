using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.ML;
using Microsoft.ML.Data;
using Vk.Post.Predict.Abstractions;
using Vk.Post.Predict.Entities;
using Vk.Post.Predict.Models;

namespace Vk.Post.Predict.Services;

public class MessagePredictService : global::MessagePredictService.MessagePredictServiceBase
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

    public override async Task<MessagePredictResponse> Predict(MessagePredictRequest request, ServerCallContext context)
    {
        var messages =
            await _messageService.GetMessages(
                request.Messages.Select(f => new MessageId(f.Id, f.OwnerId)).ToArray(), context.CancellationToken);

        return new MessagePredictResponse
        {
            Messages =
            {
                request.Messages.GroupJoin(messages,
                    a => new {a.Id, a.OwnerId},
                    a => new {a.Id, a.OwnerId},
                    (e, y) =>
                    {
                        var category = y.Select(f => f.Category).FirstOrDefault();

                        if (!string.IsNullOrEmpty(category))
                        {
                            return new MessagePredictResponse.Types.ResponseItem
                            {
                                OwnerId = e.OwnerId, 
                                Id = e.Id, 
                                Category = category, 
                                IsAccept = true
                            };
                        }

                        var predictResult = PredictInternal(e);

                        return new MessagePredictResponse.Types.ResponseItem
                        {
                            OwnerId = e.OwnerId,
                            Id = e.Id,
                            Category = predictResult.Category,
                            IsAccept = false,
                            Scores = {predictResult.Scores}
                        };
                    }).ToArray()
            }
        };
    }

    public override async Task<Empty> Save(MessageSaveRequest request, ServerCallContext context)
    {
        await _messageService.CreateOrUpdate(new Message
        {
            OwnerId = request.OwnerId,
            Text = request.Text,
            Category = request.Category,
            OwnerName = request.OwnerName,
            Id = request.Id
        });

        return new Empty();
    }

    private (string Category, IDictionary<string, float> Scores) PredictInternal(
        MessagePredictRequest.Types.PredictRequest request)
    {
        var response = _predictionEnginePool.Predict(new()
        {
            Id = request.Id, OwnerId = request.OwnerId, Text = request.Text
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
