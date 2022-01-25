using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.ML;
using Microsoft.ML.Data;
using Vk.Post.Predict.Entities;
using Vk.Post.Predict.Models;

namespace Vk.Post.Predict.Services;

public interface IMessagePredictService
{
    Task<MessagePredictResponse> Predict(MessagePredictRequest[] request, CancellationToken ct);
    Task<IReadOnlyDictionary<string, decimal>> Predict(MessagePredictRequest request, CancellationToken ct);
}

public class MessagePredictService : IMessagePredictService
{
    private static string[] Categories = new[]
    {
        "Деньги", "Еда", "Косметика", "Одежда", "Домашняя техника", "Эл. техника", "Кондитерские изд.", "Для детей",
        "Мебель", "Сертификаты", "Белье", "Для авто", "Билеты", "Ювелирка", "Посуда", "Для животных",
        "Не конкурс", "Книги", "Досуг", "Спорт", "Комп. игры", "Бытовая химия", "Медицина", "Разное",
        "Инструменты и ремонт", "Канцтовары"
    };
    
    private readonly PredictionEnginePool<VkMessageML, VkMessagePredict> _predictionEnginePool;
    private readonly IMessageService _messageService;

    public MessagePredictService(PredictionEnginePool<VkMessageML, VkMessagePredict> predictionEnginePool,
        IMessageService messageService)
    {
        _predictionEnginePool = predictionEnginePool;
        _messageService = messageService;
    }

    public async Task<MessagePredictResponse> Predict(MessagePredictRequest[] request, CancellationToken ct)
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
                    return new MessagePredictResponseItem(e.OwnerId, e.Id, string.IsNullOrEmpty(category)
                        ? _predictionEnginePool
                            .Predict(new VkMessageML {Text = e.Text, OwnerId = e.OwnerId, Id = e.Id})
                            ?.Category
                        : category, category != default);
                }).ToArray()
        };
    }

    public async Task<IReadOnlyDictionary<string, decimal>> Predict(MessagePredictRequest request, CancellationToken ct)
    {
        var response = _predictionEnginePool.Predict(new VkMessageML
        {
            Id = request.Id, OwnerId = request.OwnerId, Text = request.Text
        });

        var top10scores = Categories.ToDictionary(
                l => l,
                l => (decimal)response.Score[Array.IndexOf(Categories, l)]
            )
            .OrderByDescending(kv => kv.Value)
            .Take(10);

        return top10scores.ToDictionary(x => x.Key, x => x.Value);
    }
}
