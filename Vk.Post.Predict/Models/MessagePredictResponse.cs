namespace Vk.Post.Predict
{
    public record MessagePredictResponse
    {
        public int OwnerId { get; set; }
        public int Id { get; set; }
        public string Category { get; set; }
    }
}