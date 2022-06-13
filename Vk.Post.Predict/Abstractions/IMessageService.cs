using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Vk.Post.Predict.Entities;

namespace Vk.Post.Predict.Abstractions;

public interface IMessageService
{
    Task<IReadOnlyCollection<Message>> GetMessages();

    Task<IReadOnlyCollection<(int OwnerId, int Id, string Category)>> GetMessages(
        IReadOnlyCollection<MessageId> messageIds, CancellationToken ct);

    Task CreateOrUpdate(Message message);
}
