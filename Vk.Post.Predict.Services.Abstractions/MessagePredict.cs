namespace Vk.Post.Predict.Services.Abstractions;

public record MessagePredictRequest(int OwnerId, int Id, string Text);
