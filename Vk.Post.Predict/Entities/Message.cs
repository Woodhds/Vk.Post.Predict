namespace Vk.Post.Predict.Entities
{
    public class Message
    {
        public int Id { get; set; }
        public int OwnerId { get; set; }
        public string Text { get; set; }
        public string Category { get; set; }
        public string OwnerName { get; set; }
    }
}
