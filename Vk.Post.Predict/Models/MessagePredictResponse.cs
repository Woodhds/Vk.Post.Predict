namespace Vk.Post.Predict.Models;

public record MessagePredictResponseItem(int OwnerId, int Id, string Category, bool IsAccept);

public class MessagePredictResponse
{
    public MessagePredictResponseItem[] Messages { get; set; }
}
