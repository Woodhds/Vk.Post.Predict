namespace Vk.Post.Predict.Models;

public record MessageSaveRequest(int OwnerId, int Id, string Category, string Text);
