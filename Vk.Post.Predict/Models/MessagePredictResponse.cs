namespace Vk.Post.Predict.Models
{
    public record MessagePredictResponse(int OwnerId, int Id, string Category, bool IsAccept);
}
