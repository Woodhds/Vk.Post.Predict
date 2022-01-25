namespace Vk.Post.Predict.Entities;

public class Message
{
    public int Id { get; init; }
    public int OwnerId { get; init; }
    public string Text { get; init; }
    public string Category { get; init; }
    public string OwnerName { get; init; }
}
