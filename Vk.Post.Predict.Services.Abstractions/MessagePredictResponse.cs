using System.Collections.Generic;

namespace Vk.Post.Predict.Services.Abstractions;

public record MessagePredictResponseItem(int OwnerId, int Id, string Category, bool IsAccept, IReadOnlyDictionary<string, float> Scores = null);

public class MessagePredictResponse
{
    public MessagePredictResponseItem[] Messages { get; set; }
}
