namespace Vk.Post.Predict.Models
{
    public record MessagePredictRequest
    {
        public int OwnerId { get; set; }
        public int Id { get; set; }
        public string Text { get; set; }
    }
}
