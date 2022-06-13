using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Vk.Post.Predict.Services.Abstractions;

public interface IMessagePredictService
{
    Task<MessagePredictResponse> Predict(IReadOnlyCollection<MessagePredictRequest> request, CancellationToken ct);
}
