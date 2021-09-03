namespace Vk.Post.Predict.Models
{
    public record MessagePredictResponse
    {
        public int OwnerId { get; init; }
        public int Id { get; init; }
        public string Category { get; init; }
        public bool IsAccept { get; init; }
    }
}
