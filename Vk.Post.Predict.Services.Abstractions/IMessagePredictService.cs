using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Vk.Post.Predict.Models;

namespace Vk.Post.Predict.Services.Abstractions;

public interface IMessagePredictService
{
    Task<MessagePredictResponse> Predict(IReadOnlyCollection<MessagePredictRequest> request, CancellationToken ct);
}
