using System.Data.Common;

namespace Vk.Post.Predict.Persistence.Abstractions;

public interface IConnectionFactory
{
    DbConnection GetConnection();
}
