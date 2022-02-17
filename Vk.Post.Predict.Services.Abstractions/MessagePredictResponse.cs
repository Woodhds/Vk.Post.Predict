using System.Collections.Generic;

namespace Vk.Post.Predict.Models;

public record MessagePredictResponseItem(int OwnerId, int Id, string Category, bool IsAccept, IReadOnlyDictionary<string, float> Scores = null);

public class MessagePredictResponse
{
    public MessagePredictResponseItem[] Messages { get; set; }
}
